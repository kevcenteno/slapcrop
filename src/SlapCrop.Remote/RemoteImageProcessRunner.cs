using System.Drawing;
using System.Net;
using System.Web;

namespace SlapCrop.Remote
{
    public class RemoteImageProcessRunner : ImageProcessRunner
    {
        private HttpContextBase _context;
        private string _applicationPath;
        private string _imgSrc;
        private int _remoteTimeout;

        public RemoteImageProcessRunner(HttpContextBase context, string applicationPath)
            : base(context)
        {
            _context = context;
            _applicationPath = applicationPath;
            _imgSrc = _context.Request.QueryString["src"];
            _remoteTimeout = 1000;
        }

        protected override Bitmap GetSourceImage()
        {
            // this is a bad request if no src has been set
            if (string.IsNullOrEmpty(_imgSrc))
            {
                return null;
            }

            // if the image request is from a remote server, request the image and return that
            if (_imgSrc.StartsWith("http"))
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(_imgSrc);
                httpWebRequest.Timeout = _remoteTimeout;

                return (Bitmap)Image.FromStream(httpWebRequest.GetResponse().GetResponseStream());
            }
            // otherwise the request is for a local image, so return that
            else
            {
                return (Bitmap)Image.FromFile(_applicationPath + _imgSrc);
            }
        }

        protected override void SetMimeType(HttpResponseBase response)
        {
            var ext = _imgSrc.Substring(_imgSrc.IndexOf('.') + 1).ToLower();
            var type = "image/jpeg";

            if (ext.Length == 3 && ext != "jpg")
            {
                type = "image/" + ext;
            }

            response.ContentType = type;
        }
    }
}
