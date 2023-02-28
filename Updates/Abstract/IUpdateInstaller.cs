using XLAutoDeploy.Logging;

namespace XLAutoDeploy.Updates
{
    internal interface IUpdateInstaller
    {
        ILogger Logger { get; }

        void Install(string addinTitle, string filePath);
        void TryInstall(string addinTitle, string filePath, out bool success);

        void Uninstall(string filePath);
        void TryUninstall(string addinTitle, string filePath, out bool success);
    }
}
