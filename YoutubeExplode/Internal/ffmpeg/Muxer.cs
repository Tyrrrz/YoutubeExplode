using System;
using System.Diagnostics;
using YoutubeExplode.Exceptions;

namespace YoutubeExplode.Internal.ffmpeg
{
#if NETSTANDARD2_0 || NET45 || NETCOREAPP1_0
    internal class Muxer : IDisposable
    {
        public string FFmpegPath { get; set; }

        public bool OverwriteFiles { get; set; }

        private Process _FFmpegProcess;

        public Muxer(string fFmpegPath, bool overwriteFiles)
        {
            this.FFmpegPath = fFmpegPath;
            this.OverwriteFiles = overwriteFiles;
        }

        public void Mux(string audioFile, string videoFile, string muxedFile, LogLevel logLevel)
        {
            var overwriteArgument = OverwriteFiles ? "-y" : "-n";

            _FFmpegProcess = new Process();
            _FFmpegProcess.EnableRaisingEvents = true;
            _FFmpegProcess.StartInfo.FileName = FFmpegPath;
            _FFmpegProcess.StartInfo.CreateNoWindow = true;
            _FFmpegProcess.StartInfo.UseShellExecute = false;
            _FFmpegProcess.StartInfo.RedirectStandardError = true;
            _FFmpegProcess.StartInfo.Arguments = $"-v {logLevel} {overwriteArgument} -i \"{audioFile}\" -i \"{videoFile}\" -c copy \"{muxedFile}\"";
            _FFmpegProcess.ErrorDataReceived += (s, e) =>
            {
                if (e.Data != null)
                    throw new FFmpegException(e.Data);
            };

            _FFmpegProcess.Start();
            _FFmpegProcess.BeginErrorReadLine();

            _FFmpegProcess.WaitForExit();
        }

        public void Dispose()
        {
            _FFmpegProcess?.Dispose();
        }
    }
#endif
}
