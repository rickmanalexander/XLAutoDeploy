using XLAutoDeploy.Updates;
using XLAutoDeploy.Logging;

using XLAutoDeploy.Manifests;
using XLAutoDeploy.Manifests.Utilities;

using ExcelDna.Integration;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
        private readonly IEnumerable<DeploymentPayload> _deploymentPayloads;
        private readonly IUpdateCoordinator _updateCoordinator;
        private readonly ILogger _logger;

        private List<NonOverlappingTimer> _timers = new List<NonOverlappingTimer>();

        private bool _disposed = false;

        public UpdateMonitor(IEnumerable<DeploymentPayload> deploymentPayloads,
                IUpdateCoordinator updateCoordinator, ILogger logger)
        {
            _deploymentPayloads = deploymentPayloads?.Where(d => d.Deployment.Settings.UpdateBehavior.Expiration is not null)?.ToList(); 

            if (_deploymentPayloads?.Any() == false)
            {
                throw new InvalidOperationException(Common.GetFormatedErrorMessage($"Constructing type {nameof(Deployments.UpdateMonitor)}.",
                    $"The where no {nameof(deploymentPayloads)} add-ins found whose Deployment.Settings.UpdateBehavior.Expiration was non-null.",
                    "Supply one or more RemoteAddIns that match the aforementioned criteria."));
            }

            _updateCoordinator = updateCoordinator;
            _logger = logger;

            SetMonitoredAddIns();
        }

        private void SetMonitoredAddIns()
        {
            foreach (var payload in _deploymentPayloads)
            {
                try
                {
                    int interval = ToMilliseconds(payload.Deployment.Settings.UpdateBehavior.Expiration);

                    var timer = new NonOverlappingTimer(interval, () => EnqueueUpdateNotification(payload));
                    timer.Start(); 

                    _timers.Add(timer);

                    System.Diagnostics.Debug.WriteLine($"UpdateMonitor.SetMonitoredAddIns timer created for add-in titled {payload.AddIn.Identity.Title}");
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, $"Update monitoring setup failed for add-in titled {payload.AddIn.Identity.Title}");
                    throw;
                }
            }
        }

        private void EnqueueUpdateNotification(DeploymentPayload payload)
        {
            System.Diagnostics.Debug.WriteLine($"UpdateMonitor.EnqueueUpdateNotification invoked for add-in titled {payload.AddIn.Identity.Title}");

            try
            {
                NotifyAddInUpdate(payload);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Update monitoring 'EnqueueUpdateNotification' failed for add-in titled {payload.AddIn.Identity.Title}");
                throw;
            }
        }

        private void NotifyAddInUpdate(DeploymentPayload payload)
        {
            System.Diagnostics.Debug.WriteLine($"UpdateMonitor.NotifyAddInUpdate invoked for add-in titled {payload.AddIn.Identity.Title}");

            var deployedAddInManifestFilePath = payload.GetAddInManifestFilePath();

            var deployedAddInVersion = ManifestSerialization.DeserializeManifestFile<AddIn>(deployedAddInManifestFilePath).Identity.Version;

            var updateQueryInfoManifestFilePath = payload.GetUpdateQueryInfoManifestFilePath();

            UpdateQueryInfo existingUpdateQueryInfo = null;
            if (File.Exists(updateQueryInfoManifestFilePath))
            {
                existingUpdateQueryInfo = ManifestSerialization.DeserializeManifestFile<UpdateQueryInfo>(updateQueryInfoManifestFilePath);
            }

            // If the remote addin manifest file version was updated while XLAutoDeploy was
            // running, we need to get that version and compare against the deployed version
            var xLAutoDeployManifest = Common.GetLocalXLAutoDeployManifest(ExcelDnaUtil.XllPath);
            var remoteDeploymentPayload = DeploymentService.GetDeploymentPayloadByAddInTitle(xLAutoDeployManifest, payload.AddIn.Identity.Title);

            var checkedUpdate = DeploymentService.GetCheckedUpdate(remoteDeploymentPayload, deployedAddInVersion, DateTime.UtcNow, true);
            checkedUpdate.Info.FirstNotified = existingUpdateQueryInfo?.FirstNotified;

            var updateBehavior = payload.Deployment.Settings.UpdateBehavior;
            if ((updateBehavior.NotifyClient && checkedUpdate.Info.UpdateAvailable) || (checkedUpdate.Info.IsMandatoryUpdate))
            {
                string message;
                if (checkedUpdate.Info.IsMandatoryUpdate)
                {
                    message = $"Please re-start Excel (at your earliest convenience) to install a manadatory update.";  
                }
                else
                {
                    message = "Please re-start Excel (at your earliest convenience) to install the latest update."; 
                }

                string unitOfTime = Enum.GetName(typeof(UnitOfTime), updateBehavior.Expiration.UnitOfTime).ToLower();

                string frequency = updateBehavior.Expiration.MaximumAge > 1 ? $"{updateBehavior.Expiration.MaximumAge} {unitOfTime}" : unitOfTime.TrimEnd('s');

                message = $"{message} You will continue to be notified every {frequency} that the Excel application remains open until you do so.";

                _updateCoordinator.Notifier.Notify(message,
    checkedUpdate.Payload.Deployment.Description.Product, checkedUpdate.Payload.Deployment.Description.Publisher, checkedUpdate.Info.DeployedVersion, checkedUpdate.Info.AvailableVersion, false);

                var now = DateTime.UtcNow;
                checkedUpdate.Info.FirstNotified = checkedUpdate.Info?.FirstNotified ?? now;
                checkedUpdate.Info.LastNotified = now;
            }

            Serialization.SerializeToXmlFile(checkedUpdate.Info, updateQueryInfoManifestFilePath);
        }

        private int ToMilliseconds(UpdateExpiration updateExpiration)
        {
            const int millsecondsInMinute = 60000;
            const int minutesInADay = 1440;

            int maximumAge = (int)updateExpiration.MaximumAge; 

            switch (updateExpiration.UnitOfTime)
            {
                case UnitOfTime.Minutes:
                    return maximumAge * millsecondsInMinute;

                case UnitOfTime.Days:
                    return (maximumAge * minutesInADay) * millsecondsInMinute;

                case UnitOfTime.Weeks:
                    return ((maximumAge * 7) * minutesInADay) * millsecondsInMinute;

                case UnitOfTime.Months:
                    return ((maximumAge * 30 * 7) * minutesInADay) * millsecondsInMinute;
            }

            return -1; 
        }

        ~UpdateMonitor()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (this._disposed)
                return;

            if (disposing)
            {
                GC.SuppressFinalize(this);
            }

            if (_timers?.Any() == true)
            {
                foreach (var timer in _timers)
                {
                    timer.Stop();
                    timer.Dispose(); 
                }
            }

            this._disposed = true;
        }
    }
}
