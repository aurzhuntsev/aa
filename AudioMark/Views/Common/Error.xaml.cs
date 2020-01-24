using AudioMark.Common;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using System;

namespace AudioMark.Views.Common
{
    public class Error : Window
    {
        public Exception Exception { get; }

        public static readonly StyledProperty<string> TextProperty = AvaloniaProperty.Register<Error, string>(nameof(Text));
        public string Text
        {
            get => GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public Error()
        {

            this.InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif            
        }

        public Error(Exception e)
        {
            Exception = e;
            Text = e.Message + Environment.NewLine + e.StackTrace;

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

            if (e.Key == Key.Escape || e.Key == Key.Enter)
            {
                Ok();
            }            
        }

        public void Ok() => Close();        
    }
}
