using XLAutoDeploy.Logging;

namespace XLAutoDeploy.FileSystem.Access
{
    public sealed class RemoteFileDownloaderFactory : IRemoteFileDownloaderFactory
    {
        public IRemoteFileDownloader Create()
        {
            return new RemoteFileDownloader(new NLoggerProxy<RemoteFileDownloader>()); 
        }
    }
}
