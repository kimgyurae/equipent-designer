using System;
using System.Globalization;
using System.Windows.Data;

namespace EquipmentDesigner.Converters
{
    /// <summary>
    /// Converts Models.TextAlignment to System.Windows.TextAlignment for XAML binding.
    /// </summary>
    public class TextAlignmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Models.TextAlignment alignment)
            {
                return alignment switch
                {
                    Models.TextAlignment.Left => System.Windows.TextAlignment.Left,
                    Models.TextAlignment.Center => System.Windows.TextAlignment.Center,
                    Models.TextAlignment.Right => System.Windows.TextAlignment.Right,
                    _ => System.Windows.TextAlignment.Center
                };
            }
            return System.Windows.TextAlignment.Center;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is System.Windows.TextAlignment alignment)
            {
                return alignment switch
                {
                    System.Windows.TextAlignment.Left => Models.TextAlignment.Left,
                    System.Windows.TextAlignment.Center => Models.TextAlignment.Center,
                    System.Windows.TextAlignment.Right => Models.TextAlignment.Right,
                    _ => Models.TextAlignment.Center
                };
            }
            return Models.TextAlignment.Center;
        }
    }
}
