using System.IO;

namespace XLAutoDeploy.FileSystem.Monitoring
{
    internal interface IFileSystemWatcherFactory
    {
        FileSystemWatcher Create(string directory, NotifyFilters notifyFilters, bool monitorSubdirectories = false);
        FileSystemWatcher Create(string directory, string fileFilter, NotifyFilters notifyFilters, bool monitorSubdirectories = false);
        FileSystemWatcher Create(string filePath, NotifyFilters notifyFilters);
    }
}
