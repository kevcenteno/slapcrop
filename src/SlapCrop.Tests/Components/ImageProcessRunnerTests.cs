using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web;
using System.IO;
using Moq;
using SlapCrop.Tests.Mocks;
using System.Drawing;
using System.Configuration;

namespace SlapCrop.Tests.Components
{
    [TestClass]
    public class ImageProcessRunnerTests
    {
        [TestMethod]
        public void ImageProcessRunner_DeterminesMimeTypeFromExtension()
        {
            // jpeg, gif and png
            this.VerifyContentType("http://localhost:8080/assets/square.jpg", "image/jpeg");
            this.VerifyContentType("http://localhost:8080/assets/sample.gif", "image/gif");
            this.VerifyContentType("http://localhost:8080/assets/sample.png", "image/png");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpException))]
        public void ImageProcessRunner_ReturnsA404WhenImageIsNotFound()
        {
            var request = new HttpRequest("somehandler.ashx", "http://localhost:8080/assets/NOTREAL.jpg", "");

            this.MockRequest(request, (ms, mockResponse, mockContext) =>
            {
                var context = mockContext.Object;
                var subject = new TestImageRunner(context);

                subject.HandleRequest();
            });
        }

        #region [Scaling Only Functionality]

        [TestMethod]
        public void ImageProcessRunner_ReturnsSourceImageWhenNoParametersSupplied()
        {
            var request = new HttpRequest("somehandler.ashx", "http://localhost:8080/assets/square.jpg", "");

            this.MockRequest(request, (ms, mockResponse, mockContext) =>
            {
                mockResponse.Setup(m => m.StatusCode).Returns(200);
                mockResponse.Setup(m => m.ContentType).Returns("image/jpeg");

                var context = mockContext.Object;
                new TestImageRunner(context).HandleRequest();

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
        public void ImageProcessRunner_ReturnsLetterboxedImageWhenSizesAreOfDifferentAspectRatio()
        {
            var request = new HttpRequest("somehandler.ashx", "http://localhost:8080/assets/square.jpg", "sz=60x60;90x90;120x120;180x120");

            this.MockRequest(request, (ms, mockResponse, mockContext) =>
            {
                mockResponse.Setup(m => m.StatusCode).Returns(200);
                mockResponse.Setup(m => m.ContentType).Returns("image/jpeg");

                var context = mockContext.Object;
                new TestImageRunner(context).HandleRequest();
                
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
        public void ImageProcessRunner_ReturnsLargestImageWhenSizesSpecifiedWithoutQueryBreakPoint()
        {
            var request = new HttpRequest("somehandler.ashx", "http://localhost:8080/assets/square.jpg", "sz=60x60;90x90;120x120;180x180");

            this.MockRequest(request, (ms, mockResponse, mockContext) =>
            {
                mockResponse.Setup(m => m.StatusCode).Returns(200);
                mockResponse.Setup(m => m.ContentType).Returns("image/jpeg");

                var context = mockContext.Object;
                new TestImageRunner(context).HandleRequest();

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
        public void ImageProcessRunner_ReturnsScaledImageWhenSizesSpecifiedWithQueryBreakPoint()
        {
            var request = new HttpRequest("somehandler.ashx", "http://localhost:8080/assets/square.jpg", "sz=60x60;90x90;120x120;180x180&mbr=1");
            
            this.MockRequest(request, (ms, mockResponse, mockContext) =>
            {
                mockResponse.Setup(m => m.StatusCode).Returns(200);
                mockResponse.Setup(m => m.ContentType).Returns("image/jpeg");

                var context = mockContext.Object;
                new TestImageRunner(context).HandleRequest();

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

        #region [Cropping Tests]

        [TestMethod]
        public void ImageProcessRunner_ScalesAndCropsImage()
        {
            var request = new HttpRequest("somehandler.ashx", "http://localhost:8080/assets/square.jpg", "sz=90x90;100x100&cropspec=bl,0.5,0.5&mbr=0");

            this.MockRequest(request, (ms, mockResponse, mockContext) =>
            {
                mockResponse.Setup(m => m.StatusCode).Returns(200);
                mockResponse.Setup(m => m.ContentType).Returns("image/jpeg");

                var context = mockContext.Object;
                new TestImageRunner(context).HandleRequest();

                ms.Seek(0, SeekOrigin.Begin);
                Assert.AreEqual(200, context.Response.StatusCode);
                Assert.AreEqual("image/jpeg", context.Response.ContentType);

                var image = Bitmap.FromStream(context.Response.OutputStream) as Bitmap;
                Assert.AreEqual(45, image.Width);
                Assert.AreEqual(45, image.Height);
                Assert.AreEqual(0, ResHelper.PixelCount(image, Color.Black));
            });
        }

        [TestMethod]
        public void ImageProcessRunner_ScalesAndCropsImageExceptWhenToldToSkip()
        {
            var request = new HttpRequest("somehandler.ashx", "http://localhost:8080/assets/square.jpg", "sz=90x90;100x100&cropspec=skip;tc,0.5,0.5&mbr=0");

            this.MockRequest(request, (ms, mockResponse, mockContext) =>
            {
                mockResponse.Setup(m => m.StatusCode).Returns(200);
                mockResponse.Setup(m => m.ContentType).Returns("image/jpeg");

                var context = mockContext.Object;
                new TestImageRunner(context).HandleRequest();

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

        #region [Support Methods]

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
                new TestImageRunner(context).HandleRequest();

                Assert.AreEqual(200, context.Response.StatusCode);
                Assert.AreEqual(mimeType, context.Response.ContentType);
            });
        }

        #endregion
    }
}
