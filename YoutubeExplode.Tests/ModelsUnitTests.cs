using System;
using NUnit.Framework;
using YoutubeExplode.Models.ClosedCaptions;
using YoutubeExplode.Models.MediaStreams;

namespace YoutubeExplode.Tests
{
    [TestFixture]
    public class ModelsUnitTests
    {
        private ClosedCaptionTrack GetClosedCaptionTrack()
        {
            var info = new ClosedCaptionTrackInfo("test", new Language("en", "English (auto-generated)"), true);
            return new ClosedCaptionTrack(info,
                new[]
                {
                    new ClosedCaption("Hello", TimeSpan.Zero, TimeSpan.FromSeconds(1)),
                    new ClosedCaption("world", TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(1))
                });
        }

        [Test]
        public void ClosedCaptionTrack_GetByTime_Test()
        {
            var captionTrack = GetClosedCaptionTrack();

            var caption = captionTrack.GetByTime(TimeSpan.FromSeconds(0.5));

            Assert.That(caption, Is.Not.Null);
            Assert.That(caption.Text, Is.EqualTo("Hello"));
        }

        [Test]
        public void ClosedCaptionTrack_GetByTime_NonExisting_Test()
        {
            var captionTrack = GetClosedCaptionTrack();

            var caption = captionTrack.GetByTime(TimeSpan.FromSeconds(5));

            Assert.That(caption, Is.Null);
        }

        [Test]
        public void VideoResolution_Equality_Test()
        {
            var vr1 = new VideoResolution(800, 600);
            var vr2 = new VideoResolution(800, 600);
            var vr3 = new VideoResolution(640, 480);

            Assert.That(vr1, Is.EqualTo(vr2));
            Assert.That(vr2, Is.Not.EqualTo(vr3));
            Assert.That(vr1.GetHashCode(), Is.EqualTo(vr2.GetHashCode()));
            Assert.That(vr2.GetHashCode(), Is.Not.EqualTo(vr3.GetHashCode()));
        }

        [Theory]
        public void Extensions_Container_GetFileExtension_Test(Container container)
        {
            var ext = container.GetFileExtension();

            Assert.That(ext, Is.Not.Null.Or.Empty);
        }

        [Theory]
        public void Extensions_VideoQuality_GetVideoQualityLabel_Test(VideoQuality quality,
            [Values(24, 30, 60)] int framerate)
        {
            var label = quality.GetVideoQualityLabel(framerate);

            Assert.That(label, Is.Not.Null.Or.Empty);
        }
    }
}