using XLAutoDeploy.Manifests.Utilities;

using System.IO;

namespace XLAutoDeploy.Deployments
{
    internal static class DeployedFileUtilities
    {
        public static string GetAddInManifestFilePath(DeploymentPayload deploymentPayload)
        {
            return Path.Combine(deploymentPayload.Destination.ParentDirectory, GetAddInManifestFileName(deploymentPayload));
        }

        public static string GetAddInManifestFileName(DeploymentPayload deploymentPayload)
        {
            return ManifestFileNaming.AddInManifestFileName(deploymentPayload.AddIn.Identity.Name);
        }

        public static string GetUpdateQueryInfoManifestFilePath(DeploymentPayload deploymentPayload)
        {
            return Path.Combine(deploymentPayload.Destination.ParentDirectory, GetUpdateQueryInfoManifestFileName(deploymentPayload));
        }

        public static string GetUpdateQueryInfoManifestFileName(DeploymentPayload deploymentPayload)
        {
            return ManifestFileNaming.UpdateQueryInfoManifestFileName (deploymentPayload.AddIn.Identity.Name);
        }
    }
}
