using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using EquipmentDesigner.Models;
using EquipmentDesigner.Views.Drawboard.UMLEngine.Contexts;
using EquipmentDesigner.Views.Drawboard.UMLEngine.Results;

namespace EquipmentDesigner.Views.Drawboard.UMLEngine
{
    /// <summary>
    /// Stateless engine for calculating text editing transformations.
    /// All methods are pure functions with no side effects.
    /// </summary>
    public static class TextEditingEngine
    {
        #region Shape Margin Constants

        /// <summary>
        /// Text margin for ActionElement (rectangle).
        /// </summary>
        public const double ActionMargin = 4.0;

        /// <summary>
        /// Text margin for DecisionElement (diamond - larger due to pointed edges).
        /// </summary>
        public const double DecisionMargin = 12.0;

        /// <summary>
        /// Text margin for InitialElement (ellipse).
        /// </summary>
        public const double InitialMargin = 8.0;

        /// <summary>
        /// Text margin for TerminalElement (double ellipse).
        /// </summary>
        public const double TerminalMargin = 10.0;

        /// <summary>
        /// Text margin for PredefinedActionElement (double rectangle).
        /// </summary>
        public const double PredefinedActionMargin = 10.0;

        /// <summary>
        /// Text margin for TextboxElement.
        /// </summary>
        public const double TextboxMargin = 4.0;

        /// <summary>
        /// Minimum text area width.
        /// </summary>
        public const double MinTextWidth = 20.0;

        /// <summary>
        /// Minimum text area height.
        /// </summary>
        public const double MinTextHeight = 16.0;

        /// <summary>
        /// Ratio for inscribed rectangle in ellipse (1/sqrt(2) ≈ 0.707).
        /// </summary>
        private const double EllipseInscribedRatio = 0.707;

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the text-safe margin for a specific shape type.
        /// </summary>
        public static double GetTextMargin(DrawingShapeType shapeType)
        {
            return shapeType switch
            {
                DrawingShapeType.Action => ActionMargin,
                DrawingShapeType.Decision => DecisionMargin,
                DrawingShapeType.Initial => InitialMargin,
                DrawingShapeType.Terminal => TerminalMargin,
                DrawingShapeType.PredefinedAction => PredefinedActionMargin,
                DrawingShapeType.Textbox => TextboxMargin,
                _ => ActionMargin
            };
        }

        /// <summary>
        /// Calculates the text-safe bounds for a shape.
        /// For most shapes, this is elementBounds inset by margin.
        /// For Decision (diamond), this is the inscribed rectangle.
        /// For ellipses (Initial, Terminal), this uses the inscribed rectangle ratio.
        /// </summary>
        public static Rect GetTextSafeBounds(DrawingShapeType shapeType, Rect elementBounds)
        {
            double margin = GetTextMargin(shapeType);

            if (shapeType == DrawingShapeType.Decision)
            {
                return CalculateDiamondTextBounds(elementBounds, margin);
            }

            if (shapeType == DrawingShapeType.Initial || shapeType == DrawingShapeType.Terminal)
            {
                return CalculateEllipseTextBounds(elementBounds, margin);
            }

            // Rectangle shapes (Action, PredefinedAction, Textbox): simple margin inset
            return CalculateRectangleTextBounds(elementBounds, margin);
        }

        /// <summary>
        /// Creates a TextEditContext for starting text editing.
        /// </summary>
        public static TextEditContext CreateTextEditContext(DrawingElement element)
        {
            double fontSizePixels = GetFontSizePixels(element.FontSize);

            return new TextEditContext(
                element.Bounds,
                element.ShapeType,
                fontSizePixels,
                element.Text,
                element.Height);
        }

        /// <summary>
        /// Calculates text-safe bounds when starting text editing.
        /// </summary>
        public static TextEditResult CalculateTextEditBounds(TextEditContext context)
        {
            var textSafeBounds = GetTextSafeBounds(context.ShapeType, context.ElementBounds);
            double margin = GetTextMargin(context.ShapeType);

            return TextEditResult.ForStart(textSafeBounds, margin);
        }

        /// <summary>
        /// Calculates required element height based on text content.
        /// Uses WPF FormattedText for accurate measurement.
        /// </summary>
        public static TextEditResult CalculateRequiredSize(TextEditContext context, string newText)
        {
            var textSafeBounds = GetTextSafeBounds(context.ShapeType, context.ElementBounds);
            double margin = GetTextMargin(context.ShapeType);

            if (string.IsNullOrEmpty(newText))
            {
                return new TextEditResult(
                    textSafeBounds,
                    context.ElementBounds.Width,
                    context.OriginalHeight,
                    needsResize: false,
                    margin);
            }

            // Measure text height using FormattedText
            double requiredTextHeight = MeasureTextHeight(newText, textSafeBounds.Width, context.FontSizePixels);
            double requiredElementHeight = CalculateElementHeightForTextHeight(
                context.ShapeType, requiredTextHeight, margin);

            // Only grow, never shrink below original
            requiredElementHeight = Math.Max(requiredElementHeight, context.OriginalHeight);

            bool needsResize = Math.Abs(requiredElementHeight - context.ElementBounds.Height) > 1.0;

            // Recalculate text-safe bounds with new height if needed
            Rect newTextSafeBounds;
            if (needsResize)
            {
                var newElementBounds = new Rect(
                    context.ElementBounds.X,
                    context.ElementBounds.Y,
                    context.ElementBounds.Width,
                    requiredElementHeight);
                newTextSafeBounds = GetTextSafeBounds(context.ShapeType, newElementBounds);
            }
            else
            {
                newTextSafeBounds = textSafeBounds;
            }

            return new TextEditResult(
                newTextSafeBounds,
                context.ElementBounds.Width,
                requiredElementHeight,
                needsResize,
                margin);
        }

