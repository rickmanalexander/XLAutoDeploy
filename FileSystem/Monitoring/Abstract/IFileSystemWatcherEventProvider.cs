using System;
using System.IO;

namespace XLAutoDeploy.FileSystem.Monitoring
{
    public interface IFileSystemWatcherEventProvider
    {
        event EventHandler<FileSystemEventArgs> Changed;
        event EventHandler<FileSystemEventArgs> Created;
        event EventHandler<FileSystemEventArgs> Deleted;
        event EventHandler<RenamedEventArgs> Renamed;
        event EventHandler<ErrorEventArgs> Error;
    }
}
