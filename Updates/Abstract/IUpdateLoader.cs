using XLAutoDeploy.Logging;

namespace XLAutoDeploy.Updates
{
    public interface IUpdateLoader
    {
        ILogger Logger { get; }

        void Load(string filePath);
        bool TryLoad(string filePath);

        void Unload(string filePath);
        bool TryUnload(string filePath);
    }
}
