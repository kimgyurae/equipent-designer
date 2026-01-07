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
                return Application.Current.FindResource("Brush.HardwareLayer.Equipment.Chip.Background");
            }

            return layer switch
            {
                HardwareLayer.Equipment => Application.Current.FindResource("Brush.HardwareLayer.Equipment.Chip.Background"),
                HardwareLayer.System => Application.Current.FindResource("Brush.HardwareLayer.System.Chip.Background"),
                HardwareLayer.Unit => Application.Current.FindResource("Brush.HardwareLayer.Unit.Chip.Background"),
                HardwareLayer.Device => Application.Current.FindResource("Brush.HardwareLayer.Device.Chip.Background"),
                _ => Application.Current.FindResource("Brush.HardwareLayer.Equipment.Chip.Background")
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
            if (value is not HardwareLayer layer)
            {
                return Application.Current.FindResource("Brush.HardwareLayer.Equipment.Chip.Foreground");
            }

            return layer switch
            {
                HardwareLayer.Equipment => Application.Current.FindResource("Brush.HardwareLayer.Equipment.Chip.Foreground"),
                HardwareLayer.System => Application.Current.FindResource("Brush.HardwareLayer.System.Chip.Foreground"),
                HardwareLayer.Unit => Application.Current.FindResource("Brush.HardwareLayer.Unit.Chip.Foreground"),
                HardwareLayer.Device => Application.Current.FindResource("Brush.HardwareLayer.Device.Chip.Foreground"),
                _ => Application.Current.FindResource("Brush.HardwareLayer.Equipment.Chip.Foreground")
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts HardwareLayer enum values to appropriate card background brushes.
    /// Equipment uses a gradient, others use solid colors.
    /// </summary>
    public class HardwareLayerToCardBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not HardwareLayer layer)
            {
                return Application.Current.FindResource("Brush.HardwareLayer.Equipment.Card.Background");
            }

            return layer switch
            {
                HardwareLayer.Equipment => Application.Current.FindResource("Brush.HardwareLayer.Equipment.Card.Background"),
                HardwareLayer.System => Application.Current.FindResource("Brush.HardwareLayer.System.Card.Background"),
                HardwareLayer.Unit => Application.Current.FindResource("Brush.HardwareLayer.Unit.Card.Background"),
                HardwareLayer.Device => Application.Current.FindResource("Brush.HardwareLayer.Device.Card.Background"),
                _ => Application.Current.FindResource("Brush.HardwareLayer.Equipment.Card.Background")
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts HardwareLayer enum values to appropriate card border brushes.
    /// </summary>
    public class HardwareLayerToCardBorderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not HardwareLayer layer)
            {
                return Application.Current.FindResource("Brush.HardwareLayer.Equipment.Card.Border");
            }

            return layer switch
            {
                HardwareLayer.Equipment => Application.Current.FindResource("Brush.HardwareLayer.Equipment.Card.Border"),
                HardwareLayer.System => Application.Current.FindResource("Brush.HardwareLayer.System.Card.Border"),
                HardwareLayer.Unit => Application.Current.FindResource("Brush.HardwareLayer.Unit.Card.Border"),
                HardwareLayer.Device => Application.Current.FindResource("Brush.HardwareLayer.Device.Card.Border"),
                _ => Application.Current.FindResource("Brush.HardwareLayer.Equipment.Card.Border")
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}