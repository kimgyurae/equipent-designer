using System;
using System.Windows;
using EquipmentDesigner.Views.Drawboard.UMLEngine.Contexts;
using EquipmentDesigner.Views.Drawboard.UMLEngine.Results;

namespace EquipmentDesigner.Views.Drawboard.UMLEngine
{
    /// <summary>
    /// Stateless engine for calculating zoom transformations.
    /// All methods are pure functions with no side effects.
    /// </summary>
    public static class ZoomControlEngine
    {
        #region Constants

        /// <summary>Minimum zoom level (10%).</summary>
        public const int MinZoom = 10;

        /// <summary>Maximum zoom level (3000%).</summary>
        public const int MaxZoom = 3000;

        /// <summary>Linear zoom step for button clicks (+/-10).</summary>
        public const int LinearZoomStep = 10;

        /// <summary>Threshold below which linear zoom is used (100%).</summary>
        public const int LinearZoomThreshold = 100;

        /// <summary>Exponential zoom factor for mouse scroll (~10% per step).</summary>
        public const double ScrollZoomFactor = 1.1;

        #endregion

        #region Basic Calculations

        /// <summary>
        /// Clamps zoom level to valid range (MinZoom to MaxZoom).
        /// </summary>
        public static int ClampZoomLevel(int zoomLevel)
        {
            return Math.Clamp(zoomLevel, MinZoom, MaxZoom);
        }

        /// <summary>
        /// Converts zoom level percentage to scale factor.
        /// </summary>
        public static double CalculateZoomScale(int zoomLevel)
        {
            return zoomLevel / 100.0;
        }

        /// <summary>
        /// Calculates linear zoom in result (+LinearZoomStep).
        /// </summary>
        public static int CalculateLinearZoomIn(int currentZoom)
        {
            return Math.Min(currentZoom + LinearZoomStep, MaxZoom);
        }

        /// <summary>
        /// Calculates linear zoom out result (-LinearZoomStep).
        /// </summary>
        public static int CalculateLinearZoomOut(int currentZoom)
        {
            return Math.Max(currentZoom - LinearZoomStep, MinZoom);
        }

        /// <summary>
        /// Calculates exponential zoom in result (*ScrollZoomFactor).
        /// </summary>
        public static int CalculateExponentialZoomIn(int currentZoom)
        {
            int newZoom = (int)Math.Round(currentZoom * ScrollZoomFactor);
            return Math.Min(newZoom, MaxZoom);
        }

        /// <summary>
        /// Calculates exponential zoom out result (/ScrollZoomFactor).
        /// </summary>
        public static int CalculateExponentialZoomOut(int currentZoom)
        {
            int newZoom = (int)Math.Round(currentZoom / ScrollZoomFactor);
            return Math.Max(newZoom, MinZoom);
        }

        #endregion

        #region Context Factory

        /// <summary>
        /// Creates a zoom context from viewport state.
        /// </summary>
        public static ZoomContext CreateZoomContext(
            int currentZoomLevel,
            Point mousePosition,
            Point scrollOffset,
            Size viewportSize)
        {
            return new ZoomContext(currentZoomLevel, mousePosition, scrollOffset, viewportSize);
        }

        #endregion

        #region Mouse Wheel Zoom

        /// <summary>
        /// Calculates zoom result for mouse wheel operation.
        /// Uses linear zoom for levels below threshold, exponential for above.
        /// Maintains mouse position as focus point.
        /// </summary>
        public static ZoomResult CalculateMouseWheelZoom(ZoomContext context, bool zoomIn)
        {
            int oldZoom = context.CurrentZoomLevel;
            int newZoom;

            if (zoomIn)
            {
                // Zoom In: Linear below threshold, exponential at/above
                if (oldZoom < LinearZoomThreshold)
                {
                    newZoom = CalculateLinearZoomIn(oldZoom);
                }
                else
                {
                    newZoom = CalculateExponentialZoomIn(oldZoom);
                }
            }
            else
            {
                // Zoom Out: Linear at/below threshold, exponential above
                if (oldZoom <= LinearZoomThreshold)
                {
                    newZoom = CalculateLinearZoomOut(oldZoom);
                }
                else
                {
                    newZoom = CalculateExponentialZoomOut(oldZoom);
                }
            }

            // Check if zoom actually changed
            if (newZoom == oldZoom)
            {
                return new ZoomResult(
                    oldZoom,
                    CalculateZoomScale(oldZoom),
                    context.ScrollOffset,
                    zoomChanged: false);
            }

            // Calculate new scroll offset to maintain mouse position
            double oldScale = CalculateZoomScale(oldZoom);
            double newScale = CalculateZoomScale(newZoom);

            // Content coordinate at mouse position (before zoom)
            double contentX = (context.ScrollOffset.X + context.MousePosition.X) / oldScale;
            double contentY = (context.ScrollOffset.Y + context.MousePosition.Y) / oldScale;

            // New scroll offset to keep same content under mouse
            double newScrollX = contentX * newScale - context.MousePosition.X;
            double newScrollY = contentY * newScale - context.MousePosition.Y;

            // Clamp to non-negative values
            newScrollX = Math.Max(0, newScrollX);
            newScrollY = Math.Max(0, newScrollY);

            return new ZoomResult(
                newZoom,
                newScale,
                new Point(newScrollX, newScrollY),
                zoomChanged: true);
        }

