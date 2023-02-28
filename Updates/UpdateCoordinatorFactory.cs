using XLAutoDeploy.Logging;

using System;

namespace XLAutoDeploy.Updates
{
    internal class UpdateCoordinatorFactory : IUpdateCoordinatorFactory
    {
        public IUpdateCoordinator Create(INLoggerProxyFactory loggerProxyFactory)
        {
            if (loggerProxyFactory == null)
            {
                throw new ArgumentNullException(Common.GetFormatedErrorMessage($"Constructing type {nameof(UpdateCoordinatorFactory)}",
                    $"The {nameof(loggerProxyFactory)} parameter is null.",
                    $"Supply a valid {nameof(loggerProxyFactory)}."));
            }

            var notifier = new UpdateNotifier();
            var deployer = new UpdateDownloader(loggerProxyFactory.Create(typeof(UpdateDownloader)));
            var loader = new UpdateLoader(loggerProxyFactory.Create(typeof(UpdateLoader)));
            var installer = new UpdateInstaller(loggerProxyFactory.Create(typeof(UpdateInstaller)));

            return new UpdateCoordinator(notifier, deployer, loader, installer);
        }
    }
}
