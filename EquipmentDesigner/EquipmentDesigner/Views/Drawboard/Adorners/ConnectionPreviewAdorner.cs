using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using EquipmentDesigner.Models;

namespace EquipmentDesigner.Views.Drawboard.Adorners
{
    /// <summary>
    /// Adorner that renders a preview of the connection being drawn.
    /// Shows an orthogonal polyline from the source port to the current mouse position
    /// with an arrowhead at the end.
    /// </summary>
    public class ConnectionPreviewAdorner : Adorner
    {
        private const double LineThickness = 2.0;
        private const double ArrowHeadLength = 10.0;
        private const double ArrowHeadWidth = 8.0;
        private const double ThumbSize = 12.0;

        private static readonly Brush LineBrush;
        private static readonly Brush LineBrushSnapped;
        private static readonly Brush ThumbFillBrush;
        private static readonly Pen LinePen;
        private static readonly Pen LinePenSnapped;
        private static readonly Pen ThumbPen;

        private IReadOnlyList<Point> _routePoints;
        private bool _isSnapped;
        private Point? _snapPosition;

        static ConnectionPreviewAdorner()
        {
            // Normal state: gray line
            LineBrush = new SolidColorBrush(Color.FromRgb(0x33, 0x33, 0x33));
            LineBrush.Freeze();
            LinePen = new Pen(LineBrush, LineThickness);
            LinePen.Freeze();

            // Snapped state: blue line (primary color)
            LineBrushSnapped = new SolidColorBrush(Color.FromRgb(0x60, 0xA5, 0xFA)); // Primary.400
            LineBrushSnapped.Freeze();
            LinePenSnapped = new Pen(LineBrushSnapped, LineThickness);
            LinePenSnapped.Freeze();

            // Thumb appearance
            ThumbFillBrush = Brushes.White;
            ThumbPen = new Pen(LineBrush, 1.5);
            ThumbPen.Freeze();
        }

        /// <summary>
        /// Initializes a new instance of the ConnectionPreviewAdorner class.
        /// </summary>
        /// <param name="adornedElement">The canvas element to adorn.</param>
        public ConnectionPreviewAdorner(UIElement adornedElement) : base(adornedElement)
        {
            IsHitTestVisible = false; // Preview doesn't need to receive mouse events
            _routePoints = Array.Empty<Point>();
        }

        /// <summary>
        /// Updates the route points for the connection preview.
        /// </summary>
        /// <param name="routePoints">The orthogonal route points.</param>
        public void UpdateRoute(IReadOnlyList<Point> routePoints)
        {
            _routePoints = routePoints ?? Array.Empty<Point>();
            InvalidateVisual();
        }

        /// <summary>
        /// Sets the snap state for visual feedback.
        /// </summary>
        /// <param name="isSnapped">Whether the connection is snapping to a target.</param>
        /// <param name="snapPosition">The position of the snap target (if snapped).</param>
        public void SetSnapState(bool isSnapped, Point? snapPosition = null)
        {
            _isSnapped = isSnapped;
            _snapPosition = snapPosition;
            InvalidateVisual();
        }

        /// <summary>
        /// Renders the connection preview.
        /// </summary>
        protected override void OnRender(DrawingContext drawingContext)
        {
            if (_routePoints == null || _routePoints.Count < 2)
                return;

            var pen = _isSnapped ? LinePenSnapped : LinePen;
            var brush = _isSnapped ? LineBrushSnapped : LineBrush;

            // Draw the polyline segments
            for (int i = 0; i < _routePoints.Count - 1; i++)
            {
                drawingContext.DrawLine(pen, _routePoints[i], _routePoints[i + 1]);
            }

            // Draw arrowhead at the end
            var endPoint = _routePoints[_routePoints.Count - 1];
            var previousPoint = _routePoints[_routePoints.Count - 2];
            DrawArrowHead(drawingContext, previousPoint, endPoint, brush);

            // Draw thumb indicator at the arrow head
            DrawThumb(drawingContext, endPoint, _isSnapped);
        }

        /// <summary>
        /// Draws an arrowhead at the end of the connection.
        /// </summary>
        private void DrawArrowHead(DrawingContext dc, Point from, Point to, Brush brush)
        {
            // Calculate direction
            var dx = to.X - from.X;
            var dy = to.Y - from.Y;
            var length = Math.Sqrt(dx * dx + dy * dy);

            if (length < 0.1) return;

            // Normalize direction
            dx /= length;
            dy /= length;

            // Calculate perpendicular
            var px = -dy;
            var py = dx;

            // Arrow head points
            var tip = to;
            var baseLeft = new Point(
                to.X - ArrowHeadLength * dx + ArrowHeadWidth / 2 * px,
                to.Y - ArrowHeadLength * dy + ArrowHeadWidth / 2 * py);
            var baseRight = new Point(
                to.X - ArrowHeadLength * dx - ArrowHeadWidth / 2 * px,
                to.Y - ArrowHeadLength * dy - ArrowHeadWidth / 2 * py);

            // Draw filled triangle
            var geometry = new StreamGeometry();
            using (var ctx = geometry.Open())
            {
                ctx.BeginFigure(tip, true, true);
                ctx.LineTo(baseLeft, true, false);
                ctx.LineTo(baseRight, true, false);
            }
            geometry.Freeze();

            dc.DrawGeometry(brush, null, geometry);
        }

        /// <summary>
        /// Draws a thumb indicator at the arrow head for visual feedback.
        /// </summary>
        private void DrawThumb(DrawingContext dc, Point position, bool isSnapped)
        {
            var radius = ThumbSize / 2;

            // Draw circle (filled white with border)
            dc.DrawEllipse(
                ThumbFillBrush,
                isSnapped ? LinePenSnapped : ThumbPen,
                position,
                radius,
                radius);

            // Draw a small dot in the center when snapped
            if (isSnapped)
            {
                dc.DrawEllipse(
                    LineBrushSnapped,
                    null,
                    position,
                    3,
                    3);
            }
        }

        /// <summary>
        /// Measures the adorner size.
        /// </summary>
        protected override Size MeasureOverride(Size constraint)
        {
            return AdornedElement.RenderSize;
        }

        /// <summary>
        /// Arranges the adorner.
        /// </summary>
        protected override Size ArrangeOverride(Size finalSize)
        {
            return finalSize;
        }
    }
}
