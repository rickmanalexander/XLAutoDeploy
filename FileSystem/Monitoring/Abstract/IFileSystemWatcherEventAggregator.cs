using System;
using System.IO;

namespace XLAutoDeploy.FileSystem.Monitoring
{
    internal interface IFileSystemWatcherEventAggregator : IFileSystemWatcherEventProvider
    {
        void RaiseWatcherChanged(FileSystemWatcher watcher, WatcherChangeTypes changeType, string fullPath, string name, DateTime timeStamp);
        void RaiseWatcherCreated(FileSystemWatcher watcher, WatcherChangeTypes changeType, string fullPath, string name, DateTime timeStamp);
        void RaiseWatcherDeleted(FileSystemWatcher watcher, WatcherChangeTypes changeType, string fullPath, string name, DateTime timeStamp);
        void RaiseWatcherRenamed(FileSystemWatcher watcher, WatcherChangeTypes changeType, string directory, string name, string oldName, DateTime timeStamp);
        void RaiseWatcherError(FileSystemWatcher watcher, Exception exception);
    }
}
