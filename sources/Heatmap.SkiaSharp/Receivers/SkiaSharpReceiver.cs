using Heatmap.Primitives;
using Heatmap.Receivers;
using SkiaSharp;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Heatmap.SkiaSharp.Receivers
{
    public sealed class SkiaSharpReceiver : IReceiver
    {
        private ConcurrentBag<Fragment> Fragments { get; } = new();

        public void Receive(Fragment fragment) => Fragments.Add(fragment);

        public Task<Stream> GetPngStreamAsync(Resolution resolution)
        {
            using SKBitmap bitmap = new(resolution.Width, resolution.Height, SKColorType.Rgba8888, SKAlphaType.Unpremul);

            //byte[,,] buffer = new byte[resolution.Height, resolution.Width, 4];
            uint[,] buffer = new uint[resolution.Height, resolution.Width];

            foreach (var fragment in Fragments)
            {
                var basePosition = fragment.Position * resolution;
                var pixelSize = fragment.Size * resolution;

                for (var offsetX = 0; offsetX < Math.Ceiling(pixelSize.X); offsetX++)
                    for (var offsetY = 0; offsetY < Math.Ceiling(pixelSize.Y); offsetY++)
                    {
                        var offset = new Vector2(offsetX, offsetY);
                        var shifted = basePosition + offset;

                        var shiftedX = (int)shifted.X;
                        var shiftedY = (int)shifted.Y;

                        if (shiftedX >= 0 && shiftedY >= 0 && shiftedX < resolution.Width && shiftedY < resolution.Height)
                        {
                            var color = ConvertColor(fragment.Color);

                            buffer[shiftedY, shiftedX] = (uint)ConvertColor(fragment.Color);

                            //buffer[shiftedY, shiftedX, 0] = color.Red;
                            //buffer[shiftedY, shiftedX, 1] = color.Green;
                            //buffer[shiftedY, shiftedX, 2] = color.Blue;
                            //buffer[shiftedY, shiftedX, 3] = 0xFF;
                            //bitmap.SetPixel(shiftedX, shiftedY, color);
                        }
                        else
                        {

                        }

                    }
            }

            unsafe
            {
                //fixed (byte* ptr = buffer)
                fixed (uint* ptr = buffer)
                {
                    bitmap.SetPixels((IntPtr)ptr);
                }
            }

            var data = bitmap.Encode(SKEncodedImageFormat.Png, 100);
            return Task.FromResult(data.AsStream(true));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        //private static SKColor ConvertColor(RgbColor color) => (uint)((/*color.Alpha*/255 << 24) | (color.Blue << 16) | (color.Green << 8) | color.Red);
        private static SKColor ConvertColor(RgbColor color) => new(color.Red, color.Green, color.Blue);
    }
}
