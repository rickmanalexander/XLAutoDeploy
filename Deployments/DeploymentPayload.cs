using XLAutoDeploy.Manifests;

namespace XLAutoDeploy.Deployments
{
    public sealed class DeploymentPayload
    {
        public FileHost FileHost => _fileHost;
        public Deployment Deployment => _deployment;
        public AddIn AddIn => _addIn;
        public AddIn AlternateAddIn => _alternateAddIn;

        private readonly FileHost _fileHost;
        private readonly Deployment _deployment;
        private readonly AddIn _addIn;
        private readonly AddIn _alternateAddIn;

        public DeploymentPayload(FileHost fileHost, Deployment deployment, AddIn addIn, AddIn alternateAddIn)
        {
            _fileHost = fileHost; 
            _deployment = deployment;
            _addIn = addIn;
            _alternateAddIn = alternateAddIn; 
        }

        public UpdateDeploymentDestination Destination =>
                new UpdateDeploymentDestination(this.Deployment.Settings.DeploymentBasis, 
                    this.Deployment.Description.Manufacturer, 
                    this.Deployment.Description.Product, 
                    this.AddIn.Identity.Version, 
                    this.AddIn.Identity.Name, 
                    this.AddIn.Identity.FileExtension);
    }
}
