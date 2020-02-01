using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AudioMark.Views.Settings
{
    public class FftSettingsView : UserControl
    {
        public FftSettingsView()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
