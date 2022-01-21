using System;
using System.Collections.Generic;
using System.Linq;

namespace YoutubeExplode.Converter.Utils;

internal class ProgressMuxer
{
    private readonly object _lock = new();

    private readonly IProgress<double> _output;
    private readonly Dictionary<int, double> _splitTotals;

    private int _splitCount;

    public ProgressMuxer(IProgress<double> output)
    {
        _output = output;
        _splitTotals = new Dictionary<int, double>();
    }

    public IProgress<double> Fork(double multiplier)
    {
        var index = _splitCount++;
        return new Progress<double>(p =>
        {
            lock (_lock)
            {
                _splitTotals[index] = multiplier * p;
                _output.Report(_splitTotals.Values.Sum());
            }
        });
    }
}