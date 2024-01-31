using XLAutoDeploy.FileSystem.Access;
using XLAutoDeploy.Updates;

using XLAutoDeploy.Manifests;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;

namespace XLAutoDeploy.Deployments
{
    /// <summary>
    /// A set of methods for automatically deploying and updating Excel add-ins on client machines using 
    /// configuration(s) defined in an instances of the <see cref="DeploymentPayload"/>'s.
    /// </summary>
    /// <remarks>
    /// Minimal validation is performed here as the methods in this class were intended to be called from <see cref="DeploymentService"/> which performs the required validation.
    /// </remarks>
    internal static class UpdateService
    {
        public static void StageUpdate(DeploymentPayload deploymentPayload, IUpdateCoordinator updateCoordinator)
        {
            UnLoadOrUnInstallAddIn(deploymentPayload, updateCoordinator);

            Directory.CreateDirectory(deploymentPayload.Destination.TempDirectory);

            // If an addin is installed, then the physical file will be locked by Excel. 
            // To update a locked addin file, do the following:
            // 1. Unload/Uninstall addin  
            // 2. Move local addin to a temp file path (this way it can be retrived in case of  error).

            // Delete existing temp file
            if (File.Exists(deploymentPayload.Destination.TempAddInPath))
            {
                File.SetAttributes(deploymentPayload.Destination.TempAddInPath, FileAttributes.Normal); // account for readonly file
                File.Delete(deploymentPayload.Destination.TempAddInPath);
            }

            File.SetAttributes(deploymentPayload.Destination.AddInPath, FileAttributes.Normal);

            // File should now be unlocked, so the FileCopy/FileDelete operations
            // that occur as a result of File.Move() will succeed
            File.Move(deploymentPayload.Destination.AddInPath, deploymentPayload.Destination.TempAddInPath);

            // Move any files stored next to the add-in
            var otherFilePaths = Directory.GetFiles(deploymentPayload.Destination.ParentDirectory);

            if (otherFilePaths is not null || otherFilePaths.Length > 0)
            {
                for (int i = 0; i < otherFilePaths.Length; i++)
                {
                    var filePath = otherFilePaths[i];
                    var fileName = Path.GetFileName(filePath);
                    var newFilePath = Path.Combine(deploymentPayload.Destination.TempDirectory, fileName); ;

                    // Delete existing temp file
                    if (File.Exists(newFilePath))
                    {
                        File.SetAttributes(newFilePath, FileAttributes.Normal);
                        File.Delete(newFilePath);
                    }

                    File.SetAttributes(filePath, FileAttributes.Normal);
                    File.Move(filePath, newFilePath);
                }
            }
        }

        public static void RevertToOldAddIn(DeploymentPayload deploymentPayload, IUpdateCoordinator updateCoordinator)
        {
            try
            {
                // if the original add-in exists in the temp path; this shoudl always be true 
                if (File.Exists(deploymentPayload.Destination.TempAddInPath))
                {
                    // if the new add-in exists, then delete it 
                    if (File.Exists(deploymentPayload.Destination.AddInPath))
                    {
                        File.SetAttributes(deploymentPayload.Destination.AddInPath, FileAttributes.Normal); // account for readonly file
                        File.Delete(deploymentPayload.Destination.AddInPath);
                    }

                    // move the original add-in back to the correct path 
                    if (!File.Exists(deploymentPayload.Destination.AddInPath))
                    {
                        File.SetAttributes(deploymentPayload.Destination.TempAddInPath, FileAttributes.Normal); // account for readonly file
                        File.Move(deploymentPayload.Destination.TempAddInPath, deploymentPayload.Destination.AddInPath);
                    }

                    // move any other files and load the old add-in 
                    if (File.Exists(deploymentPayload.Destination.AddInPath))
                    {
                        // Move any files stored next to the add-in
                        var otherFilePaths = Directory.GetFiles(deploymentPayload.Destination.TempDirectory);

                        if (otherFilePaths is not null || otherFilePaths.Length > 0)
                        {
                            for (int i = 0; i < otherFilePaths.Length; i++)
                            {
                                var filePath = otherFilePaths[i];
                                var fileName = Path.GetFileName(filePath);
                                var newFilePath = Path.Combine(deploymentPayload.Destination.ParentDirectory, fileName); ;

                                // Delete existing temp file
                                if (File.Exists(newFilePath))
                                {
                                    File.SetAttributes(newFilePath, FileAttributes.Normal);
                                    File.Delete(newFilePath);
                                }

                                File.SetAttributes(filePath, FileAttributes.Normal);
                                File.Move(filePath, newFilePath);
                            }
                        }

                        LoadOrInstallAddIn(deploymentPayload, updateCoordinator);
                    }
                }
                else  // The new add-in was downloaded and placed in the correct directory, but something else failed
                {

                }
            }
            catch
            {
            }
            finally
            {
                try
                {
                    if (Directory.Exists(deploymentPayload.Destination.TempDirectory))
                    {
                        Directory.Delete(deploymentPayload.Destination.TempDirectory, true);
                    }
                }
                catch
                {
                }
            }
        }

