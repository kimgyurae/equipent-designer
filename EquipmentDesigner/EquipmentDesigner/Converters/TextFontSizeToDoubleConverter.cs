using System;
using System.Globalization;
using System.Windows.Data;

namespace EquipmentDesigner.Converters
{
    /// <summary>
    /// Converts TextFontSize enum to double for XAML binding.
    /// </summary>
    public class TextFontSizeToDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Models.TextFontSize fontSize)
                return (double)(int)fontSize;
            return 14.0; // Default: Base
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double d)
            {
                int intVal = (int)d;
                if (Enum.IsDefined(typeof(Models.TextFontSize), intVal))
                    return (Models.TextFontSize)intVal;
            }
            return Models.TextFontSize.Base;
        }
    }
}
