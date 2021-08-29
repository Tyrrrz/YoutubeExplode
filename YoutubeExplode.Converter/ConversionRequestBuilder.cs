using System;
using System.IO;
using System.Linq;
using YoutubeExplode.Converter.Utils.Extensions;

namespace YoutubeExplode.Converter
{
    /// <summary>
    /// Builder for <see cref="ConversionRequest"/>.
    /// </summary>
    public partial class ConversionRequestBuilder
    {
        private readonly string _outputFilePath;

        private string? _ffmpegCliFilePath;
        private ConversionFormat? _format;
        private ConversionPreset _preset;

        /// <summary>
        /// Initializes an instance of <see cref="ConversionRequestBuilder"/>.
        /// </summary>
        public ConversionRequestBuilder(string outputFilePath) =>
            _outputFilePath = outputFilePath;

        private ConversionFormat GetDefaultFormat() => new(
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
        /// Sets conversion format.
        /// </summary>
        public ConversionRequestBuilder SetFormat(ConversionFormat format)
        {
            _format = format;
            return this;
        }

        /// <summary>
        /// Sets conversion format.
        /// </summary>
        public ConversionRequestBuilder SetFormat(string format) =>
            SetFormat(new ConversionFormat(format));

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
            _format ?? GetDefaultFormat(),
            _preset
        );
    }

    public partial class ConversionRequestBuilder
    {
        private static readonly Lazy<string> DefaultFFmpegCliPathLazy = new(() =>
            // Try to find FFmpeg in the probe directory
            new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory)
                .EnumerateFiles()
                .Select(f => f.FullName)
                .FirstOrDefault(f => string.Equals(Path.GetFileNameWithoutExtension(f), "ffmpeg",
                    StringComparison.OrdinalIgnoreCase))

            // Otherwise fallback to just "ffmpeg" and hope it's on the PATH
            ?? "ffmpeg"
        );
    }
}