using XLAutoDeploy.FileSystem.Access;

using System;
using System.Net;

namespace XLAutoDeploy.FileSystem
{
    internal sealed class FileNetworkConnection : IFileNetworkConnection
    {
        public NetworkCredential NetworkCredential => _networkCredential;
        public string RemoteServerName => _remoteServerName;
        public FileNetworkConnectionState State => _state;

        private readonly NetworkCredential _networkCredential;
        private readonly string _remoteServerName;

        private FileNetworkConnectionState _state;
        private bool _disposed = false;

        private static readonly CredentialCache _cache = new CredentialCache();

        public FileNetworkConnection(NetworkCredential networkCredential, string remoteServerName)
        {
            if (networkCredential is null)
            {
                throw new ArgumentNullException(Common.GetFormatedErrorMessage($"Constructing type {nameof(FileNetworkConnection)}",
                    $"The {nameof(networkCredential)} parameter is null.",
                    $"Supply a valid {nameof(networkCredential)}."));
            }

            if (String.IsNullOrEmpty(remoteServerName))
            {
                throw new ArgumentNullException(Common.GetFormatedErrorMessage($"Constructing type {nameof(FileNetworkConnection)}",
                    $"The {nameof(remoteServerName)} parameter is null or empty.",
                    $"Supply a valid {nameof(remoteServerName)}."));
            }

            _networkCredential = networkCredential;
            _remoteServerName = GetEscapedFileServerName(remoteServerName);
        }

        public void Open()
        {
            try
            {
                _cache.Add(new Uri(_remoteServerName), "Basic", _networkCredential);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(Common.GetFormatedErrorMessage($"Attempting to open a {nameof(FileNetworkConnection)}.",
                                "Connection is already open.",
                                "Ensure connection is closed before attempting to open."), ex);
            }

            _state = FileNetworkConnectionState.Open;
        }

        public void Close()
        {
            try
            {
                _cache.Remove(new Uri(_remoteServerName), "Basic");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(Common.GetFormatedErrorMessage($"Attempting to close a {nameof(FileNetworkConnection)}.",
                                "Connection is not open.",
                                "Ensure connection is open before attempting to close."), ex);
            }

            _state = FileNetworkConnectionState.Closed;
        }

        private string GetEscapedFileServerName(string remoteServerName)
        {
            return @"\\" + remoteServerName.Replace(@"\\", String.Empty);
        }

        ~FileNetworkConnection()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (this._disposed)
                return;

            if (disposing)
            {
                GC.SuppressFinalize(this);
            }

            if (_remoteServerName is not null)
            {
                _cache?.Remove(new Uri(_remoteServerName), "Basic");
            }

            _state = FileNetworkConnectionState.Closed;

            this._disposed = true;
        }
    }
}
