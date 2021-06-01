﻿using System;
using System.Diagnostics;
using System.Text;

namespace LibSidWiz.Triggers
{
    // ReSharper disable once UnusedMember.Global
    internal class AutoCorrelationTrigger: ITriggerAlgorithm
    {
        private float[] _normalDistribution;

        public int GetTriggerPoint(Channel channel, int startIndex, int endIndex, int previousIndex)
        {
            var width = endIndex - startIndex;
            if (_normalDistribution == null || _normalDistribution.Length != width)
            {
                _normalDistribution = new float[width];
                // Generate distribution
                // We fit 2 standard deviations to the width 
                double variance = width * width / 16.0;
                double mu = width / 2.0;
                var scale = Math.Sqrt(2 * Math.PI * variance);
                for (int i = 0; i < width; ++i)
                {
                    _normalDistribution[i] = (float) (Math.Exp(-(i - mu) * (i - mu) / (2 * variance)) / scale);
                }
            }

            var maxCorrelation = double.MinValue;
            var bestOffset = startIndex;
            var previousStart = previousIndex - width / 2;
            // We compute the correlation between the previous window and each possible offset in the new one,
            // weighted by a normal distribution so we prefer ones near the middle.
            // The correlation is defined as
            // sum((x - mean(x)) * (y - mean(y)) / sqrt(sum(pow(x - mean(x), 2)) * sum(pow(y - mean(y), 2)))
            // where x and y are the samples in each series. Since the reference is fixed, we can compute it once.
            float sumY = 0;
            float sumX = 0;
            for (int i = 0; i < width; ++i)
            {
                sumY += channel.GetSample(previousStart + i);
                sumX += channel.GetSample(startIndex + i);
            }
            var meanY = sumY / width;
            float sumSquaredYDiff = 0;
            for (int i = 0; i < width; ++i)
            {
                var y = channel.GetSample(previousStart + i);
                var yDiff = y - meanY;
                sumSquaredYDiff += yDiff * yDiff;
            }

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (sumSquaredYDiff == 0.0f)
            {
                // No point continuing - we return the middle of the data
                return startIndex + width / 2;
            }

            var correlations = new double[width];
            for (int trialOffset = 0; trialOffset < width; ++trialOffset)
            {
                // We compute mean(x) as we go
                var meanX = sumX / width;
                // ...by amending the moving sum. This will accumulate floating point error but it's not significant.
                sumX -= channel.GetSample(startIndex + trialOffset);
                sumX += channel.GetSample(startIndex + trialOffset + width);

                // sum((x - mean(x)) * (y - mean(y)) / sqrt(sum(pow(x - mean(x), 2)) * sum(pow(y - mean(y), 2)))
                float sumTop = 0;
                float sumSquaredXDiff = 0;
                for (int i = 0; i < width; ++i)
                {
                    var x = channel.GetSample(startIndex + trialOffset + i);
                    var y = channel.GetSample(previousStart + i);
                    sumTop += (x - meanX) * (y - meanY);
                    sumSquaredXDiff += (x - meanX) * (x - meanX);
                }

                var correlation = sumTop / Math.Sqrt(sumSquaredXDiff * sumSquaredYDiff);

                // We then weight by our normal distribution so we will prefer points near the middle.
                correlation *= _normalDistribution[trialOffset];

                // debug
                correlations[trialOffset] = correlation;

                if (correlation > maxCorrelation)
                {
                    maxCorrelation = correlation;
                    bestOffset = trialOffset;
                }
            }

#if DEBUG
            Debug.WriteLine($"Autocorrelation: between {startIndex} and {endIndex}, max = {maxCorrelation}, offset = {bestOffset} ({(float)(bestOffset - startIndex)/(endIndex - startIndex):P})");

            var sb = new StringBuilder();
            for (int i = 0; i < width; ++i)
            {
                sb.AppendLine($"{channel.GetSample(previousStart + i)}\t{channel.GetSample(startIndex + i)}\t{correlations[i]}");
            }
#endif

            return startIndex + bestOffset;
        }
    }
}
