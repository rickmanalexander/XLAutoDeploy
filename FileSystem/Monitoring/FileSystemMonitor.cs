using System;
using System.Collections.Generic;
using System.IO;

namespace XLAutoDeploy.FileSystem.Monitoring
{
    public sealed class FileSystemMonitor : IFileSystemMonitor
    {
        public IFileSystemWatcherEventProvider Events => _eventAggregator;

        private FileSystemWatcher _watcher;
        private readonly IFileSystemWatcherEventAggregator _eventAggregator;

        private IDictionary<string, string> _monitoredFiles = 
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private bool _disposed;

        public FileSystemMonitor(FileSystemWatcher watcher, IFileSystemWatcherEventAggregator eventAggregator)
        {
            if (watcher == null)
            {
                throw new ArgumentNullException(Common.GetFormatedErrorMessage($"Constructing type {nameof(FileSystemMonitor)}",
                    $"The {nameof(watcher)} parameter is null.",
                    $"Supply a valid {nameof(watcher)}."));
            }

            if (eventAggregator == null)
            {
                throw new ArgumentNullException(Common.GetFormatedErrorMessage($"Constructing type {nameof(FileSystemMonitor)}",
                    $"The {nameof(eventAggregator)} parameter is null.",
                    $"Supply a valid {nameof(eventAggregator)}."));
            }

            _watcher = watcher;
            _eventAggregator = eventAggregator;

            _watcher.Changed += _watcher_Changed;
            _watcher.Created += _watcher_Created;
            _watcher.Deleted += _watcher_Deleted;
            _watcher.Renamed += _watcher_Renamed;
            _watcher.Error += _watcher_Error;
        }

        private void _watcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (_monitoredFiles.ContainsKey(e.FullPath))
            {
                _eventAggregator.RaiseWatcherChanged(_watcher, e.ChangeType, e.FullPath, e.Name, DateTime.Now);
            }
        }

        private void _watcher_Created(object sender, FileSystemEventArgs e)
        {
            if (_monitoredFiles.ContainsKey(e.FullPath))
            {
                _eventAggregator.RaiseWatcherCreated(_watcher, e.ChangeType, e.FullPath, e.Name, DateTime.Now);
            }
        }

        private void _watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            if (_monitoredFiles.ContainsKey(e.FullPath))
            {
                _eventAggregator.RaiseWatcherDeleted(_watcher, e.ChangeType, e.FullPath, e.Name, DateTime.Now);
            }
        }

        private void _watcher_Renamed(object sender, RenamedEventArgs e)
        {
            if (_monitoredFiles.ContainsKey(e.FullPath))
            {
                _eventAggregator.RaiseWatcherRenamed(_watcher, e.ChangeType, e.FullPath, e.Name, e.OldName, DateTime.Now);
            }
        }

        private void _watcher_Error(object sender, ErrorEventArgs e)
        {
            _eventAggregator.RaiseWatcherError(_watcher, e.GetException());
        }

        public void MonitorFile(string filePath)
        {
            var localFilePath = GetValidFileFullPath(filePath);

            ThrowNotInDirectoryException(localFilePath);

            _monitoredFiles[localFilePath] = System.IO.Path.GetFileName(localFilePath);
        }

        public void ForgetFile(string filePath)
        {
            var localFilePath = GetValidFileFullPath(filePath);

            if (_monitoredFiles.ContainsKey(localFilePath))
            {
                _monitoredFiles.Remove(localFilePath);
            }
        }

        private string GetValidFileFullPath(string filePath)
        {
            return Path.GetFullPath(filePath);
        }

        private void ThrowNotInDirectoryException(string filePath)
        {
            if (!_watcher.Path.Equals(Path.GetDirectoryName(filePath), StringComparison.OrdinalIgnoreCase))
                throw new Exception($"The following file {filePath} is not located in the monitored directory {_watcher.Path}");
        }

        ~FileSystemMonitor()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Dispose(bool disposing)
        {
            if (this._disposed)
                return;

            if (disposing)
            {
                // Clean up managed resources
                if (_watcher != null)
                {
                    _watcher.EnableRaisingEvents = false;
                    _watcher.Dispose();

                    _watcher = null;
                    _monitoredFiles = null;
                }

                this._disposed = true;
            }
        }

    }
}
