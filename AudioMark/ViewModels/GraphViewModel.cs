using AudioMark.Core.Common;
using AudioMark.Core.Settings;
using AudioMark.Views;
using Avalonia;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Skia;
using ReactiveUI;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AudioMark.ViewModels
{
    public class GraphViewModel : ViewModelBase, IGraphViewUpdateHandle
    {
        private const int MaxFrameDurationMilliseconds = 30; // ~30fps

        private const int MinValueExponent = 10;

        private double _kFrequencyToViewX = 0.0;
        private double _kDbToViewY = 0.0;

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

        private SpectralData _data;
        public SpectralData Data
        {
            get => _data;
            set => this.RaiseAndSetIfChanged(ref _data, value);
        }

        public double MaxFrequency
        {
            get => Data == null ? (AppSettings.Current.Device.SampleRate / 2.0) : Data.MaxFrequency;
        }

        public Bitmap Source { get; set; }
        public EventWaitHandle Handle { get; set; } = new EventWaitHandle(false, EventResetMode.ManualReset);
        public EventHandler<GraphPointerEventArgs> OnPointerChanged { get; set; }

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

        private readonly SKPaint _dataPaint = new SKPaint()
        {
            Color = new SKColor(255, 45, 0, 255),
            IsAntialias = true
        };

        private readonly EventWaitHandle _renderWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);

        private SKPoint? _pointer = null;

        public GraphViewModel()
        {
            this.WhenAnyValue(_ => _.ViewBounds)
                .Subscribe(_ => OnViewContextChanged());

            this.WhenAnyValue(_ => _.Data)
                .Subscribe(_ => OnViewContextChanged());

            this.OnPointerChanged += PointerChanged;

            Task.Run(() =>
            {
                DateTime lastUpdated = DateTime.Now;
                while (true)
                {
                    _renderWaitHandle.WaitOne(MaxFrameDurationMilliseconds);
                    _renderWaitHandle.Reset();

                    try
                    {
                        RenderInternal();
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex);
                    }

                    var frameDuration = (int)DateTime.Now.Subtract(lastUpdated).Duration().TotalMilliseconds;
                    lastUpdated = DateTime.Now;
                    if (frameDuration < MaxFrameDurationMilliseconds)
                    {
                        Thread.Sleep(MaxFrameDurationMilliseconds - frameDuration);
                    }
                }
            });
        }

        private void PointerChanged(object sender, GraphPointerEventArgs e)
        {
            if (e.Present)
            {
                _pointer = new SKPoint(e.Left, e.Top);
            }
            else
            {
                _pointer = null;
            };
        }

        public void Render()
        {
            _renderWaitHandle.Set();
        }

        private void OnViewContextChanged()
        {
            if (ViewBounds != null && ViewBounds.Width > 0 && ViewBounds.Height > 0)
            {
                SetImageSize(ViewBounds);

                _kFrequencyToViewX = (ViewBounds.Width / Math.Log10(MaxFrequency));
                _kDbToViewY = (ViewBounds.Height / Math.Log10(Math.Pow(10.0, -MinValueExponent)));

                Render();
            }
        }

        private void SetImageSize(Rect bounds)
        {
            DisposeSkObject();

            _skImageInfo = new SKImageInfo((int)bounds.Width, (int)bounds.Height);
            _skSurface = SKSurface.Create(_skImageInfo);
            _skCanvas = _skSurface.Canvas;
        }

        private void DisposeSkObject()
        {
            /*if (_skSurface != null)
            {
                _skSurface.Dispose();
            }*/
        }

        private void RenderInternal()
        {
            _skCanvas.Clear(new SKColor(30, 30, 31));

            DrawGrid();
            DrawData();

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

        private void DrawData()
        {
            if (Data != null)
            {
                int currentOffset = 0;
                int binWidth = 0;
                int startingBin = 2;
                int bins = 0;
                bool isFirstPoint = true;
                int lx = 0, ly = 0;

                while (startingBin + bins < MaxFrequency)
                {
                    while (currentOffset == (binWidth = FreqToViewX(startingBin + bins)))
                    {
                        bins++;
                    }

                    var meanFrequency = double.MinValue;
                    for (var i = startingBin; i < startingBin + bins; i++)
                    {
                        var stat = Data.Statistics[i];
                        if (meanFrequency < stat.Mean)
                        {
                            meanFrequency = stat.Mean;
                        }
                    }

                    var x = currentOffset + (int)((binWidth - currentOffset) * 0.5);
                    var y = DbToViewY(meanFrequency);

                    if (isFirstPoint)
                    {
                        isFirstPoint = false;
                    }
                    else
                    {
                        _skCanvas.DrawLine(lx, ly, x, y, _dataPaint);
                        if (_pointer != null)
                        {
                            if (_pointer.Value.X >= currentOffset && _pointer.Value.X < binWidth)
                            {
                                _skCanvas.DrawLine(x, 0, x, (float)ViewBounds.Height - 1, _gridLabelPaint);
                            }
                        }
                    }

                    lx = x;
                    ly = y;

                    startingBin = startingBin + bins;
                    bins = 0;

                    currentOffset = binWidth;
                }
            }
        }

        private void DrawGrid()
        {
            DrawHorizontalGuides();
            DrawVerticalGuides();
        }

        private void DrawHorizontalGuides()
        {
            const int labelMargin = 5;

            var canvas = _skCanvas;
            var current = 1.0;

            for (current = 0.5; current < MinValueExponent; current++)
            {
                var y = DbToViewY(Math.Pow(10.0, -current));
                canvas.DrawLine(0, y, (float)ViewBounds.Width, y, _secondaryGridPaint);
            }

            for (current = 1; current < MinValueExponent; current++)
            {
                var y = DbToViewY(Math.Pow(10.0, -current));
                canvas.DrawLine(0, y, (float)ViewBounds.Width, y, _primaryGridPaint);

                var labelText = $"-{current * 20.0}dB";
                canvas.DrawText(labelText, (float)(ViewBounds.Width - _gridLabelPaint.MeasureText(labelText) - labelMargin), y - labelMargin, _gridLabelPaint);
            }
        }

        private void DrawVerticalGuides()
        {
            const int labelMargin = 5;

            var canvas = _skCanvas;
            var step = 1;
            var current = 0.0;

            do
            {
                current = Math.Pow(10.0, step);

                var x = FreqToViewX(current);
                canvas.DrawLine(x, 0, x, (float)ViewBounds.Height, _primaryGridPaint);

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
                for (var cx = current * 2.0; cx < nextStepPow10 && cx < MaxFrequency; cx += current)
                {
                    x = FreqToViewX(cx);
                    canvas.DrawLine(x, 0, x, (float)ViewBounds.Height, _secondaryGridPaint);
                }

                step++;
            } while (current < MaxFrequency);
        }

        private int FreqToViewX(double value)
        {
            if (value == 0.0)
            {
                return 0;
            }

            return (int)(_kFrequencyToViewX * Math.Log10(value));
        }

        private int DbToViewY(double value)
        {
            return (int)(-_kDbToViewY * Math.Log10(1.0 / value));
        }
    }
}
