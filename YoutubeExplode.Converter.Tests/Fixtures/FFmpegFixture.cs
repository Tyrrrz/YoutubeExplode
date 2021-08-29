using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using CliWrap;
using Xunit;

namespace YoutubeExplode.Converter.Tests.Fixtures
{
    public partial class FFmpegFixture : IAsyncLifetime
    {
        public string FilePath => Path.Combine(
            Path.GetDirectoryName(typeof(FFmpegFixture).Assembly.Location) ?? Directory.GetCurrentDirectory(),
            GetFFmpegFileName()
        );

        private async ValueTask EnsureFFmpegHasExecutePermissionAsync()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return;

            await Cli.Wrap("/bin/bash")
                .WithArguments(new[] {"-c", $"chmod +x {FilePath}"})
                .ExecuteAsync();
        }

        private async ValueTask DownloadFFmpegAsync()
        {
            using var httpClient = new HttpClient();

            await using var zipStream = await httpClient.GetStreamAsync(GetFFmpegDownloadUrl());
            using var zip = new ZipArchive(zipStream, ZipArchiveMode.Read);

            var entry = zip.GetEntry(GetFFmpegFileName());

            if (entry is null)
                throw new FileNotFoundException("Downloaded archive doesn't contain FFmpeg.");

            await using var entryStream = entry.Open();
            await using var fileStream = File.Create(FilePath);
            await entryStream.CopyToAsync(fileStream);

            await EnsureFFmpegHasExecutePermissionAsync();
        }

        public async Task InitializeAsync()
        {
            // Don't re-download FFmpeg from last time
            if (File.Exists(FilePath))
                return;

            await DownloadFFmpegAsync();
        }

        public Task DisposeAsync() => Task.CompletedTask;
    }

    public partial class FFmpegFixture
    {
        private static string GetFFmpegFileName() =>
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? "ffmpeg.exe"
                : "ffmpeg";

        private static string GetFFmpegDownloadUrl()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return "https://github.com/vot/ffbinaries-prebuilt/releases/download/v4.2.1/ffmpeg-4.2.1-win-64.zip";

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return "https://github.com/vot/ffbinaries-prebuilt/releases/download/v4.2.1/ffmpeg-4.2.1-linux-64.zip";

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return "https://github.com/vot/ffbinaries-prebuilt/releases/download/v4.2.1/ffmpeg-4.2.1-osx-64.zip";

            throw new InvalidOperationException("Unknown OS.");
        }
    }
}