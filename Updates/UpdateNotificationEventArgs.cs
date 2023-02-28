using System;

namespace XLAutoDeploy.Updates
{
    internal sealed class UpdateNotificationEventArgs : EventArgs
    {
        public bool DoUpdate => _doUpdate;

        private readonly bool _doUpdate;

        public UpdateNotificationEventArgs(bool doUpdate)
        {
            _doUpdate = doUpdate;
        }
    }
}
