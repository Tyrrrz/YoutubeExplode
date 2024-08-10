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

    public static Version Version { get; } = new(7, 0);

    private static string FileName { get; } = OperatingSystem.IsWindows() ? "ffmpeg.exe" : "ffmpeg";

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
            if (OperatingSystem.IsWindows())
                return "windows";

            if (OperatingSystem.IsLinux())
                return "linux";

            if (OperatingSystem.IsMacOS())
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
            if (OperatingSystem.IsWindows())
            {
                if (RuntimeInformation.ProcessArchitecture == Architecture.X64)
                    return "96f2d2fae3a298adadf8aaa19c8b79c04ba18afef61f8b8d157032ccd5170992";

                if (RuntimeInformation.ProcessArchitecture == Architecture.X86)
                    return "81b49b5d9cd3ff9ace26f29b0b3cf7cd6358769f27ad32f8079b14a9db0a1e7a";

                if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
                    return "58efff14efe66ae666f9d9145ee035e360e80cc0d61b0ebc4162e3528e7aa933";
            }

            if (OperatingSystem.IsLinux())
            {
                if (RuntimeInformation.ProcessArchitecture == Architecture.X64)
                    return "d1e03fb8dbe439b5f626706140973d48e5704bf0b30d529828a0fcb8cf5abed8";
            }

            if (OperatingSystem.IsMacOS())
            {
                if (RuntimeInformation.ProcessArchitecture == Architecture.X64)
                    return "af9ef6994ef259ae3ae6dc215170c80db5d4390ea7cfe53cc30a544dd8f68a9b";

                if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
                    return "d799c74e8b17bd40b42cf7a2ad02b6045022085bcd14ecfaea3cd1012d6add30";
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
