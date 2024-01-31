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
        /// </remarks>
        public void Notify(string message, string product,
            string publisher, Version deployedVersion, Version availableVersion, bool allowSkip)
        {
            _doUpdate = false;

            _view = new UpdateNotificationView(message, product, publisher, deployedVersion, availableVersion, allowSkip) ?? throw new NullReferenceException(Common.GetFormatedErrorMessage($"Constructing type {nameof(UpdateNotificationView)}",
                    $"The {nameof(UpdateNotificationView)} contructor returned null unexpectedly.",
                    $"Return a valid instance from the constructor.")); 

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
