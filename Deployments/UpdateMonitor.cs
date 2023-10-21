using XLAutoDeploy.Updates;
using XLAutoDeploy.Logging;

using XLAutoDeploy.Manifests;
using XLAutoDeploy.Manifests.Utilities;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

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
            _deploymentPayloads = deploymentPayloads?.Where(d => d.Deployment.Settings.UpdateBehavior.Expiration != null)?.ToList();

            if (_deploymentPayloads?.Any() == false)
            {
                throw new InvalidOperationException(Common.GetFormatedErrorMessage($"Constructing type {nameof(UpdateMonitor)}.",
                    $"The where no {nameof(deploymentPayloads)} add-ins found whose FileHostConfiguration {nameof(FileHostType)} == {nameof(FileHostType.FileServer)} with UpdateBehavior.DoInRealTime == true.",
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

            var updateBehavior = payload.Deployment.Settings.UpdateBehavior;
            if ((updateBehavior.NotifyClient && checkedUpdate.Info.UpdateAvailable) || (checkedUpdate.Info.IsMandatoryUpdate))
            {
                _updateCoordinator.Notifier.Notify("To update please close Excel and re-open.",
    checkedUpdate.Payload.Deployment.Description, checkedUpdate.Info, false);

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
