using XLAutoDeploy.Logging;

using System;

namespace XLAutoDeploy.Updates
{
    internal sealed class UpdateInstaller : IUpdateInstaller
    {
        public ILogger Logger => _logger;

        private readonly ILogger _logger;

        public UpdateInstaller(ILogger logger)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(Common.GetFormatedErrorMessage($"Constructing type {nameof(UpdateInstaller)}",
                    $"The {nameof(logger)} parameter is null.",
                    $"Supply a valid {nameof(logger)}."));
            }

            _logger = logger;
        }

        public void Install(string addInTitle, string filePath)
        {
            try
            {
                if (!InteropIntegration.IsAddInInstalled(addInTitle))
                {
                    InteropIntegration.InstallAddIn(addInTitle, filePath);

                    _logger.Info($"Add-in titled {addInTitle} was installed at file path: {filePath}");
                }
            }
            catch(Exception ex)
            {
                _logger.Error(ex, $"Install failed for the add-in titled {addInTitle} from the following file path: {filePath}");
                throw; 
            }
        }

        public void TryInstall(string addInTitle, string filePath, out bool success)
        {
            try
            {
                if (!InteropIntegration.IsAddInInstalled(addInTitle))
                {
                    InteropIntegration.InstallAddIn(addInTitle, filePath);

                    _logger.Info($"Add-in titled {addInTitle} was installed at file path: {filePath}");
                }
                success = true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Install failed for the add-in titled {addInTitle} from the following file path: {filePath}");
                success = false;
            }
        }

        public void Uninstall(string addInTitle, string filePath)
        {
            try
            {
                if (InteropIntegration.IsAddInInstalled(addInTitle))
                {
                    InteropIntegration.UninstallAddIn(addInTitle);

                    _logger.Info($"Add-in titled {addInTitle} was un-installed at file path: {filePath}");
                }
            }
            catch(Exception ex)
            {
                _logger.Error(ex, $"Un-install failed for the add-in titled {addInTitle} from the following file path: {filePath}");
                throw;
            }
        }

        public void TryUninstall(string addInTitle, string filePath, out bool success)
        {
            try
            {
                if (InteropIntegration.IsAddInInstalled(addInTitle))
                {
                    InteropIntegration.UninstallAddIn(addInTitle);

                    _logger.Info($"Add-in titled {addInTitle} was un-installed at file path: {filePath}");
                }
                success = true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Un-install failed for the add-in titled {addInTitle} from the following file path: {filePath}");
                success = false;
            }
        }
    }
}
