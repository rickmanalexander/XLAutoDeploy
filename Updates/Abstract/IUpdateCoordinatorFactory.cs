using XLAutoDeploy.Logging;

namespace XLAutoDeploy.Updates
{
    internal interface IUpdateCoordinatorFactory
    {
        IUpdateCoordinator Create(INLoggerProxyFactory loggerProxyFactory);
    }
}
