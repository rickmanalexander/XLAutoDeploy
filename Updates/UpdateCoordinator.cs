using System;

namespace XLAutoDeploy.Updates
{
    internal sealed class UpdateCoordinator : IUpdateCoordinator
    {
        public IUpdateNotifier Notifier { get; }
        public IUpdateDownloader Deployer { get; }
        public IUpdateLoader Loader { get; }
        public IUpdateInstaller Installer { get; }

        public UpdateCoordinator(IUpdateNotifier notifier, IUpdateDownloader deployer, IUpdateLoader loader, IUpdateInstaller installer)
        {
            Notifier = notifier ?? throw new ArgumentNullException(Common.GetFormatedErrorMessage($"Constructing type {nameof(UpdateCoordinator)}",
                    $"The {nameof(notifier)} parameter is null.",
                    $"Supply a valid {nameof(notifier)}."));
            Deployer = deployer ?? throw new ArgumentNullException(Common.GetFormatedErrorMessage($"Constructing type {nameof(UpdateCoordinator)}",
                    $"The {nameof(deployer)} parameter is null.",
                    $"Supply a valid {nameof(deployer)}.")); ;
            Loader = loader ?? throw new ArgumentNullException(Common.GetFormatedErrorMessage($"Constructing type {nameof(UpdateCoordinator)}",
                    $"The {nameof(loader)} parameter is null.",
                    $"Supply a valid {nameof(loader)}."));

            Installer = installer ?? throw new ArgumentNullException(Common.GetFormatedErrorMessage($"Constructing type {nameof(UpdateCoordinator)}",
                    $"The {nameof(installer)} parameter is null.",
                    $"Supply a valid {nameof(installer)}.")); ;
        }
    }
}
