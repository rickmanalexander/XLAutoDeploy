using XLAutoDeploy.Logging;
using XLAutoDeploy.Updates;

using System.Collections.Generic;

namespace XLAutoDeploy.Deployments
{
    internal sealed class UpdateMonitorFactory
    {
        public static UpdateMonitor Create(IEnumerable<DeploymentPayload> deploymentPayloads,
        IUpdateCoordinator updateCoordinator)
        {
            var logger = new NLoggerProxy<UpdateMonitor>();

            return new UpdateMonitor(deploymentPayloads, updateCoordinator, logger);
        }
    }
}
