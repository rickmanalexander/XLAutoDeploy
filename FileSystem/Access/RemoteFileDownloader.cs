using XLAutoDeploy.Logging;

using System;
using System.IO;
using System.Net;
using System.Security;
using System.Threading.Tasks;

namespace XLAutoDeploy.FileSystem.Access
{
    internal sealed class RemoteFileDownloader : IRemoteFileDownloader
    {
        public ILogger Logger => _logger;

        private readonly ILogger _logger;

        public RemoteFileDownloader(ILogger logger)
        {
            if (logger is null)
            {
                throw new ArgumentNullException(Common.GetFormatedErrorMessage($"Constructing type {nameof(RemoteFileDownloader)}",
                    $"The {nameof(logger)} parameter is null.",
                    $"Supply a valid {nameof(logger)}."));
            }

            _logger = logger;
        }

        public void Download(string remoteFilePath, string targetFilePath, bool overwrite = false)
        {
            try
            {
                File.Copy(remoteFilePath, targetFilePath, overwrite);
            }
            catch (IOException ex)
            {
                _logger.Error(ex, $"Invalid remote ({remoteFilePath}) and/or target ({targetFilePath}) file path");
                throw;
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.Error(ex, $"Inaccessible remote ({remoteFilePath}) and/or target ({targetFilePath}) file path");
                throw;
            }
            catch (SecurityException ex)
            {
                _logger.Error(ex, $"Inaccessible remote ({remoteFilePath}) and/or target ({targetFilePath}) file path");
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error downloading file from remote host ({remoteFilePath}) to local target ({targetFilePath})");
                throw;
            }
        }

        public async Task DownloadAsync(string remoteFilePath, string targetFilePath, bool overwrite = false)
        {
            try
            {
                using (var remoteStream = File.OpenRead(remoteFilePath))
                {
                    using (var targetStream = File.Create(targetFilePath)) // this always overwites the file if it exists
                    {
                        await remoteStream.CopyToAsync(targetStream);
                    }
                }
            }
            catch (IOException ex)
            {
                _logger.Error(ex, $"Invalid remote ({remoteFilePath}) and/or target ({targetFilePath}) file path");
                throw; 
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.Error(ex, $"Inaccessible remote ({remoteFilePath}) and/or target ({targetFilePath}) file path");
                throw;
            }
            catch (SecurityException ex)
            {
                _logger.Error(ex, $"Inaccessible remote ({remoteFilePath}) and/or target ({targetFilePath}) file path");
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error downloading file from remote host ({remoteFilePath}) to local target ({targetFilePath})");
                throw;
            }
        }


        public void Download(WebClient webClient, string url, string targetFilePath, bool overwrite = false)
        {
            Download(webClient, new Uri(url), targetFilePath, overwrite);
        }

        public async Task DownloadAsync(WebClient webClient, string url, string targetFilePath, bool overwrite = false)
        {
            await DownloadAsync(webClient, new Uri(url), targetFilePath, overwrite);
        }

        // using separate methods for Uri, b/c Uri.ToString() can return control characters, which can corrupt the state of a console application. 
        // Also, Uri.ToString() does not contain port information when the port is the default port for the scheme.
        public void Download(WebClient webClient, Uri uri, string targetFilePath, bool overwrite = false)
        {
            try
            {
                webClient.DownloadFile(uri, targetFilePath); // this always overwites the file if it exists
            }
            catch (IOException ex)
            {
                _logger.Error(ex, $"Invalid url ({uri}) and/or target ({targetFilePath}) file path");
                throw;
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.Error(ex, $"Inaccessible url ({uri}) and/or target ({targetFilePath}) file path");
                throw;
            }
            catch (SecurityException ex)
            {
                _logger.Error(ex, $"Inaccessible url ({uri}) and/or target ({targetFilePath}) file path");
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error downloading file from remote host ({uri}) to local target ({targetFilePath})");
                throw;
            }
        }

        public async Task DownloadAsync(WebClient webClient, Uri uri, string targetFilePath, bool overwrite = false)
        {
            try
            {
                await webClient.DownloadFileTaskAsync(uri, targetFilePath);
            }
            catch (IOException ex)
            {
                _logger.Error(ex, $"Invalid url ({uri}) and/or target ({targetFilePath}) file path");
                throw;
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.Error(ex, $"Inaccessible url ({uri}) and/or target ({targetFilePath}) file path");
                throw;
            }
            catch (SecurityException ex)
            {
                _logger.Error(ex, $"Inaccessible url ({uri}) and/or target ({targetFilePath}) file path");
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error downloading file from remote host ({uri}) to local target ({targetFilePath})");
                throw;
            }
        }


        public Stream Download(string filePath)
        {
            return File.OpenRead(filePath);
        }

        public Stream Download(WebClient webClient, string url)
        {
            return Download(webClient, new Uri(url));
        }

        public async Task<Stream> DownloadAsync(WebClient webClient, string url)
        {
            return await DownloadAsync(webClient, new Uri(url));
        }

        public Stream Download(WebClient webClient, Uri uri)
        {
            return webClient.OpenRead(uri);
        }

        public async Task<Stream> DownloadAsync(WebClient webClient, Uri uri)
        {
            return await webClient.OpenReadTaskAsync(uri);
        }


        public byte[] DownloadBytes(string filePath)
        {
            using (var stream = File.OpenRead(filePath))
            {
                byte[] buffer = new byte[16 * 1024];
                using (MemoryStream ms = new MemoryStream())
                {
                    int index;
                    while ((index = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        ms.Write(buffer, 0, index);
                    }

                    if(index < (buffer.Length - 1))
                    {
                        byte[] result = new byte[index]; ;
                        Array.Copy(ms.ToArray(), 0, result, 0, index + 1); // Get rid of empty bytes at end

                        return result;
                    }
                    else
                    {
                        return buffer; 
                    }
                }
            }
        }

        public byte[] DownloadBytes(WebClient webClient, string url)
        {
            return DownloadBytes(webClient, new Uri(url));
        }

        public async Task<byte[]> DownloadBytesAsync(WebClient webClient, string url)
        {
            return await DownloadBytesAsync(webClient, new Uri(url));
        }

        public byte[] DownloadBytes(WebClient webClient, Uri uri)
        {
            return webClient.DownloadData(uri);
        }

        public async Task<byte[]> DownloadBytesAsync(WebClient webClient, Uri uri)
        {
            return await webClient.DownloadDataTaskAsync(uri);
        }
    }
}
