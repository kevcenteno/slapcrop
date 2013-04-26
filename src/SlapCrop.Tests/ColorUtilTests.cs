using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Drawing;

namespace SlapCrop.Tests
{
    [TestClass]
    public class ColorUtilTests
    {
        [TestMethod]
        public void ColorUtil_CanConvertToBlack()
        {
            this.AssertColorMatch(Color.Black, "#000000");
            this.AssertColorMatch(Color.Black, "000000");
            this.AssertColorMatch(Color.Black, "000");
        }

        [TestMethod]
        public void ColorUtil_CanConvertToWhite()
        {
            this.AssertColorMatch(Color.White, "#FFFFFF");
            this.AssertColorMatch(Color.White, "ffffff");
            this.AssertColorMatch(Color.White, "fff");
        }

        [TestMethod]
        public void ColorUtil_CanConvertToRed()
        {
            this.AssertColorMatch(Color.Red, "#ff0000");
            this.AssertColorMatch(Color.Red, "FF0000");
            this.AssertColorMatch(Color.Red, "F00");
        }

        [TestMethod]
        public void ColorUtil_CanConvertToGreen()
        {
            // RGB for Green X11: (0%, 100%, 0%) but in W3C: (0%, 50%, 0%) 
            // .NET opted for W3C option
            // more info here: http://stackoverflow.com/questions/4342300/why-is-system-drawing-color-green-0-128-0
            
            this.AssertColorMatch(Color.Green, "#008000");
            this.AssertColorMatch(Color.Green, "008000");
        }

        [TestMethod]
        public void ColorUtil_CanConvertToBlue()
        {
            this.AssertColorMatch(Color.Blue, "#0000ff");
            this.AssertColorMatch(Color.Blue, "0000FF");
            this.AssertColorMatch(Color.Blue, "00F");
        }

        private void AssertColorMatch(Color expectedColor, string hexCode)
        {
            var color = ColorUtil.MakeColorFromHex(hexCode);
            Assert.AreEqual(expectedColor.R, color.R);
            Assert.AreEqual(expectedColor.G, color.G);
            Assert.AreEqual(expectedColor.B, color.B);
        }
    }
}
