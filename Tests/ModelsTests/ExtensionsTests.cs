using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using YoutubeExplode.Models;
using YoutubeExplode.Models.MediaStreams;

namespace YoutubeExplode.Tests.ModelsTests
{
    [TestClass]
    public class ExtensionsTests
    {
        [TestMethod]
        public void GetFileExtension_Test()
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