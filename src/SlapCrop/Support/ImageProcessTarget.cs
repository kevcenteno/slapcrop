using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlapCrop
{
    public class ImageProcessTarget
    {
        public static readonly ImageProcessTarget Empty = new ImageProcessTarget(Size.Empty, ImageCropType.None, 0, 0);

        public Size ScaleSize { get; private set; }

        public Size CropSize { get; private set; }

        public ImageCropType CropType { get; private set; }        
             
        public ImageProcessTarget(Size targetSize, ImageCropType cropType, float cropWidthPercentage, float cropHeightPercentage)
        {
            this.ScaleSize = targetSize;
            this.CropType = cropType;

            this.CropSize = new Size(
                    (int)Math.Round(this.ScaleSize.Width * cropWidthPercentage, 0),
                    (int)Math.Round(this.ScaleSize.Height * cropHeightPercentage, 0)
                );
        }        
    }
}
