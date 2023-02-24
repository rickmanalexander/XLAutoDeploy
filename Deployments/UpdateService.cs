﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;

using XLAutoDeploy.Manifests;
using XLAutoDeploy.Manifests.Utilities;

using XLAutoDeploy.FileSystem.Access;

using XLAutoDeploy.Updates;

namespace XLAutoDeploy.Deployments
{
    /// <summary>
    /// A set of methods for automatically deploying and updating Excel add-ins on client machines using 
    /// configuration(s) defined in an instances of the <see cref="DeploymentPayload"/>'s.
    /// </summary>
    public static class UpdateService
    {
        public const string AddInManifestParameterizedFileName = "{0}-AddIn.manifest.xml";
        public const string UpdateQueryInfoManifestParameterizedFileName = "{0}-UpdateQueryInfo.manifest.xml";

        public static void UpdateAddIn(DeploymentPayload deploymentPayload, IUpdateCoordinator updateService, IRemoteFileDownloader fileDownloader) =>
            UpdateAddInFromFileServer(deploymentPayload, updateService, fileDownloader);

        public static void UpdateAddIn(DeploymentPayload deploymentPayload, IUpdateCoordinator updateService, IRemoteFileDownloader fileDownloader,
            IFileNetworkConnection fileNetworkConnection) => UpdateAddInFromProtectedFileServer(deploymentPayload, updateService, fileDownloader, fileNetworkConnection);

        public static void UpdateAddIn(DeploymentPayload deploymentPayload, IUpdateCoordinator updateService, IRemoteFileDownloader fileDownloader, WebClient webClient) =>
            UpdateAddInFromWebServer(deploymentPayload, updateService, fileDownloader, webClient);


        #region ImplAutoUpdateMethods
        private static void UpdateAddInFromProtectedFileServer(DeploymentPayload deploymentPayload, IUpdateCoordinator updateService, IRemoteFileDownloader fileDownloader,
            IFileNetworkConnection fileNetworkConnection)
        {
            if (!(deploymentPayload.FileHost.HostType == FileHostType.FileServer & deploymentPayload.FileHost.RequiresAuthentication))
            {
                throw new InvalidOperationException(Common.GetFormatedErrorMessage($"Updating add-in titled {deploymentPayload.AddIn.Identity.Title} from protected file server.",
                    $"The {nameof(deploymentPayload.FileHost)} for the DeploymentPayload is not correct.",
                    $"The {nameof(deploymentPayload.FileHost.HostType)} should be {nameof(FileHostType.FileServer)} and {nameof(deploymentPayload.FileHost.RequiresAuthentication)} should be set to true."));
            }

            if (fileNetworkConnection == null)
            {
                throw new ArgumentNullException(Common.GetFormatedErrorMessage($"Updating add-in titled {deploymentPayload.AddIn.Identity.Title} from protected file server.",
                    $"The {nameof(fileNetworkConnection)} parameter is null.",
                    $"Supply a valid instance of a {fileNetworkConnection.GetType().Name}."));
            }

            try
            {
                if (fileNetworkConnection.State == FileNetworkConnectionState.Closed)
                    fileNetworkConnection.Open();

                UpdateAddInFromFileServer(deploymentPayload, updateService, fileDownloader);
            }
            catch
            {
                throw;
            }
        }

        private static void UpdateAddInFromFileServer(DeploymentPayload deploymentPayload, IUpdateCoordinator updateService, IRemoteFileDownloader fileDownloader)
        {
            if (deploymentPayload.Deployment.RequiredOperatingSystem.Bitness != ClientSystemDetection.GetOsBitness())
            {
                throw new InvalidOperationException(Common.GetFormatedErrorMessage($"Updating add-in titled {deploymentPayload.AddIn.Identity.Title} from a file server.",
                    $"The {nameof(deploymentPayload.Deployment.RequiredOperatingSystem.Bitness)} for the DeploymentPayload is not correct.",
                    $"The {nameof(deploymentPayload.Deployment.RequiredOperatingSystem.Bitness)} should be {Enum.GetName(typeof(OperatingSystemBitness), ClientSystemDetection.GetOsBitness())}."));
            }

            if (deploymentPayload.FileHost.HostType != FileHostType.FileServer)
            {
                throw new InvalidOperationException(Common.GetFormatedErrorMessage($"Updating add-in titled {deploymentPayload.AddIn.Identity.Title} from a file server.",
                    $"The {nameof(deploymentPayload.FileHost)} for the DeploymentPayload is not correct.",
                    $"The {nameof(deploymentPayload.FileHost.HostType)} should be {nameof(FileHostType.FileServer)}."));
            }

            try
            {
                var addInManifestTargetFilePath = GetAddInManifestFilePath(deploymentPayload);

                UpdateAddInFromFileServerImpl(deploymentPayload, updateService, fileDownloader, addInManifestTargetFilePath);
            }
            catch
            {
                throw;
            }
        }

