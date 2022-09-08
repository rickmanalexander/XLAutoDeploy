using System;

using XLAutoDeploy.Logging;

namespace XLAutoDeploy.Updates
{
    public sealed class UpdateInstaller : IUpdateInstaller
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
            }
            catch(Exception ex)
            {
                _logger.Error(ex, $"File Install Failed for file: {filePath}");
                throw; 
            }
        }

        public bool TryInstall(string addinTitle, string filePath)
        {
            try
            {
                InteropIntegration.InstallAddIn(addinTitle, filePath);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"File Install Failed for file: {filePath}");
                return false;
            }
        }

        public void Uninstall(string filePath)
        {
            try
            {
                InteropIntegration.UninstallAddIn(filePath);
            }
            catch(Exception ex)
            {
                _logger.Error(ex, $"File Un-install Failed for file: {filePath}");
                throw;
            }
        }

        public bool TryUninstall(string addinTitle, string filePath)
        {
            try
            {
                InteropIntegration.UninstallAddIn(addinTitle);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"File Un-install Failed for file: {filePath}");
                return false;
            }
        }
    }
}
