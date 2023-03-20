using ExcelDna.Integration;
using ExcelDna.Integration.Extensibility;

using System;
using System.Runtime.InteropServices;

namespace XLAutoDeploy
{
    [ComVisible(true)]
    public sealed class ComAddInExtensibility : ExcelComAddIn
    {
        private readonly Action _startUpAction;
        private readonly Action _shutdownAction;

        private bool _isInitialized;
        private bool _hasBeginShutdownExecuted;

        //See: https://codereview.stackexchange.com/questions/141956/idtextensibility2-implementation-for-rubberducks-entry-point
        public ComAddInExtensibility(Action startUpAction, Action shutdownAction)
        {
            _startUpAction = startUpAction ?? throw new ArgumentNullException(Common.GetFormatedErrorMessage($"Constructing type {nameof(ComAddInExtensibility)}",
                    $"The {nameof(startUpAction)} parameter is null.",
                    $"Supply a valid {nameof(startUpAction)}."));

            _shutdownAction = shutdownAction ?? throw new ArgumentNullException(Common.GetFormatedErrorMessage($"Constructing type {nameof(ComAddInExtensibility)}",
                    $"The {nameof(shutdownAction)} parameter is null.",
                    $"Supply a valid {nameof(shutdownAction)}."));
        }

        public override void OnConnection(object Application, ext_ConnectMode ConnectMode, object AddInInst, ref Array custom)
        {
            switch (ConnectMode)
            {
                case ext_ConnectMode.ext_cm_Startup:
                    // normal execution path - don't initialize just yet, wait for OnStartupComplete to be called by the host.
                    break;
                case ext_ConnectMode.ext_cm_AfterStartup:
                    InitializeAddIn();
                    break;
            }
        }

        public override void OnDisconnection(ext_DisconnectMode RemoveMode, ref Array custom)
        {
            switch (RemoveMode)
            {
                case ext_DisconnectMode.ext_dm_UserClosed:
                    ShutdownAddIn();
                    break;

                case ext_DisconnectMode.ext_dm_HostShutdown:
                    if (_hasBeginShutdownExecuted)
                    {
                        // this is the normal case: nothing to do here, we already ran ShutdownAddIn.
                    }
                    else
                    {
                        // some hosts do not call OnBeginShutdown: this mitigates it.
                        ShutdownAddIn();
                    }
                    break;
            }
        }

        public override void OnAddInsUpdate(ref Array custom)
        {
        }

        public override void OnStartupComplete(ref Array custom)
        {
            InitializeAddIn();
        }

        public override void OnBeginShutdown(ref Array custom)
        {
            _hasBeginShutdownExecuted = true;
            ShutdownAddIn();
        }


        private void InitializeAddIn()
        {
            if (_isInitialized)
            {
                // The add-in is already initialized. See:
                // The strange case of the add-in initialized twice
                // http://msmvps.com/blogs/carlosq/archive/2013/02/14/the-strange-case-of-the-add-in-initialized-twice.aspx
                return;
            }

            _startUpAction();
            _isInitialized = true;
        }

        private void ShutdownAddIn()
        {
            _shutdownAction();
            _isInitialized = false;
        }
    }
}
