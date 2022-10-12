using System;
using System.Diagnostics.CodeAnalysis;
using YoutubeExplode.Videos.Streams;

namespace YoutubeExplode.Converter;

/// <summary>
/// Conversion options.
/// </summary>
public class ConversionRequest
{
    /// <summary>
    /// Path to FFmpeg CLI.
    /// </summary>
    public string FFmpegCliFilePath { get; }

    /// <summary>
    /// Output file path.
    /// </summary>
    public string OutputFilePath { get; }

    /// <summary>
    /// Output container.
    /// </summary>
    public Container Container { get; }

    /// <summary>
    /// Output format.
    /// </summary>
    [Obsolete("Use Container instead."), ExcludeFromCodeCoverage]
    public ConversionFormat Format => new(Container.Name);

    /// <summary>
    /// Encoder preset.
    /// </summary>
    public ConversionPreset Preset { get; }

    /// <summary>
    /// Initializes an instance of <see cref="ConversionRequest" />.
    /// </summary>
    public ConversionRequest(
        string ffmpegCliFilePath,
        string outputFilePath,
        Container container,
        ConversionPreset preset)
    {
        FFmpegCliFilePath = ffmpegCliFilePath;
        OutputFilePath = outputFilePath;
        Container = container;
        Preset = preset;
    }

    /// <summary>
    /// Initializes an instance of <see cref="ConversionRequest" />.
    /// </summary>
    [Obsolete("Use the other constructor overload"), ExcludeFromCodeCoverage]
    public ConversionRequest(
        string ffmpegCliFilePath,
        string outputFilePath,
        ConversionFormat format,
        ConversionPreset preset)
        : this(ffmpegCliFilePath, outputFilePath, new Container(format.Name), preset)
    {
    }
}