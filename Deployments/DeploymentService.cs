using XLAutoDeploy.FileSystem.Access;
using XLAutoDeploy.Updates;

using XLAutoDeploy.Manifests;
using XLAutoDeploy.Manifests.Utilities;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;

namespace XLAutoDeploy.Deployments
{
    internal static class DeploymentService
    {
        private static readonly IDictionary<NetClrVersion, HashSet<System.Version>> _installedClrAndNetFrameworks =
            ClientSystemDetection.GetAllInstalledClrAndNetFrameworkVersions();

        public static DeploymentRegistry GetDeploymentRegistry(Uri uri)
        {
            if (uri.IsFile || uri.IsUnc)
            {
                return ManifestSerialization.DeserializeManifestFile<DeploymentRegistry>(uri.LocalPath);
            }
            else
            {
                return GetDeploymentRegistry(new WebClient(), uri);
            }
        }

        public static DeploymentRegistry GetDeploymentRegistry(WebClient webClient, Uri uri)
        {
            return ManifestSerialization.DeserializeManifestFile<DeploymentRegistry>(webClient, uri);
        }

        public static IReadOnlyCollection<DeploymentPayload> GetDeploymentPayloads(DeploymentRegistry registry)
        {
            WebClient webClient = new WebClient();

            return GetDeploymentPayloads(registry, null, webClient);
        }

        public static IReadOnlyCollection<DeploymentPayload> GetDeploymentPayloads(DeploymentRegistry registry,
            IFileNetworkConnection fileNetworkConnection = null, WebClient webClient = null)
        {
            if (registry == null)
            {
                throw new ArgumentNullException(Common.GetFormatedErrorMessage($"Attempting to retrieve deployment payloads.",
                    $"The {nameof(DeploymentRegistry)} is null.",
                    $"Retrieve the deployment registry using GetDeploymentRegistry."));
            }

            if (registry?.PublishedDeployments?.Count == 0)
            {
                throw new ArgumentNullException(Common.GetFormatedErrorMessage($"Attempting to retrieve deployment payloads.",
                    $"The {nameof(DeploymentRegistry.PublishedDeployments)} is null.",
                    $"Retrieve the deployment registry using GetDeploymentRegistry."));
            }

            List<DeploymentPayload> payloads = new List<DeploymentPayload>();
            foreach (var publishedDeployment in registry.PublishedDeployments)
            {
                var payload = GetDeploymentPayload(publishedDeployment, fileNetworkConnection, webClient);

                payloads.Add(payload);
            }

            return new ReadOnlyCollection<DeploymentPayload>(payloads);
        }

        // This is a mess and could use some cleaning up
        public static void ProcessDeploymentPayloads(IEnumerable<DeploymentPayload> deploymentPayloads, IUpdateCoordinator updateCoordinator,
            IRemoteFileDownloader remoteFileDownloader, IFileNetworkConnection fileNetworkConnection = null,
            WebClient webClient = null)
        {

            foreach (var payload in deploymentPayloads)
            {
                if (IsAddInDeployed(payload.Destination))
                {
                    // get deployed version from add-in manifest file
                    var deployedAddInManifestFilePath = payload.GetAddInManifestFilePath();

                    var deployedAddInVersion = ManifestSerialization.DeserializeManifestFile<AddIn>(deployedAddInManifestFilePath).Identity.Version;

                    System.Diagnostics.Debug.WriteLine($"Deployed Version: {deployedAddInVersion}");

                    var updateQueryInfoManifestFilePath = payload.GetUpdateQueryInfoManifestFilePath();

                    var checkedUpdate = GetCheckedUpdate(payload, deployedAddInVersion, DateTime.UtcNow);

                    UpdateQueryInfo existingUpdateQueryInfo = null;
                    if (File.Exists(updateQueryInfoManifestFilePath))
                    {
                        existingUpdateQueryInfo = ManifestSerialization.DeserializeManifestFile<UpdateQueryInfo>(updateQueryInfoManifestFilePath);
                    }

                    checkedUpdate.Info.FirstNotified = existingUpdateQueryInfo?.FirstNotified;

                    System.Diagnostics.Debug.WriteLine($"Available Version: {checkedUpdate.Info.AvailableVersion}");

                    if (UpdateService.CanProceedWithUpdate(checkedUpdate, updateCoordinator))
                    {
                        ProcessUpdate(checkedUpdate, updateCoordinator, remoteFileDownloader, fileNetworkConnection, webClient);
                    }
                    else
                    {
                        // If UpdateService.CanProceedWithUpdate == false then this means the user
                        // declined a mandatory update, so we DO NOT want to load the add-in
                        if (!checkedUpdate.Info.IsMandatoryUpdate)
                        {
                            UpdateService.LoadOrInstallAddIn(payload, updateCoordinator);
                        }
                    }

                    Serialization.SerializeToXmlFile(checkedUpdate.Info, updateQueryInfoManifestFilePath);
                }
                else
                {
                    DeployAddIn(payload, updateCoordinator, remoteFileDownloader, fileNetworkConnection, webClient);
                }
            }
        }

