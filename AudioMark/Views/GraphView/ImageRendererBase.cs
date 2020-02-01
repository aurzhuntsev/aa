using AudioMark.Core.Common;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Skia;
using Avalonia.Threading;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AudioMark.Views.GraphView
{
    /* TODO: Update to use RenderTargetBitmap/DrawingContext */
    public abstract class ImageRendererBase : IDisposable
    {
        private bool _autoUpdate;
        public bool AutoUpdate
        {
            get => _autoUpdate;
            set
            {
                _autoUpdate = value;
                _renderWaitHandle.Set();
            }
        }

        private Image _target;
        public Image Target
        {
            get => _target;
        }

        public ViewContext Context
        {
            get;
            private set;
        }

        public IEnumerable<Bin> Bins
        {
            get; set;
        }

        public IEnumerable<Series> Series
        {
            get; set;
        }

        public double MaxFramesPerSecond
        {
            get; set;
        } = 30;

        private int MaxRenderThreadSleepTime => (int)(1000.0 / MaxFramesPerSecond);

        private EventWaitHandle _renderWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
        private bool _running = true;
        private bool _isDisposed = false;

        private SKImageInfo _skImageInfo;
        private SKSurface _skSurface;
        private Bitmap _bitmap = null;

        private volatile object _sync = new object();

        public ImageRendererBase(Image target)
        {
            _target = target;

            /* TODO: Better to implement this task explicitly */
        }

        public void Start()
        {
            _running = true;
            Task.Run(() =>
            {
                DateTime lastUpdated = DateTime.Now;
                while (_running)
                {
                    if (AutoUpdate)
                    {
                        _renderWaitHandle.WaitOne(MaxRenderThreadSleepTime);
                    }
                    else
                    {
                        _renderWaitHandle.WaitOne();
                    }
                    _renderWaitHandle.Reset();

                    Render();

                    if (AutoUpdate)
                    {
                        var frameDuration = (int)DateTime.Now.Subtract(lastUpdated).Duration().TotalMilliseconds;
                        lastUpdated = DateTime.Now;

                        if (frameDuration < MaxRenderThreadSleepTime)
                        {
                            Thread.Sleep(MaxRenderThreadSleepTime - frameDuration);
                        }
                    }
                }
            });
        }

        public void Stop()
        {
            _running = false;
            _renderWaitHandle.Set();
        }

        public void Render()
        {
            lock (_sync)
            {
                try
                {
                    RenderInternal(_skSurface.Canvas);

                    using (var image = _skSurface.Snapshot())
                    {
                        using (var bitmap = SKBitmap.FromImage(image))
                        {
                            var previousBitmap = _bitmap;
                            _bitmap = new Bitmap(bitmap.ColorType.ToPixelFormat(),
                                                    bitmap.GetPixels(),
                                                    new PixelSize(bitmap.Width,
                                                    bitmap.Height),
                                                    SkiaPlatform.DefaultDpi,
                                                    bitmap.RowBytes);
                            Dispatcher.UIThread.Post(() =>
                            {
                                _target.Source = _bitmap;
                                _target.InvalidateVisual();

                                if (previousBitmap != null)
                                {
                                    previousBitmap.Dispose();
                                }
                            }, DispatcherPriority.Render);
                        }
                    }
                }

                catch (Exception ex)
                {
                    Trace.WriteLine(ex);
                }
            }
        }

        public void Update(ViewContext viewContext)
        {
            Context = viewContext;

            if (_skSurface != null)
            {
                _skSurface.Dispose();
            }

            _skImageInfo = new SKImageInfo((int)Context.Bounds.Width, (int)Context.Bounds.Height);
            _skSurface = SKSurface.Create(_skImageInfo);

            _renderWaitHandle.Set();
        }

        protected abstract void RenderInternal(SKCanvas canvas);

        protected double GetSeriesValue(SpectralData series, Bin bin)
        {
            var indices = Enumerable.Range(bin.From, bin.To - bin.From);
            var sequence = indices.Select(i => series.Statistics[i]);
            if (!sequence.Any())
            {
                return double.NaN;
            }

            return sequence.Select(series.GetDefaultValueSelector()).Max();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    _running = false;
                    if (_skSurface != null)
                    {
                        _skSurface.Dispose();
                    }
                }

                _isDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

    }
}
