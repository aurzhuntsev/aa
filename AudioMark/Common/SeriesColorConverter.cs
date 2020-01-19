using Avalonia.Data.Converters;
using Avalonia.Media;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace AudioMark.Common
{
    public class SeriesColorConverter : IValueConverter
    {
        public static readonly string[] Colors = new[]
        {
            "#ff611e",
            "#226dff",
            "#ffaf24",
            "#7830ff",
            "#71d81e",
            "#ff1686",
            "#1affda",
            "#8da1b5",
            "#ff090f",
            "#58c9ff",
        };

        private static SKPaint[] _skPaintCache;

        static SeriesColorConverter()
        {
            _skPaintCache = Colors.Select(c =>
            {
                var color = ParseHexColor(c);
                return new SKPaint()
                {
                    Color = new SKColor(color.r, color.g, color.b, color.a),
                    IsAntialias = true
                };
            }).ToArray();
        }

        private static (byte r, byte g, byte b, byte a) ParseHexColor(string color)
        {
            var result = (r: (byte)0, g: (byte)0, b: (byte)0, a: (byte)0);
            result.r = (byte)System.Convert.ToInt32(color.Substring(1, 2), 16);
            result.g = (byte)System.Convert.ToInt32(color.Substring(3, 2), 16);
            result.b = (byte)System.Convert.ToInt32(color.Substring(5, 2), 16);
            result.a = (byte)255;
            return result;
        }

        public static Brush GetBrush(int index)
        {
            var color = ParseHexColor(Colors[index % Colors.Length]);
            return new SolidColorBrush(new Color(color.a, color.r, color.g, color.b));
        }

        public static SKPaint GetSKPaint(int index)
        {
            return _skPaintCache[index % Colors.Length];
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return GetBrush((int)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
