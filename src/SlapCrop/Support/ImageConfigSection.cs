using System.Configuration;

namespace SlapCrop
{
    public class ImageConfigSection : ConfigurationSection
    {
        [ConfigurationProperty("fillColor", DefaultValue="#000000")]
        public string FillColor
        {
            get { return this["fillColor"] as string; }
            set { this["fillColor"] = value; }
        }
    }
}
