using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using EquipmentDesigner.Models;

namespace EquipmentDesigner.Converters
{
    /// <summary>
    /// Converts SupportedTextColor enum to corresponding SolidColorBrush.
    /// </summary>
    public class SupportedTextColorToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not SupportedTextColor textColor)
            {
                return Application.Current.FindResource("Brush.DrawingElement.Text.Black");
            }

            return textColor switch
            {
                SupportedTextColor.Black => Application.Current.FindResource("Brush.DrawingElement.Text.Black"),
                SupportedTextColor.Green => Application.Current.FindResource("Brush.DrawingElement.Text.Green"),
                SupportedTextColor.Orange => Application.Current.FindResource("Brush.DrawingElement.Text.Orange"),
                SupportedTextColor.Red => Application.Current.FindResource("Brush.DrawingElement.Text.Red"),
                SupportedTextColor.Purple => Application.Current.FindResource("Brush.DrawingElement.Text.Purple"),
                SupportedTextColor.Blue => Application.Current.FindResource("Brush.DrawingElement.Text.Blue"),
                _ => Application.Current.FindResource("Brush.DrawingElement.Text.Black")
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
