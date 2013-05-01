using System.Configuration;

namespace SlapCrop
{
    /// <summary>
    /// A configuration section for defining options for the <see cref="ImageProcessRunner"/> class
    /// </summary>
    public class ImageConfigSection : ConfigurationSection
    {
        /// <summary>
        /// The fill color used when an image will be letterboxes (scaled outside of it's original aspect ratio)
        /// </summary>
        [ConfigurationProperty("fillColor", DefaultValue="#000000")]
        public string FillColor
        {
            get { return this["fillColor"] as string; }
            set { this["fillColor"] = value; }
        }
    }
}
