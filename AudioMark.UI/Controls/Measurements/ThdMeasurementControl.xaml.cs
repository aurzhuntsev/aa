using AudioMark.Core.Measurements;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

namespace AudioMark.UI.Controls.Measurements
{
    public class ThdMeasurementControl : UserControl
    {
        public ThdMeasurementControl()
        {
            this.InitializeComponent();
            DataContext = new ThdMeasurement();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public void OnNumericTextInput(object sender, KeyEventArgs e)
        {
            
        }
    }
}
