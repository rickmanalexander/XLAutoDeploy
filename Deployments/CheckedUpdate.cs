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
                    return $"A new version of the Excel Add-In titled " +
                        $"{_payload.AddIn.Identity.Title} is avaliable! " +
                        $"{System.Environment.NewLine}{System.Environment.NewLine} " +
                        $"Please wait while the update is processed. " +
                        $"{System.Environment.NewLine} Note: Once the update is complete, " +
                        $"you MUST restart Excel for it to take effect.";
                }
                else
                {
                    return $"A new version of the Excel Add-In titled " +
                        $"{_payload.AddIn.Identity.Title} is avaliable! " +
                        $"{System.Environment.NewLine}{System.Environment.NewLine} " +
                        $"Please wait while the update is processed.";
                }
            }
            else
            {
                if (_info.IsRestartRequired)
                {
                    return $"A new version of the Excel Add-In titled " +
                        $"{_payload.AddIn.Identity.Title} is avaliable! " +
                        $"{System.Environment.NewLine}{System.Environment.NewLine} " +
                        $"Would you like to update now, or defer until later? " +
                        $"{System.Environment.NewLine} Note: Once the update is complete, " +
                        $"you MUST restart Excel for it to take effect.";

                }
                else
                {
                    return $"A new version of the Excel Add-In titled " +
                        $"{_payload.AddIn.Identity.Title} is avaliable! " +
                        $"{System.Environment.NewLine}{System.Environment.NewLine} " +
                        $"Would you like to update now, or defer until later?.";
                }
            }
        }

    }
}
