using System.IO;

namespace XLAutoDeploy.FileSystem.Monitoring
{
    internal sealed class FileSystemWatcherFactory : IFileSystemWatcherFactory
    {
        public FileSystemWatcher Create(string directory, NotifyFilters notifyFilters, bool monitorSubdirectories = false)
        {
            return new FileSystemWatcher(directory)
            {
                NotifyFilter = notifyFilters,
                IncludeSubdirectories = monitorSubdirectories,
                EnableRaisingEvents = true
            };
        }

        public FileSystemWatcher Create(string directory, string fileFilter, NotifyFilters notifyFilters, bool monitorSubdirectories = false)
        {
            var result = Create(directory, notifyFilters, monitorSubdirectories);
            result.Filter = fileFilter;

            return result;
        }

        public FileSystemWatcher Create(string filePath, NotifyFilters notifyFilters)
        {
            var directory = Path.GetDirectoryName(filePath);
            var result = Create(directory, notifyFilters);
            result.Filter = Path.GetFileName(filePath);

            return result;
        }
    }
}
