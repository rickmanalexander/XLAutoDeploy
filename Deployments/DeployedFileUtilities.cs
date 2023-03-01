using XLAutoDeploy.Manifests;

using System;
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
            return String.Format(Constants.AddInManifestParameterizedFileName, deploymentPayload.AddIn.Identity.Name);
        }

        public static string GetUpdateQueryInfoManifestFilePath(DeploymentPayload deploymentPayload)
        {
            return Path.Combine(deploymentPayload.Destination.ParentDirectory, GetUpdateQueryInfoManifestFileName(deploymentPayload));
        }

        public static string GetUpdateQueryInfoManifestFileName(DeploymentPayload deploymentPayload)
        {
            return String.Format(Constants.UpdateQueryInfoManifestParameterizedFileName, deploymentPayload.AddIn.Identity.Name);
        }
    }
}
