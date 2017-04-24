using System;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using YoutubeExplode.Models.ClosedCaptions;

namespace YoutubeExplode.Tests.ModelsTests.ClosedCaptionsTests
{
    [TestClass]
    public class ClosedCaptionTrackTests
    {
        private ClosedCaptionTrack _captionTrack;

        [TestInitialize]
        public void Init()
        {
            var info = new ClosedCaptionTrackInfo("test", new CultureInfo("en"), true);
            _captionTrack = new ClosedCaptionTrack(info,
                new[]
                {
                    new ClosedCaption("Hello", TimeSpan.Zero, TimeSpan.FromSeconds(1)),
                    new ClosedCaption("world", TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(1))
                });
        }

        [TestMethod]
        public void GetByTime_Existing_Test()
        {
            var caption = _captionTrack.GetByTime(TimeSpan.FromSeconds(0.5));

            Assert.IsNotNull(caption);
            Assert.AreEqual("Hello", caption.Text);
        }

        [TestMethod]
        public void GetByTime_NonExisting_Test()
        {
            var caption = _captionTrack.GetByTime(TimeSpan.FromSeconds(5));

            Assert.IsNull(caption);
        }
    }
}