using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using AudioMark.ViewModels;
using AudioMark.Views;
using System;
using AudioMark.Core.AudioData;

namespace AudioMark
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);            
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(),                    
                };

                desktop.Exit += OnExit;
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void OnExit(object sender, ControlledApplicationLifetimeExitEventArgs e)
        {
            var audioAdapter = AudioDataAdapterProvider.Get();
            if (audioAdapter.Running)
            {
                audioAdapter.Stop();
            }
        }
    }
}
