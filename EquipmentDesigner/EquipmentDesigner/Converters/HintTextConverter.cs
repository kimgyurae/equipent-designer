using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace EquipmentDesigner.Converters
{
    /// <summary>
    /// Converts hint text with bracketed segments like [Space] into formatted inline elements.
    /// Bracketed text is rendered in a rounded rectangle badge.
    /// </summary>
    public class HintTextConverter : IValueConverter
    {
        private static readonly Regex BracketPattern = new Regex(@"\[([^\]]+)\]", RegexOptions.Compiled);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var text = value as string;
            if (string.IsNullOrEmpty(text))
            {
                return new List<Inline>();
            }

            var inlines = new List<Inline>();
            var lastIndex = 0;

            foreach (Match match in BracketPattern.Matches(text))
            {
                // Add text before the match
                if (match.Index > lastIndex)
                {
                    var beforeText = text.Substring(lastIndex, match.Index - lastIndex);
                    inlines.Add(new Run(beforeText));
                }

                // Create badge for bracketed text
                var badgeText = match.Groups[1].Value;
                var badge = CreateKeyboardBadge(badgeText);
                inlines.Add(new InlineUIContainer(badge));

                lastIndex = match.Index + match.Length;
            }

            // Add remaining text after last match
            if (lastIndex < text.Length)
            {
                var remainingText = text.Substring(lastIndex);
                inlines.Add(new Run(remainingText));
            }

            return inlines;
        }

        private Border CreateKeyboardBadge(string text)
        {
            var border = new Border
            {
                Background = (Brush)Application.Current.FindResource("Brush.Background.Tertiary"),
                BorderBrush = (Brush)Application.Current.FindResource("Brush.Border.Primary"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(4, 1, 4, 1),
                Margin = new Thickness(2, 0, 2, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            var textBlock = new TextBlock
            {
                Text = text,
                FontSize = 10,
                FontWeight = FontWeights.Medium,
                Foreground = (Brush)Application.Current.FindResource("Brush.Text.Secondary"),
                VerticalAlignment = VerticalAlignment.Center
            };

            border.Child = textBlock;
            return border;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Behavior to apply formatted hint text to a TextBlock.
    /// </summary>
    public static class HintTextBehavior
    {
        public static readonly DependencyProperty HintTextProperty =
            DependencyProperty.RegisterAttached(
                "HintText",
                typeof(string),
                typeof(HintTextBehavior),
                new PropertyMetadata(null, OnHintTextChanged));

        public static string GetHintText(DependencyObject obj)
        {
            return (string)obj.GetValue(HintTextProperty);
        }

        public static void SetHintText(DependencyObject obj, string value)
        {
            obj.SetValue(HintTextProperty, value);
        }

        private static readonly Regex BracketPattern = new Regex(@"\[([^\]]+)\]", RegexOptions.Compiled);

        private static void OnHintTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is TextBlock textBlock)) return;

            var text = e.NewValue as string;
            textBlock.Inlines.Clear();

            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            var lastIndex = 0;

            foreach (Match match in BracketPattern.Matches(text))
            {
                // Add text before the match
                if (match.Index > lastIndex)
                {
                    var beforeText = text.Substring(lastIndex, match.Index - lastIndex);
                    textBlock.Inlines.Add(new Run(beforeText));
                }

                // Create badge for bracketed text
                var badgeText = match.Groups[1].Value;
                var badge = CreateKeyboardBadge(badgeText);
                textBlock.Inlines.Add(new InlineUIContainer(badge) { BaselineAlignment = BaselineAlignment.Center });

                lastIndex = match.Index + match.Length;
            }

            // Add remaining text after last match
            if (lastIndex < text.Length)
            {
                var remainingText = text.Substring(lastIndex);
                textBlock.Inlines.Add(new Run(remainingText));
            }
        }

        private static Border CreateKeyboardBadge(string text)
        {
            var border = new Border
            {
                Background = (Brush)Application.Current.FindResource("Brush.Background.Tertiary"),
                BorderBrush = (Brush)Application.Current.FindResource("Brush.Border.Primary"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(4, 1, 4, 1),
                Margin = new Thickness(2, 0, 2, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            var textBlock = new TextBlock
            {
                Text = text,
                FontSize = 12,
                Foreground = (Brush)Application.Current.FindResource("Brush.Text.Secondary"),
                VerticalAlignment = VerticalAlignment.Center
            };

            border.Child = textBlock;
            return border;
        }
    }

    /// <summary>
    /// Converts a boolean to a Brush value.
    /// </summary>
    public class BoolToBrushConverter : IValueConverter
    {
        public Brush TrueValue { get; set; }
        public Brush FalseValue { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? TrueValue : FalseValue;
            }
            return FalseValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}