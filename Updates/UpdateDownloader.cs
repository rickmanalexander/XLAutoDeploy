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

        //??what if targetFilePath != local prod file path becuase it was changed remotely?
        public void Download(IRemoteFileDownloader fileDownloader, string remoteFilePath, string targetFilePath)
        {
            lock (_threadLock)
            {
                FileSystem.FileUtilities.CreateHiddenDirectory(targetFilePath, _logger);

                fileDownloader.Download(remoteFilePath, targetFilePath, true);
            }
        }

        public async Task DownloadAsync(IRemoteFileDownloader fileDownloader, string remoteFilePath, string targetFilePath)
        {
            FileSystem.FileUtilities.CreateHiddenDirectory(targetFilePath, _logger);

            await fileDownloader.DownloadAsync(remoteFilePath, targetFilePath, true);
        }

        public void TryDownload(IRemoteFileDownloader fileDownloader, string remoteFilePath, string targetFilePath, out bool success)
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

        public void Download(IRemoteFileDownloader fileDownloader, WebClient webClient, string address, string targetFilePath)
        {
            lock (_threadLock)
            {
                FileSystem.FileUtilities.CreateHiddenDirectory(targetFilePath, _logger);

                fileDownloader.Download(webClient, address, targetFilePath, true);
            }
        }

        public async Task DownloadAsync(IRemoteFileDownloader fileDownloader, WebClient webClient, string address, string targetFilePath)
        {
            FileSystem.FileUtilities.CreateHiddenDirectory(targetFilePath, _logger);

            await fileDownloader.DownloadAsync(webClient, address, targetFilePath, true);
        }

        public void TryDownload(IRemoteFileDownloader fileDownloader, WebClient webClient, string address, string targetFilePath, out bool success)
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

        public void Download(IRemoteFileDownloader fileDownloader, WebClient webClient, Uri uri, string targetFilePath)
        {
            lock (_threadLock)
            {
                FileSystem.FileUtilities.CreateHiddenDirectory(targetFilePath, _logger);

                //??what if targetFilePath != local prod file path becuase it was changed remotely?
                fileDownloader.Download(webClient, uri, targetFilePath, true);
            }
        }

        public async Task DownloadAsync(IRemoteFileDownloader fileDownloader, WebClient webClient, Uri uri, string targetFilePath)
        {
            FileSystem.FileUtilities.CreateHiddenDirectory(targetFilePath, _logger);

            //?? if targetFilePath != local prod file path becuase it was changed remotely?
            await fileDownloader.DownloadAsync(webClient, uri, targetFilePath, true);
        }

        public void TryDownload(IRemoteFileDownloader fileDownloader, WebClient webClient, Uri uri, string targetFilePath, out bool success)
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
