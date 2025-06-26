using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode.Converter.Tests.Utils.Extensions;

namespace YoutubeExplode.Converter.Tests.Utils;

public static class FFmpeg
{
    private static readonly SemaphoreSlim Lock = new(1, 1);

    public static Version Version { get; } = new(7, 1, 1);

    private static string FileName { get; } = OperatingSystem.IsWindows() ? "ffmpeg.exe" : "ffmpeg";

    public static string FilePath { get; } =
        Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                ?? Directory.GetCurrentDirectory(),
            FileName
        );

    private static string GetDownloadUrl()
    {
        static string GetSystemMoniker()
        {
            if (OperatingSystem.IsWindows())
                return "windows";

            if (OperatingSystem.IsLinux())
                return "linux";

            if (OperatingSystem.IsMacOS())
                return "osx";

            throw new NotSupportedException("Unsupported operating system.");
        }

        static string GetArchitectureMoniker()
        {
            if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
                return "arm64";

            if (RuntimeInformation.ProcessArchitecture == Architecture.X64)
                return "x64";

            if (RuntimeInformation.ProcessArchitecture == Architecture.X86)
                return "x86";

            throw new NotSupportedException("Unsupported architecture.");
        }

        var sys = GetSystemMoniker();
        var arch = GetArchitectureMoniker();

        return $"https://github.com/Tyrrrz/FFmpegBin/releases/download/{Version}/ffmpeg-{sys}-{arch}.zip";
    }

    private static async ValueTask DownloadAsync()
    {
        using var archiveFile = TempFile.Create();
        using var http = new HttpClient();

        // Download the archive
        await http.DownloadAsync(GetDownloadUrl(), archiveFile.Path);

        // Extract the executable
        using (var zip = ZipFile.OpenRead(archiveFile.Path))
        {
            var entry =
                zip.GetEntry(FileName)
                ?? throw new FileNotFoundException(
                    "Downloaded archive doesn't contain the FFmpeg executable."
                );

            entry.ExtractToFile(FilePath, true);
        }

        // Add the execute permission on Unix
        if (!OperatingSystem.IsWindows())
        {
            File.SetUnixFileMode(
                FilePath,
                File.GetUnixFileMode(FilePath) | UnixFileMode.UserExecute
            );
        }
    }

    public static async ValueTask InitializeAsync()
    {
        await Lock.WaitAsync();

        try
        {
            if (!File.Exists(FilePath))
                await DownloadAsync();
        }
        finally
        {
            Lock.Release();
        }
    }
}
