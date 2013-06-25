using System;
using System.IO;
using System.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SlapCrop.Remote;

namespace SlapCrop.Remote.Tests
{
    [TestClass]
    public class SampleImageProcessRunnerTests
    {
        [TestMethod]
        [ExpectedException(typeof(HttpException))]
        public void RemoteImageProcessRunner_GetSourceImage_NoSrc()
        {
            VerifySourceImage(new HttpRequest("slapcrop/", "http://localhost:35606/", ""));
        }

        [TestMethod]
        public void RemoteImageProcessRunner_GetSourceImage_GoodSrc()
        {
            VerifySourceImage(new HttpRequest("slapcrop/", "http://localhost:35606/", "src=http://placehold.it/300.gif"));
            VerifySourceImage(new HttpRequest("slapcrop/", "http://localhost:35606/", "src=/Images/300.gif"));
        }

        [TestMethod]
        public void RemoteImageProcessRunner_SetMimeType()
        {
            this.VerifyContentType("src=http://placehold.it/350x150.gif", "image/gif");
            this.VerifyContentType("src=http://placehold.it/350x150.jpeg", "image/jpeg");
            this.VerifyContentType("src=http://placehold.it/350x150.jpg", "image/jpeg");
            this.VerifyContentType("src=http://placehold.it/350x150.png", "image/png");
        }

        private void VerifySourceImage(HttpRequest request)
        {
            this.MockRequest(request, (ms, mockResponse, mockContext) =>
            {
                mockResponse.Setup(m => m.StatusCode).Returns(200);
                mockResponse.Setup(m => m.ContentType).Returns("image/gif");

                var context = mockContext.Object;
                var subject = new RemoteImageProcessRunner(context, "D:\\git\\slapcrop\\src\\SlapCrop.Remote.Tests");

                subject.HandleRequest();
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

        private void VerifyContentType(string queryString, string mimeType)
        {
            var request = new HttpRequest("slapcrop/", "http://localhost:35606/", queryString);

            this.MockRequest(request, (ms, mockResponse, mockContext) =>
            {
                mockResponse.Setup(m => m.StatusCode).Returns(200);
                mockResponse.Setup(m => m.ContentType).Returns(mimeType);

                var context = mockContext.Object;
                new RemoteImageHandler().ProcessRequest(context);
                Assert.AreEqual(200, context.Response.StatusCode);
                Assert.AreEqual(mimeType, context.Response.ContentType);
            });
        }
    }
}
