using System;
using System.Net;

namespace XLAutoDeploy.FileSystem.Access
{
    public interface IFileNetworkConnection : IDisposable
    {
        NetworkCredential NetworkCredential { get; }
        string RemoteServerName { get; }

        FileNetworkConnectionState State { get; }

        void Open();
        void Close();
    }

    public enum FileNetworkConnectionState
    {
        Closed,
        Open
    }
}
