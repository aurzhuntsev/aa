﻿using AudioMark.Core.Settings;
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

        protected override void RenderInternal(SKCanvas canvas)
        {
            const int labelOffset = 5;
            const int labelTop = 25;

            canvas.Clear();

            if (Bins != null && Pointer.HasValue)
            {
                var selectedBin = Bins.FirstOrDefault(bin => Pointer.Value.X >= bin.Left && Pointer.Value.X < bin.Right);
                if (selectedBin != null)
                {
                    canvas.DrawRect(selectedBin.Left, 0, selectedBin.Right - selectedBin.Left, (float)Context.Bounds.Height, new SKPaint()
                    {
                        Color = new SKColor(255, 255, 255, 30)
                    });

                    var db = (20.0 * Math.Log10(1.0 / selectedBin.Value)).ToString("F2");
                    var freq = (Context.MaxFrequency * selectedBin.SpectrumBin / Context.SpectrumBins).ToString("F2");
                    var labelText = $"-{db}dB@{freq}Hz";
                    
                    var labelPosition = new SKRect();
                    _labelPaint.MeasureText(labelText, ref labelPosition);

                    if (selectedBin.Right + labelPosition.Width + 2 * labelOffset < Context.Bounds.Width)
                    {
                        labelPosition.Left = selectedBin.Right + labelOffset;
                    }
                    else
                    {
                        labelPosition.Left = selectedBin.Left - labelPosition.Width - labelOffset;
                    }

                    canvas.DrawText(labelText, labelPosition.Left, labelTop, _labelPaint);
                }
            }
        }
    }
}
