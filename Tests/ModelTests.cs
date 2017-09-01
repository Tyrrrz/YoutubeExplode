using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using YoutubeExplode.Models;
using YoutubeExplode.Models.MediaStreams;

namespace Tests
{
    [TestClass]
    public class ModelTests
    {
        [TestMethod]
        public void ClosedCaptionTrack_GetByTime_Test()
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
        public void VideoResolution_Equality_Test()
        {
            var vr1 = new VideoResolution(800, 600);
            var vr2 = new VideoResolution(800, 600);
            var vr3 = new VideoResolution(640, 480);

            Assert.IsTrue(vr1 == vr2);
            Assert.IsFalse(vr2 == vr3);
            Assert.IsTrue(vr2 != vr3);
            Assert.AreEqual(vr1, vr2);
            Assert.AreNotEqual(vr2, vr3);
            Assert.AreEqual(vr1.GetHashCode(), vr2.GetHashCode());
            Assert.AreNotEqual(vr2.GetHashCode(), vr3.GetHashCode());
        }

        [TestMethod]
        public void Extensions_GetFileExtension_Test()
        {
            var possibleValues = Enum.GetValues(typeof(Container)).Cast<Container>();

            foreach (var value in possibleValues)
            {
                var result = value.GetFileExtension();

                Assert.That.IsNotBlank(result);
            }
        }
    }
}