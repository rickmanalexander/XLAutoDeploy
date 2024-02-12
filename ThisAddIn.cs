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
using System.Linq;
using System.Threading;

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

        private string _loggerFileDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData); 

        private XLAutoDeployManifest _xLAutoDeployManifest;
        private IReadOnlyCollection<DeploymentPayload> _deploymentPayloads;
        private UpdateMonitor _updateMonitor;

        private ComAddInExtensibility _comExtensibility;
        private int _onExcelAppShutdownExecutionCount = 0;

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

                SetUpLoggerFilePath(_loggerFileDirectory);

                try
                {
                    _xLAutoDeployManifest = Common.GetLocalXLAutoDeployManifest(ExcelDnaUtil.XllPath);

                    Debug.WriteLine($"Retrieved XLAutoDeployManifest file");

                    // reset
                    _loggerFileDirectory = _xLAutoDeployManifest.LoggerFileDirectory ?? _loggerFileDirectory;

                    SetUpLoggerFilePath(_loggerFileDirectory);

                    // CheckForXLAutoDeployUpdateAndNotify(_xLAutoDeployManifest);

                    var remoteFileDownloader = new RemoteFileDownloaderFactory().Create();

                    _deploymentPayloads = DeploymentService.GetDeploymentPayloads(_xLAutoDeployManifest);

                    if (_deploymentPayloads?.Any() == false)
                    {
                        LogDisplay.WriteLine($"{Common.GetAppName()} startup: Early Exit - No {nameof(DeploymentPayload)}(s) found");

                        _logger.Warn($"{Common.GetAppName()} startup: Early Exit - No {nameof(DeploymentPayload)}(s) found");

                        return;
                    }

                    DeploymentService.ProcessDeploymentPayloads(_deploymentPayloads, _updateCoordinator, remoteFileDownloader);

                    if (_deploymentPayloads?.Where(d => d.Deployment.Settings.UpdateBehavior.Expiration is not null)?.Any() == true)
                    {
                        _updateMonitor = UpdateMonitorFactory.Create(_deploymentPayloads, _updateCoordinator);
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
            if (Interlocked.Increment(ref _onExcelAppShutdownExecutionCount) > 1)
                return;

            Debug.WriteLine($"Begin {Common.GetAppName()} shutdown");

            SetUpLoggerFilePath(_loggerFileDirectory);

            if (_deploymentPayloads is null)
            {
                LogDisplay.WriteLine($"{Common.GetAppName()} - An error ocurred while attempting auto Un-load/Un-install add-ins.");

                _logger.Fatal("OnExcelAppShutdown: _deploymentPayloads is null.");
            }

            try
            {
                _updateMonitor?.Dispose();

                if (_deploymentPayloads is not null)
                {
                    UpdateService.TryUnInstallAddIns(_deploymentPayloads, _updateCoordinator);
                }

                // if (_xLAutoDeployManifest is not null)
                // {
                //     CheckForXLAutoDeployUpdateAndNotify(_xLAutoDeployManifest);
                // }
                
                // Flush and close down internal threads and timers
                NLog.LogManager.Shutdown();
            }
            catch (Exception ex)
            {
                _logger.Fatal(ex, "Failed application shutdown.");

                Debug.WriteLine(ex.ToString());

                LogDisplay.WriteLine($"{Common.GetAppName()} - An error ocurred while attempting auto Un-load/Un-install add-ins.");
            }

            Debug.WriteLine($"End {Common.GetAppName()} shutdown");
        }

        /*
        private static void CheckForXLAutoDeployUpdateAndNotify(XLAutoDeployManifest xlAutoDeployManifest)
        {
            var remoteManifestUri = xlAutoDeployManifest.ManifestUri;
            var remoteManifest = Common.GetXLAutoDeployManifest(remoteManifestUri);

            if (UpdateService.IsNewVersionAvailable(xlAutoDeployManifest.Version, remoteManifest.Version))
            {
                Common.DisplayMessage($"A new version of {Common.XLAutoDeployAssemblyName} was detected{Environment.NewLine}{Environment.NewLine}When time permits, please close Excel and run the installation manager found in the following location: " +
                        $"{Environment.NewLine}{Environment.NewLine}{remoteManifest.InstallerUri}", string.Empty, System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
            }
        }
        */ 

        private static void SetUpLoggerFilePath(string baseDirectory)
        {
            NLog.LogManager.Configuration.Variables[Common.NLogConfigurationVariableNames.BaseDirectory] = baseDirectory;

            NLog.LogManager.Configuration.Variables[Common.NLogConfigurationVariableNames.AppVersion] = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

            var officeBitness = ClientSystemDetection.GetMicrosoftOfficeBitness();
            switch (officeBitness)
            {
                case MicrosoftOfficeBitness.Bit32:
                    NLog.LogManager.Configuration.Variables[Common.NLogConfigurationVariableNames.OfficeBittness] = "32bit";
                    break;

                case MicrosoftOfficeBitness.Bit64:
                    NLog.LogManager.Configuration.Variables[Common.NLogConfigurationVariableNames.OfficeBittness] = "64bit";
                    break;

                case MicrosoftOfficeBitness.Unknown:
                    NLog.LogManager.Configuration.Variables[Common.NLogConfigurationVariableNames.OfficeBittness] = "UnknownBitness";
                    break;
            }
        }
    }
}
