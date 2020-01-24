using AudioMark.Common;
using AudioMark.Core.Common;
using Avalonia;
using Avalonia.Controls;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
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
                foreach (var series in Series)
                {
                    if (!series.Visible)
                    {
                        continue;
                    }

                    bool isFirstPoint = true;
                    int lx = 0, ly = 0;

                    foreach (var bin in Bins)
                    {
                        var x = bin.Left + (int)((bin.Right - bin.Left) * 0.5);
                        var y = Context.DbToViewY(GetSeriesValue(series.Data, bin));
                        if (y < 0)
                        {
                            continue;
                        }

                        if (isFirstPoint)
                        {
                            isFirstPoint = false;
                        }
                        else
                        {
                            canvas.DrawLine(lx, ly, x, y, SeriesColorConverter.GetSKPaint(series.ColorIndex));
                        }

                        lx = x;
                        ly = y;
                    }
                }
            }
        }
    }
}