        #endregion

        #region Viewport Center Zoom

        /// <summary>
        /// Calculates zoom result for button click operation.
        /// Always uses linear zoom step (+/-10).
        /// Maintains viewport center as focus point.
        /// </summary>
        public static ZoomResult CalculateViewportCenterZoom(ZoomContext context, bool zoomIn)
        {
            int oldZoom = context.CurrentZoomLevel;
            int newZoom;

            // Button zoom always uses linear step
            if (zoomIn)
            {
                newZoom = CalculateLinearZoomIn(oldZoom);
            }
            else
            {
                newZoom = CalculateLinearZoomOut(oldZoom);
            }

            // Check if zoom actually changed
            if (newZoom == oldZoom)
            {
                return new ZoomResult(
                    oldZoom,
                    CalculateZoomScale(oldZoom),
                    context.ScrollOffset,
                    zoomChanged: false);
            }

            // Calculate viewport center position
            double viewportCenterX = context.ViewportSize.Width / 2;
            double viewportCenterY = context.ViewportSize.Height / 2;

            // Calculate new scroll offset to maintain viewport center
            double oldScale = CalculateZoomScale(oldZoom);
            double newScale = CalculateZoomScale(newZoom);

            // Content coordinate at viewport center (before zoom)
            double contentCenterX = (context.ScrollOffset.X + viewportCenterX) / oldScale;
            double contentCenterY = (context.ScrollOffset.Y + viewportCenterY) / oldScale;

            // New scroll offset to keep same content at viewport center
            double newScrollX = contentCenterX * newScale - viewportCenterX;
            double newScrollY = contentCenterY * newScale - viewportCenterY;

            // Clamp to non-negative values
            newScrollX = Math.Max(0, newScrollX);
            newScrollY = Math.Max(0, newScrollY);

            return new ZoomResult(
                newZoom,
                newScale,
                new Point(newScrollX, newScrollY),
                zoomChanged: true);
        }

        /// <summary>
        /// Calculates zoom result for resetting zoom to 100%.
        /// Maintains viewport center as focus point.
        /// </summary>
        public static ZoomResult CalculateZoomReset(ZoomContext context)
        {
            const int targetZoom = 100;
            int oldZoom = context.CurrentZoomLevel;

            // Check if zoom is already at 100%
            if (oldZoom == targetZoom)
            {
                return new ZoomResult(
                    targetZoom,
                    CalculateZoomScale(targetZoom),
                    context.ScrollOffset,
                    zoomChanged: false);
            }

            // Calculate viewport center position
            double viewportCenterX = context.ViewportSize.Width / 2;
            double viewportCenterY = context.ViewportSize.Height / 2;

            // Calculate new scroll offset to maintain viewport center
            double oldScale = CalculateZoomScale(oldZoom);
            double newScale = CalculateZoomScale(targetZoom);

            // Content coordinate at viewport center (before zoom)
            double contentCenterX = (context.ScrollOffset.X + viewportCenterX) / oldScale;
            double contentCenterY = (context.ScrollOffset.Y + viewportCenterY) / oldScale;

            // New scroll offset to keep same content at viewport center
            double newScrollX = contentCenterX * newScale - viewportCenterX;
            double newScrollY = contentCenterY * newScale - viewportCenterY;

            // Clamp to non-negative values
            newScrollX = Math.Max(0, newScrollX);
            newScrollY = Math.Max(0, newScrollY);

            return new ZoomResult(
                targetZoom,
                newScale,
                new Point(newScrollX, newScrollY),
                zoomChanged: true);
        }

        #endregion

        #region Initial Centering

        /// <summary>
        /// Calculates scroll offset to center the canvas in the viewport.
        /// </summary>
        /// <param name="canvasSize">Total canvas size (Width, Height).</param>
        /// <param name="viewportSize">Visible viewport size (Width, Height).</param>
        /// <param name="zoomScale">Current zoom scale factor.</param>
        /// <returns>Scroll offset (X, Y) to center the canvas.</returns>
        public static Point CalculateCenterScrollOffset(Size canvasSize, Size viewportSize, double zoomScale)
        {
            // Effective canvas size after zoom transformation
            double scaledCanvasWidth = canvasSize.Width * zoomScale;
            double scaledCanvasHeight = canvasSize.Height * zoomScale;

            // Center position = (scaled canvas size - viewport size) / 2
            double centerScrollX = (scaledCanvasWidth - viewportSize.Width) / 2;
            double centerScrollY = (scaledCanvasHeight - viewportSize.Height) / 2;

            // Clamp to non-negative values (handles viewport larger than canvas)
            centerScrollX = Math.Max(0, centerScrollX);
            centerScrollY = Math.Max(0, centerScrollY);

            return new Point(centerScrollX, centerScrollY);
        }

        #endregion
    }
}