using System;
using System.IO;
using System.Threading;
using YoutubeExplode.Tests.Utils;

namespace YoutubeExplode.Tests.Fixtures
{
    public class TempOutputFixture : IDisposable
    {
        private static int _instanceCount;

        public string DirPath { get; }

        public TempOutputFixture()
        {
            DirPath = Path.Combine(
                Path.GetDirectoryName(typeof(TempOutputFixture).Assembly.Location) ?? Directory.GetCurrentDirectory(),
                "Temp",
                Interlocked.Increment(ref _instanceCount).ToString()
            );

            DirectoryEx.Reset(DirPath);
        }

        public string GetTempFilePath(string fileName) => Path.Combine(DirPath, fileName);

        public string GetTempFilePath() => GetTempFilePath(Guid.NewGuid().ToString());

        public void Dispose() => DirectoryEx.DeleteIfExists(DirPath);
    }
}