        private static void UpdateAddInFromWebServer(DeploymentPayload deploymentPayload, IUpdateCoordinator updateService, IRemoteFileDownloader fileDownloader,
            WebClient webClient)
        {
            if (deploymentPayload.Deployment.RequiredOperatingSystem.Bitness != ClientSystemDetection.GetOsBitness())
            {
                throw new InvalidOperationException(Common.GetFormatedErrorMessage($"Updating add-in titled {deploymentPayload.AddIn.Identity.Title} from a web server.",
                    $"The {nameof(deploymentPayload.Deployment.RequiredOperatingSystem.Bitness)} for the DeploymentPayload is not correct.",
                    $"The {nameof(deploymentPayload.Deployment.RequiredOperatingSystem.Bitness)} should be {Enum.GetName(typeof(OperatingSystemBitness), ClientSystemDetection.GetOsBitness())}."));
            }

            if (deploymentPayload.FileHost.HostType != FileHostType.WebServer)
            {
                throw new InvalidOperationException(Common.GetFormatedErrorMessage($"Updating add-in titled {deploymentPayload.AddIn.Identity.Title} from a web server.",
                    $"The {nameof(deploymentPayload.FileHost)} for the DeploymentPayload is not correct.",
                    $"The {nameof(deploymentPayload.FileHost.HostType)} should be {nameof(FileHostType.WebServer)}."));
            }

            if (webClient == null)
            {
                throw new ArgumentNullException(Common.GetFormatedErrorMessage($"Updating add-in titled {deploymentPayload.AddIn.Identity.Title} from a web server.",
                    $"The {nameof(webClient)} parameter is null.",
                    $"Supply a valid instance of a {webClient.GetType().Name}."));
            }

            if (deploymentPayload.FileHost.RequiresAuthentication)
            {
                if (webClient.Credentials == null)
                    throw new InvalidOperationException(Common.GetFormatedErrorMessage($"Updating add-in titled {deploymentPayload.AddIn.Identity.Title} from a protected web server.",
                        $"The {nameof(webClient.Credentials)} property of the {nameof(webClient)} is null.",
                        $"Supply a valid instance of {webClient.Credentials.GetType().Name} to the {nameof(webClient)}."));
            }

            var addInManifestTargetFilePath = GetAddInManifestFilePath(deploymentPayload);

            try
            {
                UpdateAddInFromWebServerImpl(deploymentPayload, updateService, fileDownloader, webClient, addInManifestTargetFilePath);
            }
            catch
            {
                throw;
            }
        }

        private static void UpdateAddInFromFileServerImpl(DeploymentPayload deploymentPayload, IUpdateCoordinator updateService, IRemoteFileDownloader fileDownloader, string addInManifestFilePath)
        {
            UnLoadOrUnInstallAddIn(deploymentPayload, updateService);

            var targetFilePath = deploymentPayload.Destination.AddInPath;
            var tempFilePath = deploymentPayload.Destination.TempAddInPath;

            //If an addin is installed, then the physical file will be locked by Excel. 
            //To update a locked addin file, do the following:
            //1. Unload/Uninstall addin  
            //2. Move local addin to a temp file path (this way it can be retrived in case of error).

            //Delete existing temp file
            if (File.Exists(tempFilePath))
                File.Delete(tempFilePath);

            //File should now be unlocked, so the FileCopy/FileDelete operations
            //that occur as a result of File.Move() will succeed
            File.Move(targetFilePath, tempFilePath);

            try
            {
                DownloadAddInFromFileServer(deploymentPayload, updateService, fileDownloader, addInManifestFilePath);
            }
            catch
            {
                //Re-Load/Install original addin
                if (File.Exists(tempFilePath))
                {
                    if (File.Exists(targetFilePath))
                        File.Delete(targetFilePath);

                    File.Move(tempFilePath, targetFilePath);

                    LoadOrInstallAddIn(deploymentPayload, updateService);
                }

                throw;
            }
        }

