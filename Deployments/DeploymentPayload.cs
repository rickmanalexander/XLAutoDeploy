using XLAutoDeploy.Mage;

namespace XLAutoDeploy.Deployments
{
    public sealed class DeploymentPayload
    {
        public FileHost FileHost => _fileHost;
        public Deployment Deployment => _deployment;
        public AddIn AddIn => _addIn;

        private readonly FileHost _fileHost;
        private readonly Deployment _deployment;
        private readonly AddIn _addIn; 

        public DeploymentPayload(FileHost fileHost, Deployment deployment, AddIn addIn)
        {
            _fileHost = fileHost; 
            _deployment = deployment;
            _addIn = addIn; 
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
