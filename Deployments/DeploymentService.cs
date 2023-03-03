using XLAutoDeploy.FileSystem.Access;
using XLAutoDeploy.Updates;

using XLAutoDeploy.Manifests;
using XLAutoDeploy.Manifests.Utilities;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Linq;

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
                return ManifestSerialization.DeserializeManifestFile<DeploymentRegistry>(uri.AsString());
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

 
        public static void ProcessDeploymentPayloads(IEnumerable<DeploymentPayload> deploymentPayloads, IUpdateCoordinator updateCoordinator,
            IRemoteFileDownloader remoteFileDownloader, IFileNetworkConnection fileNetworkConnection = null,
            WebClient webClient = null)
        {

            foreach (var payload in deploymentPayloads)
            {
                if (IsAddInDeployed(payload.Destination))
                {
                    // get deployed version from add-in manifest file
                    var deployedAddInManifestFilePath = DeployedFileUtilities.GetAddInManifestFilePath(payload);

                    var deployedAddInVersion = ManifestSerialization.DeserializeManifestFile<AddIn>(deployedAddInManifestFilePath).Identity.Version;

                    // get the incoming update
                    CheckedUpdate checkedUpdate = GetUpdate(payload, deployedAddInVersion);

                    ProcessUpdate(checkedUpdate, updateCoordinator, remoteFileDownloader, fileNetworkConnection, webClient);
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

                        UpdateService.UpdateAddIn(payload, updateCoordinator, remoteFileDownloader, fileNetworkConnection);
                    }
                    else
                    {
                        UpdateService.UpdateAddIn(payload, updateCoordinator, remoteFileDownloader);
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

                    UpdateService.UpdateAddIn(payload, updateCoordinator, remoteFileDownloader, webClient);

                    break;
            }

            if (payload.Deployment.Settings.UpdateBehavior.RemoveDeprecatedVersion)
            {
                DeleteDeprecatedVersionFiles(update);
            }

            // Save UpdateQueryInfo
            var updateQueryInfoFilePath = DeployedFileUtilities.GetUpdateQueryInfoManifestFilePath(payload);

            XmlConversion.SerializeToXmlFile(update.Info, updateQueryInfoFilePath);
        }

        // need to check if is deployed first
        public static CheckedUpdate GetUpdate(DeploymentPayload deploymentPayload, System.Version deployedAddInVersion)
        {
            var now = DateTime.UtcNow;
            UpdateQueryInfo updateQueryInfo = new UpdateQueryInfo
            {
                UpdateAvailable = UpdateService.IsNewVersionAvailable(deployedAddInVersion, deploymentPayload.AddIn.Identity.Version),
                AvailableVersion = deploymentPayload.AddIn.Identity.Version,
                MinimumRequiredVersion = deploymentPayload.Deployment.Settings.MinimumRequiredVersion,
                DeployedVersion = deployedAddInVersion,
                IsMandatoryUpdate = UpdateService.IsMandatoryUpdate(deployedAddInVersion, deploymentPayload.Deployment.Settings),
                IsRestartRequired = UpdateService.IsRestartRequired(deploymentPayload),

                Size = deploymentPayload.AddIn.GetTotalSize(),

                LastChecked = now
            };

            if (deploymentPayload.AddIn.Identity.Version.CompareTo(deployedAddInVersion) != 0)
            {
                updateQueryInfo.FirstNotified = now;
                updateQueryInfo.LastNotified = now;
            }
            else
            {
                if (UpdateService.PersistedUpdateQueryInfoExists(deploymentPayload))
                {
                    var persistedUpdateQueryInfoFilePath = DeployedFileUtilities.GetUpdateQueryInfoManifestFilePath(deploymentPayload);

                    var persistedUpdateQueryInfo = ManifestSerialization.DeserializeManifestFile<UpdateQueryInfo>(persistedUpdateQueryInfoFilePath);

                    updateQueryInfo.FirstNotified = persistedUpdateQueryInfo.FirstNotified;
                    updateQueryInfo.LastNotified = persistedUpdateQueryInfo.LastNotified;
                }
                else
                {
                    updateQueryInfo.FirstNotified = DateTime.MinValue;
                    updateQueryInfo.LastNotified = DateTime.MinValue;
                }
            }

            return new CheckedUpdate(updateQueryInfo, deploymentPayload);
        }

        public static void SetRealtimeUpdateMonitoring(IEnumerable<DeploymentPayload> deploymentPayloads, IUpdateCoordinator updateCoordinator,
            IRemoteFileDownloader remoteFileDownloader, out UpdateMonitor updateMonitor)
        {
            updateMonitor = null;

            if (deploymentPayloads.Where(d => d.FileHost.HostType == FileHostType.FileServer & d.Deployment.Settings.UpdateBehavior.DoInRealTime)?.Any() == true)
            {
                updateMonitor = UpdateMonitorFactory.Create(deploymentPayloads, updateCoordinator, remoteFileDownloader, 1);
            }
        }

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
                    }

                    if (fileNetworkConnection.State == FileNetworkConnectionState.Closed)
                        fileNetworkConnection.Open();

                    deployment = GetDeploymentManifest(publishedDeployment.ManifestUri.AsString());
                    addIn = GetAddInManifest(deployment.AddInUri.AsString());

                    if (fileNetworkConnection.State == FileNetworkConnectionState.Open)
                        fileNetworkConnection.Close();

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
                    break;
            }

            return new DeploymentPayload(publishedDeployment.FileHost, deployment, addIn);
        }

        private static void DeployAddIn(DeploymentPayload deploymentPayload, IUpdateCoordinator updateCoordinator, IRemoteFileDownloader remoteFileDownloader,
            IFileNetworkConnection fileNetworkConnection = null, WebClient webClient = null)
        {
            ValidateDeploymentBasis(deploymentPayload);
            ValidateOfficeBitnessAndOsRequirements(deploymentPayload);
            ValidateCompatibleFrameworks(deploymentPayload, _installedClrAndNetFrameworks);

            var addInManifestFilePath = DeployedFileUtilities.GetAddInManifestFilePath(deploymentPayload);

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

                    UpdateService.DownloadAddInFromFileServer(deploymentPayload, updateCoordinator, remoteFileDownloader, addInManifestFilePath);

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

                    UpdateService.DownloadAddInFromWebServer(deploymentPayload, updateCoordinator, remoteFileDownloader, webClient, addInManifestFilePath);

                    break;
            }
        }

        private static void DeleteDeprecatedVersionFiles(CheckedUpdate update)
        {
            var deprecatedVersionDirectory = Path.Combine(update.Payload.Destination.ParentDirectory, update.Info.DeployedVersion.ToString());
            if (Directory.Exists(deprecatedVersionDirectory))
            {
                Directory.Delete(deprecatedVersionDirectory);
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

            var os = Environment.OSVersion;

            if (os.Platform != requiredOs.PlatformId)
            {
                throw new PlatformNotSupportedException(Common.GetFormatedErrorMessage($"Deploying add-in titled {addInTitle} to client.",
                    $"The {nameof(OperatingSystemBitness)} is not correct.",
                    $"The {nameof(requiredOs.Bitness)} should be {Enum.GetName(typeof(OperatingSystemBitness), osBitness)}."));
            }

            //must be exact version
            if (os.Version.CompareTo(requiredOs.Version) != 0)
            {
                throw new PlatformNotSupportedException(Common.GetFormatedErrorMessage($"Deploying add-in titled {addInTitle} to client.",
                    $"The {nameof(requiredOs.Version)} is not correct.",
                    $"The {nameof(requiredOs.Version)} should be {requiredOs.Version}."));
            }
        }

        private static void ValidateCompatibleFrameworks(DeploymentPayload deploymentPayload, IDictionary<NetClrVersion, HashSet<System.Version>> installedClrAndNetFrameworks)
        {
            var compatibleFrameworks = deploymentPayload.Deployment.CompatibleFrameworks;

            var addInTitle = deploymentPayload.AddIn.Identity.Title;

            foreach (var framework in compatibleFrameworks)
            {
                if (installedClrAndNetFrameworks.TryGetValue(framework.SupportedRuntime, out HashSet<System.Version> versions))
                {
                    if (!versions.Contains(framework.TargetVersion))
                    {
                        throw new PlatformNotSupportedException(Common.GetFormatedErrorMessage($"Deploying add-in titled {addInTitle} to client.",
                            $"The {nameof(framework.TargetVersion)} could not be found.",
                            $"Supply the correct {nameof(framework.TargetVersion)}."));
                    }
                }
                else
                {
                    throw new PlatformNotSupportedException(Common.GetFormatedErrorMessage($"Deploying add-in titled {addInTitle} to client.",
                        $"The {nameof(framework.SupportedRuntime)} could not be found.",
                        $"Supply the correct {nameof(framework.TargetVersion)}."));
                }
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
