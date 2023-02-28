using XLAutoDeploy.Manifests;

namespace XLAutoDeploy.Updates
{
    internal interface IUpdateNotifier
    {
        bool DoUpdate { get; }

        void Notify(string message, Description deploymentDescription, UpdateQueryInfo updateQueryInfo, bool allowSkip);
    }
}