        public static void ProcessUpdate(CheckedUpdate update, IUpdateCoordinator updateCoordinator, IRemoteFileDownloader remoteFileDownloader,
            IFileNetworkConnection fileNetworkConnection = null, WebClient webClient = null)
        {
            var payload = update.Payload;

            ValidateDeploymentBasis(payload);
            ValidateOfficeBitnessAndOsRequirements(payload);
            ValidateCompatibleFrameworks(payload, _installedClrAndNetFrameworks);

            switch (payload.FileHost.HostType)
            {
                case FileHostType.FileServer:
                    if (payload.FileHost.RequiresAuthentication)
                    {
                        if (fileNetworkConnection == null)
                        {
                            throw new ArgumentNullException(Common.GetFormatedErrorMessage($"Attempting to process update for {payload.AddIn.Identity.Title}.",
                                $"The {nameof(fileNetworkConnection)} is null, but it is required.",
                                $"Supply a valid {nameof(fileNetworkConnection)}."));
                        }

                        if (fileNetworkConnection.State == FileNetworkConnectionState.Closed)
                            fileNetworkConnection.Open();
                    }

                    UpdateService.StageUpdate(payload, updateCoordinator);

                    try
                    {
                        UpdateService.DownloadAddInFromFileServer(payload, updateCoordinator, remoteFileDownloader);

                        // delete staged temp directory
                        if (Directory.Exists(payload.Destination.TempDirectory))
                        {
                            Directory.Delete(payload.Destination.TempDirectory, true);
                        }

                        FinalizeDeploymentAndNotifyUser(payload, updateCoordinator, false);
                    }
                    catch (Exception ex)
                    {
                        // add logging functionality
                        UpdateService.RevertToOldAddIn(payload, updateCoordinator);
                        throw;
                    }

                    break;

                case FileHostType.WebServer:
                    if (webClient == null)
                    {
                        throw new ArgumentNullException(Common.GetFormatedErrorMessage($"Attempting to process update for {payload.AddIn.Identity.Title}.",
                            $"The {nameof(webClient)} is null, but it is required.",
                            $"Supply a valid {nameof(webClient)}."));
                    }

                    if (payload.FileHost.RequiresAuthentication)
                    {
                        if (webClient.Credentials == null)
                        {
                            throw new InvalidOperationException(Common.GetFormatedErrorMessage($"Updating add-in titled {payload.AddIn.Identity.Title} from a protected web server.",
                                $"The {nameof(webClient.Credentials)} property of the {nameof(webClient)} is null.",
                                $"Supply a valid instance of {webClient.Credentials.GetType().Name} to the {nameof(webClient)}."));
                        }
                    }

                    UpdateService.StageUpdate(payload, updateCoordinator);

                    try
                    {
                        UpdateService.DownloadAddInFromWebServer(payload, updateCoordinator, remoteFileDownloader, webClient);

                        // delete staged temp directory
                        if (Directory.Exists(payload.Destination.TempDirectory))
                        {
                            Directory.Delete(payload.Destination.TempDirectory, true);
                        }

                        FinalizeDeploymentAndNotifyUser(payload, updateCoordinator, false);
                    }
                    catch (Exception ex)
                    {
                        // add logging functionality
                        UpdateService.RevertToOldAddIn(payload, updateCoordinator);
                        throw;
                    }

                    break;
            }

            if (payload.Deployment.Settings.UpdateBehavior.RemoveDeprecatedVersion)
            {
                DeleteDeprecatedVersionFiles(update);
            }
        }

