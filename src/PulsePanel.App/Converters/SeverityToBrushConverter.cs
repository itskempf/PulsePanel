using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media;
using System;
using PulsePanel.App.Models;

namespace PulsePanel.App.Converters
{
    public class SeverityToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value switch
            {
                LogSeverity.Info => new SolidColorBrush(Microsoft.UI.Colors.Gray),
                LogSeverity.Warning => new SolidColorBrush(Microsoft.UI.Colors.Orange),
                LogSeverity.Error => new SolidColorBrush(Microsoft.UI.Colors.Red),
                _ => new SolidColorBrush(Microsoft.UI.Colors.White)
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }
}