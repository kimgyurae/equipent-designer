using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using EquipmentDesigner.Models;

namespace EquipmentDesigner.Views.Drawboard.Adorners
{
    /// <summary>
    /// Adorner that displays selection box and resize handles for a DrawingElement.
    /// </summary>
    public class SelectionAdorner : Adorner
    {
        private const double SelectionPadding = 4.0;
        private const double HandleSize = 8.0;
        private const double HandleCornerRadius = 2.0;
        private const double EdgeHitTestThickness = 6.0;

        private static readonly Brush HandleFillBrush = Brushes.White;
        private static readonly Brush SelectionBorderBrush = new SolidColorBrush(Color.FromRgb(0x60, 0xA5, 0xFA)); // #60A5FA
        private static readonly Pen SelectionPen;
        private static readonly Pen HandlePen;

        private readonly DrawingElement _element;

        static SelectionAdorner()
        {
            SelectionBorderBrush.Freeze();
            SelectionPen = new Pen(SelectionBorderBrush, 1.0);
            SelectionPen.Freeze();
            HandlePen = new Pen(SelectionBorderBrush, 1.5);
            HandlePen.Freeze();
        }

        /// <summary>
        /// Initializes a new instance of the SelectionAdorner class.
        /// </summary>
        /// <param name="adornedElement">The UI element to adorn.</param>
        /// <param name="element">The DrawingElement data model.</param>
        public SelectionAdorner(UIElement adornedElement, DrawingElement element) : base(adornedElement)
        {
            _element = element ?? throw new ArgumentNullException(nameof(element));

            // Subscribe to element property changes to update adorner
            _element.PropertyChanged += Element_PropertyChanged;

            // Adorner should not block hit testing on the element
            IsHitTestVisible = true;
        }

        /// <summary>
        /// Gets the DrawingElement this adorner is attached to.
        /// </summary>
        public DrawingElement Element => _element;

        /// <summary>
        /// Renders the selection box and resize handles.
        /// </summary>
        protected override void OnRender(DrawingContext drawingContext)
        {
            var renderSize = AdornedElement.RenderSize;

            // Calculate selection box bounds (4px padding around element)
            var selectionRect = new Rect(
                -SelectionPadding,
                -SelectionPadding,
                renderSize.Width + (SelectionPadding * 2),
                renderSize.Height + (SelectionPadding * 2)
            );

            // Draw selection box (transparent fill, blue stroke)
            drawingContext.DrawRectangle(null, SelectionPen, selectionRect);

            // Draw corner resize handles
            DrawHandle(drawingContext, GetHandleRect(ResizeHandleType.TopLeft, selectionRect));
            DrawHandle(drawingContext, GetHandleRect(ResizeHandleType.TopRight, selectionRect));
            DrawHandle(drawingContext, GetHandleRect(ResizeHandleType.BottomLeft, selectionRect));
            DrawHandle(drawingContext, GetHandleRect(ResizeHandleType.BottomRight, selectionRect));
        }

        /// <summary>
        /// Draws a resize handle at the specified rectangle.
        /// </summary>
        private void DrawHandle(DrawingContext drawingContext, Rect handleRect)
        {
            var geometry = new RectangleGeometry(handleRect, HandleCornerRadius, HandleCornerRadius);
            drawingContext.DrawGeometry(HandleFillBrush, HandlePen, geometry);
        }

        /// <summary>
        /// Gets the rectangle for a specific resize handle type.
        /// </summary>
        private Rect GetHandleRect(ResizeHandleType handleType, Rect selectionRect)
        {
            double halfHandle = HandleSize / 2;

            return handleType switch
            {
                ResizeHandleType.TopLeft => new Rect(
                    selectionRect.Left - halfHandle,
                    selectionRect.Top - halfHandle,
                    HandleSize,
                    HandleSize),

                ResizeHandleType.TopRight => new Rect(
                    selectionRect.Right - halfHandle,
                    selectionRect.Top - halfHandle,
                    HandleSize,
                    HandleSize),

                ResizeHandleType.BottomLeft => new Rect(
                    selectionRect.Left - halfHandle,
                    selectionRect.Bottom - halfHandle,
                    HandleSize,
                    HandleSize),

                ResizeHandleType.BottomRight => new Rect(
                    selectionRect.Right - halfHandle,
                    selectionRect.Bottom - halfHandle,
                    HandleSize,
                    HandleSize),

                ResizeHandleType.Top => new Rect(
                    selectionRect.Left + (selectionRect.Width / 2) - halfHandle,
                    selectionRect.Top - halfHandle,
                    HandleSize,
                    HandleSize),

                ResizeHandleType.Right => new Rect(
                    selectionRect.Right - halfHandle,
                    selectionRect.Top + (selectionRect.Height / 2) - halfHandle,
                    HandleSize,
                    HandleSize),

                ResizeHandleType.Bottom => new Rect(
                    selectionRect.Left + (selectionRect.Width / 2) - halfHandle,
                    selectionRect.Bottom - halfHandle,
                    HandleSize,
                    HandleSize),

                ResizeHandleType.Left => new Rect(
                    selectionRect.Left - halfHandle,
                    selectionRect.Top + (selectionRect.Height / 2) - halfHandle,
                    HandleSize,
                    HandleSize),

                _ => Rect.Empty
            };
        }

