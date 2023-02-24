using System;
using System.ComponentModel;
using System.Xml.Serialization;

using XLAutoDeploy.Manifests;

namespace XLAutoDeploy
{
    /// <summary>
    /// Represents the location of the <see cref="DeploymentRegistry">.
    /// </summary>  
    [Serializable]
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = false)]
    public sealed class XLAutoDeployManifest
    {
        public XLAutoDeployManifest() { }

        [XmlIgnore]
        public Uri DeploymentRegistryUri => new Uri(DeploymentRegistryUriString);

        [XmlElement("DeploymentRegistryUri", IsNullable = false)]
        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public string DeploymentRegistryUriString { get; set; }
    }
}
