using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using EquipmentDesigner.Models;

namespace EquipmentDesigner.Views.Drawboard.Adorners
{
    /// <summary>
    /// Adorner that displays selection box and interactive resize handles using Thumb controls.
    /// Handles cursor changes and drag events automatically through UIElement events.
    /// </summary>
    public class SelectionAdorner : Adorner
    {
        private const double SelectionPadding = 4.0;
        private const double HandleSize = 8.0;

        private static readonly Brush HandleFillBrush = Brushes.White;
        private static readonly Brush SelectionBorderBrush;
        private static readonly Pen SelectionPen;
        private static readonly Pen HandlePen;

        private readonly DrawingElement _element;
        private readonly VisualCollection _visualChildren;
        private readonly Dictionary<ResizeHandleType, Thumb> _handles;

        // Drag state tracking - fixed start point for consistent coordinate calculation
        private Point _dragStartPoint;
        private Point _initialDragStartPoint;  // Fixed reference point (never changes during drag)
        private double _cumulativeHorizontal;   // Accumulated horizontal movement
        private double _cumulativeVertical;     // Accumulated vertical movement

        /// <summary>
        /// Raised when a resize operation begins.
        /// </summary>
        public event EventHandler<ResizeEventArgs> ResizeStarted;

        /// <summary>
        /// Raised during resize drag with current position.
        /// </summary>
        public event EventHandler<ResizeEventArgs> ResizeDelta;

        /// <summary>
        /// Raised when a resize operation completes.
        /// </summary>
        public event EventHandler<ResizeEventArgs> ResizeCompleted;

        static SelectionAdorner()
        {
            SelectionBorderBrush = new SolidColorBrush(Color.FromRgb(0x60, 0xA5, 0xFA)); // #60A5FA
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
            _visualChildren = new VisualCollection(this);
            _handles = new Dictionary<ResizeHandleType, Thumb>();

            // Subscribe to element property changes to update adorner
            _element.PropertyChanged += Element_PropertyChanged;

            // Adorner must be hit-testable for child Thumbs to receive events
            IsHitTestVisible = true;

            // Create Thumb controls for all resize handles
            CreateResizeHandles();
        }

        /// <summary>
        /// Gets the DrawingElement this adorner is attached to.
        /// </summary>
        public DrawingElement Element => _element;

        /// <summary>
        /// Gets the number of visual children.
        /// </summary>
        protected override int VisualChildrenCount => _visualChildren.Count;

        /// <summary>
        /// Gets the visual child at the specified index.
        /// </summary>
        protected override Visual GetVisualChild(int index) => _visualChildren[index];

        /// <summary>
        /// Creates Thumb controls for corner and edge resize handles.
        /// </summary>
        private void CreateResizeHandles()
        {
            // Create handles for all 8 positions (4 corners + 4 edges)
            var handleTypes = new[]
            {
                // Corners
                ResizeHandleType.TopLeft,
                ResizeHandleType.TopRight,
                ResizeHandleType.BottomLeft,
                ResizeHandleType.BottomRight,
                // Edges
                ResizeHandleType.Top,
                ResizeHandleType.Right,
                ResizeHandleType.Bottom,
                ResizeHandleType.Left
            };

            foreach (var handleType in handleTypes)
            {
                var thumb = CreateThumb(handleType);
                _handles[handleType] = thumb;
                _visualChildren.Add(thumb);
            }
        }

        /// <summary>
        /// Creates a single Thumb control for the specified handle type.
        /// </summary>
        private Thumb CreateThumb(ResizeHandleType handleType)
        {
            var thumb = new Thumb
            {
                Width = HandleSize,
                Height = HandleSize,
                Cursor = GetCursorForHandle(handleType),
                Background = HandleFillBrush,
                BorderBrush = SelectionBorderBrush,
                BorderThickness = new Thickness(1.5),
                Opacity = 1.0
            };

            // Wire up drag events
            thumb.DragStarted += (s, e) => OnThumbDragStarted(handleType, e);
            thumb.DragDelta += (s, e) => OnThumbDragDelta(handleType, e);
            thumb.DragCompleted += (s, e) => OnThumbDragCompleted(handleType, e);

            return thumb;
        }

        /// <summary>
        /// Gets the appropriate cursor for a resize handle type.
        /// </summary>
        private static Cursor GetCursorForHandle(ResizeHandleType handleType) => handleType switch
        {
            ResizeHandleType.TopLeft or ResizeHandleType.BottomRight => Cursors.SizeNWSE,
            ResizeHandleType.TopRight or ResizeHandleType.BottomLeft => Cursors.SizeNESW,
            ResizeHandleType.Top or ResizeHandleType.Bottom => Cursors.SizeNS,
            ResizeHandleType.Left or ResizeHandleType.Right => Cursors.SizeWE,
            _ => Cursors.Arrow
        };

        #region Thumb Drag Event Handlers

        private void OnThumbDragStarted(ResizeHandleType handleType, DragStartedEventArgs e)
        {
            // Calculate start position in Canvas coordinate system for consistency
            var canvas = FindCanvasParent();
            if (canvas != null)
            {
                // Get the adorner's position relative to Canvas
                var adornerInCanvas = TranslatePoint(new Point(0, 0), canvas);
                var handleOffset = GetHandleCenter(handleType, AdornedElement.RenderSize);
                _dragStartPoint = new Point(
                    adornerInCanvas.X + handleOffset.X,
                    adornerInCanvas.Y + handleOffset.Y);
            }
            else
            {
                // Fallback to AdornedElement-relative coordinates
                var adornerPosition = TranslatePoint(new Point(0, 0), AdornedElement);
                var handlePosition = GetHandleCenter(handleType, AdornedElement.RenderSize);
                _dragStartPoint = new Point(
                    adornerPosition.X + handlePosition.X,
                    adornerPosition.Y + handlePosition.Y);
            }

            // Initialize cumulative tracking variables
            _initialDragStartPoint = _dragStartPoint;
            _cumulativeHorizontal = 0;
            _cumulativeVertical = 0;

            ResizeStarted?.Invoke(this, new ResizeEventArgs(handleType, _dragStartPoint));
        }