        // need to check if is deployed first
        public static CheckedUpdate GetCheckedUpdate(DeploymentPayload deploymentPayload, System.Version deployedAddInVersion, DateTime checkedDate)
        {
            UpdateQueryInfo updateQueryInfo = new UpdateQueryInfo
            {
                LastChecked = checkedDate,
                UpdateAvailable = UpdateService.IsNewVersionAvailable(deployedAddInVersion, deploymentPayload.AddIn.Identity.Version),
                AvailableVersion = deploymentPayload.AddIn.Identity.Version,
                MinimumRequiredVersion = deploymentPayload.Deployment.Settings.MinimumRequiredVersion,
                DeployedVersion = deployedAddInVersion,
                IsMandatoryUpdate = UpdateService.IsMandatoryUpdate(deployedAddInVersion, deploymentPayload.Deployment.Settings),
                IsRestartRequired = UpdateService.IsRestartRequired(deploymentPayload),

                Size = deploymentPayload.AddIn.GetTotalSize(),
            };

            return new CheckedUpdate(updateQueryInfo, deploymentPayload);
        }

        // should this also check for the add-in manifest??
        public static bool IsAddInDeployed(DeploymentDestination deploymentDestination)
        {
            return File.Exists(deploymentDestination.AddInPath);
        }

        #region PrivateMethods
        private static DeploymentPayload GetDeploymentPayload(PublishedDeployment publishedDeployment,
                      IFileNetworkConnection fileNetworkConnection = null, WebClient webClient = null)
        {
            Deployment deployment = null;
            AddIn addIn = null;
            string addInSchemaLocation = null;
            switch (publishedDeployment.FileHost.HostType)
            {
                case FileHostType.FileServer:
                    if (publishedDeployment.FileHost.RequiresAuthentication)
                    {
                        if (fileNetworkConnection == null)
                        {
                            throw new ArgumentNullException(Common.GetFormatedErrorMessage($"Attempting to retrieve a {nameof(DeploymentPayload)}.",
                                $"The {nameof(fileNetworkConnection)} is null, but it is required.",
                                $"Supply a valid {nameof(fileNetworkConnection)}."));
                        }

                        if (fileNetworkConnection.State == FileNetworkConnectionState.Closed)
                            fileNetworkConnection.Open();

                        deployment = GetDeploymentManifest(publishedDeployment.ManifestUri.LocalPath);
                        addIn = GetAddInManifest(deployment.AddInUri.LocalPath);
                        addInSchemaLocation = Serialization.GetSchemaLocationFromXmlFile(deployment.AddInUri.LocalPath);

                        if (fileNetworkConnection.State == FileNetworkConnectionState.Open)
                            fileNetworkConnection.Close();
                    }
                    else
                    {
                        deployment = GetDeploymentManifest(publishedDeployment.ManifestUri.LocalPath);
                        addIn = GetAddInManifest(deployment.AddInUri.LocalPath);
                        addInSchemaLocation = Serialization.GetSchemaLocationFromXmlFile(deployment.AddInUri.LocalPath);
                    }
                    break;

                case FileHostType.WebServer:
                    if (webClient == null)
                    {
                        throw new ArgumentNullException(Common.GetFormatedErrorMessage($"Attempting to retrieve a {nameof(DeploymentPayload)} from a web server.",
                            $"The {nameof(webClient)} is null, but it is required.",
                            $"Supply a valid {nameof(webClient)}."));
                    }

                    if (publishedDeployment.FileHost.RequiresAuthentication)
                    {
                        if (webClient.Credentials == null)
                            throw new InvalidOperationException(Common.GetFormatedErrorMessage($"Attempting to retrieve {nameof(DeploymentPayload)} from a protected web server.",
                                $"The credentials property of the {nameof(webClient)} is null.",
                                $"Supply a valid instance of {nameof(ICredentials)} to the {nameof(webClient)}."));
                    }

                    deployment = GetDeploymentManifest(webClient, publishedDeployment.ManifestUri);
                    addIn = GetAddInManifest(webClient, deployment.AddInUri);
                    addInSchemaLocation = Serialization.GetSchemaLocationFromXmlFile(webClient, deployment.AddInUri);
                    break;
            }

            return new DeploymentPayload(publishedDeployment.FileHost, deployment, addIn, addInSchemaLocation);
        }

