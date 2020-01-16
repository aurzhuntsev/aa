using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Input;
using System.Collections;

namespace AudioMark.Controls
{
    public class PropertySection : UserControl
    {
        public static readonly StyledProperty<object> PropertiesProperty =
            DirectProperty<PropertySection, object>.Register<PropertySection, object>("Properties");
        public object Properties
        {
            get => GetValue(PropertiesProperty);
            set { SetValue(PropertiesProperty, value); }
        }

        public static readonly StyledProperty<object> TitleControlsProperty =
            DirectProperty<PropertySection, object>.Register<PropertySection, object>("TitleControls");
        public object TitleControls
        {
            get => GetValue(TitleControlsProperty);
            set { SetValue(TitleControlsProperty, value); }
        }

        public static readonly StyledProperty<string> TitleProperty =
            DirectProperty<PropertySection, string>.Register<PropertySection, string>("Title");
        public string Title
        {
            get { return GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly StyledProperty<bool> ExpandedProperty =
            DirectProperty<PropertySection, bool>.Register<PropertySection, bool>("Expanded");
        public bool Expanded
        {
            get { return GetValue(ExpandedProperty); }
            set { SetValue(ExpandedProperty, value); }
        }        

        public void OnSectionTitlePointerPressed(object sender, PointerPressedEventArgs e)
        {            
            Expanded = !Expanded;
        }

        public PropertySection()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);            
        }                
    }
}
