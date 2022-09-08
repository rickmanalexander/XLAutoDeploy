using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
// using System.Threading;

using ExcelDna.Integration;
using ExcelDna.Logging;

using XLAutoDeploy.Mage;

using XLAutoDeploy.FileSystem.Access;
using XLAutoDeploy.Logging;
using XLAutoDeploy.Updates;
using XLAutoDeploy.Deployments;

namespace XLAutoDeploy
{
    /// <summary>
    /// Main entry point for the application that implements <see cref="IExcelAddIn"/>, which hooks the Excel <see cref="IExcelAddIn.AutoOpen"/> 
    /// and <see cref="IExcelAddIn.AutoClose"/> events.
    /// This is where and instance of a <see cref="DeploymentRegistry"/> should be loaded/deserialized, and 
    /// the setup for automatic updates should be performed. 
    /// </summary>
    public sealed class ThisAddIn : IExcelAddIn
    {
        private static readonly INLoggerProxyFactory _loggerProxyFactory = new NLoggerProxyFactory();
        private static readonly ILogger _logger = _loggerProxyFactory.Create(typeof(ThisAddIn));
        private readonly IUpdateCoordinator _updateCoordinator = new UpdateCoordinatorFactory().Create(_loggerProxyFactory);

        private IReadOnlyCollection<DeploymentPayload> _deploymentPayloads;
        private UpdateMonitor _updateMonitor;

        private ComAddInExtensibility _comExtensibility;
        private bool _hasExcelAppShutdownExecuted = false;

        public void AutoOpen()
        {
            _comExtensibility = new ComAddInExtensibility(() => OnExcelAppStartup(), () => OnExcelAppShutdown());

            ExcelComAddInHelper.LoadComAddIn(_comExtensibility);
        }

        // AutoClose is only called when the add-in is actually removed by the user, and not
        // when Excel exits.
        //Due to this, we are using and instance of the 'AddInComAdapter' to run Excel App
        //open and close events.
        public void AutoClose()
        {
            if (!_hasExcelAppShutdownExecuted)
                OnExcelAppShutdown();
        }

        private void OnExcelAppStartup()
        {
            Debug.WriteLine($"Begin {Common.GetAppName()} startup");

            var remoteFileDownloader = new RemoteFileDownloaderFactory().Create();

            try
            {
                var applicationDirectory = Path.GetDirectoryName(ExcelDnaUtil.XllPath);
                var manifestFilePath = Path.Combine(applicationDirectory, Common.XLAutoDeployManifestFileName);
                var xlAutoDeployManifest = ManifestSerialization.DeserializeManifestFile<XLAutoDeployManifest>(manifestFilePath);

                // multiple parameter options: choose based on the FileHost(s)
                var registry = DeploymentService.GetDeploymentRegistry(xlAutoDeployManifest.DeploymentRegistryUri);

                // multiple parameter options: choose based on the FileHost(s) 
                _deploymentPayloads = DeploymentService.GetDeploymentPayloads(registry);

                if (_deploymentPayloads?.Any() == true)
                {
                    Debug.WriteLine($"{Common.GetAppName()} startup: Early Exit - No {nameof(DeploymentPayload)}(s) found");
                    return;
                }

                DeploymentService.ProcessDeploymentPayloads(_deploymentPayloads, _updateCoordinator, remoteFileDownloader);
                DeploymentService.SetRealtimeUpdateMonitoring(_deploymentPayloads, _updateCoordinator, remoteFileDownloader, out _updateMonitor);
            }
            catch (Exception ex)
            {
                _logger.Fatal(ex, "Failed application startup.");
                LogDisplay.WriteLine($"{Common.GetAppName()} - An error ocurred while attempting auto load/install add-ins.");
            }

            // This would possibly access excel COM objects from another thread which could lead to 
            // unexpected (and/or fatal) errors
            // var method = new ThreadStart(() =>
            // {
            //     try
            //     {
            //         var applicationDirectory = Path.GetDirectoryName(ExcelDnaUtil.XllPath);
            //         var manifestFilePath = Path.Combine(applicationDirectory, Common.XLAutoDeployManifestFileName);
            //         var xlAutoDeployManifest = ManifestSerialization.DeserializeManifestFile<XLAutoDeployManifest>(manifestFilePath);
            // 
            //         // multiple parameter options: choose based on the FileHost(s)
            //         var registry = DeploymentService.GetDeploymentRegistry(xlAutoDeployManifest.DeploymentRegistryUri);
            // 
            //         // multiple parameter options: choose based on the FileHost(s) 
            //         _deploymentPayloads = DeploymentService.GetDeploymentPayloads(registry);
            // 
            //         if (_deploymentPayloads?.Any() == true)
            //         {
            //             Debug.WriteLine($"{Common.GetAppName()} startup: Early Exit - No {nameof(DeploymentPayload)}(s) found");
            //             return;
            //         }
            // 
            //         DeploymentService.ProcessDeploymentPayloads(_deploymentPayloads, _updateCoordinator, remoteFileDownloader);
            //         DeploymentService.SetRealtimeUpdateMonitoring(_deploymentPayloads, _updateCoordinator, remoteFileDownloader, out _updateMonitor);
            //     }
            //     catch (Exception ex)
            //     {
            //         _logger.Fatal(ex, "Failed application startup.");
            //         LogDisplay.WriteLine($"{Common.GetAppName()} - An error ocurred while attempting auto load/install add-ins.");
            //     }
            // });
            // 
            // var thread = new Thread(method);
            // thread.Start();

            Debug.WriteLine($"End {Common.GetAppName()} startup");
        }

        private void OnExcelAppShutdown()
        {
            _hasExcelAppShutdownExecuted = true;

            Debug.WriteLine("Begin Excel app shutdown");

            _updateMonitor?.Dispose();

            if ( _deploymentPayloads == null || _updateCoordinator == null || _logger == null)
            {
                Debug.WriteLine("End Excel app shutdown: Early exit - One or more reuired objects are null.");
                return;
            }

            try
            {
                UpdateService.UnloadAddIns(_deploymentPayloads, _updateCoordinator);
            }
            catch (Exception ex)
            {
                _logger.Fatal(ex, "Failed application shutdown.");
                LogDisplay.WriteLine($"{Common.GetAppName()} - An error ocurred while attempting auto Un-load/Un-install add-ins.");
            }

            // var method = new ThreadStart(() =>
            // {
            //     try
            //     {
            //         UpdateService.UnloadAddIns(_deploymentPayloads, _updateCoordinator);
            //     }
            //     catch (Exception ex)
            //     {
            //         _logger.Fatal(ex, "Failed application shutdown.");
            //         LogDisplay.WriteLine($"{Common.GetAppName()} - An error ocurred while attempting auto Un-load/Un-install add-ins.");
            //     }
            // });
            // 
            // var thread = new Thread(method);
            // thread.Start();

            Debug.WriteLine("End Excel app shutdown");
        }
    }
}
