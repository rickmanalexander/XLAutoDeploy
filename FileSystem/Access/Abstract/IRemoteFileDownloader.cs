using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

using XLAutoDeploy.Logging;

namespace XLAutoDeploy.FileSystem.Access
{
    public interface IRemoteFileDownloader
    {
        ILogger Logger { get; }

        void Download(string remoteFilePath, string targetFilePath, bool overwrite = false);
        Task DownloadAsync(string remoteFilePath, string targetFilePath, bool overwrite = false);

        void Download(WebClient webClient, string url, string targetFilePath, bool overwrite = false);
        Task DownloadAsync(WebClient webClient, string url, string targetFilePath, bool overwrite = false);

        void Download(WebClient webClient, Uri uri, string targetFilePath, bool overwrite = false);
        Task DownloadAsync(WebClient webClient, Uri uri, string targetFilePath, bool overwrite = false);


        Stream Download(string remoteFilePath);

        Stream Download(WebClient webClient, string url);
        Task<Stream> DownloadAsync(WebClient webClient, string url);

        Stream Download(WebClient webClient, Uri uri);
        Task<Stream> DownloadAsync(WebClient webClient, Uri uri);


        byte[] DownloadBytes(string filePath);

        byte[] DownloadBytes(WebClient webClient, string url);
        Task<byte[]> DownloadBytesAsync(WebClient webClient, string url);

        byte[] DownloadBytes(WebClient webClient, Uri uri);
        Task<byte[]> DownloadBytesAsync(WebClient webClient, Uri uri);
    }
}
