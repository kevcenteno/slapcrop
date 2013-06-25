using System.Web;

namespace SlapCrop.Remote
{
    public class RemoteImageHandler : ImageHandler
    {
        private System.Web.Routing.RequestContext requestContext;
        private string _applicationPath;

        public RemoteImageHandler()
            : base()
        {
        }

        public RemoteImageHandler(string applicationPath)
            : base()
        {
            _applicationPath = applicationPath;
        }

        public RemoteImageHandler(System.Web.Routing.RequestContext requestContext)
        {
            this.requestContext = requestContext;
            _applicationPath = requestContext.HttpContext.Request.PhysicalApplicationPath;
        }

        protected override ImageProcessRunner GetProcessRunner(HttpContextBase context)
        {
            return new RemoteImageProcessRunner(context, _applicationPath);
        }
    }
}
