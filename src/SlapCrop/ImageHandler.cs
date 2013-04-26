using System.Web;

namespace SlapCrop
{
    /// <summary>
    /// A HttpHandler for processing image scaling/cropping requests. This class delegates all processing 
    /// to an instance of <see cref="ImageProcessRunner"/> supplied by <see cref="ImageHandler.GetProcessRunner"/>.
    /// </summary>
    /// <remarks>To change the ImageProcessRunner, simple subclass this class and override the GetProcessRunner method</remarks>
    public class ImageHandler : IHttpHandler
    {
        /// <summary>
        /// Process the request. This method simply calls the equivalent method
        /// wrapping the <paramref name="context"/>
        /// </summary>
        /// <param name="context">The current request context</param>
        public void ProcessRequest(HttpContext context)
        {
            this.ProcessRequest(new HttpContextWrapper(context));
        }

        /// <summary>
        /// Processes the current image scaling/cropping request.
        /// </summary>
        /// <param name="context">The current request context</param>
        /// <remarks>
        /// This method delegates all processing to an instance of <see cref="ImageProcessRunner"/>. This instance is 
        /// retrieved from <see cref="ImageHandler.GetProcessRunner"/>
        /// </remarks>
        public void ProcessRequest(HttpContextBase context)
        {
            var process = this.GetProcessRunner(context);
            process.HandleRequest();
        }

        /// <summary>
        /// Returns the <see cref="ImageProcessRunner"/> to process the current
        /// request
        /// </summary>
        /// <param name="context">The current request context</param>        
        protected virtual ImageProcessRunner GetProcessRunner(HttpContextBase context)
        {
            return new ImageProcessRunner(context);
        }
        
        /// <summary>
        /// This implementation is not available
        /// for reuse yet...not sure how the threading 
        /// would work with P/Invoke and GDI so not bothering
        /// </summary>
        public bool IsReusable
        {
            get { return false; }
        }
    }
}
