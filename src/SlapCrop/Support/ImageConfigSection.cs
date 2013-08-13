using System;
using System.Configuration;

namespace SlapCrop
{
    /// <summary>
    /// A configuration section for defining options for the <see cref="ImageProcessRunner"/> class
    /// </summary>
    public class ImageConfigSection : ConfigurationSection
    {
        private const string PROP_FILL_COLOR = "fillColor";
        private const string PROP_TIMEOUT = "timeout";
        private const string PROP_BREAKPOINTS = "breakpoints";
        private const string PROP_SOURCES = "sources";

        private static Lazy<ImageConfigSection> _instance = new Lazy<ImageConfigSection>(() =>
        {
            return ConfigurationManager.GetSection("SlapCrop") as ImageConfigSection;
        });

        public static ImageConfigSection Instance { get { return _instance.Value; } }

        /// <summary>
        /// The fill color used when an image will be letterboxes (scaled outside of it's original aspect ratio)
        /// </summary>
        [ConfigurationProperty(PROP_FILL_COLOR, DefaultValue = "#000000")]
        public string FillColor
        {
            get { return this[PROP_FILL_COLOR] as string; }
            set { this[PROP_FILL_COLOR] = value; }
        }

        /// <summary>
        /// The duration in milliseconds before timing out remote requests
        /// </summary>
        [ConfigurationProperty(PROP_TIMEOUT, DefaultValue = (int)1000)]
        public int Timeout
        {
            get { return (int)this[PROP_TIMEOUT]; }
            set { this[PROP_TIMEOUT] = value; }
        }

        /// <summary>
        /// The breakpoint definitions
        /// </summary>
        [ConfigurationProperty(PROP_BREAKPOINTS, IsRequired = true)]
        public KeyValueConfigurationCollection Breakpoints
        {
            get { return this[PROP_BREAKPOINTS] as KeyValueConfigurationCollection; }
            set { this[PROP_BREAKPOINTS] = value; }
        }

        /// <summary>
        /// The remote source URLs
        /// </summary>
        [ConfigurationProperty(PROP_SOURCES, IsRequired = true)]
        public KeyValueConfigurationCollection Sources
        {
            get { return this[PROP_SOURCES] as KeyValueConfigurationCollection; }
            set { this[PROP_SOURCES] = value; }
        }
    }
}
