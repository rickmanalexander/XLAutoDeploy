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

        public void Install(string addinTitle, string filePath)
        {
            try
            {
                InteropIntegration.InstallAddIn(addinTitle, filePath);

                _logger.Info($"Add-in titled {addinTitle} was installed at file path: {filePath}");
            }
            catch(Exception ex)
            {
                _logger.Error(ex, $"File Install Failed for file: {filePath}");
                throw; 
            }
        }

        public void TryInstall(string addinTitle, string filePath, out bool success)
        {
            try
            {
                InteropIntegration.InstallAddIn(addinTitle, filePath);
                success = true;

                _logger.Info($"Add-in titled {addinTitle} was un-installed at file path: {filePath}");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"File Install Failed for file: {filePath}");
                success = false;
            }
        }

        public void Uninstall(string filePath)
        {
            try
            {
                InteropIntegration.UninstallAddIn(filePath);

                _logger.Info($"Add-in was installed at file path: {filePath}");
            }
            catch(Exception ex)
            {
                _logger.Error(ex, $"File Un-install Failed for file: {filePath}");
                throw;
            }
        }

        public void TryUninstall(string addinTitle, string filePath, out bool success)
        {
            try
            {
                InteropIntegration.UninstallAddIn(addinTitle);
                success = true;

                _logger.Info($"Add-in was un-installed at file path: {filePath}");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"File Un-install Failed for file: {filePath}");
                success = false;
            }
        }
    }
}
