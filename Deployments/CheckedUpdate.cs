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
            if (_info.IsMandatoryUpdate)
            {
                if (_info.IsRestartRequired)
                {
                    return $"Please wait while the update is processed. " +
                        $"{System.Environment.NewLine} Note: Once the update is complete, " +
                        $"you MUST restart Excel for it to take effect.";
                }
                else
                {
                    return "Please wait while the update is processed.";
                }
            }
            else
            {
                if (_info.IsRestartRequired)
                {
                    return "Would you like to update now, or defer until later? " +
                        "Once the update is complete, " +
                        "you MUST restart Excel for it to take effect.";

                }
                else
                {
                    return "Would you like to update now, or defer until later?.";
                }
            }
        }

    }
}