        private static void DeployAddIn(DeploymentPayload deploymentPayload, IUpdateCoordinator updateCoordinator, IRemoteFileDownloader remoteFileDownloader,
            IFileNetworkConnection fileNetworkConnection = null, WebClient webClient = null)
        {
            ValidateDeploymentBasis(deploymentPayload);
            ValidateOfficeBitnessAndOsRequirements(deploymentPayload);
            ValidateCompatibleFrameworks(deploymentPayload, _installedClrAndNetFrameworks);

            switch (deploymentPayload.FileHost.HostType)
            {
                case FileHostType.FileServer:
                    if (deploymentPayload.FileHost.RequiresAuthentication)
                    {
                        if (fileNetworkConnection == null)
                        {
                            throw new ArgumentNullException(Common.GetFormatedErrorMessage($"Attempting to Deploy update for {deploymentPayload.AddIn.Identity.Title}.",
                                $"The {nameof(fileNetworkConnection)} is null, but it is required.",
                                $"Supply a valid {nameof(fileNetworkConnection)}."));
                        }

                        if (fileNetworkConnection.State == FileNetworkConnectionState.Closed)
                            fileNetworkConnection.Open();
                    }

                    UpdateService.DownloadAddInFromFileServer(deploymentPayload, updateCoordinator, remoteFileDownloader);

                    FinalizeDeploymentAndNotifyUser(deploymentPayload, updateCoordinator, true);

                    break;

                case FileHostType.WebServer:
                    if (webClient == null)
                    {
                        throw new ArgumentNullException(Common.GetFormatedErrorMessage($"Attempting to Deploy update for {deploymentPayload.AddIn.Identity.Title}.",
                            $"The {nameof(webClient)} is null, but it is required.",
                            $"Supply a valid {nameof(webClient)}."));
                    }

                    if (deploymentPayload.FileHost.RequiresAuthentication)
                    {
                        if (webClient.Credentials == null)
                            throw new InvalidOperationException(Common.GetFormatedErrorMessage($"Deploying add-in titled {deploymentPayload.AddIn.Identity.Title} from a protected web server.",
                                $"The {nameof(webClient.Credentials)} property of the {nameof(webClient)} is null.",
                                $"Supply a valid instance of {webClient.Credentials.GetType().Name} to the {nameof(webClient)}."));
                    }

                    UpdateService.DownloadAddInFromWebServer(deploymentPayload, updateCoordinator, remoteFileDownloader, webClient);

                    FinalizeDeploymentAndNotifyUser(deploymentPayload, updateCoordinator, true);

                    break;
            }
        }

