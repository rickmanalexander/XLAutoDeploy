using System;
using System.Windows.Forms;

namespace XLAutoDeploy
{
    internal static class Common
    {
        //Same name as this assembly
        public const string XLAutoDeployAssemblyName = "XLAutoDeploy";

        public const string XLAutoDeployManifestFileName = "XLAutoDeploy.Manifest.xml";

        public const string DllFileExtension = "dll";

        public static string GetFormatedErrorMessage(string context, string problem, string solution)
        {
            return String.Concat(new string[] { "Context: ", context, System.Environment.NewLine, "Problem: ", problem, System.Environment.NewLine, "Solution: ", solution });
        }

        public static string GetAppName()
        {
            return System.Reflection.Assembly.GetExecutingAssembly()?.GetName()?.Name ?? XLAutoDeployAssemblyName;
        }

        public static void DisplayMessageBox(string message, bool allowCancel = false)
        {
            using (Form form = new Form { TopMost = true })
            {
                var buttons = allowCancel ? MessageBoxButtons.OKCancel : MessageBoxButtons.OK;

                MessageBox.Show(form, message, String.Empty, buttons, MessageBoxIcon.Information);

                form.Dispose();
            }
        }

        public static string AppendFileExtension(this string fileName, string fileExtension)
        {
            return String.Concat(fileName.Replace("." + fileExtension, String.Empty), "." + fileExtension);
        }
    }
}
