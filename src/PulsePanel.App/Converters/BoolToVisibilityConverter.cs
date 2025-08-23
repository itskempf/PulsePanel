using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace PulsePanel.Converters
{
    /// <summary>
    /// Converts a boolean value to a <see cref="Visibility"/> value for WinUI.
    /// True → Visible, False → Collapsed by default.
    /// Supports inversion and Hidden mode.
    /// </summary>
    public class BoolToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// If true, the conversion logic is inverted:
        /// True → Collapsed, False → Visible.
        /// </summary>
        public bool Invert { get; set; }

        /// <summary>
        /// If true, uses <see cref="Visibility.Hidden"/> instead of <see cref="Visibility.Collapsed"/>.
        /// </summary>
        public bool UseHidden { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is not bool boolValue)
                return DependencyProperty.UnsetValue;

            if (Invert)
                boolValue = !boolValue;

            if (boolValue)
                return Visibility.Visible;

            return UseHidden ? Visibility.Hidden : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is not Visibility visibility)
                return DependencyProperty.UnsetValue;

            bool result = visibility == Visibility.Visible;

            return Invert ? !result : result;
        }
    }
}