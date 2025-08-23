using System;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace PulsePanel.Converters
{
    /// <summary>
    /// Converts multiple boolean values to a Visibility value for WinUI.
    /// Supports AND/OR logic, inversion, and Hidden mode.
    /// </summary>
    public class BoolToVisibilityMultiConverter : IMultiValueConverter
    {
        /// <summary>
        /// If true, all values must be true (AND). If false, any true value will show (OR).
        /// </summary>
        public bool UseAnd { get; set; } = true;

        /// <summary>
        /// If true, inverts the final boolean result before converting to Visibility.
        /// </summary>
        public bool Invert { get; set; }

        /// <summary>
        /// If true, uses Hidden instead of Collapsed when not visible.
        /// </summary>
        public bool UseHidden { get; set; }

        public object Convert(object[] values, Type targetType, object parameter, string language)
        {
            if (values == null || values.Length == 0)
                return DependencyProperty.UnsetValue;

            var boolValues = values.OfType<bool>().ToList();
            if (boolValues.Count != values.Length)
                return DependencyProperty.UnsetValue;

            bool result = UseAnd ? boolValues.All(v => v) : boolValues.Any(v => v);

            if (Invert)
                result = !result;

            if (result)
                return Visibility.Visible;

            return UseHidden ? Visibility.Hidden : Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, string language)
        {
            throw new NotImplementedException("ConvertBack is not supported for BoolToVisibilityMultiConverter.");
        }
    }
}