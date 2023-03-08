using XLAutoDeploy.Manifests.Utilities;

using System.IO;

namespace XLAutoDeploy.Deployments
{
    internal static class DeploymentPayloadExtensions
    {
        public static string GetAddInManifestFilePath(this DeploymentPayload deploymentPayload)
        {
            return Path.Combine(deploymentPayload.Destination.ParentDirectory, deploymentPayload.GetAddInManifestFileName());
        }

        public static string GetAddInManifestFileName(this DeploymentPayload deploymentPayload)
        {
            return ManifestFileNaming.AddInManifestFileName(deploymentPayload.AddIn.Identity.Name);
        }

        public static string GetUpdateQueryInfoManifestFilePath(this DeploymentPayload deploymentPayload)
        {
            return Path.Combine(deploymentPayload.Destination.ParentDirectory, deploymentPayload.GetUpdateQueryInfoManifestFileName());
        }

        public static string GetUpdateQueryInfoManifestFileName(this DeploymentPayload deploymentPayload)
        {
            return ManifestFileNaming.UpdateQueryInfoManifestFileName (deploymentPayload.AddIn.Identity.Name);
        }
    }
}
