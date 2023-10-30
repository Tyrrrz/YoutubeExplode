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
                    return "29289b1008a8fadbb012e7dc0e325fea9eebbe87ac2019a4fa7df7fc15af02d0";

                if (RuntimeInformation.ProcessArchitecture == Architecture.X86)
                    return "edc8c9bda8a10e138386cd9b6953127906bde0f89d2b872cf8e9046d3c559b28";

                if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
                    return "dfd42f47c47559ccb594965f897530bb9daa62d4ce6883c3f4082b7d037832d1";
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                if (RuntimeInformation.ProcessArchitecture == Architecture.X64)
                    return "0b7808c8f93a3235efc2448c33086e8ce10295999bd93a40b060fbe7f2e92338";
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                if (RuntimeInformation.ProcessArchitecture == Architecture.X64)
                    return "7898153f5785a739b1314ef3fb9c511be26bc7879d972c301a170e6ab8027652";

                if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
                    return "a26adea0b56376df8c46118c15ae478ba02e9ac57097f569a32100760cea1cd2";
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
