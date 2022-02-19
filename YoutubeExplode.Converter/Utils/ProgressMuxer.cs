using System;
using System.Collections.Generic;

namespace YoutubeExplode.Converter.Utils;

internal class ProgressMuxer
{
    private readonly IProgress<double> _target;

    private readonly object _lock = new();
    private readonly Dictionary<int, double> _splitWeights = new();
    private readonly Dictionary<int, double> _splitValues = new();

    public ProgressMuxer(IProgress<double> target) =>
        _target = target;

    public IProgress<double> CreateInput(double weight = 1)
    {
        lock (_lock)
        {
            var index = _splitWeights.Count;

            _splitWeights[index] = weight;
            _splitValues[index] = 0;

            return new Progress<double>(p =>
            {
                lock (_lock)
                {
                    _splitValues[index] = p;

                    var weightedSum = 0.0;
                    var weightedMax = 0.0;

                    for (var i = 0; i < _splitWeights.Count; i++)
                    {
                        weightedSum += _splitWeights[i] * _splitValues[i];
                        weightedMax += _splitWeights[i];
                    }

                    _target.Report(weightedSum / weightedMax);
                }
            });
        }
    }
}