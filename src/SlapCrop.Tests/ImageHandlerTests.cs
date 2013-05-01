using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using Moq;
using System.Configuration;
using SlapCrop.Tests.Mocks;

namespace SlapCrop.Tests
{
    [TestClass]
    public class ImageHandlerTests
    {
        // the maximum number of pixels we can be off by (due to rounding)
        private int _acceptableVariance = 200;

        #region [Image Scaler Tests]

        [TestMethod]
        [ExpectedException(typeof(HttpException))]
        public void ImageHandler_Returns400WhenRequestedWidthIsLargerThanSource()
        {
            // source image is 200x200, last value (requested) is too big
            var queryString = "sz=10x10;20x20;30x30;300x200&cropspec=c,.5,.5";

            // a 404 should be thrown here...
            this.ValidateCroppedImage(queryString, new Size(200, 200), (image) =>
            {
                // should never get here...
                Assert.IsFalse(true);
            });
        }

        [TestMethod]
        public void ImageHandler_Returns2xScaledImageWhenRequested()
        {
            // source image is 200x200, last value (requested) is too big
            var queryString = "sz=10x10;20x20;30x30;100x100&mbr=desktop@2x";

            // a 404 should be thrown here...
            this.ValidateCroppedImage(queryString, new Size(200, 200), (image) => { });
        }