        /// <summary>
        /// Converts TextFontSize enum to pixel value.
        /// </summary>
        public static double GetFontSizePixels(TextFontSize fontSize)
        {
            return fontSize switch
            {
                TextFontSize.XS => 10.0,
                TextFontSize.Base => 14.0,
                TextFontSize.XL => 20.0,
                _ => 14.0
            };
        }

        #endregion

        #region Private Methods - Shape-Specific Calculations

        /// <summary>
        /// Calculates text bounds for diamond (Decision) shape.
        /// The largest inscribed rectangle in a diamond is W/2 x H/2 centered.
        /// </summary>
        private static Rect CalculateDiamondTextBounds(Rect elementBounds, double margin)
        {
            // Diamond inscribed rectangle is W/2 x H/2 centered at element center
            double inscribedWidth = elementBounds.Width / 2.0;
            double inscribedHeight = elementBounds.Height / 2.0;

            double textWidth = Math.Max(MinTextWidth, inscribedWidth - 2 * margin);
            double textHeight = Math.Max(MinTextHeight, inscribedHeight - 2 * margin);

            double textX = elementBounds.X + (elementBounds.Width - textWidth) / 2.0;
            double textY = elementBounds.Y + (elementBounds.Height - textHeight) / 2.0;

            return new Rect(textX, textY, textWidth, textHeight);
        }

        /// <summary>
        /// Calculates text bounds for ellipse shapes (Initial, Terminal).
        /// Uses inscribed rectangle ratio (1/sqrt(2) ≈ 0.707).
        /// </summary>
        private static Rect CalculateEllipseTextBounds(Rect elementBounds, double margin)
        {
            // Inscribed rectangle in ellipse: W/sqrt(2) x H/sqrt(2) centered
            double inscribedWidth = elementBounds.Width * EllipseInscribedRatio;
            double inscribedHeight = elementBounds.Height * EllipseInscribedRatio;

            double textWidth = Math.Max(MinTextWidth, inscribedWidth - 2 * margin);
            double textHeight = Math.Max(MinTextHeight, inscribedHeight - 2 * margin);

            double textX = elementBounds.X + (elementBounds.Width - textWidth) / 2.0;
            double textY = elementBounds.Y + (elementBounds.Height - textHeight) / 2.0;

            return new Rect(textX, textY, textWidth, textHeight);
        }

        /// <summary>
        /// Calculates text bounds for rectangle shapes (Action, PredefinedAction, Textbox).
        /// Simple margin inset from element bounds.
        /// </summary>
        private static Rect CalculateRectangleTextBounds(Rect elementBounds, double margin)
        {
            double width = Math.Max(MinTextWidth, elementBounds.Width - 2 * margin);
            double height = Math.Max(MinTextHeight, elementBounds.Height - 2 * margin);

            return new Rect(
                elementBounds.X + margin,
                elementBounds.Y + margin,
                width,
                height);
        }

        /// <summary>
        /// Calculates required element height given required text height.
        /// Accounts for shape-specific geometry.
        /// </summary>
        private static double CalculateElementHeightForTextHeight(
            DrawingShapeType shapeType,
            double textHeight,
            double margin)
        {
            if (shapeType == DrawingShapeType.Decision)
            {
                // Diamond: text height is in inscribed rectangle (H/2 - 2*margin)
                // So element height = 2 * (textHeight + 2*margin)
                return 2 * (textHeight + 2 * margin);
            }

            if (shapeType == DrawingShapeType.Initial || shapeType == DrawingShapeType.Terminal)
            {
                // Ellipse: text height is ~70% of element height minus margin
                // So element height = (textHeight + 2*margin) / 0.707
                return (textHeight + 2 * margin) / EllipseInscribedRatio;
            }

            // Rectangle: element height = text height + 2*margin
            return textHeight + 2 * margin;
        }

        /// <summary>
        /// Measures text height using WPF FormattedText.
        /// </summary>
        private static double MeasureTextHeight(string text, double maxWidth, double fontSize)
        {
            var formattedText = new FormattedText(
                text,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Segoe UI"),
                fontSize,
                Brushes.Black,
                96.0); // Standard DPI

            formattedText.MaxTextWidth = maxWidth;
            formattedText.Trimming = TextTrimming.None;

            return formattedText.Height;
        }

        #endregion
    }
}
