using AudioMark.Core.Settings;
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

        private readonly SKPaint _labelPaint = new SKPaint()
        {
            Color = new SKColor(150, 150, 160),
            IsAntialias = true,
            LcdRenderText = true
        };

        private void DrawLabelAtBin(SKCanvas canvas, Bin bin, string text)
        {
            try
            {
                const int labelOffset = 5;
                const int labelTop = 25;

                var labelPosition = new SKRect();
                _labelPaint.MeasureText(text, ref labelPosition);

                if (bin.Right + labelPosition.Width + 2 * labelOffset < Context.Bounds.Width)
                {
                    labelPosition.Left = bin.Right + labelOffset;
                }
                else
                {
                    labelPosition.Left = bin.Left - labelPosition.Width - labelOffset;
                }

                canvas.DrawText(text, labelPosition.Left, labelTop, _labelPaint);
            }
            catch (Exception x)
            {
                int a = 10;
            }
        }

        protected override void RenderInternal(SKCanvas canvas)
        {
            canvas.Clear();
            if (Bins != null)
            {
                var labeledBins = Bins.Where(bin => bin.Labels != null && bin.Labels.Any()).ToList();
                foreach (var bin in labeledBins)
                {
                    canvas.DrawRect(bin.Left, 0, bin.Right - bin.Left, (float)Context.Bounds.Height, new SKPaint()
                    {
                        Color = new SKColor(255, 0, 0, 30)
                    });

                    DrawLabelAtBin(canvas, bin, bin.Labels[0]);
                }
            }

            if (Bins != null && Pointer.HasValue)
            {
                var selectedBin = Bins.FirstOrDefault(bin => Pointer.Value.X >= bin.Left && Pointer.Value.X < bin.Right);
                if (selectedBin != null && (selectedBin.Labels == null || !selectedBin.Labels.Any()))
                {
                    canvas.DrawRect(selectedBin.Left, 0, selectedBin.Right - selectedBin.Left, (float)Context.Bounds.Height, new SKPaint()
                    {
                        Color = new SKColor(255, 255, 255, 30)
                    });

                    var db = (20.0 * Math.Log10(1.0 / selectedBin.Value)).ToString("F2");
                    var freq = (Context.MaxFrequency * selectedBin.SpectrumBin / Context.SpectrumBins).ToString("F2");
                    var labelText = $"-{db}dB@{freq}Hz";

                    DrawLabelAtBin(canvas, selectedBin, labelText);
                }
            }
        }
    }
}
