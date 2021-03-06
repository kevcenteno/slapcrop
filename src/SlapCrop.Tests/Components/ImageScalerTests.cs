﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Drawing;

namespace SlapCrop.Tests.Components
{
    [TestClass]
    public class ImageScalerTests
    {
        private ImageScaler subject = new ImageScaler();

        [TestMethod]
        public void ImageScaler_HasDefaultFillColorSetToBlack()
        {
            Assert.AreEqual(Color.Black, this.subject.Options.DefaultFillColor);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ImageScaler_ThrowsExceptionWhenWidthIsTooLarge()
        {
            var image = ResHelper.LoadImage("landscape.jpg");
            var result = this.subject.Scale(image, 601, 200);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ImageScaler_ThrowsExceptionWhenHeightIsTooLarge()
        {
            var image = ResHelper.LoadImage("landscape.jpg");
            var result = this.subject.Scale(image, 200, 401);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void ImageScaler_ReturnsScaledImageWhenAspectRatiosMatch()
        {
            var image = ResHelper.LoadImage("landscape.jpg");
            var result = this.subject.Scale(image, 300, 200);
            Assert.IsNotNull(result);
            Assert.AreEqual(300, result.Width);
            Assert.AreEqual(200, result.Height);
            Assert.AreEqual(0, ResHelper.PixelCount(result, Color.Black));
        }

        [TestMethod]
        public void ImageScaler_ReturnsLetterboxImageWhenAspectRatioIsOffForLandscape()
        {
            var image = ResHelper.LoadImage("landscape.jpg");
            var result = this.subject.Scale(image, 200, 200);            
            Assert.IsNotNull(result);

            var ratio = (float)result.Width / image.Width;
            var height = (int)Math.Round(image.Height * ratio);            
            var blackSectionHeight = (result.Height - height) / 2;

            // sets width/height to requested size
            Assert.AreEqual(200, result.Width);
            Assert.AreEqual(200, result.Height);

            // adding result.Height due to rounding...
            var expectedBlackCount = result.Width * blackSectionHeight * 2 + result.Height;
            var blackCount = ResHelper.PixelCount(result, Color.Black);            
            Assert.AreEqual(expectedBlackCount, blackCount);            
        }

        [TestMethod]
        public void ImageScaler_ReturnsLetterboxImageWhenAspectRatioIsOffForLandscapeWithRed()
        {
            this.subject.Options.DefaultFillColor = Color.Red;

            var image = ResHelper.LoadImage("landscape.jpg");
            var result = this.subject.Scale(image, 200, 200);
            Assert.IsNotNull(result);

            var ratio = (float)result.Width / image.Width;
            var height = (int)Math.Round(image.Height * ratio);
            var blackSectionHeight = (result.Height - height) / 2;

            // sets width/height to requested size
            Assert.AreEqual(200, result.Width);
            Assert.AreEqual(200, result.Height);

            // adding result.Height due to rounding...
            var expectedRedCount = result.Width * blackSectionHeight * 2 + result.Height;
            var redCount = ResHelper.PixelCount(result, Color.Red);
            Assert.AreEqual(expectedRedCount, redCount);
        }

        [TestMethod]
        public void ImageScaler_ReturnsLetterboxImageWhenAspectRatioIsOffForPortrait()
        {
            var image = ResHelper.LoadImage("portrait.jpg");
            var result = this.subject.Scale(image, 200, 200);
            Assert.IsNotNull(result);

            var ratio = (float)result.Height / image.Height;
            var width = (int)Math.Round(image.Width * ratio);
            var blackSectionWidth = (result.Width - width) / 2;

            // sets width/height to requested size
            Assert.AreEqual(200, result.Width);
            Assert.AreEqual(200, result.Height);

            // adding result.Width due rounding...
            var expectedBlackCount = result.Width * blackSectionWidth * 2 + result.Width;
            var blackCount = ResHelper.PixelCount(result, Color.Black);
            Assert.AreEqual(expectedBlackCount, blackCount);
        }
    }
}
