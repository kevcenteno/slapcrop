using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Web;

namespace SlapCrop
{
    /// <summary>
    /// A utility class for processing incoming requests for scaled or 
    /// cropped images.
    /// 
    /// This class is used by the ImageHandler to delegate processing to 
    /// something testable and subclassable
    /// </summary>
    public class ImageProcessRunner
    {
        private static readonly string SIZE_PARAM = "sz";
        private static readonly string CROP_PARAM = "crop";
        private static readonly string QBR_PARAM = "qbr";
        
        /// <summary>
        /// The HttpContext we are working with
        /// </summary>
        public HttpContextBase Context { get; private set; }

        /// <summary>
        /// The set of sizes specified by the size parameter
        /// </summary>
        /// <remarks>If not supplied, the source image will be returned</remarks>
        public Size[] Sizes { get; private set; }

        /// <summary>
        /// The type of crop to apply to the result
        /// </summary>
        /// <remarks>The default is ImageCropType.None</remarks>
        public ImageCropType CropType { get; private set; }

        /// <summary>
        /// The current media query breakpoint to apply
        /// </summary>
        /// <remarks>If not supplied, the largest <see cref="Size"/> will be used</remarks>
        public int QueryBreakPoint { get; private set; }

        /// <summary>
        /// Options used for scaling images
        /// </summary>
        public ImageScalerOptions Options { get; private set; }

        /// <summary>
        /// Initializes the Context, Sizes, CropType and QueryBreakPoint values
        /// </summary>
        /// <param name="context">The current HttpContext</param>
        public ImageProcessRunner(HttpContextBase context)
        {
            this.Context = context;
            this.Sizes = this.ParseSizeParameter(this.Context.Request);
            this.CropType = this.ParseCropType(this.Context.Request);
            this.QueryBreakPoint = this.ParseQueryBreakPoint(this.Context.Request);
                        
            this.LoadOptionsFromConfig();
        }

        /// <summary>
        /// Processes and handles the request
        /// </summary>
        /// <remarks>
        /// This is a template method where all work is delegated out to other instance methods to ensure support 
        /// for subclassing while keeping the steps of the process rigid
        /// </remarks>
        public virtual void HandleRequest()
        {
            var response = this.Context.Response;            
            var image = this.GetSourceImage();
            
            if (image == null)
            {
                throw new HttpException(404, "The specified resource was not found");
            }

            var processedImage = this.ProcessImage(image);

            response.Clear();
            this.SetMimeType(response);
            this.WriteImageToResponse(response, processedImage);
            response.OutputStream.Flush();            
        }

        /// <summary>
        /// Loads and returns the source image to process
        /// </summary>
        /// <returns></returns>
        protected virtual Bitmap GetSourceImage()
        {
            return null;
        }

        /// <summary>
        /// Scales or crops the source image based on the supplied parameter values
        /// </summary>
        /// <param name="image">The source image</param>
        /// <returns>The processed image or <paramref name="image"/> if there are no supplied sizes for this request</returns>
        protected virtual Bitmap ProcessImage(Bitmap image)
        {
            Bitmap processed = null;

            // if sizes specified scale
            if (this.Sizes.Length > 0)
            {                
                Size size;

                if (this.QueryBreakPoint < 0)
                {
                    size = this.Sizes.Last();                    
                }
                else
                {
                    // not catching IndexOutOfRange exception here...wouldn't be able to do anything
                    // reasonable so just let it bubble
                    size = this.Sizes[this.QueryBreakPoint];
                }

                if (this.CropType != ImageCropType.None)
                {
                    processed = new ImageCropper().Crop(image, this.CropType, size.Width, size.Height);
                }
                else
                {                    
                    processed = this.GetImageScaler().Scale(image, size.Width, size.Height);
                }
            }

            return processed ?? image;
        }

        /// <summary>
        /// Creates a new instance of <see cref="ImageScaler"/> to be used for scaling. This will use the <see cref="ImageConfigSection"/>
        /// if specified in application config, otherwise the defaults supplied in <see cref="ImageScalerOptions" />
        /// </summary>
        /// <remarks>Subclasses may set any scaler options before returning to control <see cref="ImageScalerOptions"/></remarks>
        protected virtual ImageScaler GetImageScaler()
        {
            return new ImageScaler(this.Options);
        }

