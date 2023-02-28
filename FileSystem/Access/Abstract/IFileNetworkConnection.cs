using System;
using System.Net;

namespace XLAutoDeploy.FileSystem.Access
{
    internal interface IFileNetworkConnection : IDisposable
    {
        NetworkCredential NetworkCredential { get; }
        string RemoteServerName { get; }

        FileNetworkConnectionState State { get; }

        void Open();
        void Close();
    }

    internal enum FileNetworkConnectionState
    {
        Closed,
        Open
    }
}