        [TestMethod]
        public void ImageHandler_Returns2xScaledAndCroppedImageWhenRequested()
        {
            // source image is 200x200, last value (requested) is too big
            var queryString = "sz=10x10;20x20;30x30;100x100&cropspec=c,.5,.5&mbr=desktop@2x";

            // a 404 should be thrown here...
            this.ValidateCroppedImage(queryString, new Size(100, 100), (image) => { });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpException))]
        public void ImageHandler_Returns400WhenScaledSizeIsTooLarge()
        {
            // source image is 200x200, last value (requested) is too big
            var queryString = "sz=10x10;20x20;30x30;100x100&mbr=desktop@3x";

            // a 404 should be thrown here...
            this.ValidateCroppedImage(queryString, new Size(200, 200), (image) => { });
        }

        #endregion

        #region [Image Cropping Tests]

        [TestMethod]
        public void ImageHandler_ReturnsCroppedImageForMaxSizeWithoutQueryBreakPoint()
        {
            var queryString = "sz=10x10;20x20;30x30;200x200&cropspec=c,.5,.5";

            this.ValidateCroppedImage(queryString, new Size(100, 100), (image) =>
            {
                // should be all black 
                Assert.AreEqual(0, ResHelper.PixelCount(image, Color.White), this._acceptableVariance);
            });
        }

        [TestMethod]
        public void ImageHandler_ReturnsCroppedImageForSize_TopLeft()
        {
            var queryString = "sz=10x10;20x20;200x200;200x200&cropspec=skip;skip;tl,.5,.5;skip&mbr=ipad-landscape";

            this.ValidateCroppedImage(queryString, new Size(100, 100), (image) =>
            {
                // bottom right quadrant should be black 
                var rect = this.GetImagePart(image.Size, ImagePart.BottomRightQuadrant);                
                Assert.AreEqual(rect.Width * rect.Height, ResHelper.PixelCount(image, Color.Black, rect), this._acceptableVariance);

                // left half and bottom right should be white
                rect = this.GetImagePart(image.Size, ImagePart.LeftHalf);
                Assert.AreEqual(rect.Width * rect.Height, ResHelper.PixelCount(image, Color.White, rect), this._acceptableVariance);

                rect = this.GetImagePart(image.Size, ImagePart.TopRightQuandrant);
                Assert.AreEqual(rect.Width * rect.Height, ResHelper.PixelCount(image, Color.White, rect), this._acceptableVariance);
            });
        }

        [TestMethod]
        public void ImageHandler_ReturnsCroppedImageForSize_TopCenter()
        {
            var queryString = "sz=10x10;20x20;100x100;200x200&cropspec=tc,.5,.5&mbr=desktop";

            this.ValidateCroppedImage(queryString, new Size(100, 100), (image) =>
            {
                // bottom half should be black
                var rect = this.GetImagePart(image.Size, ImagePart.BottomHalf);
                Assert.AreEqual(rect.Width * rect.Height, ResHelper.PixelCount(image, Color.Black, rect), this._acceptableVariance);

                // top half should be white
                rect = this.GetImagePart(image.Size, ImagePart.TopHalf);
                Assert.AreEqual(rect.Width * rect.Height, ResHelper.PixelCount(image, Color.White, rect), this._acceptableVariance);
            });
        }

        [TestMethod]
        public void ImageHandler_ReturnsCroppedImageForSize_TopRight()
        {
            var queryString = "sz=10x10;20x20;100x100;200x200&cropspec=tr,.5,.5&mbr=desktop";

            this.ValidateCroppedImage(queryString, new Size(100, 100), (image) =>
            {
                // bottom left quadrant should be black
                var rect = this.GetImagePart(image.Size, ImagePart.BottomLeftQuadrant);
                Assert.AreEqual(rect.Width * rect.Height, ResHelper.PixelCount(image, Color.Black, rect), this._acceptableVariance);

                // top half and bottom right quadrant should be white
                rect = this.GetImagePart(image.Size, ImagePart.TopHalf);
                Assert.AreEqual(rect.Width * rect.Height, ResHelper.PixelCount(image, Color.White, rect), this._acceptableVariance);

                rect = this.GetImagePart(image.Size, ImagePart.BottomRightQuadrant);
                Assert.AreEqual(rect.Width * rect.Height, ResHelper.PixelCount(image, Color.White, rect), this._acceptableVariance);
            });
        }

        [TestMethod]
        public void ImageHandler_ReturnsCroppedImageForSize_LeftCenter()
        {
            var queryString = "sz=10x10;20x20;100x100;200x200&cropspec=lc,.5,.5&mbr=ipad-landscape";

            this.ValidateCroppedImage(queryString, new Size(50, 50), (image) =>
            {
                // right half should be black
                var rect = this.GetImagePart(image.Size, ImagePart.RightHalf);
                Assert.AreEqual(rect.Width * rect.Height, ResHelper.PixelCount(image, Color.Black, rect), this._acceptableVariance);

                // left half should be white
                rect = this.GetImagePart(image.Size, ImagePart.LeftHalf);
                Assert.AreEqual(rect.Width * rect.Height, ResHelper.PixelCount(image, Color.White, rect), this._acceptableVariance);
            });
        }

        [TestMethod]
        public void ImageHandler_ReturnsCroppedImageForSize_Center()
        {
            var queryString = "sz=10x10;20x20;100x100;200x200&cropspec=c,.5,.5&mbr=ipad-landscape";

            this.ValidateCroppedImage(queryString, new Size(50, 50), (image) =>
            {
                // full image should be black
                var rect = new Rectangle(0, 0, image.Width, image.Height);                
                Assert.AreEqual(rect.Width * rect.Height, ResHelper.PixelCount(image, Color.Black, rect), this._acceptableVariance);                
            });
        }

        [TestMethod]
        public void ImageHandler_ReturnsCroppedImageForSize_RightCenter()
        {
            var queryString = "sz=10x10;20x20;100x100;200x200&cropspec=rc,.5,.5&mbr=ipad-landscape";

            this.ValidateCroppedImage(queryString, new Size(50, 50), (image) =>
            {
                // left half should be black
                var rect = this.GetImagePart(image.Size, ImagePart.LeftHalf);
                Assert.AreEqual(rect.Width * rect.Height, ResHelper.PixelCount(image, Color.Black, rect), this._acceptableVariance);

                // right half should be white
                rect = this.GetImagePart(image.Size, ImagePart.RightHalf);
                Assert.AreEqual(rect.Width * rect.Height, ResHelper.PixelCount(image, Color.White, rect), this._acceptableVariance);
            });
        }

        [TestMethod]
        public void ImageHandler_ReturnsCroppedImageForSize_BottomLeft()
        {
            var queryString = "sz=10x10;20x20;100x100;200x200&cropspec=bl,.5,.5&mbr=ipad-landscape";

            this.ValidateCroppedImage(queryString, new Size(50, 50), (image) =>
            {
                // top right quadrant should be black
                var rect = this.GetImagePart(image.Size, ImagePart.TopRightQuandrant);
                Assert.AreEqual(rect.Width * rect.Height, ResHelper.PixelCount(image, Color.Black, rect), this._acceptableVariance);

                // top left and bottom half should be white
                rect = this.GetImagePart(image.Size, ImagePart.TopLeftQuadrant);
                Assert.AreEqual(rect.Width * rect.Height, ResHelper.PixelCount(image, Color.White, rect), this._acceptableVariance);

                rect = this.GetImagePart(image.Size, ImagePart.BottomHalf);
                Assert.AreEqual(rect.Width * rect.Height, ResHelper.PixelCount(image, Color.White, rect), this._acceptableVariance);
            });
        }

        [TestMethod]
        public void ImageHandler_ReturnsCroppedImageForSize_BottomCenter()
        {
            var queryString = "sz=10x10;20x20;100x100;200x200&cropspec=bc,.5,.5&mbr=ipad-landscape";

            this.ValidateCroppedImage(queryString, new Size(50, 50), (image) =>
            {
                // top half should be black
                var rect = this.GetImagePart(image.Size, ImagePart.TopHalf);
                Assert.AreEqual(rect.Width * rect.Height, ResHelper.PixelCount(image, Color.Black, rect), this._acceptableVariance);

                // bottom half should be white
                rect = this.GetImagePart(image.Size, ImagePart.BottomHalf);
                Assert.AreEqual(rect.Width * rect.Height, ResHelper.PixelCount(image, Color.White, rect), this._acceptableVariance);
            });
        }

        [TestMethod]
        public void ImageHandler_ReturnsCroppedImageForSize_BottomRight()
        {
            var queryString = "sz=10x10;20x20;100x100;200x200&cropspec=br,.5,.5&mbr=ipad-landscape";

            this.ValidateCroppedImage(queryString, new Size(50, 50), (image) =>
            {
                // top left quadrant should be black
                var rect = this.GetImagePart(image.Size, ImagePart.TopLeftQuadrant);
                Assert.AreEqual(rect.Width * rect.Height, ResHelper.PixelCount(image, Color.Black, rect), this._acceptableVariance);

                // bottom half and top right quadrant should be white
                rect = this.GetImagePart(image.Size, ImagePart.BottomHalf);
                Assert.AreEqual(rect.Width * rect.Height, ResHelper.PixelCount(image, Color.White, rect), this._acceptableVariance);

                rect = this.GetImagePart(image.Size, ImagePart.TopRightQuandrant);
                Assert.AreEqual(rect.Width * rect.Height, ResHelper.PixelCount(image, Color.White, rect), this._acceptableVariance);
            });
        }

        #endregion

        #region [Support Methods]

        enum ImagePart
        {
            TopHalf,
            BottomHalf,
            LeftHalf,
            RightHalf,
            TopLeftQuadrant,
            TopRightQuandrant,
            BottomLeftQuadrant,
            BottomRightQuadrant
        }

        private Rectangle GetImagePart(Size size, ImagePart part)
        {
            var rect = new Rectangle(0, 0, size.Width, size.Height);

            switch(part)
            {
                case ImagePart.TopHalf:
                    rect.Height /= 2;
                    break;
                case ImagePart.BottomHalf:                    
                    rect.Height /= 2;
                    rect.Y += rect.Height;
                    break;
                case ImagePart.LeftHalf:
                    rect.Width /= 2;
                    break;
                case ImagePart.RightHalf:
                    rect.Width /= 2;
                    rect.X += rect.Width;
                    break;
                case ImagePart.TopLeftQuadrant:
                    rect.Width /= 2;
                    rect.Height /= 2;
                    break;
                case ImagePart.TopRightQuandrant:
                    rect.Width /= 2;
                    rect.Height /= 2;
                    rect.X += rect.Width;
                    break;
                case ImagePart.BottomLeftQuadrant:
                    rect.Width /= 2;
                    rect.Height /= 2;
                    rect.Y += rect.Height;
                    break;
                case ImagePart.BottomRightQuadrant:
                    rect.Width /= 2;
                    rect.Height /= 2;
                    rect.X += rect.Width;
                    rect.Y += rect.Height;
                    break;
            }

            return rect;
        }

        private void ValidateCroppedImage(string queryString, Size expectedSize, Action<Bitmap> postProcess, string imageFile = "croppable.jpg")
        {
            var request = new HttpRequest("somehandler.ashx", "http://localhost:8080/assets/" + imageFile, queryString);

            this.MockRequest(request, (ms, mockResponse, mockContext) =>
            {
                mockResponse.Setup(m => m.StatusCode).Returns(200);
                mockResponse.Setup(m => m.ContentType).Returns("image/jpeg");

                var context = mockContext.Object;
                new TestImageHandler().ProcessRequest(context);
                ms.Seek(0, SeekOrigin.Begin);

                Assert.AreEqual(200, context.Response.StatusCode);
                Assert.AreEqual("image/jpeg", context.Response.ContentType);

                var image = Bitmap.FromStream(context.Response.OutputStream) as Bitmap;
                Assert.AreEqual(expectedSize.Width, image.Width);
                Assert.AreEqual(expectedSize.Height, image.Height);

                if (postProcess != null)
                {
                    postProcess.Invoke(image);
                }
            });
        }

        private void MockRequest(HttpRequest request, Action<Stream, Mock<HttpResponseBase>, Mock<HttpContextBase>> testCode)
        {
            using (var ms = new MemoryStream())
            {
                var mockResponse = new Mock<HttpResponseBase>();
                mockResponse.Setup(m => m.OutputStream).Returns(ms);

                var mockContext = new Mock<HttpContextBase>();
                mockContext.Setup(m => m.Request).Returns(new HttpRequestWrapper(request));
                mockContext.Setup(m => m.Response).Returns(mockResponse.Object);

                if (testCode != null)
                {
                    testCode.Invoke(ms, mockResponse, mockContext);
                }
            }
        }

        private void VerifyContentType(string url, string mimeType)
        {
            var request = new HttpRequest("somehandler.ashx", url, "");

            this.MockRequest(request, (ms, mockResponse, mockContext) =>
            {
                mockResponse.Setup(m => m.StatusCode).Returns(200);
                mockResponse.Setup(m => m.ContentType).Returns(mimeType);

                var context = mockContext.Object;
                new TestImageHandler().ProcessRequest(context);
                Assert.AreEqual(200, context.Response.StatusCode);
                Assert.AreEqual(mimeType, context.Response.ContentType);
            });
        }

        #endregion
    }
}
