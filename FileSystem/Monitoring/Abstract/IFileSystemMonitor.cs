using System;

namespace XLAutoDeploy.FileSystem.Monitoring
{
    internal interface IFileSystemMonitor : IDisposable
    {
        IFileSystemWatcherEventProvider Events { get; }

        void MonitorFile(string filePath);
        void ForgetFile(string filePath);
    }
}
