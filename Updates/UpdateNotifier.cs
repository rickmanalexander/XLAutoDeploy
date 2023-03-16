using XLAutoDeploy.Manifests;

using System;

namespace XLAutoDeploy.Updates
{
    internal sealed class UpdateNotifier : IUpdateNotifier
    {
        public bool DoUpdate => _doUpdate;

        private bool _doUpdate = false;

        private UpdateNotificationView _view;

        /// <summary>
        /// Notify a user of an available update. 
        /// </summary>  
        /// <remarks>
        /// This method will automatically set the <see cref="UpdateQueryInfo.FirstNotified"/> and <see cref="UpdateQueryInfo.LastNotified"/> properties using the current UTC datetime. 
        /// </remarks>
        public void Notify(string message, Description deploymentDescription, UpdateQueryInfo updateQueryInfo, bool allowSkip)
        {
            _doUpdate = false;

            _view = new UpdateNotificationView(message, deploymentDescription, updateQueryInfo, allowSkip); 

            var now = DateTime.UtcNow;

            updateQueryInfo.FirstNotified = updateQueryInfo.FirstNotified ?? now;
            updateQueryInfo.LastNotified = now;

            _view.NotificationComplete += View_NotificationComplete;

            _view.ShowDialog(); 
        }

        private void View_NotificationComplete(object sender, UpdateNotificationEventArgs e)
        {
            _doUpdate = e.DoUpdate;
            _view.Hide(); 
            _view?.Dispose();
        }
    }
}
