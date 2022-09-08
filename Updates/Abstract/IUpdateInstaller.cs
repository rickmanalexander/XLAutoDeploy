using XLAutoDeploy.Logging;

namespace XLAutoDeploy.Updates
{
    public interface IUpdateInstaller
    {
        ILogger Logger { get; }

        void Install(string addinTitle, string filePath);
        bool TryInstall(string addinTitle, string filePath);

        void Uninstall(string filePath);
        bool TryUninstall(string addinTitle, string filePath);
    }
}
