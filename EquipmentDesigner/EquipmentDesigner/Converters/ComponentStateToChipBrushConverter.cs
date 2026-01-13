using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using EquipmentDesigner.Models;

namespace EquipmentDesigner.Converters
{
    /// <summary>
    /// Converts ComponentState enum values to appropriate chip background brushes.
    /// Draft: Neutral (gray), Ready: Info (blue), Uploaded: Warning (yellow), Validated: Success (green)
    /// </summary>
    public class ComponentStateToBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var state = ParseComponentState(value);

            return state switch
            {
                ComponentState.Draft => Application.Current.FindResource("Brush.Status.Neutral.Background"),
                ComponentState.Ready => Application.Current.FindResource("Brush.Status.Info.Background"),
                ComponentState.Uploaded => Application.Current.FindResource("Brush.Status.Warning.Background"),
                ComponentState.Validated => Application.Current.FindResource("Brush.Status.Success.Background"),
                _ => Application.Current.FindResource("Brush.Status.Neutral.Background")
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private static ComponentState? ParseComponentState(object value)
        {
            if (value == null)
                return null;

            // Direct enum match
            if (value is ComponentState state)
                return state;

            // String parsing (e.g., "Uploaded")
            if (value is string str && Enum.TryParse<ComponentState>(str, ignoreCase: true, out var parsed))
                return parsed;

            // Integer parsing (e.g., 2 for Uploaded)
            if (value is int intValue && Enum.IsDefined(typeof(ComponentState), intValue))
                return (ComponentState)intValue;

            return null;
        }
    }

    /// <summary>
    /// Converts ComponentState enum values to appropriate chip foreground brushes.
    /// Uses matching foreground colors for each state to ensure readability.
    /// </summary>
    public class ComponentStateToForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var state = ParseComponentState(value);

            return state switch
            {
                ComponentState.Draft => Application.Current.FindResource("Brush.Status.Neutral"),
                ComponentState.Ready => Application.Current.FindResource("Brush.Status.Info"),
                ComponentState.Uploaded => Application.Current.FindResource("Brush.Status.Warning"),
                ComponentState.Validated => Application.Current.FindResource("Brush.Status.Success"),
                _ => Application.Current.FindResource("Brush.Status.Neutral")
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private static ComponentState? ParseComponentState(object value)
        {
            if (value == null)
                return null;

            // Direct enum match
            if (value is ComponentState state)
                return state;

            // String parsing (e.g., "Uploaded")
            if (value is string str && Enum.TryParse<ComponentState>(str, ignoreCase: true, out var parsed))
                return parsed;

            // Integer parsing (e.g., 2 for Uploaded)
            if (value is int intValue && Enum.IsDefined(typeof(ComponentState), intValue))
                return (ComponentState)intValue;

            return null;
        }
    }
}