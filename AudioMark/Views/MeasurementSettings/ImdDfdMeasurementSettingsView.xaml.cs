using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AudioMark.Views.MeasurementSettings
{
    public class ImdDfdMeasurementSettingsView : UserControl
    {
        public ImdDfdMeasurementSettingsView()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
