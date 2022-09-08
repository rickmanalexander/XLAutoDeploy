using System;

namespace XLAutoDeploy.Updates
{
    public sealed class UpdateCoordinator : IUpdateCoordinator
    {
        public IUpdateNotifier Notifier { get; }
        public IUpdateDownloader Deployer { get; }
        public IUpdateLoader Loader { get; }
        public IUpdateInstaller Installer { get; }

        public UpdateCoordinator(IUpdateNotifier notifier, IUpdateDownloader deployer, IUpdateLoader loader, IUpdateInstaller installer)
        {
            if (notifier == null)
            {
                throw new ArgumentNullException(Common.GetFormatedErrorMessage($"Constructing type {nameof(UpdateCoordinator)}",
                    $"The {nameof(notifier)} parameter is null.",
                    $"Supply a valid {nameof(notifier)}."));
            }

            if (deployer == null)
            {
                throw new ArgumentNullException(Common.GetFormatedErrorMessage($"Constructing type {nameof(UpdateCoordinator)}",
                    $"The {nameof(deployer)} parameter is null.",
                    $"Supply a valid {nameof(deployer)}."));
            }

            if (loader == null)
            {
                throw new ArgumentNullException(Common.GetFormatedErrorMessage($"Constructing type {nameof(UpdateCoordinator)}",
                    $"The {nameof(loader)} parameter is null.",
                    $"Supply a valid {nameof(loader)}."));
            }

            if (installer == null)
            {
                throw new ArgumentNullException(Common.GetFormatedErrorMessage($"Constructing type {nameof(UpdateCoordinator)}",
                    $"The {nameof(installer)} parameter is null.",
                    $"Supply a valid {nameof(installer)}."));
            }

            Notifier = notifier;
            Deployer = deployer;
            Loader = loader;
            Installer = installer;
        }
    }
}