        private static void UpdateAddInFromWebServerImpl(DeploymentPayload deploymentPayload, IUpdateCoordinator updateService, IRemoteFileDownloader fileDownloader, WebClient webClient,
            string addInManifestFilePath)
        {
            UnLoadOrUnInstallAddIn(deploymentPayload, updateService);

            var targetFilePath = deploymentPayload.Destination.AddInPath;
            var tempFilePath = deploymentPayload.Destination.TempAddInPath;

            //If an addin is installed, then the physical file will be locked by Excel. 
            //To update a locked addin file, do the following:
            //1. Unload/Uninstall addin  
            //2. Move local addin to a temp file path (this way it can be retrived in case of error).

            //Delete existing temp file
            if (File.Exists(tempFilePath))
                File.Delete(tempFilePath);

            //File should now be unlocked, so the FileCopy/FileDelete operations
            //that occur as a result of File.Move() will succeed
            File.Move(targetFilePath, tempFilePath);

            try
            {
                DownloadAddInFromWebServer(deploymentPayload, updateService, fileDownloader, webClient, addInManifestFilePath);
            }
            catch
            {
                //Re-Load/Install original addin
                if (File.Exists(tempFilePath))
                {
                    if (File.Exists(targetFilePath))
                        File.Delete(targetFilePath);

                    File.Move(tempFilePath, targetFilePath);

                    LoadOrInstallAddIn(deploymentPayload, updateService);
                }

                throw;
            }
        }
        #endregion

        public static void DownloadAddInFromFileServer(DeploymentPayload deploymentPayload, IUpdateCoordinator updateService, IRemoteFileDownloader fileDownloader, string addInManifestFilePath)
        {
            var task = updateService.Deployer.DownloadAsync(fileDownloader, deploymentPayload.Deployment.AddInUriString, deploymentPayload.Destination.AddInPath);

            task.Wait();

            if (deploymentPayload.AddIn.Dependencies?.Any() == true)
            {
                foreach (var dependency in deploymentPayload.AddIn.Dependencies)
                {
                    string filePath = GetDependencyFilePath(dependency, deploymentPayload.Destination);

                    updateService.Deployer.Download(fileDownloader, dependency.UriString, filePath);

                    DownloadAssetFilesFromFileServer(dependency.AssetFiles, fileDownloader, deploymentPayload.Destination);
                }
            }

            DownloadAssetFilesFromFileServer(deploymentPayload.AddIn.AssetFiles, fileDownloader, deploymentPayload.Destination);

            LoadOrInstallAddIn(deploymentPayload, updateService);

            //overwrite existing file if found
            XmlConversion.SerializeToXmlFile<AddIn>(deploymentPayload.AddIn, addInManifestFilePath, true);
        }

        public static void DownloadAddInFromWebServer(DeploymentPayload deploymentPayload, IUpdateCoordinator updateService, IRemoteFileDownloader fileDownloader, WebClient webClient,
            string addInManifestFilePath)
        {
            var task = updateService.Deployer.DownloadAsync(fileDownloader, webClient, deploymentPayload.Deployment.AddInUriString, deploymentPayload.Destination.AddInPath);
            task.Wait();

            if (deploymentPayload.AddIn.Dependencies?.Any() == true)
            {
                foreach (var dependency in deploymentPayload.AddIn.Dependencies)
                {
                    string filePath = GetDependencyFilePath(dependency, deploymentPayload.Destination);

                    updateService.Deployer.Download(fileDownloader, webClient, dependency.Uri, filePath);

                    DownloadAssetFilesFromWebServer(dependency.AssetFiles, fileDownloader, webClient, deploymentPayload.Destination);
                }
            }

            DownloadAssetFilesFromWebServer(deploymentPayload.AddIn.AssetFiles, fileDownloader, webClient, deploymentPayload.Destination);

            LoadOrInstallAddIn(deploymentPayload, updateService);

            //overwrite existing file if found
            XmlConversion.SerializeToXmlFile<AddIn>(deploymentPayload.AddIn, addInManifestFilePath, true);
        }

