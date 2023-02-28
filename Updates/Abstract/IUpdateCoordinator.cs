namespace XLAutoDeploy.Updates
{
    internal interface IUpdateCoordinator
    {
        IUpdateNotifier Notifier { get; }
        IUpdateDownloader Deployer { get; }
        IUpdateLoader Loader { get; }
        IUpdateInstaller Installer { get; }
    }
}