        /// <summary>
        /// Sets the mime type for the response. The default implementation supports image/gif, image/png and image/jpeg based on 
        /// file extension
        /// </summary>
        /// <param name="response">The current response</param>
        protected virtual void SetMimeType(HttpResponseBase response)
        {
            // extension without .
            var ext = this.Context.Request.CurrentExecutionFilePathExtension.Substring(1).ToLower();
            var type = "image/jpeg";

            if (ext.Length == 3 && ext != "jpg")
            {
                type = "image/" + ext;
            }
            
            response.ContentType = type;
        }

        /// <summary>
        /// Write <paramref name="image"/> to the response
        /// </summary>
        /// <param name="response"></param>
        /// <param name="image"></param>
        /// <remarks>
        /// The default implementation writes the image with a quality level of 100. It is recommended that you 
        /// do the same unless there is a real reason to change this.
        /// </remarks>
        protected virtual void WriteImageToResponse(HttpResponseBase response, Bitmap image)
        {
            var codec = ImageCodecInfo.GetImageEncoders().First(c => c.MimeType.Equals(response.ContentType));
            var encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 100L);
            image.Save(response.OutputStream, codec, encoderParams);            
        }

        /// <summary>
        /// Parses out the size parameter from the query string and returns an array
        /// of all of the sizes
        /// </summary>
        /// <param name="request">The incoming request</param>
        /// <remarks>The default implementation uses "sz" for the key and supports multiple widthXheight sizes separated by semi-colons</remarks>
        /// <example>/image.png?sz=20x20;30x30;100x100;300x300</example>
        protected virtual Size[] ParseSizeParameter(HttpRequestBase request)
        {
            var query = request.QueryString[SIZE_PARAM];
            var sizes = new List<Size>();

            if (!string.IsNullOrEmpty(query))
            {
                foreach (var size in query.ToLower().Split(';'))
                {
                    sizes.Add(new Size(
                            int.Parse(size.Split('x')[0].Trim()),
                            int.Parse(size.Split('x')[1].Trim())
                        ));
                }
            }

            return sizes.ToArray();
        }

        /// <summary>
        /// Parses the current media query breakpoint from the request and returns the index which is 
        /// used to determine which <see cref="Sizes"/> value to use.
        /// </summary>
        /// <param name="request">The incoming request</param>
        /// <remarks>The default implementation uses "qbr" for the key and expects an integer value</remarks>
        /// <example>/image.png?sz=10x10;20x20;30x30&qbr=1 would return a 20x20 image</example>
        protected virtual int ParseQueryBreakPoint(HttpRequestBase request)
        {
            var query = request.QueryString[QBR_PARAM];

            if (!string.IsNullOrEmpty(query))
            {
                int output;
                if (!int.TryParse(query, out output))
                {
                    output = -1;
                }

                return output;
            }

            return -1;
        }

        /// <summary>
        /// Parses the crop type from the request
        /// </summary>
        /// <param name="request">The incoming request</param>
        /// <remarks>
        /// The default implementation uses "crop" as the key and supports the following values representing the starting position of the crop:
        /// <para>tl, tc, tr, rc, br, bc, bl, lc and c</para>
        /// </remarks>
        /// <example>/image.png?sz=10x10;20x20;30x30&qbr=1&crop=tl would return the top left 20x20 pixels of image.png</example>
        protected virtual ImageCropType ParseCropType(HttpRequestBase request)
        {
            var query = request.QueryString[CROP_PARAM];
            var type = ImageCropType.None;

            if (!string.IsNullOrEmpty(query))
            {
                switch (query)
                {
                    case "tl": type = ImageCropType.TopLeft; break;
                    case "tc": type = ImageCropType.TopCenter; break;
                    case "tr": type = ImageCropType.TopRight; break;
                    case "lc": type = ImageCropType.LeftCenter; break;
                    case "rc": type = ImageCropType.RightCenter; break;
                    case "bl": type = ImageCropType.BottomLeft; break;
                    case "bc": type = ImageCropType.BottomCenter; break;
                    case "br": type = ImageCropType.BottomRight; break;
                    default: type = ImageCropType.Center; break;
                }
            }

            return type;
        }

        /// <summary>
        /// Creates <see cref="ImageScalerOptions"/> for use with scaling. Will load from config section 
        /// if defined
        /// </summary>
        protected virtual void LoadOptionsFromConfig()
        {
            this.Options = new ImageScalerOptions();

            var setting = ConfigurationManager.GetSection("SlapCrop") as ImageConfigSection;
            
            if (setting != null)
            {
                this.Options.DefaultFillColor = ColorUtil.MakeColorFromHex(setting.FillColor);
            }
        }
    }
}
