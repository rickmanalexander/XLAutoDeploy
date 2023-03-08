using XLAutoDeploy.Manifests;

using System;

namespace XLAutoDeploy.Updates
{
    internal sealed class UpdateNotifier : IUpdateNotifier
    {
        public event EventHandler<UpdateNotificationEventArgs> NotificationComplete;

        public bool DoUpdate => _doUpdate;

        private bool _doUpdate = false;

        /// <summary>
        /// Notify a user of an available update. 
        /// </summary>  
        /// <remarks>
        /// This method will automatically set the <see cref="UpdateQueryInfo.FirstNotified"/> and <see cref="UpdateQueryInfo.LastNotified"/> properties using the current UTC datetime. 
        /// </remarks>
        public void Notify(string message, Description deploymentDescription, UpdateQueryInfo updateQueryInfo, bool allowSkip)
        {
            _doUpdate = false;

            using (var view = new UpdateNotificationView(message, deploymentDescription, updateQueryInfo, allowSkip))
            {
                var now = DateTime.UtcNow;

                updateQueryInfo.FirstNotified = updateQueryInfo.FirstNotified ?? now;
                updateQueryInfo.LastNotified = now;

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
