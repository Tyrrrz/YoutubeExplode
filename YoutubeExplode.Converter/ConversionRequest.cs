namespace YoutubeExplode.Converter
{
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
        /// Output format.
        /// </summary>
        public ConversionFormat Format { get; }

        /// <summary>
        /// Encoder preset.
        /// </summary>
        public ConversionPreset Preset { get; }

        /// <summary>
        /// Initializes an instance of <see cref="ConversionRequest"/>.
        /// </summary>
        public ConversionRequest(
            string ffmpegCliFilePath,
            string outputFilePath,
            ConversionFormat format,
            ConversionPreset preset)
        {
            FFmpegCliFilePath = ffmpegCliFilePath;
            OutputFilePath = outputFilePath;
            Format = format;
            Preset = preset;
        }
    }
}