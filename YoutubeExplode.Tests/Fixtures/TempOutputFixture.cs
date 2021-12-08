using System;
using System.IO;
using YoutubeExplode.Tests.Utils;

namespace YoutubeExplode.Tests.Fixtures;

public class TempOutputFixture : IDisposable
{
    public string DirPath { get; } = Path.Combine(
        Path.GetDirectoryName(typeof(TempOutputFixture).Assembly.Location) ?? Directory.GetCurrentDirectory(),
        "Temp",
        Guid.NewGuid().ToString()
    );

    public TempOutputFixture() => DirectoryEx.Reset(DirPath);

    public string GetTempFilePath(string fileName) => Path.Combine(DirPath, fileName);

    public string GetTempFilePath() => GetTempFilePath(Guid.NewGuid().ToString());

    public void Dispose() => DirectoryEx.DeleteIfExists(DirPath);
}