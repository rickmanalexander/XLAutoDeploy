using System;
using System.IO;

using XLAutoDeploy.Mage;

namespace XLAutoDeploy.Deployments
{
    /// <summary>
    /// Represents the physical file location on client machine to which a application update will be deployed.
    /// </summary>
    public sealed class UpdateDeploymentDestination
    {
        /// <summary>
        /// Windows specific file directory based on the specified <see cref="DeploymentBasis"/>.
        /// </summary>
        public string RootDirectory => 
                _deploymentBasis == DeploymentBasis.PerMachine ? 
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) : Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        public string Manufacturer => _manufacturer;
        public string Product => _product;

        /// <summary>
        /// A sub-directory comprised of the <see cref="RootDirectory"/>, <see cref="Manufacturer"/>, and 
        /// <see cref="Product"/> in the following format: <br/>
        /// RootDirectory\Manufacturer\Product\
        /// </summary>
        /// <remarks>
        /// This is the primary directory for the add-in. Both the add-in, and the <see cref="AddIn"/> 
        /// manifest should always be placed here.
        /// </remarks> 
        public string ParentDirectory => System.IO.Path.Combine(RootDirectory, Manufacturer, Product);


        public string AddInFileName => _addInFileName;

        /// <summary>
        /// The full file path of the add-in comprised of the <see cref="ParentDirectory"/> and
        /// the <see cref="AddInFileName"/>. 
        /// </summary>
        public string AddInPath => System.IO.Path.Combine(ParentDirectory, AddInFileName);

        /// <summary>
        /// A sub-directory comprised of the <see cref="ParentDirectory"/> named "Temp"
        /// in the following format: <br/>
        /// ParentDirectory\Temp\
        /// </summary>
        public string TempAddInDirectory => System.IO.Path.Combine(ParentDirectory, "Temp");

        /// <summary>
        /// The full temporary file path of the add-in comprised of the <see cref="TempAddInDirectory"/> 
        /// and the <see cref="AddInFileName"/>. 
        /// </summary>
        public string TempAddInPath => System.IO.Path.Combine(TempAddInDirectory, AddInFileName);


        public string Version => _version;

        /// <summary>
        /// A sub-directory comprised of the <see cref="RootDirectory"/>, <see cref="ParentDirectory"/>,
        /// and <see cref="Version"/> in the following format: <br/>
        /// RootDirectory\ParentDirectory\Version\
        /// </summary>
        /// <remarks>
        /// This is the parent directory for any dependencies and/or asset files.  
        /// </remarks> 
        public string WorkingDirectory => System.IO.Path.Combine(ParentDirectory, Version);


        private readonly DeploymentBasis _deploymentBasis;
        private readonly string _manufacturer;
        private readonly string _product;
        private readonly string _version;
        private readonly string _addInFileName;

        public UpdateDeploymentDestination(DeploymentBasis deploymentBasis, string manufacturer, 
            string product, System.Version version, string addInName, AddInFileExtensionType fileExtension)
        {
            var typeName = typeof(UpdateDeploymentDestination).Name;
        
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

            _deploymentBasis = deploymentBasis; 
            _manufacturer = manufacturer;
            _product = product;
            _version = version.ToString();
            _addInFileName = String.Concat(addInName, ".", Enum.GetName(typeof(AddInFileExtensionType), fileExtension).ToLower());
        
            if (System.IO.Path.Combine(_manufacturer, _product, _version, _addInFileName).IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) >= 0)
            {
                throw new ArgumentException(Common.GetFormatedErrorMessage($"Constructing type {typeName}",
                    $"Either the {nameof(addInName)} parameter, {nameof(fileExtension)} parameter (or both) contain one or more invalid characters.",
                    $"Supply a valid {nameof(addInName)} and {nameof(fileExtension)}."));
            }
        }
    }
}
