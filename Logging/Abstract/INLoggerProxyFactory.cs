using System;

namespace XLAutoDeploy.Logging
{
    public interface INLoggerProxyFactory
    {
        ILogger Create(Type type); 
    }
}
