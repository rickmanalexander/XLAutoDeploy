using System;

namespace XLAutoDeploy.Logging
{
    public sealed class NLoggerProxyFactory : INLoggerProxyFactory
    {
        public ILogger Create(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(Common.GetFormatedErrorMessage($"Constructing type {nameof(NLoggerProxyFactory)}",
                    $"The {nameof(type)} parameter is null.",
                    $"Supply a valid {nameof(type)}."));
            }

            var loggerProxy = typeof(NLoggerProxy<>).MakeGenericType(type); 

            return (ILogger)Activator.CreateInstance(loggerProxy);
        }
    }
}
