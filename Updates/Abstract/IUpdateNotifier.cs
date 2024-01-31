using System;

namespace XLAutoDeploy.Updates
{
    internal interface IUpdateNotifier
    {
        bool DoUpdate { get; }

        void Notify(string message, string product,
            string publisher, Version deployedVersion, Version availableVersion, bool allowSkip);
    }
}
