using System;
using System.IO;

namespace XLAutoDeploy.FileSystem.Monitoring
{
    internal sealed class FileSystemWatcherEventAggregator : IFileSystemWatcherEventAggregator
    {
        private readonly IFileSystemWatcherEventAggregator _parent;

        public event EventHandler<FileSystemEventArgs> Changed;
        public event EventHandler<FileSystemEventArgs> Created;
        public event EventHandler<FileSystemEventArgs> Deleted;
        public event EventHandler<RenamedEventArgs> Renamed;
        public event EventHandler<ErrorEventArgs> Error;

        public FileSystemWatcherEventAggregator()
        {
        }

        public FileSystemWatcherEventAggregator(IFileSystemWatcherEventAggregator eventAggregator)
        {
            _parent = eventAggregator ?? throw new ArgumentNullException(Common.GetFormatedErrorMessage($"Constructing type {nameof(FileSystemWatcherEventAggregator)}",
                    $"The {nameof(eventAggregator)} parameter is null.",
                    $"Supply a valid {nameof(eventAggregator)}."));
        }

        public void RaiseWatcherChanged(FileSystemWatcher watcher, WatcherChangeTypes changeType, string fullPath, string name, DateTime timeStamp)
        {
            var handler = Changed;
            if (handler != null)
            {
                var @event = new FileSystemEventArgs(changeType, fullPath, name);
                handler(watcher, @event);
            }
            _parent?.RaiseWatcherChanged(watcher, changeType, fullPath, name, timeStamp);
        }

        public void RaiseWatcherCreated(FileSystemWatcher watcher, WatcherChangeTypes changeType, string fullPath, string name, DateTime timeStamp)
        {
            var handler = Created;
            if (handler != null)
            {
                var @event = new FileSystemEventArgs(changeType, fullPath, name);
                handler(watcher, @event);
            }
            _parent?.RaiseWatcherCreated(watcher, changeType, fullPath, name, timeStamp);
        }

        public void RaiseWatcherDeleted(FileSystemWatcher watcher, WatcherChangeTypes changeType, string fullPath, string name, DateTime timeStamp)
        {
            var handler = Deleted;
            if (handler != null)
            {
                var @event = new FileSystemEventArgs(changeType, fullPath, name);
                handler(watcher, @event);
            }
            _parent?.RaiseWatcherDeleted(watcher, changeType, fullPath, name, timeStamp);
        }

        public void RaiseWatcherRenamed(FileSystemWatcher watcher, WatcherChangeTypes changeType, string directory, string name, string oldName, DateTime timeStamp)
        {
            var handler = Renamed;
            if (handler != null)
            {
                var @event = new RenamedEventArgs(changeType, directory, name, oldName);
                handler(watcher, @event);
            }
            _parent?.RaiseWatcherRenamed(watcher, changeType, directory, name, oldName, timeStamp);
        }

        public void RaiseWatcherError(FileSystemWatcher watcher, Exception exception)
        {
            var handler = Error;
            if (handler != null)
            {
                var @event = new ErrorEventArgs(exception);
                handler(watcher, @event);
            }
            _parent?.RaiseWatcherError(watcher, exception);
        }
    }
}
