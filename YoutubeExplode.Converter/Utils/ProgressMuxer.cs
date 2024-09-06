using System;
using System.Collections.Generic;
using System.Threading;

namespace YoutubeExplode.Converter.Utils;

internal class ProgressMuxer(IProgress<double> target)
{
    private readonly Lock _lock = new();
    private readonly Dictionary<int, double> _splitWeights = new();
    private readonly Dictionary<int, double> _splitValues = new();

    public IProgress<double> CreateInput(double weight = 1)
    {
        using (_lock.EnterScope())
        {
            var index = _splitWeights.Count;

            _splitWeights[index] = weight;
            _splitValues[index] = 0;

            return new Progress<double>(p =>
            {
                using (_lock.EnterScope())
                {
                    _splitValues[index] = p;

                    var weightedSum = 0.0;
                    var weightedMax = 0.0;

                    for (var i = 0; i < _splitWeights.Count; i++)
                    {
                        weightedSum += _splitWeights[i] * _splitValues[i];
                        weightedMax += _splitWeights[i];
                    }

                    target.Report(weightedSum / weightedMax);
                }
            });
        }
    }
}