        private static void DownloadAssetFilesFromWebServer(IEnumerable<AssetFile> assetFiles, IRemoteFileDownloader fileDownloader,
            WebClient webClient, UpdateDeploymentDestination destination)
        {
            if (assetFiles?.Any() == false)
                return;

            foreach (var file in assetFiles)
            {
                string targetFilePath = GetAssetFileFilePath(file, destination);

                if (file.Hash != null)
                {
                    var fileBytes = fileDownloader.DownloadBytes(webClient, file.Uri);

                    var expectedHash = System.Text.Encoding.UTF8.GetBytes(file.Hash.Value);

                    using (var algorithm = Manifests.DigSig.Hashing.CreateHashAlgorithm(file.Hash.Algorithm))
                    {
                        var actualHash = Manifests.DigSig.Hashing.ComputeHash(algorithm, fileBytes);

                        if (!FileSystem.Comparison.ByteArraysEqualUnsafe(expectedHash, actualHash))
                        {
                            throw new CryptographicException(Common.GetFormatedErrorMessage($"Attempting to verify the hash value for the {nameof(AssetFile)} named {file.Name}.",
                                "The hash value in the asset file does not match the actual hash of the file.",
                                "Update file with the correct hash or change the file."));
                        }

                        File.WriteAllBytes(targetFilePath, fileBytes);
                    }
                }
                else
                {
                    fileDownloader.Download(webClient, file.Uri, targetFilePath);
                }
            }
        }

        private static void DownloadAssetFilesFromFileServer(IEnumerable<AssetFile> assetFiles, IRemoteFileDownloader fileDownloader,
            UpdateDeploymentDestination destination)
        {
            if (assetFiles?.Any() == false)
                return;

            foreach (var file in assetFiles)
            {
                string targetFilePath = GetAssetFileFilePath(file, destination);

                if (file.Hash != null)
                {
                    var fileBytes = fileDownloader.DownloadBytes(file.Uri.AsString());

                    var expectedHash = System.Text.Encoding.UTF8.GetBytes(file.Hash.Value);

                    using (var algorithm = Manifests.DigSig.Hashing.CreateHashAlgorithm(file.Hash.Algorithm))
                    {
                        var actualHash = Manifests.DigSig.Hashing.ComputeHash(algorithm, fileBytes);

                        if (!FileSystem.Comparison.ByteArraysEqualUnsafe(expectedHash, actualHash))
                        {
                            throw new CryptographicException(Common.GetFormatedErrorMessage($"Attempting to verify the hash value for the {nameof(AssetFile)} named {file.Name}.",
                                "The hash value in the asset file does not match the actual hash of the file.",
                                "Update file with the correct hash or change the file."));
                        }

                        File.WriteAllBytes(targetFilePath, fileBytes);
                    }
                }
                else
                {
                    fileDownloader.Download(file.Uri.AsString(), targetFilePath);
                }
            }
        }

        public static void LoadOrInstallAddIn(DeploymentPayload deploymentPayload, IUpdateCoordinator updateService)
        {
            UnLoadOrUnInstallAddIn(deploymentPayload, updateService);

            if (deploymentPayload.Deployment.Settings.LoadBehavior.Install)
            {
                updateService.Installer.Install(deploymentPayload.AddIn.Identity.Title, deploymentPayload.Destination.AddInPath);
            }
            else
            {
                updateService.Loader.Load(deploymentPayload.Destination.AddInPath);
            }
        }

        public static void UnloadAddIns(IEnumerable<DeploymentPayload> deploymentPayloads, IUpdateCoordinator updateService)
        {
            foreach (var payload in deploymentPayloads)
            {
                if (!payload.Deployment.Settings.LoadBehavior.Install)
                {
                    UnLoadOrUnInstallAddIn(payload, updateService);
                }
            }
        }

        public static void UnLoadOrUnInstallAddIn(DeploymentPayload deploymentPayload, IUpdateCoordinator updateService)
        {
            updateService.Installer.Uninstall(deploymentPayload.Destination.AddInPath);

            updateService.Loader.Unload(deploymentPayload.Destination.AddInPath);
        }

