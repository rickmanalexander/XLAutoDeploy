using ExcelDna.Integration;
using Microsoft.Office.Interop.Excel;

using System;
using System.IO;
using System.Linq;

namespace XLAutoDeploy
{
    /// <summary>
    /// A set of methods for loading/unloading and installing/uninstalling add-ins on the fly 
    /// by interacting directly with the Excel Application using <see cref="Microsoft.Office.Interop"/> 
    /// and <see cref="ExcelDna.Integration"/>.
    /// </summary>
    internal static class InteropIntegration
    {
        public static void LoadAddIn(string filePath)
        {
            LoadAddIn(GetExcelApplication(), filePath);
        }

        public static void LoadAddIn(Application excelApp, string filePath)
        {
            ThrowIsNotAddInFileExeption("load", filePath);

            //If addin is installed, then it is already "loaded"
            if (IsXllAddInFile(filePath))
            {
                ExcelIntegration.RegisterXLL(filePath);
            }
            else if (IsXlamAddInFile(filePath) || IsXlaAddInFile(filePath))
            {
                if (!IsWorkbookOpen(excelApp, filePath))
                {
                    excelApp.Workbooks.Open(filePath);
                }
            }
        }

        public static void UnloadAddIn(string filePath)
        {
            UnloadAddIn(GetExcelApplication(), filePath);
        }

        public static void UnloadAddIn(Application excelApp, string filePath)
        {
            ThrowIsNotAddInFileExeption("unload", filePath);

            if (IsXllAddInFile(filePath))
            {
                ExcelIntegration.UnregisterXLL(filePath);
            }
            else if (IsXlamAddInFile(filePath) || IsXlaAddInFile(filePath))
            {
                //Check if is already open first
                if (IsWorkbookOpen(excelApp, filePath))
                {
                    var fileName = Path.GetFileName(filePath);

                    excelApp.Workbooks.Item[fileName].Close(filePath);
                }
            }
        }

        public static void InstallAddIn(string addInTitle, string filePath)
        {
            InstallAddIn(GetExcelApplication(), addInTitle, filePath);
        }

        public static void InstallAddIn(Application excelApp, string addInTitle, string filePath)
        {
            ThrowIsNotAddInFileExeption("install", filePath);

            if (!IsAddInInstalled(excelApp, addInTitle))
            {
                excelApp.AddIns2.Add(filePath);
                excelApp.AddIns2.Item[addInTitle].Installed = true;
            }
        }

        public static void UninstallAddIn(string addInTitle)
        {
            UninstallAddIn(GetExcelApplication(), addInTitle);
        }

        public static void UninstallAddIn(Application excelApp, string addInTitle)
        {
            if (IsAddInInstalled(excelApp, addInTitle))
            {
                excelApp.AddIns2.Item[addInTitle].Installed = false;
            }
        }

