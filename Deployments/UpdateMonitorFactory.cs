using System.Collections.Generic;

using XLAutoDeploy.FileSystem.Access;
using XLAutoDeploy.FileSystem.Monitoring;
using XLAutoDeploy.Logging;
using XLAutoDeploy.Updates;

namespace XLAutoDeploy.Deployments
{
    /// <summary>
    /// Method(s) to creates instances of a <see cref="UpdateMonitor"/>.
    /// </summary>
    internal sealed class UpdateMonitorFactory
    {
        public static UpdateMonitor Create(IEnumerable<DeploymentPayload> deploymentPayloads,
                IUpdateCoordinator updateService, IRemoteFileDownloader remoteFileDowloader,
                uint sessionNotificationLimit = 1)
        {
            var watcherFactory = new FileSystemWatcherFactory();
            var watcherEventAggregator = new FileSystemWatcherEventAggregator();
            var monitorFactory = new FileSystemMonitorFactory();
            var logger = new NLoggerProxy<UpdateMonitor>();

            return new UpdateMonitor(watcherFactory, watcherEventAggregator, monitorFactory, deploymentPayloads, updateService, remoteFileDowloader, logger, sessionNotificationLimit);
        }
    }
}
