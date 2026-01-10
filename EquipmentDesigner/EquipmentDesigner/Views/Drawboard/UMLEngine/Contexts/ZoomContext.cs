using System.Windows;

namespace EquipmentDesigner.Views.Drawboard.UMLEngine.Contexts
{
    /// <summary>
    /// Immutable context for zoom operations.
    /// Contains viewport state at operation start.
    /// </summary>
    public readonly struct ZoomContext
    {
        /// <summary>Current zoom level percentage (e.g., 100 = 100%).</summary>
        public int CurrentZoomLevel { get; }

        /// <summary>Mouse position relative to viewport.</summary>
        public Point MousePosition { get; }

        /// <summary>Current scroll offset (HorizontalOffset, VerticalOffset).</summary>
        public Point ScrollOffset { get; }

        /// <summary>Viewport size (Width, Height).</summary>
        public Size ViewportSize { get; }

        /// <summary>
        /// Current zoom scale factor (ZoomLevel / 100.0).
        /// </summary>
        public double CurrentZoomScale => CurrentZoomLevel / 100.0;

        public ZoomContext(
            int currentZoomLevel,
            Point mousePosition,
            Point scrollOffset,
            Size viewportSize)
        {
            CurrentZoomLevel = currentZoomLevel;
            MousePosition = mousePosition;
            ScrollOffset = scrollOffset;
            ViewportSize = viewportSize;
        }
    }
}
