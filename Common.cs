using XLAutoDeploy.Manifests;

using System;
using System.IO;
using System.Windows.Forms;

namespace XLAutoDeploy
{
    internal static class Common
    {
        // Same name as this assembly
        public const string XLAutoDeployAssemblyName = "XLAutoDeploy";

        public const string XLAutoDeployManifestFileName = XLAutoDeployAssemblyName + ".Manifest.xml";

        public const string DllFileExtension = "dll";

        public static class NLogConfigurationVariableNames 
        {
            public const string AppVersion = "appVersion";
            public const string OfficeBittness = "officeBitness";
            public const string BaseDirectory = "baseDirectory";
        }

        public static string GetFormatedErrorMessage(string context, string problem, string solution)
        {
            return String.Concat(new string[] { "Context: ", context, System.Environment.NewLine, "Problem: ", problem, System.Environment.NewLine, "Solution: ", solution });
        }

        public static string GetAppName()
        {
            return System.Reflection.Assembly.GetExecutingAssembly()?.GetName()?.Name ?? XLAutoDeployAssemblyName;
        }

        public static DialogResult DisplayMessage(string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            using (var form = new Form() { TopMost = true })
            {
                return MessageBox.Show(form, message, caption, buttons, icon);
            }
        }

        public static string AppendFileExtension(this string fileName, string fileExtension)
        {
            return String.Concat(fileName.Replace("." + fileExtension, String.Empty), "." + fileExtension);
        }

        public static XLAutoDeployManifest GetXLAutoDeployManifest(string xlAutoDeployCurrentFilePath)
        {
            var applicationDirectory = Path.GetDirectoryName(xlAutoDeployCurrentFilePath);
            var manifestFilePath = Path.Combine(applicationDirectory, XLAutoDeployManifestFileName);

            return ManifestSerialization.DeserializeManifestFile<XLAutoDeployManifest>(manifestFilePath);
        }
    }
}
