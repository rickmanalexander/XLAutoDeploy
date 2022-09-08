namespace XLAutoDeploy.FileSystem.Access
{
    public interface IRemoteFileDownloaderFactory
    {
        IRemoteFileDownloader Create();
    }
}