        public static bool CanProceedWithUpdate(CheckedUpdate checkedUpdate, IUpdateCoordinator updateService)
        {
            var updateBehavior = checkedUpdate.Payload.Deployment.Settings.UpdateBehavior;

            if ((checkedUpdate.Info.IsMandatoryUpdate && checkedUpdate.Info.IsRestartRequired) 
                || updateBehavior.NotifyClient)
            {
                updateService.Notifier.Notify(checkedUpdate.GetDescription(),
                    checkedUpdate.Payload.Deployment.Description, checkedUpdate.Info,
                    !checkedUpdate.Info.IsMandatoryUpdate);

                return updateService.Notifier.DoUpdate;
            }

            // Silent Update
            return true;
        }

        public static bool IsAddInDeployed(UpdateDeploymentDestination deploymentDestination)
        {
            return File.Exists(deploymentDestination.AddInPath);
        }

        public static bool IsMandatoryUpdate(System.Version deployedAddInVersion, DeploymentSettings deploymentSettings)
        {
            return (deploymentSettings.UpdateBehavior.Mode == UpdateMode.Forced
                || IsNewVersionAvailable(deployedAddInVersion, deploymentSettings.MinimumRequiredVersion));
        }

        public static bool IsNewVersionAvailable(System.Version deployed, System.Version incoming)
        {
            return (deployed.CompareTo(incoming) < 0);
        }

        public static bool IsRestartRequired(DeploymentPayload deploymentPayload)
        {
            return (deploymentPayload.Deployment.Settings.UpdateBehavior.RequiresRestart
                || InteropIntegration.IsAddInInstalled(deploymentPayload.AddIn.Identity.Title));
        }


        public static string GetAddInManifestFilePath(DeploymentPayload deploymentPayload)
        {
            return Path.Combine(deploymentPayload.Destination.ParentDirectory, GetAddInManifestFileName(deploymentPayload));
        }

        public static string GetAddInManifestFileName(DeploymentPayload deploymentPayload)
        {
            return String.Format(AddInManifestParameterizedFileName, deploymentPayload.AddIn.Identity.Name);
        }

        public static string GetUpdateQueryInfoManifestFilePath(DeploymentPayload deploymentPayload)
        {
            return Path.Combine(deploymentPayload.Destination.ParentDirectory, GetUpdateQueryInfoManifestFileName(deploymentPayload));
        }

        public static string GetUpdateQueryInfoManifestFileName(DeploymentPayload deploymentPayload)
        {
            return String.Format(UpdateQueryInfoManifestParameterizedFileName, deploymentPayload.AddIn.Identity.Name);
        }

        public static string GetDependencyFilePath(Dependency dependency, UpdateDeploymentDestination destination)
        {
            string fileName = dependency.AssemblyIdentity.Name.AppendFileExtension(Common.DllFileExtension);

            if (dependency.FilePlacement.NextToAddIn)
            {
                return Path.Combine(destination.ParentDirectory, fileName);
            }
            else if (!String.IsNullOrEmpty(dependency.FilePlacement.SubDirectory) && !String.IsNullOrWhiteSpace(dependency.FilePlacement.SubDirectory))
            {
                return Path.Combine(destination.WorkingDirectory, dependency.FilePlacement.SubDirectory, fileName);
            }
            else
            {
                return Path.Combine(destination.WorkingDirectory, fileName);
            }
        }

        public static string GetAssetFileFilePath(AssetFile assetFile, UpdateDeploymentDestination destination)
        {
            if (assetFile.FilePlacement.NextToAddIn)
            {
                return Path.Combine(destination.ParentDirectory, assetFile.Name);
            }
            else if (!String.IsNullOrEmpty(assetFile.FilePlacement.SubDirectory) && !String.IsNullOrWhiteSpace(assetFile.FilePlacement.SubDirectory))
            {
                return Path.Combine(destination.WorkingDirectory, assetFile.FilePlacement.SubDirectory, assetFile.Name);
            }
            else
            {
                return Path.Combine(destination.WorkingDirectory, assetFile.Name);
            }
        }
    }
}