        // All download must be synchronous, otherwise excel will hang and eventually
        // display the following message: "Microsoft excel waiting for another application to complete an ole action."
        public static void DownloadAddInFromFileServer(DeploymentPayload deploymentPayload, IUpdateCoordinator updateCoordinator, IRemoteFileDownloader fileDownloader)
        {
            updateCoordinator.Deployer.Download(fileDownloader, deploymentPayload.AddIn.Uri.LocalPath, deploymentPayload.Destination.AddInPath, overwrite: true);

            if (deploymentPayload.AddIn.Dependencies?.Any() == true)
            {
                foreach (var dependency in deploymentPayload.AddIn.Dependencies)
                {
                    string filePath = GetDependencyFilePath(dependency, deploymentPayload.Destination);

                    if (File.Exists(filePath))
                    {
                        File.SetAttributes(filePath, FileAttributes.Normal);
                    }

                    updateCoordinator.Deployer.Download(fileDownloader, dependency.Uri.LocalPath, filePath, overwrite: true);

                    DownloadAssetFilesFromFileServer(dependency.AssetFiles, fileDownloader, deploymentPayload.Destination);
                }
            }

            DownloadAssetFilesFromFileServer(deploymentPayload.AddIn.AssetFiles, fileDownloader, deploymentPayload.Destination);
        }

        // All download must be synchronous, otherwise excel will hang and eventually
        // display the following message: "Microsoft excel waiting for another application to complete an ole action."
        public static void DownloadAddInFromWebServer(DeploymentPayload deploymentPayload, IUpdateCoordinator updateCoordinator, IRemoteFileDownloader fileDownloader, WebClient webClient)
        {
            updateCoordinator.Deployer.Download(fileDownloader, webClient, deploymentPayload.AddIn.Uri, deploymentPayload.Destination.AddInPath, overwrite: true);

            if (deploymentPayload.AddIn.Dependencies?.Any() == true)
            {
                foreach (var dependency in deploymentPayload.AddIn.Dependencies)
                {
                    string filePath = GetDependencyFilePath(dependency, deploymentPayload.Destination);

                    if (File.Exists(filePath))
                    {
                        File.SetAttributes(filePath, FileAttributes.Normal);
                    }

                    updateCoordinator.Deployer.Download(fileDownloader, webClient, dependency.Uri, filePath, overwrite: true);

                    DownloadAssetFilesFromWebServer(dependency.AssetFiles, fileDownloader, webClient, deploymentPayload.Destination);
                }
            }

            DownloadAssetFilesFromWebServer(deploymentPayload.AddIn.AssetFiles, fileDownloader, webClient, deploymentPayload.Destination);
        }

