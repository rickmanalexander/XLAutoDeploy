﻿using System;
using System.Net;
using System.Threading.Tasks;

using XLAutoDeploy.Logging;
using XLAutoDeploy.FileSystem.Access;

namespace XLAutoDeploy.Updates
{
    public interface IUpdateDownloader
    {
        ILogger Logger { get; }

        void Download(IRemoteFileDownloader fileDownloader, string remoteFilePath, string targetFilePath);
        Task DownloadAsync(IRemoteFileDownloader fileDownloader, string remoteFilePath, string targetFilePath);
        void TryDownload(IRemoteFileDownloader fileDownloader, string remoteFilePath, string targetFilePath, out bool success);

        void Download(IRemoteFileDownloader fileDownloader, WebClient webClient, string address, string targetFilePath);
        Task DownloadAsync(IRemoteFileDownloader fileDownloader, WebClient webClient, string address, string targetFilePath);
        void TryDownload(IRemoteFileDownloader fileDownloader, WebClient webClient, string address, string targetFilePath, out bool success);

        void Download(IRemoteFileDownloader fileDownloader, WebClient webClient, Uri uri, string targetFilePath);
        Task DownloadAsync(IRemoteFileDownloader fileDownloader, WebClient webClient, Uri uri, string targetFilePath);
        void TryDownload(IRemoteFileDownloader fileDownloader, WebClient webClient, Uri uri, string targetFilePath, out bool success);
    }
}