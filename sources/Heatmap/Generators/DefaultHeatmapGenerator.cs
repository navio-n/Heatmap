﻿using Heatmap.Gradients;
using Heatmap.Primitives;
using Heatmap.Ranges;
using Heatmap.Receivers;
using Heatmap.Samplers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace Heatmap.Generators
{
    public sealed class DefaultHeatmapGenerator : IHeatmapGenerator
    {
        private ConcurrentBag<PositionedSample> PositionedSamples { get; } = new();

        public async Task SampleAsync(ISampler sampler, Viewport viewport, Resolution resolution)
        {
            //PositionedSamples.Clear();

            while (!PositionedSamples.IsEmpty)
            {
                PositionedSamples.TryTake(out PositionedSample _);
            }

            var sampleSize = new Vector2(1f) / resolution;

            foreach (int y in Enumerable.Range(0, resolution.Height))
                foreach (int x in Enumerable.Range(0, resolution.Width))
                {
                    Vector2 unitPosition = new Vector2(x, y) / resolution;
                    var viewPoint = viewport.GetViewPoint(unitPosition);
                    var sample = await sampler.GetAsync(viewPoint);

                    PositionedSamples.Add(new PositionedSample(unitPosition, sampleSize, sample));
                }   
        }

        public void Push(IRange range, IGradient gradient, IReceiver receiver)
        {
            var positionedSamples = GetPositionedSamples();

            foreach (PositionedSample positionedSample in positionedSamples)
            {
                var rangedFragment = range.GetValue(positionedSample.Value);
                var color = gradient.GetColor(rangedFragment);
                receiver.Receive(Fragment.FromPositionedSample(positionedSample, color));
            }
        }

        public IEnumerable<PositionedSample> GetPositionedSamples() => PositionedSamples.ToArray();
    }
}
