using AudioMark.Core.Settings;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
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
    public class GraphPointerEventArgs : EventArgs
    {
        public int Left { get; set; }
        public int Top { get; set; }
        public bool Present { get; set; }
    }

    public interface IGraphViewUpdateHandle
    {
        Bitmap Source { get; set; }
        EventWaitHandle Handle { get; set; }
        EventHandler<GraphPointerEventArgs> OnPointerChanged { get; set; }
    }

    public class GraphView : UserControl
    {
        private const int MaxFrameDurationMilliseconds = 0; // ~30fps

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
                    DateTime lastUpdated = DateTime.Now;
                    while (true)
                    {
                        updateHandle.Handle.WaitOne();
                        updateHandle.Handle.Reset();

                        Dispatcher.UIThread.Post(() =>
                        {
                            ((Image)LogicalChildren[0]).Source = updateHandle.Source;
                            ((Image)LogicalChildren[0]).InvalidateVisual();
                        }, DispatcherPriority.Background);

                        var frameDuration = (int)DateTime.Now.Subtract(lastUpdated).Duration().TotalMilliseconds;
                        lastUpdated = DateTime.Now;
                        if (frameDuration < MaxFrameDurationMilliseconds)
                        {
                            Thread.Sleep(MaxFrameDurationMilliseconds - frameDuration);
                        }
                    }
                });
            }
        }

        public void OnPointerLeave(object sender, PointerEventArgs e)
        {
            if (DataContext is IGraphViewUpdateHandle)
            {
                var updateHandle = DataContext as IGraphViewUpdateHandle;
                var eventArgs = new GraphPointerEventArgs() { Present = false };

                updateHandle.OnPointerChanged.Invoke(this, eventArgs);
            }
        }

        public void OnPointerUpdate(object sender, PointerEventArgs e)
        {
            if (DataContext is IGraphViewUpdateHandle)
            {
                if (DataContext is IGraphViewUpdateHandle)
                {
                    var updateHandle = DataContext as IGraphViewUpdateHandle;
                    var position = e.GetPosition(this);
                    var eventArgs = new GraphPointerEventArgs() { Present = true, Left = (int)position.X, Top = (int)position.Y };

                    updateHandle.OnPointerChanged.Invoke(this, eventArgs);
                }
            }
        }
    }
}
