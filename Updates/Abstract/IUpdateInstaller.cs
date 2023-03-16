using XLAutoDeploy.Logging;

namespace XLAutoDeploy.Updates
{
    internal interface IUpdateInstaller
    {
        ILogger Logger { get; }

        void Install(string addInTitle, string filePath);
        void TryInstall(string addInTitle, string filePath, out bool success);

        void Uninstall(string addInTitle, string filePath);
        void TryUninstall(string addInTitle, string filePath, out bool success);
    }
}