        public static bool IsXlamAddInFile(string filePath)
        {
            return Path.GetExtension(filePath).Trim().Equals("xlam", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsXlaAddInFile(string filePath)
        {
            return Path.GetExtension(filePath).Trim().Equals("xla", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsXllAddInFile(string filePath)
        {
            return Path.GetExtension(filePath).Trim().Equals("xll", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsExcelAddInFile(string filePath)
        {
            var fileExtension = Path.GetExtension(filePath);

            return ExcelAddInAvailableFileExtentions()
                .Any(f => fileExtension.Trim().Equals(f, StringComparison.OrdinalIgnoreCase));
        }

        public static bool IsAddInInstalled(string addInTitle)
        {
            return IsAddInInstalled(GetExcelApplication(), addInTitle);
        }

        public static bool IsAddInInstalled(Application excelApp, string addInTitle)
        {
            if (AddInExists(excelApp, addInTitle))
            {
                return excelApp.AddIns2.Item[addInTitle].Installed;
            }
            else
            {
                return false;
            }
        }

        public static bool AddInExists(string addInTitle)
        {
            return AddInExists(GetExcelApplication(), addInTitle);
        }

        public static bool AddInExists(Application excelApp, string addInTitle)
        {
            var addinTitleLocal = addInTitle.Trim();

            return excelApp.AddIns2.Cast<AddIn>()
                .Any(a => a.Title.Trim().Equals(addinTitleLocal, StringComparison.OrdinalIgnoreCase));
        }


        public static bool IsWorkbookOpen(string filePath)
        {
            return IsWorkbookOpen(GetExcelApplication(), filePath);
        }

        public static bool IsWorkbookOpen(Application excelApp, string filePath)
        {
            var fileName = Path.GetFileName(filePath).Trim();

            return excelApp.Workbooks.Cast<Workbook>()
                .Any(w => w.Name.Trim().Equals(fileName, StringComparison.OrdinalIgnoreCase));
        }

        public static bool IsXllAddRegistered(string addInNameOrFileName)
        {
            return IsXllAddRegistered(GetExcelApplication(), addInNameOrFileName);
        }

        // https://docs.microsoft.com/en-us/office/vba/api/excel.application.registeredfunctions
        // Note: Based on testing in VBA, the first column in the array returns the full file
        // path to the xll add-in.
        // Also and xll being registered is equivalent to it being "Open" (i.e.  loaded) in Excel.
        public static bool IsXllAddRegistered(Application excelApp, string addInNameOrFileName)
        {
            var addInName = Path.GetFileNameWithoutExtension(addInNameOrFileName).Trim();

            var registeredFunctions = excelApp.RegisteredFunctions();

            if (registeredFunctions != null)
            {
                var startRowIndex = registeredFunctions.GetLowerBound(0);
                var startColumnIndex = registeredFunctions.GetLowerBound(1);

                if (startRowIndex > 0)
                {
                    for (int i = startRowIndex; i <= registeredFunctions.GetLength(0); i++)
                    {
                        if (addInName.Equals((string)registeredFunctions[i, startColumnIndex].ToString().Trim(), StringComparison.OrdinalIgnoreCase))
                            return true;
                    }
                }
                else
                {
                    for (int i = startRowIndex; i < registeredFunctions.GetLength(0); i++)
                    {
                        if (addInName.Equals((string)registeredFunctions[i, startColumnIndex].ToString().Trim(), StringComparison.OrdinalIgnoreCase))
                            return true;
                    }
                }

            }

            return false;
        }

        public static void CloseExcelApp()
        {
            CloseExcelApp(GetExcelApplication());
        }

        public static void CloseExcelApp(Application excelApp)
        {
            excelApp.Quit();
        }

        public static Application GetExcelApplication()
        {
            return (Application)ExcelDnaUtil.Application;
        }

        public static bool TryRegisterXLLAddin(string filePath)
        {
            try
            {
                RegisterXLLAddin(filePath);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool TryUnregisterXLLAddin(string filePath)
        {
            try
            {
                UnregisterXLLAddin(filePath);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void RegisterXLLAddin(string filePath)
        {
            ThrowIsNotAddInFileExeption("register", filePath);

            ExcelIntegration.RegisterXLL(filePath);
        }

        public static void UnregisterXLLAddin(string filePath)
        {
            ThrowIsNotAddInFileExeption("un-register", filePath);

            ExcelIntegration.UnregisterXLL(filePath);
        }

        public static string[] ExcelAddInAvailableFileExtentions()
        {
            return new string[] { "xlam", "xla", "xll" };
        }

        public static void ThrowIsNotAddInFileExeption(string methodAction, string filePath)
        {
            if (!IsExcelAddInFile(filePath))
            {
                throw new InvalidOperationException(Common.GetFormatedErrorMessage($"{methodAction}ing file",
                    $"Cannot {methodAction} file {filePath}, because is is not an Excel Add-In file.",
                    $"Supply a valid add-in {nameof(filePath)}."));
            }
        }
    }
}
