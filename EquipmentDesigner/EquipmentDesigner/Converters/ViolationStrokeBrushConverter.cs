using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace EquipmentDesigner.Converters
{
    /// <summary>
    /// Converts HasViolations boolean to stroke/border brush.
    /// Returns Brush.Status.Danger for violations, default stroke color otherwise.
    /// </summary>
    public class ViolationStrokeBrushConverter : IValueConverter
    {
        /// <summary>
        /// The default stroke brush when no violations exist.
        /// </summary>
        public Brush DefaultBrush { get; set; }

        /// <summary>
        /// Converts HasViolations to appropriate brush.
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool hasViolations && hasViolations)
            {
                // Return danger color for violations
                return Application.Current.FindResource("Brush.Status.Danger") as Brush
                    ?? new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44));
            }

            // Return the configured default brush or parameter-based default
            return DefaultBrush ?? new SolidColorBrush(Color.FromRgb(0x33, 0x33, 0x33));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
