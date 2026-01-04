using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace EquipmentDesigner.Converters
{
    /// <summary>
    /// Converts a string value to Visibility.
    /// Non-empty string = Visible, Empty/null = Collapsed.
    /// </summary>
    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool hasValue = !string.IsNullOrWhiteSpace(value as string);

            // Support inverse parameter
            bool invert = parameter?.ToString()?.Equals("Inverse", StringComparison.OrdinalIgnoreCase) == true;
            if (invert)
            {
                hasValue = !hasValue;
            }

            return hasValue ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