        private static void FinalizeDeploymentAndNotifyUser(DeploymentPayload deploymentPayload, IUpdateCoordinator updateCoordinator, bool isInitialDeployment)
        {
            var addInManifestFilePath = deploymentPayload.GetAddInManifestFilePath();

            // overwrites existing file
            Serialization.SerializeToXmlFile(deploymentPayload.AddIn, addInManifestFilePath, true);

            Serialization.AddSchemaLocationToXmlFile(addInManifestFilePath, new Uri(deploymentPayload.AddInSchemaLocation));

            if (isInitialDeployment)
            {
                // may need to restart here if UpdateService.IsRestartRequired(deploymentPayload)==true, not sure
                UpdateService.LoadOrInstallAddIn(deploymentPayload, updateCoordinator);
            }
            else
            {
                if (UpdateService.IsRestartRequired(deploymentPayload))
                {
                    Common.DisplayMessage($"Update Deployment Complete!{Environment.NewLine}{Environment.NewLine}Excel will now shutdown. " +
                            $"The next time you open Excel, the new version of the {deploymentPayload.AddIn.Identity.Title} add-in will be available for use.", string.Empty, System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);

                    InteropIntegration.CloseExcelApp();
                }
                else
                {
                    UpdateService.LoadOrInstallAddIn(deploymentPayload, updateCoordinator);

                    Common.DisplayMessage($"Update Deployment Complete!{Environment.NewLine}{Environment.NewLine}" +
                            $"The {deploymentPayload.AddIn.Identity.Title} add-in is available for use.", string.Empty, System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                }
            }
        }

        // account for readonly files here??
        private static void DeleteDeprecatedVersionFiles(CheckedUpdate update)
        {
            var deprecatedVersionDirectory = Path.Combine(update.Payload.Destination.ParentDirectory, update.Info.DeployedVersion.ToString());
            if (Directory.Exists(deprecatedVersionDirectory))
            {
                Directory.Delete(deprecatedVersionDirectory, true);
            }
        }

        private static void ValidateDeploymentBasis(DeploymentPayload deploymentPayload)
        {
            if (deploymentPayload.Deployment.Settings.DeploymentBasis == DeploymentBasis.PerMachine)
            {
                if (!ClientSystemDetection.IsWindowsAdmin())
                {
                    throw new UnauthorizedAccessException(Common.GetFormatedErrorMessage($"Deploying add-in titled {deploymentPayload.AddIn.Identity.Title} to client.",
                        $"Insufficient permissions for {nameof(DeploymentBasis.PerMachine)} deployment.",
                        $"Deploy on nameof{DeploymentBasis.PerUser} basis or elevate user permissions to admin."));
                }
            }
        }

        private static void ValidateOfficeBitnessAndOsRequirements(DeploymentPayload deploymentPayload)
        {
            var requiredOs = deploymentPayload.Deployment.RequiredOperatingSystem;
            var osBitness = ClientSystemDetection.GetOsBitness();
            var msOfficeBitness = ClientSystemDetection.GetMicrosoftOfficeBitness();

            var addInTitle = deploymentPayload.AddIn.Identity.Title;

            if (deploymentPayload.Deployment.TargetOfficeInstallation != msOfficeBitness)
            {
                throw new PlatformNotSupportedException(Common.GetFormatedErrorMessage($"Deploying add-in titled {addInTitle} to client.",
                    $"The {nameof(deploymentPayload.Deployment.TargetOfficeInstallation)}is  not correct.",
                    $"The {nameof(deploymentPayload.Deployment.TargetOfficeInstallation)} should be {Enum.GetName(typeof(MicrosoftOfficeBitness), msOfficeBitness)}."));
            }

            if (requiredOs.Bitness != osBitness)
            {
                throw new PlatformNotSupportedException(Common.GetFormatedErrorMessage($"Deploying add-in titled {addInTitle} to client.",
                    $"The {nameof(requiredOs.Bitness)} is not correct.",
                    $"The {nameof(requiredOs.Bitness)} should be {Enum.GetName(typeof(OperatingSystemBitness), osBitness)}."));
            }

            var osVersion = ClientSystemDetection.GetOsVersion();

            // if osVersion preceeds requiredOs.MinimumVersion
            if (osVersion.CompareTo(requiredOs.MinimumVersion) < 0)
            {
                throw new PlatformNotSupportedException(Common.GetFormatedErrorMessage($"Deploying add-in titled {addInTitle} to client.",
                    $"The {nameof(requiredOs.MinimumVersion)} (i.e. {osVersion}) is not correct.",
                    $"The {nameof(requiredOs.MinimumVersion)} should be {requiredOs.MinimumVersion}."));
            }

            var os = Environment.OSVersion;

            if (os.Platform != requiredOs.PlatformId)
            {
                throw new PlatformNotSupportedException(Common.GetFormatedErrorMessage($"Deploying add-in titled {addInTitle} to client.",
                    $"The {nameof(OperatingSystemBitness)} is not correct.",
                    $"The {nameof(requiredOs.Bitness)} should be {Enum.GetName(typeof(OperatingSystemBitness), osBitness)}."));
            }
        }

        private static void ValidateCompatibleFrameworks(DeploymentPayload deploymentPayload, IDictionary<NetClrVersion, HashSet<System.Version>> installedClrAndNetFrameworks)
        {
            var compatibleFrameworks = deploymentPayload.Deployment.CompatibleFrameworks;

            var addInTitle = deploymentPayload.AddIn.Identity.Title;

            int foundCount = 0;
            foreach (var framework in compatibleFrameworks)
            {
                if (installedClrAndNetFrameworks.TryGetValue(framework.SupportedRuntime, out HashSet<System.Version> versions))
                {
                    System.Diagnostics.Debug.WriteLine($"{System.Environment.NewLine}Target Version: {framework.TargetVersion}");

                    int minimumRequiredVersionCount = 0;
                    foreach (var vers in versions)
                    {
                        System.Diagnostics.Debug.WriteLine($"Installed Frameworks:");

                        System.Diagnostics.Debug.WriteLine($"\t{vers}");

                        if (framework.Required && framework.MinimumRequiredVersion.CompareTo(vers) >= 0)
                        {
                            minimumRequiredVersionCount++;
                            foundCount++;
                        }
                    }

                    if (framework.Required && minimumRequiredVersionCount == 0)
                    {
                        throw new PlatformNotSupportedException(Common.GetFormatedErrorMessage($"Deploying add-in titled {addInTitle} to client.",
$"The {nameof(framework.MinimumRequiredVersion)} could not be found.",
$"Supply the correct {nameof(framework.MinimumRequiredVersion)}."));
                    }
                }
            }

            if (foundCount == 0)
            {
                throw new PlatformNotSupportedException(Common.GetFormatedErrorMessage($"Deploying add-in titled {addInTitle} to client.",
                    $"None of the supplied {nameof(CompatibleFramework)}s could not be found on the client machine.",
                    $"Supply one or more {nameof(CompatibleFramework)}s."));
            }
        }

        private static Deployment GetDeploymentManifest(string filePath)
        {
            return ManifestSerialization.DeserializeManifestFile<Deployment>(filePath);
        }

        private static Deployment GetDeploymentManifest(WebClient webClient, Uri uri)
        {
            return ManifestSerialization.DeserializeManifestFile<Deployment>(webClient, uri);
        }

        private static AddIn GetAddInManifest(string filePath)
        {
            return ManifestSerialization.DeserializeManifestFile<AddIn>(filePath);
        }

        private static AddIn GetAddInManifest(WebClient webClient, Uri uri)
        {
            return ManifestSerialization.DeserializeManifestFile<AddIn>(webClient, uri);
        }
        #endregion
    }
}
