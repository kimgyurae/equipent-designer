using System.Windows;
using System.Windows.Controls;

namespace EquipmentDesigner.Controls
{
    /// <summary>
    /// Attached properties for extending TextBox functionality.
    /// Provides placeholder text support without requiring custom control templates in each view.
    /// </summary>
    public static class TextBoxHelper
    {
        #region Placeholder Property

        /// <summary>
        /// Attached property for placeholder text displayed when TextBox is empty.
        /// </summary>
        public static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.RegisterAttached(
                "Placeholder",
                typeof(string),
                typeof(TextBoxHelper),
                new PropertyMetadata(string.Empty));

        /// <summary>
        /// Gets the placeholder text for the specified TextBox.
        /// </summary>
        public static string GetPlaceholder(DependencyObject obj) =>
            (string)obj.GetValue(PlaceholderProperty);

        /// <summary>
        /// Sets the placeholder text for the specified TextBox.
        /// </summary>
        public static void SetPlaceholder(DependencyObject obj, string value) =>
            obj.SetValue(PlaceholderProperty, value);

        #endregion

        #region HasText Property (Read-Only Helper)

        /// <summary>
        /// Read-only attached property indicating whether the TextBox has text.
        /// Used internally by styles to control placeholder visibility.
        /// </summary>
        private static readonly DependencyPropertyKey HasTextPropertyKey =
            DependencyProperty.RegisterAttachedReadOnly(
                "HasText",
                typeof(bool),
                typeof(TextBoxHelper),
                new PropertyMetadata(false));

        public static readonly DependencyProperty HasTextProperty =
            HasTextPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets whether the TextBox has text.
        /// </summary>
        public static bool GetHasText(DependencyObject obj) =>
            (bool)obj.GetValue(HasTextProperty);

        /// <summary>
        /// Sets whether the TextBox has text (internal use only).
        /// </summary>
        private static void SetHasText(DependencyObject obj, bool value) =>
            obj.SetValue(HasTextPropertyKey, value);

        #endregion

        #region IsMonitoring Property (Internal)

        /// <summary>
        /// Internal property to track if TextChanged event is being monitored.
        /// </summary>
        private static readonly DependencyProperty IsMonitoringProperty =
            DependencyProperty.RegisterAttached(
                "IsMonitoring",
                typeof(bool),
                typeof(TextBoxHelper),
                new PropertyMetadata(false, OnIsMonitoringChanged));

        private static bool GetIsMonitoring(DependencyObject obj) =>
            (bool)obj.GetValue(IsMonitoringProperty);

        internal static void SetIsMonitoring(DependencyObject obj, bool value) =>
            obj.SetValue(IsMonitoringProperty, value);

        private static void OnIsMonitoringChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox textBox)
            {
                if ((bool)e.NewValue)
                {
                    textBox.TextChanged += TextBox_TextChanged;
                    textBox.Loaded += TextBox_Loaded;
                    // Initialize HasText based on current value
                    UpdateHasText(textBox);
                }
                else
                {
                    textBox.TextChanged -= TextBox_TextChanged;
                    textBox.Loaded -= TextBox_Loaded;
                }
            }
        }

        private static void TextBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                UpdateHasText(textBox);
            }
        }

        private static void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                UpdateHasText(textBox);
            }
        }

        private static void UpdateHasText(TextBox textBox)
        {
            SetHasText(textBox, !string.IsNullOrEmpty(textBox.Text));
        }

        #endregion
    }
}
