using XLAutoDeploy.Manifests;

using System;
using System.IO;

namespace XLAutoDeploy.Deployments
{
    /// <summary>
    /// Represents the physical file location on client machine to which a application update will be deployed.
    /// </summary>
    internal sealed class DeploymentDestination
    {
        /// <summary>
        /// Windows specific file directory based on the specified <see cref="DeploymentBasis"/>.
        /// </summary>
        public string RootDirectory => 
                _deploymentBasis == DeploymentBasis.PerMachine ? 
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) : Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        public string Manufacturer => _manufacturer;
        public string Product => _product;

        public string OfficeBitness => _officeBittness;

        /// <summary>
        /// A sub-directory comprised of the <see cref="RootDirectory"/>, <see cref="Manufacturer"/>, <see cref="Product"/>, and <see cref="OfficeBitness"/>
        /// in the following format: <br/>
        /// RootDirectory\Manufacturer\Product\OfficeBitness\
        /// </summary>
        /// <remarks>
        /// This is the primary directory for the add-in file. Both the add-in, and the <see cref="AddIn"/> 
        /// manifest should always be placed here.
        /// </remarks> 
        public string ParentDirectory => Path.Combine(RootDirectory, Manufacturer, Product, OfficeBitness);


        public string AddInFileName => _addInFileName;

        /// <summary>
        /// The full file path of the add-in comprised of the <see cref="ParentDirectory"/> and
        /// the <see cref="AddInFileName"/>. 
        /// </summary>
        public string AddInPath => Path.Combine(ParentDirectory, AddInFileName);

        /// <summary>
        /// A sub-directory comprised of the <see cref="ParentDirectory"/> named "Temp"
        /// in the following format: <br/>
        /// ParentDirectory\Temp\
        /// </summary>
        public string TempAddInDirectory => Path.Combine(ParentDirectory, "Temp");

        /// <summary>
        /// The full temporary file path of the add-in comprised of the <see cref="TempAddInDirectory"/> 
        /// and the <see cref="AddInFileName"/>. 
        /// </summary>
        public string TempAddInPath => Path.Combine(TempAddInDirectory, AddInFileName);


        public string Version => _version;

        /// <summary>
        /// A sub-directory comprised of the <see cref="RootDirectory"/>, <see cref="ParentDirectory"/>,
        /// and <see cref="Version"/> in the following format: <br/>
        /// RootDirectory\ParentDirectory\Version\
        /// </summary>
        /// <remarks>
        /// This is the parent directory for any dependencies and/or asset files.  
        /// </remarks> 
        public string WorkingDirectory => Path.Combine(ParentDirectory, Version);


        private readonly DeploymentBasis _deploymentBasis;
        private readonly string _manufacturer;
        private readonly string _product;
        private readonly string _officeBittness;

        private readonly string _version;
        private readonly string _addInFileName;

        public DeploymentDestination(DeploymentBasis deploymentBasis, string manufacturer, 
            string product, MicrosoftOfficeBitness officeBitness, System.Version version, string addInName, AddInFileExtensionType fileExtension)
        {
            var typeName = typeof(DeploymentDestination).Name;
        
            if (String.IsNullOrEmpty(manufacturer) || String.IsNullOrWhiteSpace(manufacturer))
            {
                throw new ArgumentNullException(Common.GetFormatedErrorMessage($"Constructing type {typeName}",
                    $"The {nameof(manufacturer)} parameter is null or empty.",
                    $"Supply a valid {nameof(manufacturer)}."));
            }

            if (String.IsNullOrEmpty(product) || String.IsNullOrWhiteSpace(product))
            {
                throw new ArgumentNullException(Common.GetFormatedErrorMessage($"Constructing type {typeName}",
                    $"The {nameof(product)} parameter is null or empty.",
                    $"Supply a valid {nameof(product)}."));
            }

            if (version == null)
            {
                throw new ArgumentNullException(Common.GetFormatedErrorMessage($"Constructing type {typeName}",
                    $"The {nameof(version)} parameter is null or empty.",
                    $"Supply a valid {nameof(version)}."));
            }

            if (String.IsNullOrEmpty(addInName) || String.IsNullOrWhiteSpace(addInName))
            {
                throw new ArgumentNullException(Common.GetFormatedErrorMessage($"Constructing type {typeName}",
                    $"The {nameof(addInName)} parameter is null or empty.",
                    $"Supply a valid {nameof(addInName)}."));
            }

            if (System.IO.Path.Combine(manufacturer, product, version.ToString()).IndexOfAny(System.IO.Path.GetInvalidPathChars()) >= 0)
            {
                throw new ArgumentException(Common.GetFormatedErrorMessage($"Constructing type {typeName}",
                    $"The {nameof(manufacturer)}, {nameof(product)}, and//or {nameof(version)} parameter(s) contain one or more invalid characters.",
                    $"Supply valid {nameof(manufacturer)}, {nameof(product)}, and//or {nameof(version)}."));
            }

            _deploymentBasis = deploymentBasis; 
            _manufacturer = manufacturer;
            _product = product;
            _officeBittness = Enum.GetName(typeof(MicrosoftOfficeBitness), officeBitness);
            _version = version.ToString();
            _addInFileName = String.Concat(addInName, ".", Enum.GetName(typeof(AddInFileExtensionType), fileExtension).ToLower());
            
            if (_addInFileName.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) >= 0)
            {
                throw new ArgumentException(Common.GetFormatedErrorMessage($"Constructing type {typeName}",
                    $"The {nameof(addInName)} parameter contains one or more invalid characters.",
                    $"Supply a valid {nameof(addInName)} and {nameof(fileExtension)}."));
            }
        }
    }
}
