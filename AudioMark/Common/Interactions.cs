using AudioMark.Views.Common;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public class FileOptions
        {
            public class Filter
            {
                public string Name { get; set; }
                public List<string> Extensions { get; set; }
            }
            
            public string Directory { get; set; }
            public List<Filter> Filters { get; set; }             
            public string Title { get; set; }
            public string InitialFileName { get; set; }
        }

        public class SaveFileOptions : FileOptions
        {
            public string DefaultExtension { get; set; }            
        }

        public class LoadFileOptions: FileOptions
        {
            public bool AllowMultiple { get; set; }
        }

        public static readonly Interaction<string, bool> Confirm = new Interaction<string, bool>();
        public static readonly Interaction<InputOptions, InputResult> Input = new Interaction<InputOptions, InputResult>();
        public static readonly Interaction<Exception, Unit> Error = new Interaction<Exception, Unit>();
        public static readonly Interaction<SaveFileOptions, string> SaveFile = new Interaction<SaveFileOptions, string>();
        public static readonly Interaction<LoadFileOptions, string[]> LoadFile = new Interaction<LoadFileOptions, string[]>();

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

                SaveFile.RegisterHandler(async interaction =>
                {
                    var opt = interaction.Input;
                    var saveFile = new SaveFileDialog()
                    {
                        DefaultExtension = opt.DefaultExtension,
                        Directory = opt.Directory,                        
                        InitialFileName = opt.InitialFileName,
                        Title = opt.Title
                    };

                    saveFile.Filters = opt.Filters.Select(f => new FileDialogFilter()
                    {
                        Extensions = f.Extensions,
                        Name = f.Name
                    }).ToList();

                    var result = await saveFile.ShowAsync(mainWindow);
                    interaction.SetOutput(result);                    
                });

                LoadFile.RegisterHandler(async interaction =>
                {
                    var opt = interaction.Input;
                    var loadFile = new OpenFileDialog()
                    {
                        Directory = opt.Directory,
                        InitialFileName = opt.InitialFileName,
                        Title = opt.Title,
                        AllowMultiple = opt.AllowMultiple
                    };

                    loadFile.Filters = opt.Filters.Select(f => new FileDialogFilter()
                    {
                        Extensions = f.Extensions,
                        Name = f.Name
                    }).ToList();

                    var result = await loadFile.ShowAsync(mainWindow);
                    interaction.SetOutput(result);
                });
            }
        }
    }
}
