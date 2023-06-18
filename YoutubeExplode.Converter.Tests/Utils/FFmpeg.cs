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

    public static Version Version { get; } = new(4, 4, 1);

    private static string FileName { get; } =
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? "ffmpeg.exe"
            : "ffmpeg";

    public static string FilePath { get; } = Path.Combine(
        Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? Directory.GetCurrentDirectory(),
        FileName
    );

    private static string GetDownloadUrl()
    {
        static string GetPlatformMoniker()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return "win";

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return "linux";

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return "osx";

            throw new NotSupportedException("Unsupported OS platform.");
        }

        static string GetArchitectureMoniker()
        {
            if (RuntimeInformation.ProcessArchitecture == Architecture.X64)
                return "64";

            if (RuntimeInformation.ProcessArchitecture == Architecture.X86)
                return "32";

            if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
                return "arm-64";

            if (RuntimeInformation.ProcessArchitecture == Architecture.Arm)
                return "arm";

            throw new NotSupportedException("Unsupported architecture.");
        }

        var plat = GetPlatformMoniker();
        var arch = GetArchitectureMoniker();

        return $"https://github.com/vot/ffbinaries-prebuilt/releases/download/v{Version}/ffmpeg-{Version}-{plat}-{arch}.zip";
    }

    private static byte[] GetDownloadHash()
    {
        static string GetHashString()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Only x64 build is available
                return "d1124593b7453fc54dd90ca3819dc82c22ffa957937f33dd650082f1a495b10e";
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                if (RuntimeInformation.ProcessArchitecture == Architecture.X64)
                    return "4348301b0d5e18174925e2022da1823aebbdb07282bbe9adb64b2485e1ef2df7";

                if (RuntimeInformation.ProcessArchitecture == Architecture.X86)
                    return "a292731806fe3733b9e2281edba881d1035e4018599577174a54e275c0afc931";

                if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
                    return "7d57e730cc34208743cc1a97134541656ecd2c3adcdfad450dedb61d465857da";
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // Only x64 build is available
                return "e08c670fcbdc2e627aa4c0d0c5ee1ef20e82378af2f14e4e7ae421a148bd49af";
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
        using var httpClient = new HttpClient();

        // Download the archive
        await httpClient.DownloadAsync(GetDownloadUrl(), archiveFile.Path);

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
                zip.GetEntry(FileName) ??
                throw new FileNotFoundException("Downloaded archive doesn't contain the FFmpeg executable.");

            entry.ExtractToFile(FilePath, true);
        }

        // Add the execute permission on Unix
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            File.SetUnixFileMode(FilePath, UnixFileMode.UserExecute);
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