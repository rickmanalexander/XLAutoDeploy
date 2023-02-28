using XLAutoDeploy.Manifests;

using System;

namespace XLAutoDeploy.Updates
{
    internal sealed class UpdateNotifier : IUpdateNotifier
    {
        public event EventHandler<UpdateNotificationEventArgs> NotificationComplete;

        public bool DoUpdate => _doUpdate;

        private bool _doUpdate = false;

        //Cannot use this method b/c the following build error is raised:
        //"Cannot deserialize type 'XLAutoDeploy.UpdateDeployment.AddInUpdateNotifier'
        //because it contains property 'DoUpdate' which has no public setter."
        //public bool DoUpdate { get; private set; } = false;

        public void Notify(string message, Description deploymentDescription, UpdateQueryInfo updateQueryInfo, bool allowSkip)
        {
            _doUpdate = false;

            using (var view = new UpdateNotificationView(message, deploymentDescription, updateQueryInfo, allowSkip))
            {
                view.Show();

                view.NotificationComplete += View_NotificationComplete;
            }
        }

        private void View_NotificationComplete(object sender, UpdateNotificationEventArgs e)
        {
            _doUpdate = e.DoUpdate;

            RaiseNotificationCompleteEvent(e.DoUpdate);
        }

        private void RaiseNotificationCompleteEvent(bool doUpdate)
        {
            var handler = NotificationComplete;
            if (handler != null)
            {
                var @event = new UpdateNotificationEventArgs(doUpdate);
                handler(this, @event);
            }
        }
    }
}
