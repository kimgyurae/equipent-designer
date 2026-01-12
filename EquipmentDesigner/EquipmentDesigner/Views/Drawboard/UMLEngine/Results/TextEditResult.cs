using System.Windows;

namespace EquipmentDesigner.Views.Drawboard.UMLEngine.Results
{
    /// <summary>
    /// Result of text editing calculations.
    /// Contains text-safe bounds and required element dimensions.
    /// </summary>
    public readonly struct TextEditResult
    {
        /// <summary>
        /// Text-safe rectangular area inside the shape where TextBox can be placed.
        /// Coordinates are in canvas space (not scaled by zoom).
        /// </summary>
        public Rect TextSafeBounds { get; }

        /// <summary>
        /// Required element width if text needs more horizontal space.
        /// </summary>
        public double RequiredWidth { get; }

        /// <summary>
        /// Required element height if text needs more vertical space.
        /// </summary>
        public double RequiredHeight { get; }

        /// <summary>
        /// True if element dimensions need to change to fit text.
        /// </summary>
        public bool NeedsResize { get; }

        /// <summary>
        /// Margin used for the specific shape type.
        /// </summary>
        public double ShapeMargin { get; }

        public TextEditResult(
            Rect textSafeBounds,
            double requiredWidth,
            double requiredHeight,
            bool needsResize,
            double shapeMargin)
        {
            TextSafeBounds = textSafeBounds;
            RequiredWidth = requiredWidth;
            RequiredHeight = requiredHeight;
            NeedsResize = needsResize;
            ShapeMargin = shapeMargin;
        }

        /// <summary>
        /// Creates a result for text editing start (no resize needed).
        /// </summary>
        public static TextEditResult ForStart(Rect textSafeBounds, double shapeMargin)
        {
            return new TextEditResult(
                textSafeBounds,
                textSafeBounds.Width + 2 * shapeMargin,
                textSafeBounds.Height + 2 * shapeMargin,
                needsResize: false,
                shapeMargin);
        }
    }
}
