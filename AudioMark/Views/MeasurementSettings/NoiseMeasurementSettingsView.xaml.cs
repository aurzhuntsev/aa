using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AudioMark.Views.MeasurementSettings
{
    public class NoiseMeasurementSettingsView : UserControl
    {
        public NoiseMeasurementSettingsView()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
