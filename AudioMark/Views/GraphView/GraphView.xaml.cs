using AudioMark.Core.Common;
using AudioMark.Core.Settings;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Metadata;
using Avalonia.Platform;
using Avalonia.Skia;
using Avalonia.Threading;
using SkiaSharp;
using System;
using System.Diagnostics;
using System.Linq;
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
                _dataRenderer.DataSource = value;
            }
        }

        private ViewContext _viewContext = new ViewContext();
        private GridRenderer _gridRenderer = null;
        private SpectralDataRenderer _dataRenderer = null;

        private ImageRendererBase[] _renderers = null;

        public double MaxFrequency
        {
            get => Data == null ? (AppSettings.Current.Device.SampleRate / 2.0) : Data.MaxFrequency;
        }

        public GraphView()
        {
            this.InitializeComponent();

            _gridRenderer = new GridRenderer(this.FindControl<Image>("BackgroundImage"));
            _dataRenderer = new SpectralDataRenderer(this.FindControl<Image>("DataImage"));

            _renderers = new ImageRendererBase[] { _gridRenderer, _dataRenderer };
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void OnViewContextChanged()
        {
            if (ViewBounds != null && ViewBounds.Width > 0 && ViewBounds.Height > 0)
            {
                _viewContext.Update(ViewBounds, MaxFrequency);
                foreach (var renderer in _renderers)
                {
                    renderer.Update(_viewContext);
                }
            }
        }
    }
}
