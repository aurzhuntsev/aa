using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

namespace AudioMark.Controls
{
    public class NumberBox : UserControl
    {
        public static readonly DirectProperty<NumberBox, double> ValueProperty =
            AvaloniaProperty.RegisterDirect<NumberBox, double>(nameof(Value), x => x.Value, (x, v) => x.Value = v, default(double), Avalonia.Data.BindingMode.TwoWay);
        private double _value;
        public double Value
        {
            get => _value;
            set
            {
                if (MinValue.HasValue && value < MinValue)
                {
                    return;
                }

                if (MaxValue.HasValue && value > MaxValue)
                {
                    return;
                }

                SetAndRaise(ValueProperty, ref _value, value);
            }
        }

        public static readonly StyledProperty<bool> IsInvertedProperty =
            AvaloniaProperty.Register<NumberBox, bool>(nameof(IsInverted));
        public bool IsInverted
        {
            get => GetValue(IsInvertedProperty);
            set => SetValue(IsInvertedProperty, value);
        }

        public static readonly StyledProperty<int?> MinValueProperty =
            AvaloniaProperty.Register<NumberBox, int?>(nameof(MinValue));
        public int? MinValue
        {
            get => GetValue(MinValueProperty);
            set => SetValue(MinValueProperty, value);
        }

        public static readonly StyledProperty<int?> MaxValueProperty =
            AvaloniaProperty.Register<NumberBox, int?>(nameof(MaxValue));
        public int? MaxValue
        {
            get => GetValue(MaxValueProperty);
            set => SetValue(MaxValueProperty, value);
        }

        public NumberBox()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public void Up()
        {
            if (!IsInverted)
            {
                Value++;
            }
            else
            {
                Value--;
            }
        }

        public void Down()
        {
            if (!IsInverted)
            {
                Value--;
            }
            else
            {
                Value++;
            }
        }

        public void OnPointerWheelChanged(object sender, PointerWheelEventArgs e)
        {
            if (e.Delta.Y > 0)
            {
                Up();
            }
            else if (e.Delta.Y < 0)
            {
                Down();
            }
        }
    }
}