        private static void DownloadAssetFilesFromWebServer(IEnumerable<AssetFile> assetFiles, IRemoteFileDownloader fileDownloader,
            WebClient webClient, DeploymentDestination destination)
        {
            if (assetFiles?.Any() == false)
                return;

            foreach (var file in assetFiles)
            {
                string targetFilePath = GetAssetFileFilePath(file, destination);

                if (file.Hash is not null)
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

                        if (File.Exists(targetFilePath))
                        {
                            File.SetAttributes(targetFilePath, FileAttributes.Normal);
                        }

                        // overwrites if exists
                        File.WriteAllBytes(targetFilePath, fileBytes);
                    }
                }
                else
                {
                    if (File.Exists(targetFilePath))
                    {
                        File.SetAttributes(targetFilePath, FileAttributes.Normal);
                    }

                    fileDownloader.Download(webClient, file.Uri, targetFilePath, overwrite: true);
                }
            }
        }

        private static void DownloadAssetFilesFromFileServer(IEnumerable<AssetFile> assetFiles, IRemoteFileDownloader fileDownloader,
            DeploymentDestination destination)
        {
            if (assetFiles?.Any() == false)
                return;

            foreach (var file in assetFiles)
            {
                string targetFilePath = GetAssetFileFilePath(file, destination);

                if (file.Hash is not null)
                {
                    var fileBytes = fileDownloader.DownloadBytes(file.Uri.LocalPath);

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

                        if (File.Exists(targetFilePath))
                        {
                            File.SetAttributes(targetFilePath, FileAttributes.Normal);
                        }

                        // overwrites if exists
                        File.WriteAllBytes(targetFilePath, fileBytes);
                    }
                }
                else
                {
                    if (File.Exists(targetFilePath))
                    {
                        File.SetAttributes(targetFilePath, FileAttributes.Normal);
                    }

                    fileDownloader.Download(file.Uri.LocalPath, targetFilePath, overwrite: true);
                }
            }
        }

        public static void LoadOrInstallAddIn(DeploymentPayload deploymentPayload, IUpdateCoordinator updateCoordinator)
        {
            UnLoadOrUnInstallAddIn(deploymentPayload, updateCoordinator);

            if (deploymentPayload.Deployment.Settings.LoadBehavior.Install)
            {
                updateCoordinator.Installer.Install(deploymentPayload.AddIn.Identity.Title, deploymentPayload.Destination.AddInPath);
            }
            else
            {
                updateCoordinator.Loader.Load(deploymentPayload.Destination.AddInPath);
            }
        }

        public static void TryUnInstallAddIns(IEnumerable<DeploymentPayload> deploymentPayloads, IUpdateCoordinator updateCoordinator)
        {
            foreach (var payload in deploymentPayloads)
            {
                updateCoordinator.Installer.TryUninstall(payload.AddIn.Identity.Title, payload.Destination.AddInPath, out bool success);
            }
        }

        public static void UnLoadOrUnInstallAddIn(DeploymentPayload deploymentPayload, IUpdateCoordinator updateCoordinator)
        {
            updateCoordinator.Installer.Uninstall(deploymentPayload.AddIn.Identity.Title, deploymentPayload.Destination.AddInPath);

            updateCoordinator.Loader.Unload(deploymentPayload.Destination.AddInPath);
        }

        public static bool CanProceedWithUpdate(CheckedUpdate checkedUpdate, IUpdateCoordinator updateCoordinator)
        {
            var updateBehavior = checkedUpdate.Payload.Deployment.Settings.UpdateBehavior;
            if ((updateBehavior.NotifyClient && checkedUpdate.Info.UpdateAvailable) || (checkedUpdate.Info.IsMandatoryUpdate))
            {
                updateCoordinator.Notifier.Notify(checkedUpdate.GetDescription(),
    checkedUpdate.Payload.Deployment.Description.Product, checkedUpdate.Payload.Deployment.Description.Publisher, checkedUpdate.Info.DeployedVersion, checkedUpdate.Info.AvailableVersion, true);

                var now = DateTime.UtcNow;
                checkedUpdate.Info.FirstNotified = checkedUpdate.Info?.FirstNotified ?? now;
                checkedUpdate.Info.LastNotified = now;

                return updateCoordinator.Notifier.DoUpdate;
            }

            // Silent Update
            return checkedUpdate.Info.UpdateAvailable;
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

        public static bool IsRestartRequired(DeploymentPayload deploymentPayload, bool omitIsAddInInstalledCheck = false)
        {
            if (omitIsAddInInstalledCheck)
            {
                return deploymentPayload.Deployment.Settings.UpdateBehavior.RequiresRestart;
            }
            else
            {
                return (deploymentPayload.Deployment.Settings.UpdateBehavior.RequiresRestart
                    || InteropIntegration.IsAddInInstalled(deploymentPayload.AddIn.Identity.Title));
            }
        }

        public static bool IsUpdateExpired(UpdateQueryInfo updateQueryInfo, UpdateExpiration updateExpiration, DateTime currentUtcDateTime)
        {
            if (updateQueryInfo.LastChecked is not null)
                return false;

            DateTime lastChecked = (DateTime)updateQueryInfo.LastChecked;

            var difference = currentUtcDateTime.Subtract(lastChecked);

            switch (updateExpiration.UnitOfTime)
            {
                case UnitOfTime.Minutes:
                    return difference.Minutes >= updateExpiration.MaximumAge;

                case UnitOfTime.Days:
                    return difference.Days >= updateExpiration.MaximumAge;

                case UnitOfTime.Weeks:
                    return (double)(difference.Days / 7.00) >= (double)updateExpiration.MaximumAge;

                case UnitOfTime.Months:
                    return (double)(((double)(currentUtcDateTime.Year - lastChecked.Year) * 12) + currentUtcDateTime.Month - lastChecked.Month) >= (double)updateExpiration.MaximumAge;

                default:
                    return false;
            }
        }

        private static string GetDependencyFilePath(Dependency dependency, DeploymentDestination destination)
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

        private static string GetAssetFileFilePath(AssetFile assetFile, DeploymentDestination destination)
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