using XLAutoDeploy.Manifests;

namespace XLAutoDeploy.Deployments
{
    internal sealed class CheckedUpdate
    {
        public UpdateQueryInfo Info => _info;
        public DeploymentPayload Payload => _payload;

        private readonly UpdateQueryInfo _info;
        private readonly DeploymentPayload _payload;

        public CheckedUpdate(UpdateQueryInfo info, DeploymentPayload payload)
        {
            _info = info;
            _payload = payload;
        }

        public string GetDescription()
        {
            if (_info.IsRestartRequired)
            {
                if (_info.IsMandatoryUpdate)
                {
                    return "You can update now or defer until later. " +
                        "If you choose to defer, then add-in will NOT be loaded." +
                        "If you choose to update now, then once complete, " +
                        "you MUST restart Excel for it to take effect.";

                }
                else
                {
                    return "Would you like to update now or defer until later? " +
                        "Once the update is complete, " +
                        "you MUST restart Excel for it to take effect.";

                }
            }
            else
            {
                if (_info.IsMandatoryUpdate)
                {
                    return "You can update now or defer until later. " +
                        "If you choose to defer, then add-in will NOT be loaded."; 


                }
                else
                {
                    return "Would you like to update now, or defer until later?.";

                }
            }
        }
    }
}
