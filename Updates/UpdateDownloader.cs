using XLAutoDeploy.Logging;
using XLAutoDeploy.FileSystem.Access;

using System;
using System.Net;
using System.Threading.Tasks;

namespace XLAutoDeploy.Updates
{
    internal sealed class UpdateDownloader : IUpdateDownloader
    {
        public ILogger Logger => _logger;

        private readonly ILogger _logger;
        private readonly object _threadLock = new object();

        public UpdateDownloader(ILogger logger)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(Common.GetFormatedErrorMessage($"Constructing type {nameof(UpdateDownloader)}",
                    $"The {nameof(logger)} parameter is null.",
                    $"Supply a valid {nameof(logger)}."));
            }

            _logger = logger;
        }

        // what if targetFilePath != local prod file path becuase it was changed remotely?
        public void Download(IRemoteFileDownloader fileDownloader, string remoteFilePath, string targetFilePath, bool overwrite = false)
        {
            lock (_threadLock)
            {
                FileSystem.FileUtilities.CreateDirectory(targetFilePath, _logger);

                try
                {
                    fileDownloader.Download(remoteFilePath, targetFilePath, true);
                }
                catch(Exception ex)
                {
                    _logger.Error(ex, $"File download Failed for remote file: {remoteFilePath} to target file: {targetFilePath}");
                    throw;
                }
            }
        }

        public async Task DownloadAsync(IRemoteFileDownloader fileDownloader, string remoteFilePath, string targetFilePath, bool overwrite = false)
        {
            FileSystem.FileUtilities.CreateDirectory(targetFilePath, _logger);

            try
            {
                await fileDownloader.DownloadAsync(remoteFilePath, targetFilePath, true);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"File download Failed for remote file: {remoteFilePath} to target file: {targetFilePath}");
                throw;
            }
        }

        public void TryDownload(IRemoteFileDownloader fileDownloader, string remoteFilePath, string targetFilePath, out bool success, bool overwrite = false)
        {
            try
            {
                Download(fileDownloader, remoteFilePath, targetFilePath);
                success = true;
            }
            catch
            {
                success = false;
            }
        }

        public void Download(IRemoteFileDownloader fileDownloader, WebClient webClient, string address, string targetFilePath, bool overwrite = false)
        {
            lock (_threadLock)
            {
                FileSystem.FileUtilities.CreateDirectory(targetFilePath, _logger);

                try
                {
                    fileDownloader.Download(webClient, address, targetFilePath, true);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, $"File download Failed for remote web file: {address} to target file: {targetFilePath}");
                    throw;
                }
            }
        }

        public async Task DownloadAsync(IRemoteFileDownloader fileDownloader, WebClient webClient, string address, string targetFilePath, bool overwrite = false)
        {
            FileSystem.FileUtilities.CreateDirectory(targetFilePath, _logger);

            try
            {
                await fileDownloader.DownloadAsync(webClient, address, targetFilePath, true);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"File download Failed for remote web file: {address} to target file: {targetFilePath}");
                throw;
            }
        }

        public void TryDownload(IRemoteFileDownloader fileDownloader, WebClient webClient, string address, string targetFilePath, out bool success, bool overwrite = false)
        {
            try
            {
                Download(fileDownloader, webClient, address, targetFilePath);
                success = true;
            }
            catch
            {
                success = false;
            }
        }

        public void Download(IRemoteFileDownloader fileDownloader, WebClient webClient, Uri uri, string targetFilePath, bool overwrite = false)
        {
            lock (_threadLock)
            {
                FileSystem.FileUtilities.CreateDirectory(targetFilePath, _logger);

                try
                {
                    // what if targetFilePath != local prod file path becuase it was changed remotely?
                    fileDownloader.Download(webClient, uri, targetFilePath, true);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, $"File download Failed for remote web file: {uri.AbsoluteUri} to target file: {targetFilePath}");
                    throw;
                }
            }
        }

        public async Task DownloadAsync(IRemoteFileDownloader fileDownloader, WebClient webClient, Uri uri, string targetFilePath, bool overwrite = false)
        {
            FileSystem.FileUtilities.CreateDirectory(targetFilePath, _logger);

            try
            {
                // what if targetFilePath != local prod file path becuase it was changed remotely?
                await fileDownloader.DownloadAsync(webClient, uri, targetFilePath, true);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"File download Failed for remote web file: {uri.AbsoluteUri} to target file: {targetFilePath}");
                throw;
            }
        }

        public void TryDownload(IRemoteFileDownloader fileDownloader, WebClient webClient, Uri uri, string targetFilePath, out bool success, bool overwrite = false)
        {
            try
            {
                Download(fileDownloader, webClient, uri, targetFilePath);
                success = true;
            }
            catch
            {
                success = false;
            }
        }
    }
}
