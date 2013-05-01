using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Configuration;

namespace SlapCrop.Tests.Support
{
    [TestClass]
    public class ImageConfigSectionTests
    {
        private ImageConfigSection subject = ImageConfigSection.Instance;

        [TestMethod]
        public void ImageConfigSection_LoadsFillColor()
        {
            Assert.AreEqual("#00FF00", this.subject.FillColor, true);
        }

        [TestMethod]
        public void ImageConfigSection_LoadsBreakpoints()
        {
            var keys = this.subject.Breakpoints;
            Assert.IsNotNull(keys);
            Assert.IsInstanceOfType(keys, typeof(KeyValueConfigurationCollection));

            Assert.AreEqual(4, keys.Count);

            Assert.AreEqual(0, int.Parse(keys["iphone"].Value));
            Assert.AreEqual(1, int.Parse(keys["ipad"].Value));
            Assert.AreEqual(2, int.Parse(keys["ipad-landscape"].Value));
            Assert.AreEqual(3, int.Parse(keys["desktop"].Value));
        }
    }
}
