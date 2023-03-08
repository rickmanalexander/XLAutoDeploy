using XLAutoDeploy.Logging;
using XLAutoDeploy.FileSystem.Access;

using System;
using System.Net;
using System.Threading.Tasks;

namespace XLAutoDeploy.Updates
{
    internal interface IUpdateDownloader
    {
        ILogger Logger { get; }

        void Download(IRemoteFileDownloader fileDownloader, string remoteFilePath, string targetFilePath, bool overwrite = false);
        Task DownloadAsync(IRemoteFileDownloader fileDownloader, string remoteFilePath, string targetFilePath, bool overwrite = false);
        void TryDownload(IRemoteFileDownloader fileDownloader, string remoteFilePath, string targetFilePath, out bool success, bool overwrite = false);

        void Download(IRemoteFileDownloader fileDownloader, WebClient webClient, string address, string targetFilePath, bool overwrite = false);
        Task DownloadAsync(IRemoteFileDownloader fileDownloader, WebClient webClient, string address, string targetFilePath, bool overwrite = false);
        void TryDownload(IRemoteFileDownloader fileDownloader, WebClient webClient, string address, string targetFilePath, out bool success, bool overwrite = false);

        void Download(IRemoteFileDownloader fileDownloader, WebClient webClient, Uri uri, string targetFilePath, bool overwrite = false);
        Task DownloadAsync(IRemoteFileDownloader fileDownloader, WebClient webClient, Uri uri, string targetFilePath, bool overwrite = false);
        void TryDownload(IRemoteFileDownloader fileDownloader, WebClient webClient, Uri uri, string targetFilePath, out bool success, bool overwrite = false);
    }
}
