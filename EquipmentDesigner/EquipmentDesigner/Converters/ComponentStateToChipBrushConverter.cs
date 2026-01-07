using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using EquipmentDesigner.Models.Dtos;

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
                ComponentState.Draft => Application.Current.FindResource("Brush.Status.Warning.Background"),
                ComponentState.Ready => Application.Current.FindResource("Brush.Button.Info.Background"),
                ComponentState.Uploaded => Application.Current.FindResource("Brush.Button.Success.Background"),
                ComponentState.Validated => Application.Current.FindResource("Brush.Button.Primary.Background"),
                _ => Application.Current.FindResource("Brush.Button.Primary.Background")
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
            if (value is not ComponentState state)
            {
                return Application.Current.FindResource("Brush.Text.Inverse");
            }

            // Undefined uses warning background which may need dark text
            if (state == ComponentState.Draft)
            {
                return Application.Current.FindResource("Brush.Status.Warning");
            }

            // All other states use white/inverse text
            return Application.Current.FindResource("Brush.Text.Inverse");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}