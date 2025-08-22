using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml;

namespace PulsePanel.App.Converters
{
    public class PortsToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is IDictionary<string, int> ports && ports.Count > 0)
            {
                var parts = new List<string>();
                foreach (var kvp in ports)
                {
                    parts.Add($"{kvp.Key}: {kvp.Value}");
                }
                return string.Join(", ", parts);
            }
            return "n/a";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) => DependencyProperty.UnsetValue;
    }
}
