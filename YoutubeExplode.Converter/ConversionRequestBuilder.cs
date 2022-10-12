using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using YoutubeExplode.Converter.Utils.Extensions;
using YoutubeExplode.Videos.Streams;

namespace YoutubeExplode.Converter;

/// <summary>
/// Builder for <see cref="ConversionRequest" />.
/// </summary>
public partial class ConversionRequestBuilder
{
    private readonly string _outputFilePath;

    private string? _ffmpegCliFilePath;
    private Container? _container;
    private ConversionPreset _preset;

    /// <summary>
    /// Initializes an instance of <see cref="ConversionRequestBuilder" />.
    /// </summary>
    public ConversionRequestBuilder(string outputFilePath) =>
        _outputFilePath = outputFilePath;

    private Container GetDefaultContainer() => new(
        Path.GetExtension(_outputFilePath).TrimStart('.').NullIfWhiteSpace() ??
        "mp4"
    );

    /// <summary>
    /// Sets FFmpeg CLI path.
    /// </summary>
    public ConversionRequestBuilder SetFFmpegPath(string path)
    {
        _ffmpegCliFilePath = path;
        return this;
    }

    /// <summary>
    /// Sets output container.
    /// </summary>
    public ConversionRequestBuilder SetContainer(Container container)
    {
        _container = container;
        return this;
    }

    /// <summary>
    /// Sets output container.
    /// </summary>
    public ConversionRequestBuilder SetContainer(string container) =>
        SetContainer(new Container(container));

    /// <summary>
    /// Sets conversion format.
    /// </summary>
    [Obsolete("Use SetContainer instead."), ExcludeFromCodeCoverage]
    public ConversionRequestBuilder SetFormat(ConversionFormat format) =>
        SetContainer(new Container(format.Name));

    /// <summary>
    /// Sets conversion format.
    /// </summary>
    [Obsolete("Use SetContainer instead."), ExcludeFromCodeCoverage]
    public ConversionRequestBuilder SetFormat(string format) =>
        SetContainer(format);

    /// <summary>
    /// Sets conversion preset.
    /// </summary>
    public ConversionRequestBuilder SetPreset(ConversionPreset preset)
    {
        _preset = preset;
        return this;
    }

    /// <summary>
    /// Builds the resulting request.
    /// </summary>
    public ConversionRequest Build() => new(
        _ffmpegCliFilePath ?? DefaultFFmpegCliPathLazy.Value,
        _outputFilePath,
        _container ?? GetDefaultContainer(),
        _preset
    );
}

public partial class ConversionRequestBuilder
{
    private static readonly Lazy<string> DefaultFFmpegCliPathLazy = new(() =>
        // Try to find FFmpeg in the probe directory
        Directory
            .EnumerateFiles(AppDomain.CurrentDomain.BaseDirectory ?? Directory.GetCurrentDirectory())
            .FirstOrDefault(f =>
                string.Equals(
                    Path.GetFileNameWithoutExtension(f),
                    "ffmpeg",
                    StringComparison.OrdinalIgnoreCase
                )
            ) ??

        // Otherwise fallback to just "ffmpeg" and hope it's on the PATH
        "ffmpeg"
    );
}