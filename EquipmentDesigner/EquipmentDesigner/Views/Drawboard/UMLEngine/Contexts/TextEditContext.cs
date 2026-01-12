using System.Windows;
using EquipmentDesigner.Models;

namespace EquipmentDesigner.Views.Drawboard.UMLEngine.Contexts
{
    /// <summary>
    /// Immutable context for text editing operations.
    /// Contains element bounds, shape type, and text properties for calculation.
    /// </summary>
    public readonly struct TextEditContext
    {
        /// <summary>
        /// Element bounds in canvas coordinates.
        /// </summary>
        public Rect ElementBounds { get; }

        /// <summary>
        /// Shape type determines text-safe margin calculation.
        /// </summary>
        public DrawingShapeType ShapeType { get; }

        /// <summary>
        /// Font size in pixels for height calculation.
        /// </summary>
        public double FontSizePixels { get; }

        /// <summary>
        /// Current text content for height calculation.
        /// </summary>
        public string CurrentText { get; }

        /// <summary>
        /// Original element height before text editing (for undo/restore).
        /// </summary>
        public double OriginalHeight { get; }

        public TextEditContext(
            Rect elementBounds,
            DrawingShapeType shapeType,
            double fontSizePixels,
            string currentText,
            double originalHeight)
        {
            ElementBounds = elementBounds;
            ShapeType = shapeType;
            FontSizePixels = fontSizePixels;
            CurrentText = currentText ?? string.Empty;
            OriginalHeight = originalHeight;
        }

        /// <summary>
        /// Creates a new context with updated element bounds.
        /// </summary>
        public TextEditContext WithUpdatedBounds(Rect newBounds)
        {
            return new TextEditContext(
                newBounds,
                ShapeType,
                FontSizePixels,
                CurrentText,
                OriginalHeight);
        }
    }
}
