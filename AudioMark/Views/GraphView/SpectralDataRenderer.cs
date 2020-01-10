using AudioMark.Core.Common;
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

        public SpectralData DataSource { get; set; }

        public SpectralDataRenderer(Image target) : base(target) { }

        protected override void RenderInternal(SKCanvas canvas)
        {
            canvas.Clear();

            if (DataSource != null)
            {
                int currentOffset = 0;
                int binWidth = 0;
                int startingBin = 2;
                int bins = 0;
                bool isFirstPoint = true;
                int lx = 0, ly = 0;

                while (startingBin + bins < Context.MaxFrequency)
                {
                    while (currentOffset == (binWidth = Context.FreqToViewX(startingBin + bins)))
                    {
                        bins++;
                    }

                    var meanFrequency = double.MinValue;
                    for (var i = startingBin; i < startingBin + bins; i++)
                    {
                        var stat = DataSource.Statistics[i];
                        if (meanFrequency < stat.Mean)
                        {
                            meanFrequency = stat.Mean;
                        }
                    }

                    var x = currentOffset + (int)((binWidth - currentOffset) * 0.5);
                    var y = Context.DbToViewY(meanFrequency);

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

                    startingBin = startingBin + bins;
                    bins = 0;

                    currentOffset = binWidth;
                }
            }
        }
    }
}
