using XLAutoDeploy.Deployments;
using XLAutoDeploy.FileSystem.Access;
using XLAutoDeploy.Logging;
using XLAutoDeploy.Updates;

using XLAutoDeploy.Manifests;

using ExcelDna.Integration;
using ExcelDna.Logging;

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
        // Due to this, we are using and instance of the 'ComAddInExtensibility' to run Excel App
        // open and close events.
        public void AutoClose()
        {
            OnExcelAppShutdown(); 
        }

        private void OnExcelAppStartup()
        {
            ExcelAsyncUtil.QueueAsMacro(delegate
            {
                Debug.WriteLine($"Begin {Common.GetAppName()} startup");

                try
                {
                    SetUpLoggerBaseDirectory();

                    var applicationDirectory = Path.GetDirectoryName(ExcelDnaUtil.XllPath);
                    var manifestFilePath = Path.Combine(applicationDirectory, Common.XLAutoDeployManifestFileName);
                    var xlAutoDeployManifest = ManifestSerialization.DeserializeManifestFile<XLAutoDeployManifest>(manifestFilePath);

                    var registry = DeploymentService.GetDeploymentRegistry(xlAutoDeployManifest.DeploymentRegistryUri);

                    _deploymentPayloads = DeploymentService.GetDeploymentPayloads(registry);

                    if (_deploymentPayloads?.Any() == false)
                    {
                        Debug.WriteLine($"{Common.GetAppName()} startup: Early Exit - No {nameof(DeploymentPayload)}(s) found");
                        return;
                    }

                    var remoteFileDownloader = new RemoteFileDownloaderFactory().Create();

                    DeploymentService.ProcessDeploymentPayloads(_deploymentPayloads, _updateCoordinator, remoteFileDownloader);

                    if (_deploymentPayloads?.Where(d => d.Deployment.Settings.UpdateBehavior.Expiration != null)?.Any() == true)
                    {
                        _updateMonitor = new UpdateMonitor(_deploymentPayloads, _updateCoordinator, new NLoggerProxy<UpdateMonitor>());
                    }
                }
                catch (Exception ex)
                {
                    _logger.Fatal(ex, "Failed application startup.");

                    Debug.WriteLine(ex.ToString());

                    LogDisplay.WriteLine($"{Common.GetAppName()} - An error ocurred while attempting auto load/install add-ins.");
                }

                Debug.WriteLine($"End {Common.GetAppName()} startup");
            }
            );
        }

        private void OnExcelAppShutdown()
        {
            if (_hasExcelAppShutdownExecuted)
                return; 

            Debug.WriteLine($"Begin {Common.GetAppName()} shutdown");

            SetUpLoggerBaseDirectory();

            if (_deploymentPayloads == null)
            {
                Debug.WriteLine("OnExcelAppShutdown: _deploymentPayloads is null");
#if DEBUG
                    LogDisplay.WriteLine($"{Common.GetAppName()} - An error ocurred while attempting auto Un-load/Un-install add-ins.");
#endif
                _logger.Fatal("OnExcelAppShutdown: _deploymentPayloads is null.");

                return;
            }

            try
            {
                _updateMonitor?.Dispose();

                UpdateService.TryUnInstallAddIns(_deploymentPayloads, _updateCoordinator);

                // Flush and close down internal threads and timers
                NLog.LogManager.Shutdown();
            }
            catch (Exception ex)
            {
                _logger.Fatal(ex, "Failed application shutdown.");

                Debug.WriteLine(ex.ToString());

#if DEBUG
                    LogDisplay.WriteLine($"{Common.GetAppName()} - An error ocurred while attempting auto Un-load/Un-install add-ins.");
#endif
            }

            Debug.WriteLine($"End {Common.GetAppName()} shutdown");

            _hasExcelAppShutdownExecuted = true;
        }

        private static void SetUpLoggerBaseDirectory()
        {
            NLog.LogManager.Configuration.Variables[Common.NLogConfigAppVersionVariableName] = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

            var officeBitness = ClientSystemDetection.GetMicrosoftOfficeBitness();
            switch (officeBitness)
            {
                case MicrosoftOfficeBitness.Bit32:
                    NLog.LogManager.Configuration.Variables[Common.NLogConfigOfficeBittnessVariableName] = "32bit";
                    break;

                case MicrosoftOfficeBitness.Bit64:
                    NLog.LogManager.Configuration.Variables[Common.NLogConfigOfficeBittnessVariableName] = "64bit";
                    break;

                case MicrosoftOfficeBitness.Unknown:
                    NLog.LogManager.Configuration.Variables[Common.NLogConfigOfficeBittnessVariableName] = "UnknownBitness";
                    break;
            }
        }
    }
}
