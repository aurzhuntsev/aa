using AudioMark.Core.Settings;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;

namespace AudioMark.Views.Common
{

    public class LevelMeter : UserControl
    {
        private const string ClippingClassName = "Clipping";

        private Control _container;
        private Control _tp;
        private Control _fs;

        private Rect _containerBounds;
        public Rect ContainerBounds
        {
            get => _containerBounds;
            set
            {
                _containerBounds = value;
                UpdateControl();
            }
        }
                
        

        public static readonly DirectProperty<LevelMeter, double> LevelDbTpProperty =
            AvaloniaProperty.RegisterDirect<LevelMeter, double>(nameof(LevelDbTp), x => x.LevelDbTp, (x, v) => x.LevelDbTp = v);
        private double _levelDbTp;
        public double LevelDbTp
        {
            get => _levelDbTp;
            set
            {
                SetAndRaise(LevelDbTpProperty, ref _levelDbTp, value);
                UpdateControl();
            }
        }

        public static readonly DirectProperty<LevelMeter, double> LevelDbFsProperty =
          AvaloniaProperty.RegisterDirect<LevelMeter, double>(nameof(LevelDbFs), x => x.LevelDbFs, (x, v) => x.LevelDbFs = v);
        private double _levelDbFs;
        public double LevelDbFs
        {
            get => _levelDbFs;
            set
            {
                SetAndRaise(LevelDbFsProperty, ref _levelDbFs, value);
                UpdateControl();
            }
        }
                
        public string LevelDbTpFormat
        {
            get => $"{_levelDbTp.ToString("F1")}dBTP";
        }

        public string LevelDbFsFormat
        {
            get => $"{_levelDbFs.ToString("F1")}dBFS";
        }

        public LevelMeter()
        {
            this.InitializeComponent();

            _container = this.Find<Control>("container");
            _tp = this.Find<Control>("tp");
            _fs = this.Find<Control>("fs");
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void UpdateControl()
        {
            var k = -ContainerBounds.Width / 90.0;
            _tp.Width = k * LevelDbTp + ContainerBounds.Width;
            if (LevelDbTp < AppSettings.Current.Device.ClippingLevel)
            {
                _tp.Classes.Add(ClippingClassName);
            }
            else
            {
                _tp.Classes.Remove(ClippingClassName);
            }

            _fs.Width = k * LevelDbFs + ContainerBounds.Width;            
        }
    }
}
