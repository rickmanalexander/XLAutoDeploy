using XLAutoDeploy.Logging;

using System;
using System.IO;
using System.Security;

namespace XLAutoDeploy.FileSystem
{
    internal static class FileUtilities
    {
        public const string CouldNotDirectoryErrorMessage = "Could not create create directory";

        public static void CreateDirectory(string filePath, ILogger logger)
        {
            var folderPath = Path.GetDirectoryName(filePath);

            try
            {
                var directoryInfo = Directory.CreateDirectory(folderPath);
                // directoryInfo.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
            }
            catch (IOException ex)
            {
                logger.Error(ex, $"{CouldNotDirectoryErrorMessage}: Invalid folder path {folderPath}");
                throw;
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.Error(ex, $"{CouldNotDirectoryErrorMessage}: Inaccessible file directory {folderPath}");
                throw;
            }
            catch (SecurityException ex)
            {
                logger.Error(ex, $"{CouldNotDirectoryErrorMessage}: Inaccessible file directory {folderPath}");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"{CouldNotDirectoryErrorMessage} for {folderPath}");
                throw;
            }
        }
    }
}
