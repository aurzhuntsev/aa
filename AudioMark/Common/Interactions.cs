using AudioMark.Views.Common;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;

namespace AudioMark.Common
{
    public static class Interactions
    {
        public class InputOptions
        {
            public string Text { get; set; }
            public string Value { get; set; }
        }

        public class InputResult
        {
            public string Value { get; set; }
            public bool Canceled { get; set; }
        }

        public static readonly Interaction<string, bool> Confirm = new Interaction<string, bool>();
        public static readonly Interaction<InputOptions, InputResult> Input = new Interaction<InputOptions, InputResult>();
        public static readonly Interaction<Exception, Unit> Error = new Interaction<Exception, Unit>();

        static Interactions()
        {
            var mainWindow = (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            if (mainWindow != null)
            {
                Confirm.RegisterHandler(async interaction =>
                {
                    var confirm = new Confirm() { Text = interaction.Input };
                    var result = await confirm.ShowDialog<bool>(mainWindow);
                    interaction.SetOutput(result);
                });

                Input.RegisterHandler(async interaction =>
                {
                    var input = new Input() { Text = interaction.Input.Text, Value = interaction.Input.Value };
                    var result = await input.ShowDialog<string>(mainWindow);
                    interaction.SetOutput(new InputResult() { Value = result, Canceled = result == null });
                });

                Error.RegisterHandler(async interaction =>
                {
                    var error = new Error(interaction.Input);
                    var result = await error.ShowDialog<Unit>(mainWindow);
                    interaction.SetOutput(Unit.Default);
                });
            }
        }
    }
}
