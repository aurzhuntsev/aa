using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AudioMark.Views.MeasurementSettings.Common
{
    public class CorrectionProfileView : UserControl
    {
        public CorrectionProfileView()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
