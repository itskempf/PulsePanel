using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;

namespace PulsePanel.App.Converters
{
    public class StatusToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var s = value?.ToString();
            if (string.IsNullOrWhiteSpace(s)) return new SolidColorBrush(Microsoft.UI.Colors.Gray);
            s = s.ToLowerInvariant();
            return s switch
            {
                "running" => new SolidColorBrush(Microsoft.UI.Colors.Green),
                "stopped" => new SolidColorBrush(Microsoft.UI.Colors.Red),
                "starting" => new SolidColorBrush(Microsoft.UI.Colors.Goldenrod),
                "stopping" => new SolidColorBrush(Microsoft.UI.Colors.Orange),
                _ => new SolidColorBrush(Microsoft.UI.Colors.Gray)
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }
}
