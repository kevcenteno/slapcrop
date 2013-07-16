using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Net;
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
        /// <summary>
        /// The HttpContext we are working with
        /// </summary>
        public HttpContextBase Context { get; private set; }

        /// <summary>
        /// The definition of the target image
        /// </summary>
        public ImageProcessTarget Target { get; private set; }

        /// <summary>
        /// Options used for scaling images
        /// </summary>
        public ImageScalerOptions Options { get; private set; }

        /// <summary>
        /// Initializes the Context
        /// </summary>
        /// <param name="context">The current HttpContext</param>
        public ImageProcessRunner(HttpContextBase context)
        {
            this.Context = context;

            this.Target = ImageProcessTarget.Empty;

            var index = this.Context.Request.RawUrl.IndexOf('?');
            if (index >= 0)
            {
                var args = new ImageProcessArgs();
                args.Parse(this.Context.Request.RawUrl.Substring(index + 1));
                this.Target = args.CreateTarget();
            }
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
            this.LoadOptionsFromConfig();

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
            var imageSrc = this.Context.Request.Path;

            // compare first segment with list of keys from settings
            var segments = imageSrc.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            if (segments.Length > 0)
            {
                var remoteMapping = segments[0];
                string imgPath = string.Empty;

                for (int i = 1; i < segments.Length; i++)
                {
                    imgPath += string.Format("/{0}", segments[i]);
                }

                // if the image request is from a remote server, request the image and return that
                foreach (KeyValueConfigurationElement source in ImageConfigSection.Instance.Sources)
                {
                    if (remoteMapping.Equals(source.Key, StringComparison.OrdinalIgnoreCase))
                    {
                        var imgUrl = string.Format("{0}{1}", source.Value, imgPath);

                        var httpWebRequest = (HttpWebRequest)WebRequest.Create(imgUrl);
                        httpWebRequest.Timeout = ImageConfigSection.Instance.Timeout;

                        try
                        {
                            return (Bitmap)Image.FromStream(httpWebRequest.GetResponse().GetResponseStream());
                        }
                        catch (System.Net.WebException)
                        {
                            throw new HttpException(404, "The specified resource was not found");
                        }
                    }
                }
            }

            // otherwise, assume the request is for a local image and return that
            try
            {
                return (Bitmap)Image.FromFile(this.Context.Request.PhysicalApplicationPath + imageSrc);
            }
            catch (System.IO.FileNotFoundException)
            {
                throw new HttpException(404, "The specified resource was not found");
            }
        }

        /// <summary>
        /// Scales or crops the source image based on the supplied parameter values
        /// </summary>
        /// <param name="image">The source image</param>
        /// <returns>The processed image or <paramref name="image"/> if there are no supplied sizes for this request</returns>
        protected virtual Bitmap ProcessImage(Bitmap image)
        {
            Bitmap processed = null;

            if (this.Target != ImageProcessTarget.Empty)
            {
                var scaler = this.GetImageScaler();

                if (this.Target.CropType != ImageCropType.None)
                {
                    processed = scaler.Scale(image, this.Target.ScaleSize.Width, this.Target.ScaleSize.Height);
                    processed = new ImageCropper().Crop(processed, this.Target.CropType, this.Target.CropSize.Width, this.Target.CropSize.Height);
                }
                else
                {
                    processed = scaler.Scale(image, this.Target.ScaleSize.Width, this.Target.ScaleSize.Height);
                }

                if (this.Target.CropType != ImageCropType.None)
                {
                    processed = new ImageCropper().Crop(processed, this.Target.CropType, this.Target.CropSize.Width, this.Target.CropSize.Height);
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
