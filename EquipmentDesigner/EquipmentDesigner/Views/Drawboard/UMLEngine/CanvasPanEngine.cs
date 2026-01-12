using System;
using System.Windows;
using EquipmentDesigner.Views.Drawboard.UMLEngine.Contexts;
using EquipmentDesigner.Views.Drawboard.UMLEngine.Results;

namespace EquipmentDesigner.Views.Drawboard.UMLEngine
{
    /// <summary>
    /// Stateless engine for calculating canvas pan transformations.
    /// All methods are pure functions with no side effects.
    /// </summary>
    public static class CanvasPanEngine
    {
        /// <summary>
        /// Creates a pan context from current viewport state.
        /// </summary>
        /// <param name="dragStartPoint">Mouse position at pan start (viewport coordinates).</param>
        /// <param name="currentScrollOffset">Current scroll offset (HorizontalOffset, VerticalOffset).</param>
        /// <param name="zoomScale">Current zoom scale factor.</param>
        public static PanContext CreatePanContext(
            Point dragStartPoint,
            Point currentScrollOffset,
            double zoomScale)
        {
            return new PanContext(dragStartPoint, currentScrollOffset, zoomScale);
        }

        /// <summary>
        /// Calculates new scroll offset based on pan delta.
        /// Pan direction is inverted (drag right = scroll left).
        /// </summary>
        /// <param name="context">Pan context with initial state.</param>
        /// <param name="currentPoint">Current mouse position (viewport coordinates).</param>
        /// <returns>New scroll offsets to apply.</returns>
        public static PanResult CalculatePan(PanContext context, Point currentPoint)
        {
            // Calculate mouse delta in viewport coordinates
            double deltaX = currentPoint.X - context.DragStartPoint.X;
            double deltaY = currentPoint.Y - context.DragStartPoint.Y;

            // Invert delta: drag right = scroll left (content moves right)
            // No need to scale by ZoomScale since we work in viewport coordinates
            double newScrollX = context.OriginalScrollOffset.X - deltaX;
            double newScrollY = context.OriginalScrollOffset.Y - deltaY;

            // Clamp to non-negative values
            newScrollX = Math.Max(0, newScrollX);
            newScrollY = Math.Max(0, newScrollY);

            return new PanResult(newScrollX, newScrollY);
        }
    }
}
