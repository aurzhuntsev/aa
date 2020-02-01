using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AudioMark.Views.Settings
{
    public class DeviceSettingsView : UserControl
    {
        public DeviceSettingsView()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
