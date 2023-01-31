using System;

using XLAutoDeploy.Logging;

namespace XLAutoDeploy.Updates
{
    public sealed class UpdateLoader : IUpdateLoader
    {
        public ILogger Logger => _logger;

        private readonly ILogger _logger;

        public UpdateLoader(ILogger logger)
        {

            if (logger == null)
            {
                throw new ArgumentNullException(Common.GetFormatedErrorMessage($"Constructing type {nameof(UpdateLoader)}",
                    $"The {nameof(logger)} parameter is null.",
                    $"Supply a valid {nameof(logger)}."));
            }

            _logger = logger;
        }

        public void Load(string filePath)
        {
            try
            {
                InteropIntegration.LoadAddIn(filePath);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"File Load Failed for file: {filePath}");
                throw;
            }
        }

        public void TryLoad(string filePath, out bool success)
        {
            try
            {
                InteropIntegration.LoadAddIn(filePath);
                success = true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"File Load Failed for file: {filePath}");
                success = false;
            }
        }

        public void Unload(string filePath)
        {
            try
            {
                InteropIntegration.UnloadAddIn(filePath);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"File Un-load Failed for file: {filePath}");
                throw;
            }
        }

        public void TryUnload(string filePath, out bool success)
        {
            try
            {
                InteropIntegration.UnloadAddIn(filePath);
                success = true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"File Un-load Failed for file: {filePath}");
                success = false;
            }
        }
    }
}
