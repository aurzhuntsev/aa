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
                _seriesRenderer.AutoUpdate = value;
            }
        }


        public static readonly DirectProperty<GraphView, List<Series>> SeriesProperty =
            AvaloniaProperty.RegisterDirect<GraphView, List<Series>>(nameof(Series), x => x.Series, (x, v) => x.Series = v);
        private List<Series> _series;
        public List<Series> Series
        {
            get => _series;
            set
            {
                SetAndRaise(SeriesProperty, ref _series, value);
                UpdateRenderers();
            }
        }

        public static readonly DirectProperty<GraphView, Series> ActiveSeriesProperty =
           AvaloniaProperty.RegisterDirect<GraphView, Series>(nameof(ActiveSeries), x => x.ActiveSeries, (x, v) => x.ActiveSeries = v);
        private Series _activeSeries;
        public Series ActiveSeries
        {
            get => _activeSeries;
            set
            {
                SetAndRaise(ActiveSeriesProperty, ref _activeSeries, value);
                UpdateRenderers();                
            }
        }

        private IEnumerable<Series> AllSeries
        {
            get
            {
                var series = Series ?? Enumerable.Empty<Series>();
                if (ActiveSeries != null)
                {
                    return series.Union(new[] { ActiveSeries });
                }
                return series;
            }
        }

        public IEnumerable<Bin> Bins { get; private set; }

        private ViewContext _viewContext = new ViewContext();

        private GridRenderer _gridRenderer = null;
        private SpectralDataRenderer _seriesRenderer = null;
        private CursorRenderer _cursorRenderer = null;

        private ImageRendererBase[] _renderers = null;

        private Point? _pointer;

        public GraphView()
        {
            this.InitializeComponent();

            _gridRenderer = new GridRenderer(this.FindControl<Image>("BackgroundImage"));
            _seriesRenderer = new SpectralDataRenderer(this.FindControl<Image>("DataImage"));
            _cursorRenderer = new CursorRenderer(this.FindControl<Image>("CursorsImage"));

            _renderers = new ImageRendererBase[] { _gridRenderer, _seriesRenderer, _cursorRenderer };
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void OnViewContextChanged()
        {
            if (ViewBounds != null && ViewBounds.Width > 0 && ViewBounds.Height > 0)
            {
                var maxFrequency = !AllSeries.Any() ? (int)(AppSettings.Current.Device.SampleRate / 2.0) : AllSeries.ToList().Max(d => d.Data.MaxFrequency);
                var spectrumBins = !AllSeries.Any() ? (AppSettings.Current.Fft.WindowSize) : AllSeries.ToList().Max(d => d.Data.Size);

                _viewContext.Update(ViewBounds, maxFrequency, spectrumBins);
                BuildBins();

                foreach (var renderer in _renderers)
                {
                    renderer.Update(_viewContext);
                }
            }
        }

        private void BuildBins()
        {
            var result = new List<Bin>();
            if (AllSeries.ToList().Any())
            {
                int currentOffset = 0;
                int binWidth = 0;
                int startingBin = 2;
                int bins = 0;
                var k = _viewContext.MaxFrequency / _viewContext.SpectrumBins;

                while (startingBin + bins < _viewContext.SpectrumBins)
                {
                    while (currentOffset == (binWidth = _viewContext.FreqToViewX((startingBin + bins) * k)))
                    {
                        bins++;
                    }

                    var labels = new List<string>();
                    var bin = new Bin()
                    {
                        Left = currentOffset,
                        Right = binWidth,
                        From = startingBin,
                        To = startingBin + bins
                    };

                    result.Add(bin);

                    startingBin = startingBin + bins;
                    bins = 0;

                    currentOffset = binWidth;
                }

                foreach (var renderer in _renderers)
                {
                    renderer.Bins = result;
                }
            }
        }

        private void UpdateRenderers()
        {
            foreach (var renderer in _renderers)
            {
                renderer.Series = AllSeries.ToList();
            }

            BuildBins();
            _seriesRenderer.Render();
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
