﻿using Heatmap.Primitives;
using System.Linq;

namespace Heatmap.Gradients
{
    public class PositionedGradient : IGradient
    {
        private PositionedColor[] PositionedColors { get; }

        public PositionedGradient(params PositionedColor[] positionedColors)
        {
            PositionedColors = positionedColors.OrderBy(positionedColor => positionedColor).ToArray();
        }

        public RgbColor GetColor(double position)
        {
            for (var index = 0; index < PositionedColors.Length; index++)
            {
                var currentPositionedColor = PositionedColors[index];

                if (position < currentPositionedColor.Position)
                {
                    if (index == 0)
                        return currentPositionedColor.Color;

                    var previousPositionedColor = PositionedColors[index - 1];
                    var sectionLength = currentPositionedColor.Position - previousPositionedColor.Position;
                    var offset = (position - previousPositionedColor.Position) / sectionLength;
                    return RgbColor.Lerp(previousPositionedColor.Color, currentPositionedColor.Color, offset);
                }
            }
            //return PositionedColors[^1].Color;
            return PositionedColors[PositionedColors.Length-1].Color;
        }

        public override string ToString() => "Positioned";
    }
}
