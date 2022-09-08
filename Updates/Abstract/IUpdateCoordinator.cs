namespace XLAutoDeploy.Updates
{
    public interface IUpdateCoordinator
    {
        IUpdateNotifier Notifier { get; }
        IUpdateDownloader Deployer { get; }
        IUpdateLoader Loader { get; }
        IUpdateInstaller Installer { get; }
    }
}
