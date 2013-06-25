using System.Web.Routing;

namespace SlapCrop.Remote
{
    public class RemoteRouteHandler : IRouteHandler
    {
        public System.Web.IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            return new RemoteImageHandler(requestContext);
        }
    }
}
