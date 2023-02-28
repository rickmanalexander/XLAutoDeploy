using XLAutoDeploy.Logging;

namespace XLAutoDeploy.FileSystem.Access
{
    internal sealed class RemoteFileDownloaderFactory : IRemoteFileDownloaderFactory
    {
        public IRemoteFileDownloader Create()
        {
            return new RemoteFileDownloader(new NLoggerProxy<RemoteFileDownloader>()); 
        }
    }
}
