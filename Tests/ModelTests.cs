using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using YoutubeExplode.Models;
using YoutubeExplode.Models.MediaStreams;

namespace YoutubeExplode.Tests
{
    [TestClass]
    public class ModelTests
    {
        [TestMethod]
        public void ClosedCaptionTrack_GetByTime_Existing_Test()
        {
            var captionTrack = Dummies.GetClosedCaptionTrack();
            var caption = captionTrack.GetByTime(TimeSpan.FromSeconds(0.5));

            Assert.IsNotNull(caption);
            Assert.AreEqual("Hello", caption.Text);
        }

        [TestMethod]
        public void ClosedCaptionTrack_GetByTime_NonExisting_Test()
        {
            var captionTrack = Dummies.GetClosedCaptionTrack();
            var caption = captionTrack.GetByTime(TimeSpan.FromSeconds(5));

            Assert.IsNull(caption);
        }

        [TestMethod]
        public void Extensions_GetFileExtension_Test()
        {
            var possibleValues = Enum.GetValues(typeof(Container)).Cast<Container>();

            foreach (var value in possibleValues)
            {
                string result = value.GetFileExtension();

                Assert.That.IsNotBlank(result);
            }
        }
    }
}