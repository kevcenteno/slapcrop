using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using Moq;
using System.Configuration;

namespace SlapCrop.Tests
{
    [TestClass]
    public class ImageHandlerTests
    {
        [TestMethod]
        public void ImageHandler_SetsContentTypeBasedOnImageFileExtension()
        {            
            // jpeg, gif and png
            this.VerifyContentType("http://localhost:8080/assets/square.jpg", "image/jpeg");
            this.VerifyContentType("http://localhost:8080/assets/sample.gif", "image/gif");
            this.VerifyContentType("http://localhost:8080/assets/sample.png", "image/png");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpException))]
        public void ImageHandler_Returns404WhenImageIsNotFound()
        {
            var request = new HttpRequest("somehandler.ashx", "http://localhost:8080/assets/NOTREAL.jpg", "");

            this.MockRequest(request, (ms, mockResponse, mockContext) =>
            {
                var context = mockContext.Object;
                new TestImageHandler().ProcessRequest(context);
            });
        }

        #region [Scaling Request Tests]

        [TestMethod]
        public void ImageHandler_ReturnsSourceImageWhenNoParametersSupplied()
        {
            var request = new HttpRequest("somehandler.ashx", "http://localhost:8080/assets/square.jpg", "");

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
                Assert.AreEqual(300, image.Width);
                Assert.AreEqual(300, image.Height);
                Assert.AreEqual(0, ResHelper.PixelCount(image, Color.Black));
                Assert.AreEqual(300 * 300, ResHelper.PixelCount(image, Color.White));
            });
        }

        [TestMethod]
        public void ImageHandler_ReturnsLetterboxedImageWhenSizesAreOfDifferentAspectRatio()
        {
            var request = new HttpRequest("somehandler.ashx", "http://localhost:8080/assets/square.jpg", "sz=60x60;90x90;120x120;180x120");

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
                Assert.AreEqual(180, image.Width);
                Assert.AreEqual(120, image.Height);

                var config = ConfigurationManager.GetSection("SlapCrop") as ImageConfigSection;
                Assert.IsNotNull(config);

                var color = ColorUtil.MakeColorFromHex(config.FillColor);
                Assert.AreNotEqual(0, ResHelper.PixelCount(image, color));
            });
        }

        [TestMethod]
        public void ImageHandler_ReturnsLargestImageWhenSizesSpecifiedWithoutQueryBreakPoint()
        {
            var request = new HttpRequest("somehandler.ashx", "http://localhost:8080/assets/square.jpg", "sz=60x60;90x90;120x120;180x180");

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
                Assert.AreEqual(180, image.Width);
                Assert.AreEqual(180, image.Height);
                Assert.AreEqual(0, ResHelper.PixelCount(image, Color.Black));
            });
        }

        [TestMethod]
        public void ImageHandler_ReturnsScaledImageWhenSizesSpecifiedWithQueryBreakPoint()
        {
            var request = new HttpRequest("somehandler.ashx", "http://localhost:8080/assets/square.jpg", "sz=60x60;90x90;120x120;180x180&qbr=1");

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
                Assert.AreEqual(90, image.Width);
                Assert.AreEqual(90, image.Height);
                Assert.AreEqual(0, ResHelper.PixelCount(image, Color.Black));
            });
        }

        #endregion

        #region [Image Cropping Tests]

        [TestMethod]
        public void ImageHandler_ReturnsCroppedImageForMaxSizeWithoutQueryBreakPoint()
        {
            var queryString = "sz=10x10;20x20;30x30;100x100&crop=c";

            this.ValidateCroppedImage(queryString, new Size(100, 100), (image) =>
            {
                // should be all black (allowing 100 as epsilon in case of rounding)
                Assert.AreEqual(0, ResHelper.PixelCount(image, Color.White), image.Width); 
            });
        }

        [TestMethod]
        public void ImageHandler_ReturnsCroppedImageForSize_TopLeft()
        {
            var queryString = "sz=10x10;20x20;100x100;200x200&crop=tl&qbr=2";

            this.ValidateCroppedImage(queryString, new Size(100, 100), (image) =>
            {
                // bottom right quadrant should be black 
                var rect = new Rectangle(50, 50, 50, 50);                
                Assert.AreEqual(rect.Width * rect.Height, ResHelper.PixelCount(image, Color.Black, rect), rect.Width + rect.Height);
            });
        }

        [TestMethod]
        public void ImageHandler_ReturnsCroppedImageForSize_TopCenter()
        {
            var queryString = "sz=10x10;20x20;100x100;200x200&crop=tc&qbr=2";

            this.ValidateCroppedImage(queryString, new Size(100, 100), (image) =>
            {                
                // bottom half should be black
                var rect = new Rectangle(0, 50, 100, 50);
                Assert.AreEqual(rect.Width * rect.Height, ResHelper.PixelCount(image, Color.Black, rect), image.Width * 2);
            });
        }

        [TestMethod]
        public void ImageHandler_ReturnsCroppedImageForSize_TopRight()
        {
            var queryString = "sz=10x10;20x20;100x100;200x200&crop=tr&qbr=2";

            this.ValidateCroppedImage(queryString, new Size(100, 100), (image) =>
            {
                // bottom left quadrant should be black
                var rect = new Rectangle(0, 50, 50, 50);
                Assert.AreEqual(rect.Width * rect.Height, ResHelper.PixelCount(image, Color.Black, rect), image.Width * 2);
            });
        }

        #endregion

        #region [Support]

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

        class TestImageHandler : ImageHandler
        {
            protected override ImageProcessRunner GetProcessRunner(HttpContextBase context)
            {                
                return new TestImageRunner(context);
            }
        }

        class TestImageRunner : ImageProcessRunner
        {
            public TestImageRunner(HttpContextBase context)
                : base(context)
            {
            }

            protected override Bitmap GetSourceImage()
            {
                var filename = Path.GetFileName(this.Context.Request.Path);
                try
                {
                    return ResHelper.LoadImage(filename);
                }
                catch (ArgumentException)
                {
                    return null;
                }
            }
        }

        #endregion
    }
}
