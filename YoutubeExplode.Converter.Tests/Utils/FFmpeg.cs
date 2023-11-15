using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode.Converter.Tests.Utils.Extensions;

namespace YoutubeExplode.Converter.Tests.Utils;

public static class FFmpeg
{
    private static readonly SemaphoreSlim Lock = new(1, 1);

    public static Version Version { get; } = new(6, 1);

    private static string FileName { get; } =
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "ffmpeg.exe" : "ffmpeg";

    public static string FilePath { get; } =
        Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                ?? Directory.GetCurrentDirectory(),
            FileName
        );

    private static string GetDownloadUrl()
    {
        static string GetPlatformMoniker()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return "windows";

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return "linux";

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return "osx";

            throw new NotSupportedException("Unsupported OS platform.");
        }

        static string GetArchitectureMoniker()
        {
            if (RuntimeInformation.ProcessArchitecture == Architecture.X64)
                return "x64";

            if (RuntimeInformation.ProcessArchitecture == Architecture.X86)
                return "x86";

            if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
                return "arm64";

            throw new NotSupportedException("Unsupported architecture.");
        }

        var plat = GetPlatformMoniker();
        var arch = GetArchitectureMoniker();

        return $"https://github.com/Tyrrrz/FFmpegBin/releases/download/{Version}/ffmpeg-{plat}-{arch}.zip";
    }

    private static byte[] GetDownloadHash()
    {
        static string GetHashString()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (RuntimeInformation.ProcessArchitecture == Architecture.X64)
                    return "48130a80aebffb61d06913350c3ad3187efd85096f898045fd65001bf89d7d7f";

                if (RuntimeInformation.ProcessArchitecture == Architecture.X86)
                    return "71e83e4d5b4ed8e9e5b13a8bc118b73affef2ff12f9e14c388bfb17db7008f8d";

                if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
                    return "cd2d765565d1cc36e3fc0653d8ad6444c1736b883144de885c1f178a404c977c";
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                if (RuntimeInformation.ProcessArchitecture == Architecture.X64)
                    return "856b4f0e5cd9de45c98b703f7258d578bbdc0ac818073a645315241f9e7d5780";
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                if (RuntimeInformation.ProcessArchitecture == Architecture.X64)
                    return "1671abe5dcc0b4adfaea6f2e531e377a3ccd8ea21aa2b5a0589b0e5ae7d85a37";

                if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
                    return "bcbc7de089f68c3565dd40e8fe462df28a181af8df756621fc4004a747b845cf";
            }

            throw new NotSupportedException("Unsupported architecture.");
        }

        var hashString = GetHashString();

        return Enumerable
            .Range(0, hashString.Length)
            .Where(x => x % 2 == 0)
            .Select(x => Convert.ToByte(hashString.Substring(x, 2), 16))
            .ToArray();
    }

    private static async ValueTask DownloadAsync()
    {
        using var archiveFile = TempFile.Create();
        using var http = new HttpClient();

        // Download the archive
        await http.DownloadAsync(GetDownloadUrl(), archiveFile.Path);

        // Verify the hash
        await using (var archiveStream = File.OpenRead(archiveFile.Path))
        {
            var expectedHash = GetDownloadHash();
            var actualHash = await SHA256.HashDataAsync(archiveStream);

            if (!actualHash.SequenceEqual(expectedHash))
                throw new Exception("Downloaded archive has invalid hash.");
        }

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
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
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