        private void OnThumbDragDelta(ResizeHandleType handleType, DragDeltaEventArgs e)
        {
            // Accumulate deltas separately to minimize floating-point error propagation
            // e.HorizontalChange and e.VerticalChange are incremental since last DragDelta
            _cumulativeHorizontal += e.HorizontalChange;
            _cumulativeVertical += e.VerticalChange;

            // Calculate current position from fixed initial point + accumulated deltas
            // This prevents error accumulation from repeated _dragStartPoint updates
            var currentPoint = new Point(
                _initialDragStartPoint.X + _cumulativeHorizontal,
                _initialDragStartPoint.Y + _cumulativeVertical);

            ResizeDelta?.Invoke(this, new ResizeEventArgs(handleType, currentPoint));
        }

        private void OnThumbDragCompleted(ResizeHandleType handleType, DragCompletedEventArgs e)
        {
            ResizeCompleted?.Invoke(this, new ResizeEventArgs(handleType, _dragStartPoint));
        }

        #endregion

        #region Layout

        /// <summary>
        /// Measures the adorner and its visual children.
        /// </summary>
        protected override Size MeasureOverride(Size constraint)
        {
            foreach (Thumb thumb in _handles.Values)
            {
                thumb.Measure(new Size(HandleSize, HandleSize));
            }

            return base.MeasureOverride(constraint);
        }

        /// <summary>
        /// Arranges the resize handle Thumbs at their positions.
        /// </summary>
        protected override Size ArrangeOverride(Size finalSize)
        {
            var renderSize = AdornedElement.RenderSize;

            foreach (var kvp in _handles)
            {
                var handleType = kvp.Key;
                var thumb = kvp.Value;
                var center = GetHandleCenter(handleType, renderSize);

                var handleRect = new Rect(
                    center.X - HandleSize / 2,
                    center.Y - HandleSize / 2,
                    HandleSize,
                    HandleSize);

                thumb.Arrange(handleRect);
            }

            return finalSize;
        }

        /// <summary>
        /// Gets the center point for a handle relative to the adorned element.
        /// Handles are positioned on the selection box which is 4px outside the element.
        /// </summary>
        private static Point GetHandleCenter(ResizeHandleType handleType, Size elementSize)
        {
            double padding = SelectionPadding;

            return handleType switch
            {
                // Corners - at selection box corners
                ResizeHandleType.TopLeft => new Point(-padding, -padding),
                ResizeHandleType.TopRight => new Point(elementSize.Width + padding, -padding),
                ResizeHandleType.BottomLeft => new Point(-padding, elementSize.Height + padding),
                ResizeHandleType.BottomRight => new Point(elementSize.Width + padding, elementSize.Height + padding),

                // Edges - at selection box edge centers
                ResizeHandleType.Top => new Point(elementSize.Width / 2, -padding),
                ResizeHandleType.Right => new Point(elementSize.Width + padding, elementSize.Height / 2),
                ResizeHandleType.Bottom => new Point(elementSize.Width / 2, elementSize.Height + padding),
                ResizeHandleType.Left => new Point(-padding, elementSize.Height / 2),

                _ => new Point(0, 0)
            };
        }

        #endregion

        #region Rendering

        /// <summary>
        /// Renders the selection box border.
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

            // Draw selection box border only (Thumbs draw themselves)
            drawingContext.DrawRectangle(null, SelectionPen, selectionRect);
        }

        #endregion

        #region Hit Testing (for move operations)

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
        /// Finds the parent Canvas in the visual tree for coordinate transformation.
        /// </summary>
        /// <returns>The parent Canvas if found, null otherwise.</returns>
        private Canvas FindCanvasParent()
        {
            DependencyObject current = AdornedElement;
            while (current != null)
            {
                if (current is Canvas canvas)
                    return canvas;
                current = VisualTreeHelper.GetParent(current);
            }
            return null;
        }

        #endregion

        #region Property Change Handling

        /// <summary>
        /// Handles property changes on the DrawingElement to invalidate the adorner.
        /// </summary>
        private void Element_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Invalidate layout when element properties change
            if (e.PropertyName == nameof(DrawingElement.Width) ||
                e.PropertyName == nameof(DrawingElement.Height))
            {
                InvalidateMeasure();
                InvalidateArrange();
                InvalidateVisual();
            }
        }

        #endregion

        #region Cleanup

        /// <summary>
        /// Cleans up resources when the adorner is no longer needed.
        /// </summary>
        public void Detach()
        {
            _element.PropertyChanged -= Element_PropertyChanged;

            // Clear event handlers from Thumbs
            _handles.Clear();
            _visualChildren.Clear();
        }

        #endregion
    }

    /// <summary>
    /// Event arguments for resize operations.
    /// </summary>
    public class ResizeEventArgs : EventArgs
    {
        public ResizeHandleType HandleType { get; }
        public Point Position { get; }

        public ResizeEventArgs(ResizeHandleType handleType, Point position)
        {
            HandleType = handleType;
            Position = position;
        }
    }
}