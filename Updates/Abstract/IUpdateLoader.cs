using XLAutoDeploy.Logging;

namespace XLAutoDeploy.Updates
{
    public interface IUpdateLoader
    {
        ILogger Logger { get; }

        void Load(string filePath);
        void TryLoad(string filePath, out bool success);

        void Unload(string filePath);
        void TryUnload(string filePath, out bool success);
    }
}
