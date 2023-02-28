using System;

namespace XLAutoDeploy.Logging
{
    internal interface INLoggerProxyFactory
    {
        ILogger Create(Type type); 
    }
}
