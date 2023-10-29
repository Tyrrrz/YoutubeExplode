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

    public static Version Version { get; } = new(6, 0);

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
                    return "01cb055038df8a1b8b0c729dd016a1f490c426eff381b1ac986c2744b145cff2";

                if (RuntimeInformation.ProcessArchitecture == Architecture.X86)
                    return "519cdc8fc115b46c94d7c51f59f15ef39fe58acd59acb49a8faec686aa8b02f3";

                if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
                    return "09fa319448dd132ac81e940f19437b615b7eb0e86e5d2f6ce57980f75d8ccec1";
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                if (RuntimeInformation.ProcessArchitecture == Architecture.X64)
                    return "9d820929fec7f55839e8184e164fe44079980a2b61b257c91c997ad22604f8e4";
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                if (RuntimeInformation.ProcessArchitecture == Architecture.X64)
                    return "ece0b4b8dbd457d8f3d4b187406997c8c9d66ec7620505ce1d0617cfee7ccae6";

                if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
                    return "e39483c79ac02b9dc055f5ccc95e24cb0c718edb9c9ff270fc96c9444c71d02c";
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
