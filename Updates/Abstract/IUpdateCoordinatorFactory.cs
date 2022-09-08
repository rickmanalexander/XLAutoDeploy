using XLAutoDeploy.Logging;

namespace XLAutoDeploy.Updates
{
    public interface IUpdateCoordinatorFactory
    {
        IUpdateCoordinator Create(INLoggerProxyFactory loggerProxyFactory);
    }
}
