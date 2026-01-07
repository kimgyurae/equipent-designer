using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using EquipmentDesigner.Models;

namespace EquipmentDesigner.Converters
{
    /// <summary>
    /// Converts HardwareLayer enum values to appropriate chip background brushes.
    /// </summary>
    public class HardwareLayerToBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not HardwareLayer layer)
            {
                return Application.Current.FindResource("Brush.Button.Primary.Background");
            }

            return layer switch
            {
                HardwareLayer.Equipment => Application.Current.FindResource("Brush.Button.Primary.Background"),
                HardwareLayer.System => Application.Current.FindResource("Brush.Button.Accent.Background"),
                HardwareLayer.Unit => Application.Current.FindResource("Brush.Button.Info.Background"),
                HardwareLayer.Device => Application.Current.FindResource("Brush.Button.Success.Background"),
                _ => Application.Current.FindResource("Brush.Button.Primary.Background")
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts HardwareLayer enum values to appropriate chip foreground brushes.
    /// </summary>
    public class HardwareLayerToForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // All hardware layer chips use inverse (white) text
            return Application.Current.FindResource("Brush.Text.Inverse");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
