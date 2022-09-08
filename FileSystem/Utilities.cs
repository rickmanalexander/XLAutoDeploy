using System;
using System.IO;
using System.Security;

using XLAutoDeploy.Logging;

namespace XLAutoDeploy.FileSystem
{
    public static class Utilities
    {
        public const string CouldNotCreateHiddenDirectoryErrorMessage = "Could not create create hidden directory";

        public static void CreateHiddenDirectory(string filePath, ILogger logger)
        {
            var folderPath = Path.GetDirectoryName(filePath);

            try
            {
                var directoryInfo = Directory.CreateDirectory(folderPath);
                directoryInfo.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
            }
            catch (IOException ex)
            {
                logger.Error(ex, $"{CouldNotCreateHiddenDirectoryErrorMessage}: Invalid folder path {folderPath}");
                throw;
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.Error(ex, $"{CouldNotCreateHiddenDirectoryErrorMessage}: Inaccessible file directory {folderPath}");
                throw;
            }
            catch (SecurityException ex)
            {
                logger.Error(ex, $"{CouldNotCreateHiddenDirectoryErrorMessage}: Inaccessible file directory {folderPath}");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"{CouldNotCreateHiddenDirectoryErrorMessage} for {folderPath}");
                throw;
            }
        }
    }
}
