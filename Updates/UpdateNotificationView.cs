using System;
using System.Windows.Forms;

using XLAutoDeploy.Manifests;

namespace XLAutoDeploy.Updates
{
    internal partial class UpdateNotificationView : Form
    {
        public event EventHandler<UpdateNotificationEventArgs> NotificationComplete;

        private readonly string _message;
        private readonly Description _deploymentDescription;
        private readonly UpdateQueryInfo _updateQueryInfo;
        private readonly bool _allowSkip = false;

        private bool _doUpdate = false;

        public UpdateNotificationView(string message, Description deploymentDescription, 
            UpdateQueryInfo updateQueryInfo, bool allowSkip)
        {
            InitializeComponent();

            _message = message;
            _deploymentDescription = deploymentDescription;
            _updateQueryInfo = updateQueryInfo;
            _allowSkip = allowSkip;

            BindValuesToControls();
        }

        private void BindValuesToControls()
        {
            this.lblNewVersionAvailable.Text = $"A New Version of {_deploymentDescription.Product} is Available";
            this.lblUpdateMessage.Text = _message;
            this.txtBxPublisher.Text = _deploymentDescription.Publisher;
            this.txtBxAddIn.Text = _deploymentDescription.Product;
            this.txtBxInstalledVersion.Text = _updateQueryInfo.DeployedVersion.ToString();
            this.txtBxNewVersion.Text = _updateQueryInfo.AvailableVersion.ToString();

            this.btnSkip.Enabled = _allowSkip;
        }

        private void SkipBtnClick(object sender, EventArgs e)
        {
            _doUpdate = false;
            RaiseNotificationCompleteEvent(_doUpdate);
        }

        private void OkBtnClick(object sender, EventArgs e)
        {
            _doUpdate = true;
            RaiseNotificationCompleteEvent(_doUpdate);
        }

        private void RaiseNotificationCompleteEvent(bool doUpdate)
        {
            var handler = NotificationComplete;
            if (handler is not null)
            {
                var @event = new UpdateNotificationEventArgs(doUpdate);
                handler(this, @event);
            }
        }
    }
}
