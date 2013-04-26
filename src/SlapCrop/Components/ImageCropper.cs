using System.Drawing;

namespace SlapCrop
{
    /// <summary>
    /// The available types of crops supported by this library
    /// </summary>
    public enum ImageCropType
    {
        Center,
        TopLeft,
        BottomLeft,
        TopRight,
        BottomRight,
        LeftCenter,
        TopCenter,
        RightCenter,
        BottomCenter,
        None
    }

    /// <summary>
    /// A utility class for cropping images
    /// </summary>
    public sealed class ImageCropper
    {
        /// <summary>
        /// Crops the specified rectangle of the supplied image and returns a new image
        /// <para>
        /// If rectangle.X + rectangle.Width > the width of the supplied image or rectangle.Y + rectangle.Height 
        /// > the height of the supplied image, the original image will be returned
        /// </para>
        /// </summary>
        /// <param name="image">The source image to crop</param>
        /// <param name="rectangle">The rectangle to crop</param>
        /// <returns>A bitmap of the crop or the original supplied image</returns>
        public Bitmap Crop(Bitmap image, Rectangle rectangle)
        {
            var width = rectangle.X + rectangle.Width;
            var height = rectangle.Y + rectangle.Height;

            // guard clause for bad sizes
            if (this.IsSizeOutOfBounds(image, width, height)) return image;

            return image.Clone(rectangle, image.PixelFormat);
        }

        /// <summary>
        /// Crops an image using the specified crop type, width and height
        /// <para>This method makes use of Crop(Bitmap image, Rectangle rectangle), see that method for more information</para>
        /// </summary>
        /// <param name="image">The image to crop</param>
        /// <param name="imageCropType">The type of crop</param>
        /// <param name="width">The desired width</param>
        /// <param name="height">The desired height</param>
        /// <returns>A new image of with the specified crop</returns>
        public Bitmap Crop(Bitmap image, ImageCropType imageCropType, int width, int height)
        {
            var x = 0;
            var y = 0;

            switch (imageCropType)
            {
                case ImageCropType.Center:
                    x = (image.Width - width) / 2;
                    y = (image.Height - height) / 2;
                    break;
                case ImageCropType.TopRight:
                    x = image.Width - width;
                    break;
                case ImageCropType.BottomLeft:
                    y = image.Height - height;
                    break;                    
                case ImageCropType.BottomRight:
                    x = image.Width - width;
                    y = image.Height - height;
                    break;
                case ImageCropType.LeftCenter:
                    y = (image.Height - height) / 2;
                    break;
                case ImageCropType.TopCenter:
                    x = (image.Width - width) / 2;
                    break;
                case ImageCropType.RightCenter:
                    x = image.Width - width;
                    y = (image.Height - height) / 2;
                    break;
                case ImageCropType.BottomCenter:
                    x = (image.Width - width) / 2;
                    y = image.Height - height;
                    break;
                default:
                    // this will be TopLeft (0, 0)
                    break;
            }

            return this.Crop(image, new Rectangle(x, y, width, height));
        }

        private bool IsSizeOutOfBounds(Bitmap image, int width, int height)
        {
            return width > image.Width || height > image.Height;
        }        
    }
}
