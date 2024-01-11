using System;
using System.Diagnostics.CodeAnalysis;
using YoutubeExplode.Videos.Streams;

namespace YoutubeExplode.Converter;

/// <summary>
/// Conversion options.
/// </summary>
public class ConversionRequest(
    string ffmpegCliFilePath,
    string outputFilePath,
    Container container,
    ConversionPreset preset
)
{
    /// <summary>
    /// Initializes an instance of <see cref="ConversionRequest" />.
    /// </summary>
    [Obsolete("Use the other constructor overload"), ExcludeFromCodeCoverage]
    public ConversionRequest(
        string ffmpegCliFilePath,
        string outputFilePath,
        ConversionFormat format,
        ConversionPreset preset
    )
        : this(ffmpegCliFilePath, outputFilePath, new Container(format.Name), preset) { }

    /// <summary>
    /// Path to the FFmpeg CLI.
    /// </summary>
    public string FFmpegCliFilePath { get; } = ffmpegCliFilePath;

    /// <summary>
    /// Output file path.
    /// </summary>
    public string OutputFilePath { get; } = outputFilePath;

    /// <summary>
    /// Output container.
    /// </summary>
    public Container Container { get; } = container;

    /// <summary>
    /// Output format.
    /// </summary>
    [Obsolete("Use the Container property instead."), ExcludeFromCodeCoverage]
    public ConversionFormat Format => new(Container.Name);

    /// <summary>
    /// Encoder preset.
    /// </summary>
    public ConversionPreset Preset { get; } = preset;
}
