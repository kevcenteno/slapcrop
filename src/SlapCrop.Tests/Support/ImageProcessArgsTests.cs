using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlapCrop.Tests.Support
{
    [TestClass]
    public class ImageProcessArgsTests
    {
        private ImageProcessArgs subject = new ImageProcessArgs();

        [TestMethod]
        public void ImageProcessArgs_DeterminesSizes()
        {
            var args = "sz=1x1;2x2;3x3;4x4;&mbr=ipad&cropspec=tl,1,1";
            this.subject.Parse(args);

            Assert.AreEqual(4, this.subject.Sizes.Length);
            for (int i = 1; i <= 4; i++)
            {
                Assert.AreEqual(i, this.subject.Sizes[i - 1].Width);
                Assert.AreEqual(i, this.subject.Sizes[i - 1].Height);
            }
        }

        [TestMethod]
        public void ImageProcessArgs_DeterminesMediaBreakpoint()
        {
            var args = "sz=1x1;2x2;3x3;4x4;&mbr=ipad&cropspec=tl,1,1";
            this.subject.Parse(args);

            Assert.AreEqual(1, this.subject.MediaBreakpoint);
        }

        [TestMethod]
        public void ImageProcessArgs_DefaultsMediaBreakpointToLargest()
        {
            var args = "sz=1x1;2x2;3x3;4x4&cropspec=tl,1,1";
            this.subject.Parse(args);

            Assert.AreEqual(3, this.subject.MediaBreakpoint);
        }

        [TestMethod]
        public void ImageProcessArgs_CreatesTargetObject()
        {
            var args = "sz=1x1;2x2;3x3;4x4;&mbr=ipad&cropspec=tl,.5,.5";
            this.subject.Parse(args);

            var target = this.subject.CreateTarget();
            Assert.AreEqual(new Size(2, 2), target.ScaleSize);
        }
    }
}
