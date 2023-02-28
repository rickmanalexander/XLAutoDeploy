using System.IO;

namespace XLAutoDeploy.FileSystem.Monitoring
{
    internal sealed class FileSystemMonitorFactory: IFileSystemMonitorFactory
    {
        public IFileSystemMonitor Create(FileSystemWatcher watcher, IFileSystemWatcherEventAggregator eventAggregator)
        {
            return new FileSystemMonitor(watcher, eventAggregator);
        }
    }
}
