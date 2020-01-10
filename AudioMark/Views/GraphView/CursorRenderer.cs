using Avalonia;
using Avalonia.Controls;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AudioMark.Views.GraphView
{
    public class CursorRenderer : ImageRendererBase
    {
        public CursorRenderer(Image target) : base(target) { }

        public Point? Pointer { get; set; }

        protected override void RenderInternal(SKCanvas canvas)
        {
            canvas.Clear();

            if (Bins != null && Pointer.HasValue)
            {
                var selectedBin = Bins.FirstOrDefault(bin => Pointer.Value.X >= bin.Left && Pointer.Value.X < bin.Right);
                if (selectedBin != null)
                {
                    float x =selectedBin.Left + 0.5f * (selectedBin.Right - selectedBin.Left);
                    canvas.DrawLine(x, 0, x, (float)Context.Bounds.Height, new SKPaint() { Color = SKColors.White });
                }
            }
        }
    }
}
