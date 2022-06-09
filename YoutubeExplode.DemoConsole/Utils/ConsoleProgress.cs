using System;
using System.IO;

namespace YoutubeExplode.DemoConsole.Utils;

internal class ConsoleProgress : IProgress<double>, IDisposable
{
    private readonly TextWriter _writer;
    private readonly int _posX;
    private readonly int _posY;

    public ConsoleProgress(TextWriter writer)
    {
        _writer = writer;
        _posX = Console.CursorLeft;
        _posY = Console.CursorTop;
    }

    public ConsoleProgress()
        : this(Console.Out)
    {
    }

    public void Report(double progress)
    {
        Console.SetCursorPosition(_posX, _posY);
        _writer.Write($"{progress:P1}");
    }

    public void Dispose()
    {
        Console.SetCursorPosition(_posX, _posY);
        _writer.Write("Completed ✓");
    }
}