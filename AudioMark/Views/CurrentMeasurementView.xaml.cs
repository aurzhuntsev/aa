using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AudioMark.Views
{
    public class CurrentMeasurementView : UserControl
    {
        public CurrentMeasurementView()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);            
        }
    }
}
