using XLAutoDeploy.Manifests;

namespace XLAutoDeploy.Deployments
{
    internal sealed class DeploymentPayload
    {
        public FileHost FileHost => _fileHost;
        public Deployment Deployment => _deployment;
        public AddIn AddIn => _addIn;
        public string AddInSchemaLocation => _addInSchemaLocation;
        public DeploymentDestination Destination => _destination;

        private readonly FileHost _fileHost;
        private readonly Deployment _deployment;
        private readonly AddIn _addIn;
        private readonly string _addInSchemaLocation;
        private readonly DeploymentDestination _destination; 

        public DeploymentPayload(FileHost fileHost, Deployment deployment, AddIn addIn, string addInSchemaLocation)
        {
            _fileHost = fileHost; 
            _deployment = deployment;
            _addIn = addIn;
            _addInSchemaLocation = addInSchemaLocation;
            _destination = new DeploymentDestination(this.Deployment.Settings.DeploymentBasis,
                    this.Deployment.Description.Manufacturer,
                    this.Deployment.Description.Product,
                    this.Deployment.TargetOfficeInstallation,
                    this.AddIn.Identity.Version,
                    this.AddIn.Identity.Name,
                    this.AddIn.Identity.FileExtension);
        }       
    }
}
