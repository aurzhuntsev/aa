using AudioMark.Core.Settings;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Skia;
using Avalonia.Threading;
using SkiaSharp;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AudioMark.Views
{
    /* TODO: Obviously would be great to find some better solution */
    public interface IGraphViewUpdateHandle
    {
        Bitmap Source { get; set; }
        EventWaitHandle Handle { get; set; }
    }

    public class GraphView : UserControl
    {
        public GraphView()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);
            if (DataContext is IGraphViewUpdateHandle)
            {
                var updateHandle = DataContext as IGraphViewUpdateHandle;

                Task.Run(() =>
                {
                    while (true)
                    {
                        try
                        {
                            updateHandle.Handle.WaitOne();
                            updateHandle.Handle.Reset();

                            Dispatcher.UIThread.Post(() =>
                            {
                                ((Image)LogicalChildren[0]).Source = updateHandle.Source;
                                ((Image)LogicalChildren[0]).InvalidateVisual();
                            }, DispatcherPriority.Background);
                        }
                        catch (Exception x)
                        {
                            int a = 10;

                        }
                    }
                });
            }

            //}

            //private void SetImageSize()
            //{
            //    DisposeSkObject();

            //    _skImageInfo = new SKImageInfo((int)ViewBounds.Width, (int)ViewBounds.Height);
            //    _skSurface = SKSurface.Create(_skImageInfo);
            //    _skCanvas = _skSurface.Canvas;

            //    Render();
            //}

            //private void DisposeSkObject()
            //{
            //    if (_skSurface != null)
            //    {
            //        _skSurface.Dispose();
            //    }
            //}

            //private void Render()
            //{
            //    _skCanvas.Clear(new SKColor(30, 30, 31));
            //    DrawGrid();

            //    if (Data != null)
            //    {
            //        var x0 = 0;
            //        var val = 0.0;
            //        bool firstPoint = true;
            //        int px = 0;
            //        int py = 0;
            //        for (var x = 0; x < Width; x++)
            //        {
            //            var x1 = (int)Math.Pow(10.0, Math.Log10(AppSettings.Current.Device.SampleRate) / Width * x);
            //            if (x1 - x0 > 0)
            //            {
            //                val = Data.Skip(x0).Take(x1 - x0).Max();
            //            }

            //            x0 = x1;
            //            var y = ToViewY(val == 0 ? Math.Pow(10.0, -MinValueExponent + 1) : val);
            //            if (firstPoint)
            //            {
            //                _skCanvas.DrawCircle(x, y, 1, new SKPaint() { Color = SKColors.Red });
            //                firstPoint = false;
            //            }
            //            else
            //            {
            //                _skCanvas.DrawLine(px, py, x, y, new SKPaint() { Color = SKColors.Red });
            //            }

            //            px = x;
            //            py = y;
            //        }
            //    }

            //    using (var image = _skSurface.Snapshot())
            //    {
            //        using (var bitmap = SKBitmap.FromImage(image))
            //        {
            //            var ctl = ((Image)LogicalChildren[0]);
            //            if (ctl.Source != null)
            //            {
            //                ctl.Source.Dispose();
            //            }

            //            ctl.Source = new Bitmap(bitmap.ColorType.ToPixelFormat(), bitmap.GetPixels(), new PixelSize(bitmap.Width, bitmap.Height), SkiaPlatform.DefaultDpi, bitmap.RowBytes);
            //            ctl.InvalidateVisual();
            //        }
            //    }
            //}

            //private void DrawGrid()
            //{
            //    var canvas = _skCanvas;
            //    var step = 1;
            //    var current = 0.0;

            //    var horizontalPrimaryGridPaint = new SKPaint()
            //    {
            //        Color = new SKColor(62, 62, 66),
            //        Style = SKPaintStyle.Stroke,
            //        StrokeWidth = 1,
            //        PathEffect = SKPathEffect.CreateDash(new float[] { 6, 2 }, 0)
            //    };

            //    var horizontalSecondaryGridPaint = new SKPaint()
            //    {
            //        Color = new SKColor(45, 45, 48),
            //        Style = SKPaintStyle.Stroke,
            //        StrokeWidth = 1,
            //        PathEffect = SKPathEffect.CreateDash(new float[] { 4, 2 }, 0)
            //    };


            //    while ((current = Math.Pow(10.0, step)) < AppSettings.Current.Device.SampleRate)
            //    {
            //        var x = ToViewX(current);
            //        canvas.DrawLine(x, 0, x, (float)ViewBounds.Height, horizontalPrimaryGridPaint);

            //        for (var cx = current + Math.Pow(10.0, step); cx < Math.Pow(10.0, step + 1) && cx < AppSettings.Current.Device.SampleRate; cx += Math.Pow(10.0, step))
            //        {
            //            var lx = ToViewX(cx);
            //            canvas.DrawLine(lx, 0, lx, (float)ViewBounds.Height, horizontalSecondaryGridPaint);
            //        }
            //        step++;
            //    }

            //    step = 1;
            //    current = 1.0;


            //    for (current = 1; current < MinValueExponent; current++)
            //    {
            //        var y = ToViewY(Math.Pow(10.0, -current));
            //        canvas.DrawLine(0, y, (float)ViewBounds.Width, y, horizontalPrimaryGridPaint);
            //    }
            //}

            //private int ToViewX(double value)
            //{
            //    /* TODO: Get rid of AppSettings here */
            //    return (int)((ViewBounds.Width / Math.Log10(AppSettings.Current.Device.SampleRate)) * Math.Log10(value));
            //}

            //private int ToViewY(double value)
            //{
            //    var k = (ViewBounds.Height / Math.Log10(Math.Pow(10.0, -MinValueExponent)));
            //    return (int)(-k * Math.Log10(1.0 / value));
            //}
        }
    }
}
