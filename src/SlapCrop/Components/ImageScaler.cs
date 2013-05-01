using System;
using System.Drawing;

namespace SlapCrop
{
    /// <summary>
    /// A utility class for scaling bitmaps
    /// </summary>
    public sealed class ImageScaler
    {
        /// <summary>
        /// Options for Graphics (GDI+) object and defaults
        /// </summary>
        public ImageScalerOptions Options { get; private set; }

        public ImageScaler(ImageScalerOptions options = null)
        {
            this.Options = options ?? new ImageScalerOptions();
        }

        /// <summary>
        /// Scales an image to the target width and height
        /// <para>
        /// If either the width or the height supplied are larger than the respective values of <paramref name="image"/>, 
        /// a copy of the original image will be returned.
        /// </para>
        /// <para>
        /// If the width and height maintain the original aspect ratio a new image will be returned with an scaled version of the original
        /// </para>
        /// <para>
        /// If the width and height change the aspect ratio, an image of width X height will be returned, however it will be letterboxed and filled 
        /// with the color defined in <see cref="Options.DefaultFillColor"/>
        /// </para>
        /// </summary>
        /// <param name="image">The image to scale</param>
        /// <param name="width">The new width</param>
        /// <param name="height">The new height</param>
        /// <returns>The scaled image or <paramref name="image"/></returns>
        public Bitmap Scale(Bitmap image, int width, int height)
        {
            // guard clause for up-scaling: return a copy of the original
            if (this.IsLargerThanSource(image, width, height) || new Size(width, height) == Size.Empty)
            {
                return new Bitmap(image);
            }

            var scaled = new Bitmap(width, height);

            if (this.IsOriginalAspectRatio(image, width, height))
            {
                this.ScaleOriginalAspectRatio(image, scaled);
            }
            else
            {
                this.ScaleNewAspectRatio(image, scaled);
            }

            return scaled;
        }

        private bool IsLargerThanSource(Bitmap image, int width, int height)
        {
            return width >= image.Width && height >= image.Height;
        }

        private bool IsLandscape(Bitmap image)
        {
            return image.Width > image.Height;
        }

        private bool IsOriginalAspectRatio(Bitmap image, int width, int height)
        {
            float originalRatio = (float)image.Height / image.Width;
            float newRatio = (float)height / width;

            return originalRatio == newRatio;
        }

        private void ScaleOriginalAspectRatio(Bitmap source, Bitmap scaled)
        {
            using (var graphics = Graphics.FromImage(scaled))
            {
                this.Options.InitializeGraphics(graphics);

                //draw the source as a resize image
                graphics.DrawImage(source, new Rectangle(0, 0, scaled.Width, scaled.Height));
                graphics.Flush();
            }
        }

        private void ScaleNewAspectRatio(Bitmap image, Bitmap scaled)
        {
            using (var graphics = Graphics.FromImage(scaled))
            {
                this.Options.InitializeGraphics(graphics);

                //background
                graphics.FillRectangle(new SolidBrush(this.Options.DefaultFillColor), new Rectangle(0, 0, scaled.Width, scaled.Height));

                // assume portrait
                float ratio = (float)scaled.Height / image.Height;
                var newHeight = scaled.Height;
                var newWidth = (int)Math.Round(image.Width * ratio, 0);

                var x = (int)Math.Round((scaled.Width - newWidth) / 2.0, 0);
                var y = 0;

                if (IsLandscape(image))
                {
                    ratio = (float)scaled.Width / image.Width;
                    newWidth = scaled.Width;
                    newHeight = (int)Math.Round(image.Height * ratio, 0);

                    x = 0;
                    y = (int)Math.Round((scaled.Height - newHeight) / 2.0, 0);
                }

                // scale source image
                graphics.DrawImage(image, new Rectangle(x, y, newWidth, newHeight));
                graphics.Flush();
            }
        }        
    }
}
