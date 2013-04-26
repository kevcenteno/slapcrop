using System.Drawing;
using System.Drawing.Drawing2D;

namespace SlapCrop
{
    /// <summary>
    /// A container class for options used by the <see cref="ImageScaler"/> class.
    /// </summary>
    public class ImageScalerOptions
    {
        /// <summary>
        /// The default color used to fill backgrounds on images that are scaled 
        /// outside of the original aspect ratio
        /// </summary>
        public Color DefaultFillColor { get; set; }

        public InterpolationMode InterpolationMode { get; set; }
        public SmoothingMode SmoothingMode { get; set; }
        public PixelOffsetMode PixelOffsetMode { get; set; }
        public CompositingQuality CompositingQuality { get; set; }

        /// <summary>
        /// Default constructor
        /// <para>Sets graphics options for high-quality defaults</para>
        /// <para>Sets the DefaultFillColor to black</para>
        /// </summary>
        public ImageScalerOptions()
        {
            this.DefaultFillColor = Color.Black;
            this.InterpolationMode = InterpolationMode.HighQualityBicubic;
            this.SmoothingMode = SmoothingMode.HighQuality;
            this.PixelOffsetMode = PixelOffsetMode.HighQuality;
            this.CompositingQuality = CompositingQuality.HighQuality;
        }

        /// <summary>
        /// Sets options on the graphics object using this instance's property values
        /// </summary>
        /// <param name="graphics">The graphics object to configure</param>
        public void InitializeGraphics(Graphics graphics)
        {
            graphics.InterpolationMode = this.InterpolationMode;
            graphics.SmoothingMode = this.SmoothingMode;
            graphics.PixelOffsetMode = this.PixelOffsetMode;
            graphics.CompositingQuality = this.CompositingQuality;
        }
    }
}
