using System.IO;

namespace XLAutoDeploy.FileSystem.Monitoring
{
    public interface IFileSystemMonitorFactory
    {
        IFileSystemMonitor Create(FileSystemWatcher watcher, IFileSystemWatcherEventAggregator eventAggregator);
    }
}
