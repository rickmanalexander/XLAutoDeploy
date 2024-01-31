using System;
using System.Windows.Forms;

namespace XLAutoDeploy.Updates
{
    internal partial class UpdateNotificationView : Form
    {
        public event EventHandler<UpdateNotificationEventArgs> NotificationComplete;

        private readonly string _message;
        private readonly string _product;
        private readonly string _publisher;
        private readonly Version _deployedVersion;
        private readonly Version _availableVersion;
        private readonly bool _allowSkip = false;

        private bool _doUpdate = false;

        public UpdateNotificationView(string message, string product, 
            string publisher, Version deployedVersion, Version availableVersion, bool allowSkip)
        {
            InitializeComponent();

            _message = message;
            _product = product;
            _publisher = publisher;
            _deployedVersion = deployedVersion;
            _availableVersion = availableVersion;
            _allowSkip = allowSkip;

            BindValuesToControls();
        }

        private void BindValuesToControls()
        {
            this.lblNewVersionAvailable.Text = $"A new version of {_product} is available";
            this.lblUpdateMessage.Text = _message;
            this.txtBxPublisher.Text = _publisher;
            this.txtBxAddIn.Text = _product;
            this.txtBxInstalledVersion.Text = _deployedVersion.ToString();
            this.txtBxNewVersion.Text = _availableVersion.ToString();

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
