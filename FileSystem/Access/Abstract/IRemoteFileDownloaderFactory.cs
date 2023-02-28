namespace XLAutoDeploy.FileSystem.Access
{
    internal interface IRemoteFileDownloaderFactory
    {
        IRemoteFileDownloader Create();
    }
}
