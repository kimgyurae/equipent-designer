using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using EquipmentDesigner.Models;

namespace EquipmentDesigner.Converters
{
    /// <summary>
    /// Converts HardwareType enum values to appropriate chip background brushes.
    /// </summary>
    public class HardwareTypeToBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not HardwareType layer)
            {
                return Application.Current.FindResource("Brush.HardwareType.Equipment.Chip.Background");
            }

            return layer switch
            {
                HardwareType.Equipment => Application.Current.FindResource("Brush.HardwareType.Equipment.Chip.Background"),
                HardwareType.System => Application.Current.FindResource("Brush.HardwareType.System.Chip.Background"),
                HardwareType.Unit => Application.Current.FindResource("Brush.HardwareType.Unit.Chip.Background"),
                HardwareType.Device => Application.Current.FindResource("Brush.HardwareType.Device.Chip.Background"),
                _ => Application.Current.FindResource("Brush.HardwareType.Equipment.Chip.Background")
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts HardwareType enum values to appropriate chip foreground brushes.
    /// </summary>
    public class HardwareTypeToForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not HardwareType layer)
            {
                return Application.Current.FindResource("Brush.HardwareType.Equipment.Chip.Foreground");
            }

            return layer switch
            {
                HardwareType.Equipment => Application.Current.FindResource("Brush.HardwareType.Equipment.Chip.Foreground"),
                HardwareType.System => Application.Current.FindResource("Brush.HardwareType.System.Chip.Foreground"),
                HardwareType.Unit => Application.Current.FindResource("Brush.HardwareType.Unit.Chip.Foreground"),
                HardwareType.Device => Application.Current.FindResource("Brush.HardwareType.Device.Chip.Foreground"),
                _ => Application.Current.FindResource("Brush.HardwareType.Equipment.Chip.Foreground")
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts HardwareType enum values to appropriate card background brushes.
    /// Equipment uses a gradient, others use solid colors.
    /// </summary>
    public class HardwareTypeToCardBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not HardwareType layer)
            {
                return Application.Current.FindResource("Brush.HardwareType.Equipment.Card.Background");
            }

            return layer switch
            {
                HardwareType.Equipment => Application.Current.FindResource("Brush.HardwareType.Equipment.Card.Background"),
                HardwareType.System => Application.Current.FindResource("Brush.HardwareType.System.Card.Background"),
                HardwareType.Unit => Application.Current.FindResource("Brush.HardwareType.Unit.Card.Background"),
                HardwareType.Device => Application.Current.FindResource("Brush.HardwareType.Device.Card.Background"),
                _ => Application.Current.FindResource("Brush.HardwareType.Equipment.Card.Background")
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts HardwareType enum values to appropriate card border brushes.
    /// </summary>
    public class HardwareTypeToCardBorderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not HardwareType layer)
            {
                return Application.Current.FindResource("Brush.HardwareType.Equipment.Card.Border");
            }

            return layer switch
            {
                HardwareType.Equipment => Application.Current.FindResource("Brush.HardwareType.Equipment.Card.Border"),
                HardwareType.System => Application.Current.FindResource("Brush.HardwareType.System.Card.Border"),
                HardwareType.Unit => Application.Current.FindResource("Brush.HardwareType.Unit.Card.Border"),
                HardwareType.Device => Application.Current.FindResource("Brush.HardwareType.Device.Card.Border"),
                _ => Application.Current.FindResource("Brush.HardwareType.Equipment.Card.Border")
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}