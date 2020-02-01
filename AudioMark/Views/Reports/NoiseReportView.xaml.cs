using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AudioMark.Views.Reports
{
    public class NoiseReportView : UserControl
    {
        public NoiseReportView()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
