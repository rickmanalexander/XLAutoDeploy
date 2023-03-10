using XLAutoDeploy.FileSystem.Access;
using XLAutoDeploy.FileSystem.Monitoring;
using XLAutoDeploy.Updates;
using XLAutoDeploy.Logging;

using XLAutoDeploy.Manifests;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using XLAutoDeploy.Manifests.Utilities;

namespace XLAutoDeploy.Deployments
{
    /// <summary>
    /// Use this to monitor a set of <see cref="AddIn"/>(s) (withint a <see cref="DeploymentPayload"/>) 
    /// located on a file server. If any of the <see cref="AddIn"/> file(s) change, an event
    /// is raised that then queues an update action based on the <see cref="UpdateBehavior"/> 
    /// set in the <see cref="Deployment"/>.
    /// </summary>
    internal sealed class UpdateMonitor : IDisposable
    {
        private readonly IFileSystemWatcherFactory _watcherFactory;
        private readonly IFileSystemWatcherEventAggregator _watcherEventAggregator;
        private readonly IFileSystemMonitorFactory _monitorFactory;

        private readonly IEnumerable<DeploymentPayload> _deploymentPayloads;
        private readonly IUpdateCoordinator _updateCoordinator;
        private readonly IRemoteFileDownloader _remoteFileDowloader;
        private readonly ILogger _logger;
        private readonly uint _sessionNotificationLimit;

        private IDictionary<string, uint> _deploymentFilePathNotificationCounts = new Dictionary<string, uint>(StringComparer.OrdinalIgnoreCase);
        //For every directory we watch, keep track of all the add-ins that have files in that directory
        private IDictionary<string, IFileSystemMonitor> _monitoredDirectories = new Dictionary<string, IFileSystemMonitor>();

        private bool _disposed;

        public UpdateMonitor(IFileSystemWatcherFactory watcherFactory, IFileSystemWatcherEventAggregator eventAggregator,
                IFileSystemMonitorFactory monitorFactory, IEnumerable<DeploymentPayload> deploymentPayloads,
                IUpdateCoordinator updateService, IRemoteFileDownloader remoteFileDowloader, ILogger logger, uint sessionNotificationLimit = 1)
        {
            _watcherFactory = watcherFactory;
            _watcherEventAggregator = eventAggregator;
            _monitorFactory = monitorFactory;

            _deploymentPayloads = deploymentPayloads?.Where(d => d.FileHost.HostType == FileHostType.FileServer & d.Deployment.Settings.UpdateBehavior.DoInRealTime)?.ToList();

            if (_deploymentPayloads?.Any() == false)
            {
                throw new InvalidOperationException(Common.GetFormatedErrorMessage($"Constructing type {nameof(UpdateMonitor)}.",
                    $"The where no {nameof(deploymentPayloads)} add-ins found whose FileHostConfiguration {nameof(FileHostType)} == {nameof(FileHostType.FileServer)} with UpdateBehavior.DoInRealTime == true.",
                    "Supply one or more RemoteAddIns that match the aforementioned criteria."));
            }

            _updateCoordinator = updateService;
            _remoteFileDowloader = remoteFileDowloader;
            _logger = logger;
            _sessionNotificationLimit = sessionNotificationLimit;

            SetMonitoredAddIns();
        }

        private void SetMonitoredAddIns()
        {
            foreach (var payload in _deploymentPayloads)
            {
                if (payload.FileHost.HostType == FileHostType.FileServer)
                {
                    try
                    {
                        if (payload.Deployment.Settings.UpdateBehavior.DoInRealTime)
                        {
                            var filePath = payload.AddIn.DeploymentUriString;

                            var directory = Path.GetDirectoryName(filePath);

                            if (!_monitoredDirectories.TryGetValue(directory, out IFileSystemMonitor monitoredDirectory))
                            {
                                var watcher = _watcherFactory.Create(directory, NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size);

                                monitoredDirectory = _monitorFactory.Create(watcher, _watcherEventAggregator);
                                monitoredDirectory.Events.Changed += QueueUpdateAction;
                            }

                            monitoredDirectory.MonitorFile(filePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, $"Update monitoring setup failed for add-in titled {payload.AddIn.Identity.Title}");
                        throw;
                    }
                }
            }
        }

        //How to handle file rename?????
        private void QueueUpdateAction(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Changed)
            {
                var senderFilePath = e.FullPath;
                var payload = _deploymentPayloads.Where(x => x.AddIn.DeploymentUriString.Equals(senderFilePath, StringComparison.OrdinalIgnoreCase))?.ToList()?[0];

                if (payload == null)
                    return;

                try
                {
                    AutoUpdateAddIn(senderFilePath, payload);

                    if (UpdateService.IsRestartRequired(payload))
                    {
                        Common.DisplayMessageBox($"The Excel Application will now be closed " +
                            $"so that the deployed update(s) can take effect. " +
                            $"{System.Environment.NewLine}{System.Environment.NewLine}" +
                            $" Once closed, you may re-open Excel as you would normally.", false);

                        InteropIntegration.CloseExcelApp();
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, $"Update monitoring Update action failed for add-in titled {payload.AddIn.Identity.Title}");
                    throw;
                }
            }
        }

        private void AutoUpdateAddIn(string filePath, DeploymentPayload payload)
        {
            // use copy/clone, mutate, and replace to avoid threading issues
            var notificationClone = _deploymentFilePathNotificationCounts;

            if (!notificationClone.ContainsKey(filePath))
            {
                notificationClone.Add(filePath, 0);
            }

            if (notificationClone[filePath] <= _sessionNotificationLimit)
            {
                var deployedAddInManifestFilePath = payload.GetAddInManifestFilePath();

                var deployedAddInVersion = ManifestSerialization.DeserializeManifestFile<AddIn>(deployedAddInManifestFilePath).Identity.Version;

                var currentDateTime = DateTime.UtcNow;
                var updateQueryInfoManifestFilePath = payload.GetUpdateQueryInfoManifestFilePath();

                UpdateQueryInfo existingUpdateQueryInfo = null;
                if (File.Exists(updateQueryInfoManifestFilePath))
                {
                    existingUpdateQueryInfo = ManifestSerialization.DeserializeManifestFile<UpdateQueryInfo>(updateQueryInfoManifestFilePath);
                }

                var checkedUpdate = DeploymentService.GetCheckedUpdate(payload, deployedAddInVersion, currentDateTime);
                checkedUpdate.Info.FirstNotified = existingUpdateQueryInfo?.FirstNotified;

                if (UpdateService.CanProceedWithUpdate(checkedUpdate, _updateCoordinator))
                {
                    DeploymentService.ProcessUpdate(checkedUpdate, _updateCoordinator, _remoteFileDowloader);
                }

                Serialization.SerializeToXmlFile(checkedUpdate.Info, updateQueryInfoManifestFilePath);
            }

            notificationClone[filePath] = notificationClone[filePath]++;

            _deploymentFilePathNotificationCounts = notificationClone;
        }

        ~UpdateMonitor()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Dispose(bool disposing)
        {
            if (this._disposed)
            {
                return;
            }

            if (disposing)
            {
                foreach (var watchedDirectory in _monitoredDirectories?.Values)
                {
                    watchedDirectory?.Dispose();
                }

                _monitoredDirectories = null;
            }

            this._disposed = true;
        }
    }
}
