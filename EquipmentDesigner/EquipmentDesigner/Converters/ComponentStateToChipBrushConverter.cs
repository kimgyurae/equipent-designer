using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using EquipmentDesigner.Models;

namespace EquipmentDesigner.Converters
{
    /// <summary>
    /// Converts ComponentState enum values to appropriate chip background brushes.
    /// </summary>
    public class ComponentStateToBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not ComponentState state)
            {
                return Application.Current.FindResource("Brush.Button.Primary.Background");
            }

            return state switch
            {
                ComponentState.Draft => Application.Current.FindResource("Brush.HardwareType.Equipment.Chip.Background"),
                ComponentState.Ready => Application.Current.FindResource("Brush.HardwareType.System.Chip.Background"),
                ComponentState.Uploaded => Application.Current.FindResource("Brush.HardwareType.Unit.Chip.Background"),
                ComponentState.Validated => Application.Current.FindResource("Brush.HardwareType.Device.Chip.Background"),
                _ => Application.Current.FindResource("Brush.Status.Neutral.Background")
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts ComponentState enum values to appropriate chip foreground brushes.
    /// </summary>
    public class ComponentStateToForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not ComponentState layer)
            {
                return Application.Current.FindResource("Brush.HardwareType.Equipment.Chip.Foreground");
            }

            return layer switch
            {
                ComponentState.Draft => Application.Current.FindResource("Brush.HardwareType.Equipment.Chip.Foreground"),
                ComponentState.Ready => Application.Current.FindResource("Brush.HardwareType.System.Chip.Foreground"),
                ComponentState.Uploaded => Application.Current.FindResource("Brush.HardwareType.Unit.Chip.Foreground"),
                ComponentState.Validated => Application.Current.FindResource("Brush.HardwareType.Device.Chip.Foreground"),
                _ => Application.Current.FindResource("Brush.HardwareType.Equipment.Chip.Foreground")
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}