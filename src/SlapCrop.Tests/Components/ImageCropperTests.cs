using Microsoft.VisualStudio.TestTools.UnitTesting;
using SlapCrop;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlapCrop.Tests.Components
{
    [TestClass]
    public class ImageCropperTests
    {
        private ImageCropper subject = new ImageCropper();

        [TestMethod]
        public void ImageCropper_ReturnsOriginalWhenWidthIsTooLarge()
        {
            var source = ResHelper.LoadImage("square.jpg");
            Assert.AreEqual(300, source.Width);
            Assert.AreEqual(300, source.Height);

            var result = this.subject.Crop(source, new Rectangle(0, 0, 301, 100));
            Assert.AreEqual(source, result);

            // even though we have a 300x300, this will be too big
            result = this.subject.Crop(source, new Rectangle(100, 0, 201, 100));
            Assert.AreEqual(source, result);
        }

        [TestMethod]
        public void ImageCropper_ReturnsOriginalWhenHeightIsTooLarge()
        {
            var source = ResHelper.LoadImage("square.jpg");

            var result = this.subject.Crop(source, new Rectangle(0, 0, 100, 301));
            Assert.AreEqual(source, result);

            // even though we have a 300x300, this will be too big
            result = this.subject.Crop(source, new Rectangle(0, 100, 100, 201));
            Assert.AreEqual(source, result);
        }

        [TestMethod]
        public void ImageCropper_ReturnsCenterRect()
        {
            var source = ResHelper.LoadImage("croppable.jpg");

            var result = this.subject.Crop(source, ImageCropType.Center, 100, 100);
            Assert.IsNotNull(result);

            // allow epsilon of width+height for rounding
            Assert.AreEqual(0, ResHelper.PixelCount(result, Color.White), result.Width + result.Height);
            Assert.AreEqual(100 * 100, ResHelper.PixelCount(result, Color.Black), result.Width + result.Height);
        }

        [TestMethod]
        public void ImageCropper_ReturnsTopLeftRect()
        {            
            var blackRect = new Rectangle(50, 50, 50, 50);
            this.CropCornerAndTestForBlack(ImageCropType.TopLeft, blackRect);
        }

        [TestMethod]
        public void ImageCropper_ReturnsBottomLeftRect()
        {            
            var blackRect = new Rectangle(50, 0, 50, 50);
            this.CropCornerAndTestForBlack(ImageCropType.BottomLeft, blackRect);
        }

        [TestMethod]
        public void ImageCropper_ReturnsTopRightRect()
        {
            var blackRect = new Rectangle(0, 50, 50, 50);
            this.CropCornerAndTestForBlack(ImageCropType.TopRight, blackRect);
        }

        [TestMethod]
        public void ImageCropper_ReturnsBottomRightRect()
        {
            var blackRect = new Rectangle(0, 0, 50, 50);
            this.CropCornerAndTestForBlack(ImageCropType.BottomRight, blackRect);
        }

        [TestMethod]
        public void ImageCropper_ReturnsLeftCenterRect()
        {
            var blackRect = new Rectangle(50, 0, 50, 100);
            this.CropCornerAndTestForBlack(ImageCropType.LeftCenter, blackRect, 0.5f);
        }

        [TestMethod]
        public void ImageCropper_ReturnsTopCenterRect()
        {
            var blackRect = new Rectangle(0, 50, 100, 50);
            this.CropCornerAndTestForBlack(ImageCropType.TopCenter, blackRect, 0.5f);
        }

        [TestMethod]
        public void ImageCropper_ReturnsRightCenterRect()
        {
            var blackRect = new Rectangle(0, 0, 50, 100);
            this.CropCornerAndTestForBlack(ImageCropType.RightCenter, blackRect, 0.5f);
        }

        [TestMethod]
        public void ImageCropper_ReturnsBottomCenterRect()
        {
            var blackRect = new Rectangle(0, 0, 100, 50);
            this.CropCornerAndTestForBlack(ImageCropType.BottomCenter, blackRect, 0.5f);
        }

        private void CropCornerAndTestForBlack(ImageCropType type, Rectangle blackRect, float blackPortion = 0.25f)
        {
            var source = ResHelper.LoadImage("croppable.jpg");

            var result = this.subject.Crop(source, type, 100, 100);
            Assert.IsNotNull(result);

            // allowing 2 * width as epsilon to handle integer division / rounding
            Assert.AreEqual(result.Width * result.Height * (1 - blackPortion), ResHelper.PixelCount(result, Color.White), result.Width * 2);
            Assert.AreEqual(result.Width * result.Height * blackPortion, ResHelper.PixelCount(result, Color.Black, blackRect), result.Width * 2);
        }
    }
}
