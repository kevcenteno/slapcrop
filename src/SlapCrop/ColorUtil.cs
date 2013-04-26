using System;
using System.Drawing;

namespace SlapCrop
{
    /// <summary>
    /// A simple utility class for working with colors
    /// </summary>
    public class ColorUtil
    {
        /// <summary>
        /// Converts a hex color (eg. 000, #000, 000000, #000000) to a Color struct
        /// </summary>
        /// <param name="hexCode">A valid hex color</param>
        /// <exception cref="FormatException">thrown if not a valid hex color</exception>
        public static Color MakeColorFromHex(string hexCode)
        {
            var hex = hexCode.TrimStart('#');
            if (hex.Length == 3)
            {
                // double up on each "digit"
                hex = string.Format("{0}{0}{1}{1}{2}{2}", hex[0], hex[1], hex[2]);
            }

            if (hex.Length != 6)
            {
                throw new FormatException("color must be a valid 6 or 3 digit hex color code, with or without the leading #");
            }

            var red = Convert.ToInt32(hex.Substring(0, 2), 16);
            var green = Convert.ToInt32(hex.Substring(2, 2), 16);
            var blue = Convert.ToInt32(hex.Substring(4, 2), 16);

            return Color.FromArgb(red, green, blue);
        }
    }
}
