using XLAutoDeploy.Logging;
using XLAutoDeploy.Manifests.Utilities;

using ExcelDna.Logging;

using System;
using System.IO;
using System.Net;
using System.Security;

namespace XLAutoDeploy.Deployments
{
    internal static class ManifestSerialization
    {
        public static bool TryDeserializeManifestFile<T>(string filePath, ILogger logger, bool displayErrorMessage, out T obj)
        {
            obj = (T)Activator.CreateInstance(typeof(T)); //can't return null

            try
            {
                obj = DeserializeManifestFile<T>(filePath);

                return true;
            }
            catch (IOException ex)
            {
                logger.Error(ex, $"Invalid configuration file location for type '{nameof(T)}' from file path '{filePath}'.");
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.Error(ex, $"Inaccessible configuration file location for type '{nameof(T)}' from file path '{filePath}'.");
            }
            catch (SecurityException ex)
            {
                logger.Error(ex, $"Inaccessible configuration file location for type '{nameof(T)}' from file path '{filePath}'.");
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error loading the configuration file for type '{nameof(T)}' from file path '{filePath}'.");
            }

            if (displayErrorMessage)
                LogDisplay.WriteLine($"{Common.GetAppName()} - Error loading and deserializing required configuration file from file server.");

            return false;
        }

        public static bool TryDeserializeManifestFile<T>(WebClient webClient, Uri uri, ILogger logger, bool displayErrorMessage, out T obj)
        {
            try
            {
                obj = DeserializeManifestFile<T>(webClient, uri);

                return true;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error loading the configuration file for type '{nameof(T)}' from file path '{uri}'.");
            }

            if (displayErrorMessage)
                LogDisplay.WriteLine($"{Common.GetAppName()} -Error loading and deserializing required configuration file from web server.");

            obj = (T)Activator.CreateInstance(typeof(T)); //can't return null

            return false;
        }

        public static T DeserializeManifestFile<T>(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(Common.GetFormatedErrorMessage($"Attempting to deserialize a manifest file.",
                                                $"The {nameof(T)} manifest file could not be found in the following path: {filePath}.",
                                                $"Supply a valid manifest {nameof(filePath)}"));
            }

            return XmlConversion.DeserializeFromXml<T>(filePath, System.Xml.ConformanceLevel.Fragment);
        }

        public static T DeserializeManifestFile<T>(WebClient webClient, Uri uri)
        {
            return XmlConversion.DeserializeFromXml<T>(webClient, uri, System.Xml.ConformanceLevel.Fragment);
        }

        public static bool TrySerializeToXmlFile<T>(T obj, string filePath, ILogger logger, bool displayErrorMessage)
        {
            try
            {
                XmlConversion.SerializeToXmlFile<T>(obj, filePath); 

                return true;
            }
            catch (IOException ex)
            {
                logger.Error(ex, $"Invalid configuration file location for type '{nameof(T)}' to file path '{filePath}'.");
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.Error(ex, $"Inaccessible configuration file location for type '{nameof(T)}' to file path '{filePath}'.");
            }
            catch (SecurityException ex)
            {
                logger.Error(ex, $"Inaccessible configuration file location for type '{nameof(T)}' to file path '{filePath}'.");
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error serializing for type '{nameof(T)}' to file path '{filePath}'.");
            }

            if (displayErrorMessage)
                LogDisplay.WriteLine($"{Common.GetAppName()} - Error serializing required configuration file to client.");

            return false;
        }
    }
}
