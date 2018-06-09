using System;
using Tyrrrz.Extensions;

namespace DemoConsole
{
    public class ProgressBar : IProgress<double>, IDisposable
    {
        private const int MaxBars = 25;

        private double _progress;
        private int _barsDrawn;
        private int _barsOffset;

        public ProgressBar()
        {
            Initialize();
        }

        private void Initialize()
        {
            // Create empty progress bar
            Console.Write('[');
            _barsOffset = Console.CursorLeft;
            Console.Write(' '.Repeat(MaxBars));
            Console.Write(']');
        }

        private void DrawProgress()
        {
            // Draw bars
            var bars = (int) Math.Floor(_progress * MaxBars);
            for (var i = _barsDrawn; i < bars; i++)
            {
                Console.SetCursorPosition(_barsOffset + i, Console.CursorTop);
                Console.Write('-');
            }

            _barsDrawn = bars;

            // Draw text
            Console.SetCursorPosition(_barsOffset + MaxBars + 3, Console.CursorTop);
            Console.Write($"{_progress:P0}");
        }

        public void Report(double value)
        {
            _progress = value;
            DrawProgress();
        }

        public void Dispose()
        {
            Console.WriteLine();
        }
    }
}