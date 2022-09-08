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

        public bool TryLoad(string filePath)
        {
            try
            {
                InteropIntegration.LoadAddIn(filePath);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"File Load Failed for file: {filePath}");
                return false;
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

        public bool TryUnload(string filePath)
        {
            try
            {
                InteropIntegration.UnloadAddIn(filePath);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"File Un-load Failed for file: {filePath}");
                return false;
            }
        }
    }
}
