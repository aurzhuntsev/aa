using AudioMark.Core.Common;
using Avalonia;
using Avalonia.Controls;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMark.Views.GraphView
{
    public class SpectralDataRenderer : ImageRendererBase
    {
        private readonly SKPaint _dataPaint = new SKPaint()
        {
            Color = new SKColor(255, 45, 0, 255),
            IsAntialias = true
        };
        
        public SpectralDataRenderer(Image target) : base(target) { }

        protected override void RenderInternal(SKCanvas canvas)
        {
            canvas.Clear();

            if (Bins != null)
            {
                bool isFirstPoint = true;
                int lx = 0, ly = 0;

                foreach (var bin in Bins)
                {
                    var x = bin.Left + (int)((bin.Right - bin.Left) * 0.5);
                    var y = Context.DbToViewY(bin.Frequency);

                    if (isFirstPoint)
                    {
                        isFirstPoint = false;
                    }
                    else
                    {
                        canvas.DrawLine(lx, ly, x, y, _dataPaint);
                    }

                    lx = x;
                    ly = y;
                }
            }
        }
    }
}