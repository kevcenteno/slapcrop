using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlapCrop
{
    public class CropSpec
    {
        public float Width { get; set; }
        public float Height { get; set; }
        public ImageCropType Type { get; set; }
    }

    public class ImageProcessArgs
    {
        protected static readonly string SIZE_PARAM = "sz";
        protected static readonly string CROP_PARAM = "cropspec";
        protected static readonly string MBR_PARAM = "mbr";

        public Size[] Sizes { get; protected set; }

        public CropSpec[] Crops { get; protected set; }

        public int MediaBreakpoint { get; protected set; }

        public void Parse(string queryStringParameters)
        {
            this.Sizes = Enumerable.Empty<Size>().ToArray();

            var pairs = this.MakeParameters(queryStringParameters);
            
            this.ParseSizes(pairs);
            this.ParseMediaBreakpoint(pairs);
            this.ParseCropSpec(pairs);
        }

        public ImageProcessTarget CreateTarget()
        {
            if (this.Sizes == null || this.Sizes.Length == 0)
            {
                return ImageProcessTarget.Empty;
            }

            var crop = new CropSpec();
            crop.Type = ImageCropType.None;
            crop.Width = 1.0f;
            crop.Height = 1.0f;

            if (this.Crops != null)
            {
                if (this.Crops.Length == 1)
                {
                    crop = this.Crops[0];
                }
                else
                {
                    crop = this.Crops[this.MediaBreakpoint];
                }
            }

            var target = new ImageProcessTarget(
                    this.Sizes[this.MediaBreakpoint],
                    crop.Type,
                    crop.Width,
                    crop.Height
                );

            return target;
        }

        protected virtual void ParseSizes(Dictionary<string, string> pairs)
        {
            // don't bother if empty
            if (!pairs.ContainsKey(SIZE_PARAM) || string.IsNullOrEmpty(pairs[SIZE_PARAM]))
            {
                this.Sizes = Enumerable.Empty<Size>().ToArray();
                return;
            }

            var sizeValues = pairs[SIZE_PARAM].ToLower().Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            var parts = new string[] { };

            this.Sizes = new Size[sizeValues.Length];

            for (int i = 0; i < sizeValues.Length; i++)
            {
                parts = sizeValues[i].Split('x');
                this.Sizes[i] = new Size(
                        int.Parse(parts[0]), 
                        int.Parse(parts[1])
                    );
            }
        }

        protected virtual void ParseMediaBreakpoint(Dictionary<string, string> pairs)
        {
            this.MediaBreakpoint = this.Sizes.Length - 1;

            if (pairs.ContainsKey(MBR_PARAM))
            {
                int mbr = 0;
                if (int.TryParse(pairs[MBR_PARAM], out mbr))
                {
                    this.MediaBreakpoint = mbr;
                }
            }
        }

        protected virtual void ParseCropSpec(Dictionary<string, string> pairs)
        {
            if (pairs.ContainsKey(CROP_PARAM))
            {
                var details = pairs[CROP_PARAM].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);                
                this.Crops = new CropSpec[details.Length];

                for (int i = 0; i < this.Crops.Length; i++)
                {
                    if (details[i].Equals("skip"))
                    {
                        this.Crops[i] = new CropSpec
                        {
                            Width = 1.0f,
                            Height = 1.0f,
                            Type = ImageCropType.None
                        };
                    }
                    else
                    {
                        var parts = details[i].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                        var widthScale = float.Parse(parts[1]);
                        var heightScale = float.Parse(parts[2]);

                        this.Crops[i] = new CropSpec
                        {
                            Width = Math.Min(1.0f, widthScale),
                            Height = Math.Min(1.0f, heightScale),
                            Type = this.GetCropType(parts[0])
                        };
                    }
                }
            }
        }

        protected virtual ImageCropType GetCropType(string value)
        {
            var type = ImageCropType.None;

            if (!string.IsNullOrEmpty(value))
            {
                switch (value)
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

        protected virtual Dictionary<string, string> MakeParameters(string queryStringParameters)
        {
            var parameters = queryStringParameters.TrimStart('?').Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
            var pairs = new Dictionary<string, string>();

            var parts = new string[] { };
            foreach (var param in parameters)
            {
                parts = param.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                pairs.Add(parts[0].ToLower(), parts[1].Trim());
            }
            return pairs;
        }
    }
}
