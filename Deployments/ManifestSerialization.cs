using XLAutoDeploy.Logging;
using XLAutoDeploy.Manifests.Utilities;

using System;
using System.IO;
using System.Net;
using System.Security;

namespace XLAutoDeploy.Deployments
{
    internal static class ManifestSerialization
    {
        public static bool TryDeserializeManifestFile<T>(string filePath, ILogger logger, out T obj)
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

            return false;
        }

        public static bool TryDeserializeManifestFile<T>(WebClient webClient, Uri uri, ILogger logger, out T obj)
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

            return Serialization.DeserializeFromXml<T>(filePath);
        }

        public static T DeserializeManifestFile<T>(WebClient webClient, Uri uri)
        {
            return Serialization.DeserializeFromXml<T>(webClient, uri);
        }

        public static bool TrySerializeToXmlFile<T>(T obj, string filePath, ILogger logger)
        {
            try
            {
                Serialization.SerializeToXmlFile<T>(obj, filePath); 

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

            return false;
        }
    }
}
