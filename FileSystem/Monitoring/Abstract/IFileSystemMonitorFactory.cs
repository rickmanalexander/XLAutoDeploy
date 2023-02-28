using System.IO;

namespace XLAutoDeploy.FileSystem.Monitoring
{
    internal interface IFileSystemMonitorFactory
    {
        IFileSystemMonitor Create(FileSystemWatcher watcher, IFileSystemWatcherEventAggregator eventAggregator);
    }
}
