using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using CliWrap;

namespace YoutubeExplode.Converter.Tests.Utils;

public static class FFmpeg
{
    private static readonly SemaphoreSlim Lock = new(1, 1);

    private static string FileName { get; } =
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? "ffmpeg.exe"
            : "ffmpeg";

    public static string FilePath { get; } = Path.Combine(
        Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? Directory.GetCurrentDirectory(),
        FileName
    );

    private static async ValueTask EnsureExecutePermissionAsync()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return;

        await Cli.Wrap("/bin/bash")
            .WithArguments(new[] {"-c", $"chmod +x {FilePath}"})
            .ExecuteAsync();
    }

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
                return "86";

            if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
                return "arm-64";

            if (RuntimeInformation.ProcessArchitecture == Architecture.Arm)
                return "arm";

            throw new NotSupportedException("Unsupported architecture.");
        }

        const string version = "4.4.1";
        var plat = GetPlatformMoniker();
        var arch = GetArchitectureMoniker();

        return $"https://github.com/vot/ffbinaries-prebuilt/releases/download/v{version}/ffmpeg-{version}-{plat}-{arch}.zip";
    }

    private static async ValueTask DownloadAsync()
    {
        using var httpClient = new HttpClient();

        await using var zipStream = await httpClient.GetStreamAsync(GetDownloadUrl());
        using var zip = new ZipArchive(zipStream, ZipArchiveMode.Read);

        var entry = zip.GetEntry(FileName);
        if (entry is null)
            throw new FileNotFoundException("Downloaded archive doesn't contain FFmpeg.");

        await using var entryStream = entry.Open();
        await using var fileStream = File.Create(FilePath);
        await entryStream.CopyToAsync(fileStream);

        await EnsureExecutePermissionAsync();
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