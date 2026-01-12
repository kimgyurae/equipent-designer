using System.Windows;

namespace EquipmentDesigner.Views.Drawboard.UMLEngine.Contexts
{
    /// <summary>
    /// Immutable context for canvas pan operations.
    /// Stores initial state when pan begins.
    /// </summary>
    public readonly struct PanContext
    {
        /// <summary>
        /// Mouse position at pan start (in viewport/screen coordinates).
        /// </summary>
        public Point DragStartPoint { get; }

        /// <summary>
        /// Scroll offset at pan start (HorizontalOffset, VerticalOffset).
        /// </summary>
        public Point OriginalScrollOffset { get; }

        /// <summary>
        /// Current zoom scale for coordinate transformation.
        /// </summary>
        public double ZoomScale { get; }

        public PanContext(Point dragStartPoint, Point originalScrollOffset, double zoomScale)
        {
            DragStartPoint = dragStartPoint;
            OriginalScrollOffset = originalScrollOffset;
            ZoomScale = zoomScale;
        }
    }
}
