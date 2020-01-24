using AudioMark.Common;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using System;

namespace AudioMark.Views.Common
{
    public class Input : Window
    {
        public static readonly StyledProperty<string> TextProperty = AvaloniaProperty.Register<Input, string>(nameof(Text));
        public string Text
        {
            get => GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public static readonly StyledProperty<string> ValueProperty = AvaloniaProperty.Register<Input, string>(nameof(Value));
        public string Value
        {
            get => GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public Input()
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

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);

            var textBox = this.FindControl<TextBox>("input");
            if (textBox != null)
            {
                textBox.SelectionStart = 0;
                textBox.SelectionEnd = Value.Length;
                textBox.Focus();
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);

            if (e.Key == Key.Escape)
            {
                Cancel();
            }
            else if (e.Key == Key.Enter)
            {
                Ok();
            }
        }

        public void Ok() => Close(Value);
        public void Cancel() => Close(null);
    }
}
