using System.IO;

namespace XLAutoDeploy.FileSystem.Monitoring
{
    public sealed class FileSystemMonitorFactory: IFileSystemMonitorFactory
    {
        public IFileSystemMonitor Create(FileSystemWatcher watcher, IFileSystemWatcherEventAggregator eventAggregator)
        {
            return new FileSystemMonitor(watcher, eventAggregator);
        }
    }
}
