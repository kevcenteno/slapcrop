using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SlapCrop.Tests
{
    class ResHelper
    {
        public static Bitmap LoadImage(string name)
        {
            var asm = Assembly.GetExecutingAssembly();

            using (var stream = asm.GetManifestResourceStream("SlapCrop.Tests.Resources." + name))
            {
                return new Bitmap(Bitmap.FromStream(stream));
            }
        }

        public static int PixelCount(Bitmap image, Color color)
        {
            return PixelCount(image, color, new Rectangle(0, 0, image.Width, image.Height));
        }

        public static int PixelCount(Bitmap image, Color color, Rectangle rectangle)
        {
            int count = 0;

            for (int x = rectangle.X; x < rectangle.Width + rectangle.X; x++)
            {
                for (int y = rectangle.Y; y < rectangle.Height + rectangle.Y; y++)
                {
                    if (SameColor(image.GetPixel(x, y), color))
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        public static IEnumerable<Color> ColorsInImage(Bitmap image)
        {
            var colors = new List<Color>();

            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    var color = image.GetPixel(x, y);
                    if (!colors.Contains(color))
                    {
                        colors.Add(color);
                    }
                }
            }

            return colors;
        }

        private static bool SameColor(Color source, Color target)
        {
            return source.A == target.A && source.R == target.R && source.G == target.G && source.B == target.B;
        }
    }
}
