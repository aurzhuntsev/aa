using Avalonia.Controls;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMark.Views.GraphView
{
    public class GridRenderer : ImageRendererBase
    {
        private readonly SKPaint _gridLabelPaint = new SKPaint()
        {
            Color = new SKColor(99, 99, 106),
            IsAntialias = true
        };

        private readonly SKPaint _primaryGridPaint = new SKPaint()
        {
            Color = new SKColor(62, 62, 66),
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1,
            PathEffect = SKPathEffect.CreateDash(new float[] { 6, 2 }, 0)
        };

        private readonly SKPaint _secondaryGridPaint = new SKPaint()
        {
            Color = new SKColor(45, 45, 48),
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1,
            PathEffect = SKPathEffect.CreateDash(new float[] { 4, 2 }, 0)
        };

        private readonly SKColor _backgroundColor = new SKColor(30, 30, 31);

        public GridRenderer(Image target) : base(target) { }

        protected override void RenderInternal(SKCanvas canvas)
        {
            canvas.Clear(_backgroundColor);

            DrawHorizontalGuides(canvas);
            DrawVerticalGuides(canvas);
        }

        private void DrawHorizontalGuides(SKCanvas canvas)
        {
            const int labelMargin = 5;

            var current = 1.0;

            for (current = 0.5; current < Context.MinValueExponent; current++)
            {
                var y = Context.DbToViewY(Math.Pow(10.0, -current));
                canvas.DrawLine(0, y, (float)Context.Bounds.Width, y, _secondaryGridPaint);
            }

            for (current = 1; current < Context.MinValueExponent; current++)
            {
                var y = Context.DbToViewY(Math.Pow(10.0, -current));
                canvas.DrawLine(0, y, (float)Context.Bounds.Width, y, _primaryGridPaint);

                var labelText = $"-{current * 20.0}dB";
                canvas.DrawText(labelText, (float)(Context.Bounds.Width - _gridLabelPaint.MeasureText(labelText) - labelMargin), y - labelMargin, _gridLabelPaint);
            }
        }

        private void DrawVerticalGuides(SKCanvas canvas)
        {
            const int labelMargin = 5;

            var step = 1;
            var current = 0.0;

            do
            {
                current = Math.Pow(10.0, step);

                var x = Context.FreqToViewX(current);
                canvas.DrawLine(x, 0, x, (float)Context.Bounds.Height, _primaryGridPaint);

                var labelText = string.Empty;
                if (current >= 1000)
                {
                    labelText = $"{current / 1000.0}kHz";
                }
                else
                {
                    labelText = $"{current}Hz";
                }
                SKRect labelBounds = new SKRect();
                _gridLabelPaint.MeasureText(labelText, ref labelBounds);
                canvas.DrawText(labelText, x + labelMargin, labelMargin + labelBounds.Height, _gridLabelPaint);

                var nextStepPow10 = Math.Pow(10.0, step + 1);
                for (var cx = current * 2.0; cx < nextStepPow10 && cx < Context.MaxFrequency; cx += current)
                {
                    x = Context.FreqToViewX(cx);
                    canvas.DrawLine(x, 0, x, (float)Context.Bounds.Height, _secondaryGridPaint);
                }

                step++;
            } while (current < Context.MaxFrequency);
        }

    }
}
