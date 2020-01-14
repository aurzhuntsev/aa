using AudioMark.Core.Common;
using AudioMark.Core.Settings;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Metadata;
using Avalonia.Platform;
using Avalonia.Skia;
using Avalonia.Threading;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AudioMark.Views.GraphView
{
    public class GraphView : UserControl
    {
        private Rect _viewBounds;
        public Rect ViewBounds
        {
            get => _viewBounds;
            set
            {
                _viewBounds = value;
                OnViewContextChanged();
            }
        }

        public static readonly DirectProperty<GraphView, bool> DynamicRenderProperty =
            AvaloniaProperty.RegisterDirect<GraphView, bool>(nameof(DynamicRender), x => x.DynamicRender, (x, v) => x.DynamicRender = v);
        private bool _dynamicRender;
        public bool DynamicRender
        {
            get => _dynamicRender;
            set 
            {
                SetAndRaise(DynamicRenderProperty, ref _dynamicRender, value);
                _dataRenderer.AutoUpdate = value;
            }
        }

        public static readonly DirectProperty<GraphView, SpectralData> DataProperty =
            AvaloniaProperty.RegisterDirect<GraphView, SpectralData>(nameof(Data), x => x.Data, (x, v) => x.Data = v);
        private SpectralData _data;
        public SpectralData Data
        {
            get => _data;
            set
            {
                SetAndRaise(DataProperty, ref _data, value);
                Bins = BuildBins();

                foreach (var renderer in _renderers)
                {
                    renderer.Bins = Bins;
                }
            }
        }

        public IEnumerable<Bin> Bins { get; private set; }

        private ViewContext _viewContext = new ViewContext();
        
        private GridRenderer _gridRenderer = null;
        private SpectralDataRenderer _dataRenderer = null;
        private CursorRenderer _cursorRenderer = null;

        private ImageRendererBase[] _renderers = null;

        private Point? _pointer;

        public int MaxFrequency
        {
            get => Data == null ? (int)(AppSettings.Current.Device.SampleRate / 2.0) : Data.MaxFrequency;
        }

        public int SpectrumBins
        {
            get => Data == null ? (AppSettings.Current.Fft.WindowSize) : Data.Size;
        }


        public GraphView()
        {
            this.InitializeComponent();

            _gridRenderer = new GridRenderer(this.FindControl<Image>("BackgroundImage"));
            _dataRenderer = new SpectralDataRenderer(this.FindControl<Image>("DataImage"));
            _cursorRenderer = new CursorRenderer(this.FindControl<Image>("CursorsImage"));

            _renderers = new ImageRendererBase[] { _gridRenderer, _dataRenderer, _cursorRenderer };
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void OnViewContextChanged()
        {
            if (ViewBounds != null && ViewBounds.Width > 0 && ViewBounds.Height > 0)
            {
                _viewContext.Update(ViewBounds, MaxFrequency, SpectrumBins);
                foreach (var renderer in _renderers)
                {
                    renderer.Update(_viewContext);
                }
            }
        }

        private List<Bin> BuildBins()
        {
            var result = new List<Bin>();
            if (Data != null)
            {
                int currentOffset = 0;
                int binWidth = 0;
                int startingBin = 2;
                int bins = 0;
         
                while (startingBin + bins < Data.Size)
                {
                    while (currentOffset == (binWidth = _viewContext.FreqToViewX((startingBin + bins) * Data.FrequencyPerBin)))
                    {
                        bins++;
                    }

                    var meanValue = double.MinValue;
                    var meanValueBin = 0;
                    for (var i = startingBin; i < startingBin + bins; i++)
                    {
                        var value = Data.DefaultValueSelector(Data.Statistics[i]);
                        if (meanValue < value)
                        {
                            meanValue = value;
                            meanValueBin = i;
                        }
                    }

                    var bin = new Bin()
                    {
                        Left = currentOffset,
                        Right = binWidth,
                        From = startingBin,
                        To = startingBin + bins,
                        Value = meanValue,
                        SpectrumBin = meanValueBin
                    };

                    result.Add(bin);

                    startingBin = startingBin + bins;
                    bins = 0;

                    currentOffset = binWidth;
                }
            }

            return result;
        }

        public static readonly AvaloniaProperty<int> LeftLeftProperty = AvaloniaProperty.Register<GraphView, int>(nameof(LeftLeft));
        public int LeftLeft
        {
            get => GetValue(LeftLeftProperty);
            set => SetValue(LeftLeftProperty, value);
        }

        public void OnPointerMoved(object sender, PointerEventArgs e)
        {
            _pointer = e.GetPosition(this);
            _cursorRenderer.Pointer = _pointer;
            _cursorRenderer.Render();
        }

        public void OnPointerEnter(object sender, PointerEventArgs e)
        {
            _pointer = e.GetPosition(this);
        }

        public void OnPointerLeave(object sender, PointerEventArgs e)
        {            
            _pointer = null;
            _cursorRenderer.Pointer = _pointer;
            _cursorRenderer.Render();
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);

            foreach (var renderer in _renderers)
            {
                renderer.Start();
            }
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            foreach (var renderer in _renderers)
            {
                renderer.Stop();
            }

            base.OnDetachedFromVisualTree(e);
        }
    }
}
