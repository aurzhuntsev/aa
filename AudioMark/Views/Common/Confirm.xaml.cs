using AudioMark.Common;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

namespace AudioMark.Views.Common
{
    public class Confirm : Window
    {
        public static readonly StyledProperty<string> TextProperty = AvaloniaProperty.Register<Confirm, string>(nameof(Text));
        public string Text
        {
            get => GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public Confirm()
        {
            this.InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif            
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);

            if (e.Key == Key.Escape)
            {
                No();
            }
            else if (e.Key == Key.Enter)
            {
                Yes();
            }
        }

        public void Yes() => Close(true);
        public void No() => Close(false);
    }
}