        /// <summary>
        /// Performs hit testing to determine if a point is over a resize handle or edge.
        /// </summary>
        /// <param name="point">The point in adorner coordinates.</param>
        /// <returns>The type of resize handle at the point, or None if not over a handle.</returns>
        public ResizeHandleType HitTestHandle(Point point)
        {
            var renderSize = AdornedElement.RenderSize;
            var selectionRect = new Rect(
                -SelectionPadding,
                -SelectionPadding,
                renderSize.Width + (SelectionPadding * 2),
                renderSize.Height + (SelectionPadding * 2)
            );

            // Check corner handles first (they have priority)
            if (GetHandleRect(ResizeHandleType.TopLeft, selectionRect).Contains(point))
                return ResizeHandleType.TopLeft;
            if (GetHandleRect(ResizeHandleType.TopRight, selectionRect).Contains(point))
                return ResizeHandleType.TopRight;
            if (GetHandleRect(ResizeHandleType.BottomLeft, selectionRect).Contains(point))
                return ResizeHandleType.BottomLeft;
            if (GetHandleRect(ResizeHandleType.BottomRight, selectionRect).Contains(point))
                return ResizeHandleType.BottomRight;

            // Check edges
            var topEdge = new Rect(selectionRect.Left + HandleSize, selectionRect.Top - EdgeHitTestThickness / 2,
                selectionRect.Width - HandleSize * 2, EdgeHitTestThickness);
            if (topEdge.Contains(point))
                return ResizeHandleType.Top;

            var bottomEdge = new Rect(selectionRect.Left + HandleSize, selectionRect.Bottom - EdgeHitTestThickness / 2,
                selectionRect.Width - HandleSize * 2, EdgeHitTestThickness);
            if (bottomEdge.Contains(point))
                return ResizeHandleType.Bottom;

            var leftEdge = new Rect(selectionRect.Left - EdgeHitTestThickness / 2, selectionRect.Top + HandleSize,
                EdgeHitTestThickness, selectionRect.Height - HandleSize * 2);
            if (leftEdge.Contains(point))
                return ResizeHandleType.Left;

            var rightEdge = new Rect(selectionRect.Right - EdgeHitTestThickness / 2, selectionRect.Top + HandleSize,
                EdgeHitTestThickness, selectionRect.Height - HandleSize * 2);
            if (rightEdge.Contains(point))
                return ResizeHandleType.Right;

            return ResizeHandleType.None;
        }

        /// <summary>
        /// Checks if a point is within the element surface (for move operations).
        /// </summary>
        /// <param name="point">The point in adorner coordinates.</param>
        /// <returns>True if the point is on the element surface.</returns>
        public bool IsOnElementSurface(Point point)
        {
            var renderSize = AdornedElement.RenderSize;
            var elementRect = new Rect(0, 0, renderSize.Width, renderSize.Height);
            return elementRect.Contains(point);
        }

        /// <summary>
        /// Handles property changes on the DrawingElement to invalidate the adorner.
        /// </summary>
        private void Element_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Invalidate visual when element properties change
            if (e.PropertyName == nameof(DrawingElement.Width) ||
                e.PropertyName == nameof(DrawingElement.Height))
            {
                InvalidateVisual();
            }
        }

        /// <summary>
        /// Cleans up resources when the adorner is no longer needed.
        /// </summary>
        public void Detach()
        {
            _element.PropertyChanged -= Element_PropertyChanged;
        }
    }
}
