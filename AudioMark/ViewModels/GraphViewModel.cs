using AudioMark.Core.Settings;
using AudioMark.Views;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Skia;
using ReactiveUI;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace AudioMark.ViewModels
{
    public class GraphViewModel : ViewModelBase, IGraphViewUpdateHandle
    {
        private const int MinValueExponent = 10;

        private SKImageInfo _skImageInfo;
        private SKSurface _skSurface;
        private SKCanvas _skCanvas;

        private Rect _viewBounds;
        public Rect ViewBounds
        {
            get => _viewBounds;
            set => this.RaiseAndSetIfChanged(ref _viewBounds, value);
        }

        private Bitmap _bitmap;
        public Bitmap Bitmap
        {
            get => _bitmap;
            set => this.RaiseAndSetIfChanged(ref _bitmap, value);
        }

        private double[] _data;
        public double[] Data
        {
            get => _data;
            set
            {
                this.RaiseAndSetIfChanged(ref _data, value);
            }
        }

        public Bitmap Source { get; set; }
        public EventWaitHandle Handle { get; set; } = new EventWaitHandle(false, EventResetMode.ManualReset);

        public GraphViewModel()
        {            
            this.WhenAnyValue(_ => _.ViewBounds)
                .Subscribe(OnViewBoundsChanged);
            
        }

        private void OnViewBoundsChanged(Rect bounds)
        {
            if (bounds != null && bounds.Width > 0 && bounds.Height > 0)
            {
                SetImageSize(bounds);
            }
        }

        private void SetImageSize(Rect bounds)
        {
            DisposeSkObject();

            _skImageInfo = new SKImageInfo((int)bounds.Width, (int)bounds.Height);
            _skSurface = SKSurface.Create(_skImageInfo);
            _skCanvas = _skSurface.Canvas;

            Render();
        }

        private void DisposeSkObject()
        {
            /*if (_skSurface != null)
            {
                _skSurface.Dispose();
            }*/
        }

        public void Render()
        {
            _skCanvas.Clear(new SKColor(30, 30, 31));
            DrawGrid();

            if (Data != null)
            {
                var x0 = 0;
                var val = 0.0;
                bool firstPoint = true;
                int px = 0;
                int py = 0;
                for (var x = 0; x < ViewBounds.Width; x++)
                {
                    var x1 = (int)Math.Pow(10.0, Math.Log10(AppSettings.Current.Device.SampleRate / 2) / ViewBounds.Width * x);
                    if (x1 - x0 > 0)
                    {
                        var subset = Data.Skip(x0).Take(x1 - x0);
                        val = subset.Any() ? subset.Max() : 0;
                    }

                    x0 = x1;
                    var y = ToViewY(val == 0 ? Math.Pow(10.0, -MinValueExponent + 1) : val);
                    if (firstPoint)
                    {
                        _skCanvas.DrawCircle(x, y, 1, new SKPaint() { Color = SKColors.Gold });
                        firstPoint = false;
                    }
                    else
                    {
                        _skCanvas.DrawLine(px, py, x, y, new SKPaint() { Color = SKColors.Gold });
                    }

                    px = x;
                    py = y;
                }
            }

            using (var image = _skSurface.Snapshot())
            {
                using (var bitmap = SKBitmap.FromImage(image))
                {
                    if (Bitmap != null)
                    {
                        Bitmap.Dispose();
                    }

                    Source = new Bitmap(bitmap.ColorType.ToPixelFormat(), bitmap.GetPixels(), new PixelSize(bitmap.Width, bitmap.Height), SkiaPlatform.DefaultDpi, bitmap.RowBytes);
                    Handle.Set();
                }
            }
        }

        private void DrawGrid()
        {
            var canvas = _skCanvas;
            var step = 1;
            var current = 0.0;

            var horizontalPrimaryGridPaint = new SKPaint()
            {
                Color = new SKColor(62, 62, 66),
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1,
                PathEffect = SKPathEffect.CreateDash(new float[] { 6, 2 }, 0)
            };

            var horizontalSecondaryGridPaint = new SKPaint()
            {
                Color = new SKColor(45, 45, 48),
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1,
                PathEffect = SKPathEffect.CreateDash(new float[] { 4, 2 }, 0)
            };


            while ((current = Math.Pow(10.0, step)) < AppSettings.Current.Device.SampleRate / 2)
            {
                var x = ToViewX(current);
                canvas.DrawLine(x, 0, x, (float)ViewBounds.Height, horizontalPrimaryGridPaint);

                for (var cx = current + Math.Pow(10.0, step); cx < Math.Pow(10.0, step + 1) && cx < AppSettings.Current.Device.SampleRate / 2; cx += Math.Pow(10.0, step))
                {
                    var lx = ToViewX(cx);
                    canvas.DrawLine(lx, 0, lx, (float)ViewBounds.Height, horizontalSecondaryGridPaint);
                }
                step++;
            }

            step = 1;
            current = 1.0;


            for (current = 1; current < MinValueExponent; current++)
            {
                var y = ToViewY(Math.Pow(10.0, -current));
                canvas.DrawLine(0, y, (float)ViewBounds.Width, y, horizontalPrimaryGridPaint);
            }
        }

        private int ToViewX(double value)
        {
            /* TODO: Get rid of AppSettings here */
            return (int)((ViewBounds.Width / Math.Log10(AppSettings.Current.Device.SampleRate / 2)) * Math.Log10(value));
        }

        private int ToViewY(double value)
        {
            var k = (ViewBounds.Height / Math.Log10(Math.Pow(10.0, -MinValueExponent)));
            return (int)(-k * Math.Log10(1.0 / value));
        }
    }
}
