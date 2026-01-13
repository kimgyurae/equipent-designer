using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using EquipmentDesigner.Models;

namespace EquipmentDesigner.Views.Drawboard.Adorners
{
    /// <summary>
    /// Adorner that provides draggable head and tail thumbs for editing a selected connection.
    /// Attached to the ZoomableGrid (canvas-level) and positions thumbs at the connection endpoints.
    /// </summary>
    public class ConnectionEditAdorner : Adorner
    {
        #region Constants

        private const double ThumbSize = 14.0;
        private const double LineThickness = 2.0;
        private const double ArrowHeadLength = 10.0;
        private const double ArrowHeadWidth = 8.0;

        private static readonly Brush ThumbFillBrush;
        private static readonly Brush ThumbFillBrushSnapped;
        private static readonly Brush LineBrushSelected;
        private static readonly Pen ThumbPen;
        private static readonly Pen ThumbPenSnapped;

        #endregion

        #region Static Constructor

        static ConnectionEditAdorner()
        {
            // Normal state
            ThumbFillBrush = Brushes.White;

            // Selected/snapped state: blue (primary color)
            LineBrushSelected = new SolidColorBrush(Color.FromRgb(0x60, 0xA5, 0xFA)); // Primary.400
            LineBrushSelected.Freeze();

            ThumbFillBrushSnapped = new SolidColorBrush(Color.FromRgb(0xDB, 0xEA, 0xFE)); // Primary.100
            ThumbFillBrushSnapped.Freeze();

            var thumbBorderBrush = new SolidColorBrush(Color.FromRgb(0x33, 0x33, 0x33));
            thumbBorderBrush.Freeze();
            ThumbPen = new Pen(thumbBorderBrush, 1.5);
            ThumbPen.Freeze();

            ThumbPenSnapped = new Pen(LineBrushSelected, 2.0);
            ThumbPenSnapped.Freeze();
        }

        #endregion

        #region Fields

        private readonly VisualCollection _visuals;
        private readonly Thumb _headThumb;
        private readonly Thumb _tailThumb;

        private Point _headPosition;
        private Point _tailPosition;
        private bool _isHeadSnapped;
        private bool _isTailSnapped;
        private bool _isDraggingHead;
        private bool _isDraggingTail;
        private IReadOnlyList<Point> _routePoints;

        // Drag tracking fields - track the actual dragged position separately from visual position
        private Point _dragStartPosition;
        private Point _currentDragPosition;

        #endregion

        #region Events

        /// <summary>
        /// Raised when the head thumb drag starts.
        /// </summary>
        public event EventHandler<ConnectionThumbDragEventArgs> HeadDragStarted;

        /// <summary>
        /// Raised when the head thumb is being dragged.
        /// </summary>
        public event EventHandler<ConnectionThumbDragEventArgs> HeadDragDelta;

        /// <summary>
        /// Raised when the head thumb drag completes.
        /// </summary>
        public event EventHandler<ConnectionThumbDragEventArgs> HeadDragCompleted;

        /// <summary>
        /// Raised when the tail thumb drag starts.
        /// </summary>
        public event EventHandler<ConnectionThumbDragEventArgs> TailDragStarted;

        /// <summary>
        /// Raised when the tail thumb is being dragged.
        /// </summary>
        public event EventHandler<ConnectionThumbDragEventArgs> TailDragDelta;

        /// <summary>
        /// Raised when the tail thumb drag completes.
        /// </summary>
        public event EventHandler<ConnectionThumbDragEventArgs> TailDragCompleted;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new ConnectionEditAdorner.
        /// </summary>
        /// <param name="adornedElement">The canvas element (ZoomableGrid) to adorn.</param>
        /// <param name="headPosition">The initial head (arrow tip) position.</param>
        /// <param name="tailPosition">The initial tail (start) position.</param>
        /// <param name="routePoints">The initial route points for the connection line.</param>
        public ConnectionEditAdorner(
            UIElement adornedElement,
            Point headPosition,
            Point tailPosition,
            IReadOnlyList<Point> routePoints)
            : base(adornedElement)
        {
            _headPosition = headPosition;
            _tailPosition = tailPosition;
            _routePoints = routePoints ?? Array.Empty<Point>();

            Debug.WriteLine($"[ConnectionEditAdorner] === CONSTRUCTOR ===");
            Debug.WriteLine($"[ConnectionEditAdorner] HeadPosition: ({_headPosition.X:F1}, {_headPosition.Y:F1})");
            Debug.WriteLine($"[ConnectionEditAdorner] TailPosition: ({_tailPosition.X:F1}, {_tailPosition.Y:F1})");
            Debug.WriteLine($"[ConnectionEditAdorner] RoutePoints count: {_routePoints.Count}");
            for (int i = 0; i < _routePoints.Count; i++)
            {
                Debug.WriteLine($"[ConnectionEditAdorner]   RoutePoint[{i}]: ({_routePoints[i].X:F1}, {_routePoints[i].Y:F1})");
            }
            Debug.WriteLine($"[ConnectionEditAdorner] AdornedElement: {adornedElement?.GetType().Name}");

            _visuals = new VisualCollection(this);

            // Create head thumb (at arrow tip)
            _headThumb = CreateThumb();
            _headThumb.DragStarted += OnHeadThumbDragStarted;
            _headThumb.DragDelta += OnHeadThumbDragDelta;
            _headThumb.DragCompleted += OnHeadThumbDragCompleted;
            _visuals.Add(_headThumb);

            // Create tail thumb (at arrow start)
            _tailThumb = CreateThumb();
            _tailThumb.DragStarted += OnTailThumbDragStarted;
            _tailThumb.DragDelta += OnTailThumbDragDelta;
            _tailThumb.DragCompleted += OnTailThumbDragCompleted;
            _visuals.Add(_tailThumb);

            // Position thumbs
            UpdateThumbPositions();
            Debug.WriteLine($"[ConnectionEditAdorner] === END CONSTRUCTOR ===\n");
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Updates the head position and visual state.
        /// </summary>
        /// <param name="position">The new head position.</param>
        /// <param name="isSnapped">Whether the head is snapped to a target.</param>
        public void UpdateHeadPosition(Point position, bool isSnapped)
        {
            _headPosition = position;
            _isHeadSnapped = isSnapped;
            UpdateThumbPositions();
            InvalidateVisual();
        }

        /// <summary>
        /// Updates the tail position and visual state.
        /// </summary>
        /// <param name="position">The new tail position.</param>
        /// <param name="isSnapped">Whether the tail is snapped to a target.</param>
        public void UpdateTailPosition(Point position, bool isSnapped)
        {
            _tailPosition = position;
            _isTailSnapped = isSnapped;
            UpdateThumbPositions();
            InvalidateVisual();
        }

        /// <summary>
        /// Updates both positions from a connection line.
        /// </summary>
        /// <param name="headPosition">The head position.</param>
        /// <param name="tailPosition">The tail position.</param>
        /// <param name="routePoints">The route points for the line.</param>
        public void UpdatePositions(Point headPosition, Point tailPosition, IReadOnlyList<Point> routePoints)
        {
            _headPosition = headPosition;
            _tailPosition = tailPosition;
            _routePoints = routePoints ?? Array.Empty<Point>();
            _isHeadSnapped = true; // Connected positions are always "snapped"
            _isTailSnapped = true;
            UpdateThumbPositions();
            InvalidateVisual();
        }

        /// <summary>
        /// Updates the route points for visual preview during drag.
        /// </summary>
        /// <param name="routePoints">The new route points.</param>
        public void UpdateRoutePreview(IReadOnlyList<Point> routePoints)
        {
            _routePoints = routePoints ?? Array.Empty<Point>();
            
            // Update thumb positions to match new route endpoints
            // This is critical for the structural fix - thumbs must follow route endpoints
            UpdateThumbPositions();
            
            InvalidateVisual();
        }

        /// <summary>
        /// Detaches the adorner by unsubscribing from events.
        /// </summary>
        public void Detach()
        {
            _headThumb.DragStarted -= OnHeadThumbDragStarted;
            _headThumb.DragDelta -= OnHeadThumbDragDelta;
            _headThumb.DragCompleted -= OnHeadThumbDragCompleted;

            _tailThumb.DragStarted -= OnTailThumbDragStarted;
            _tailThumb.DragDelta -= OnTailThumbDragDelta;
            _tailThumb.DragCompleted -= OnTailThumbDragCompleted;
        }

        #endregion

        #region Thumb Creation

        /// <summary>
        /// Creates a circular thumb for endpoint manipulation.
        /// </summary>
        private Thumb CreateThumb()
        {
            var thumb = new Thumb
            {
                Width = ThumbSize,
                Height = ThumbSize,
                Cursor = Cursors.Hand,
                Template = CreateThumbTemplate()
            };

            return thumb;
        }

        /// <summary>
        /// Creates the visual template for the thumb.
        /// </summary>
        private static System.Windows.Controls.ControlTemplate CreateThumbTemplate()
        {
            var template = new System.Windows.Controls.ControlTemplate(typeof(Thumb));

            var ellipseFactory = new FrameworkElementFactory(typeof(System.Windows.Shapes.Ellipse));
            ellipseFactory.SetValue(System.Windows.Shapes.Ellipse.FillProperty, ThumbFillBrush);
            ellipseFactory.SetValue(System.Windows.Shapes.Ellipse.StrokeProperty, LineBrushSelected);
            ellipseFactory.SetValue(System.Windows.Shapes.Ellipse.StrokeThicknessProperty, 2.0);
            ellipseFactory.SetValue(System.Windows.Shapes.Ellipse.WidthProperty, ThumbSize);
            ellipseFactory.SetValue(System.Windows.Shapes.Ellipse.HeightProperty, ThumbSize);

            template.VisualTree = ellipseFactory;

            return template;
        }

        #endregion

        #region Thumb Event Handlers

        private void OnHeadThumbDragStarted(object sender, DragStartedEventArgs e)
        {
            _isDraggingHead = true;

            // Use actual mouse position instead of thumb center (_headPosition)
            // This prevents the "teleportation" effect when user clicks on the edge of the thumb
            // (not the exact center). DragDelta provides deltas from the mouse capture position,
            // so we must initialize _currentDragPosition to the actual mouse position.
            var actualMousePosition = Mouse.GetPosition(AdornedElement);

            Debug.WriteLine($"[ConnectionEditAdorner] === HEAD DRAG STARTED ===");
            Debug.WriteLine($"[ConnectionEditAdorner] _headPosition (thumb center): ({_headPosition.X:F1}, {_headPosition.Y:F1})");
            Debug.WriteLine($"[ConnectionEditAdorner] Mouse.GetPosition(AdornedElement): ({actualMousePosition.X:F1}, {actualMousePosition.Y:F1})");
            Debug.WriteLine($"[ConnectionEditAdorner] DragStartedEventArgs Offset: ({e.HorizontalOffset:F1}, {e.VerticalOffset:F1})");
            Debug.WriteLine($"[ConnectionEditAdorner] Difference (Mouse - HeadPos): ({actualMousePosition.X - _headPosition.X:F1}, {actualMousePosition.Y - _headPosition.Y:F1})");

            _dragStartPosition = actualMousePosition;
            _currentDragPosition = actualMousePosition;

            Debug.WriteLine($"[ConnectionEditAdorner] _currentDragPosition set to: ({_currentDragPosition.X:F1}, {_currentDragPosition.Y:F1})");
            Debug.WriteLine($"[ConnectionEditAdorner] === END HEAD DRAG STARTED ===\n");

            HeadDragStarted?.Invoke(this, new ConnectionThumbDragEventArgs(actualMousePosition));
            e.Handled = true;
        }

        private void OnHeadThumbDragDelta(object sender, DragDeltaEventArgs e)
        {
            var oldPosition = _currentDragPosition;

            // Calculate new position based on current drag position (not the visual position)
            // This ensures smooth dragging even when visual position is adjusted for routing
            _currentDragPosition = new Point(
                _currentDragPosition.X + e.HorizontalChange,
                _currentDragPosition.Y + e.VerticalChange);

            Debug.WriteLine($"[ConnectionEditAdorner] HEAD DRAG DELTA: delta=({e.HorizontalChange:F1}, {e.VerticalChange:F1}), old=({oldPosition.X:F1}, {oldPosition.Y:F1}), new=({_currentDragPosition.X:F1}, {_currentDragPosition.Y:F1})");

            HeadDragDelta?.Invoke(this, new ConnectionThumbDragEventArgs(_currentDragPosition));
            e.Handled = true;
        }

        private void OnHeadThumbDragCompleted(object sender, DragCompletedEventArgs e)
        {
            _isDraggingHead = false;
            HeadDragCompleted?.Invoke(this, new ConnectionThumbDragEventArgs(_headPosition));
            e.Handled = true;
        }

        private void OnTailThumbDragStarted(object sender, DragStartedEventArgs e)
        {
            _isDraggingTail = true;

            // Use actual mouse position instead of thumb center (_tailPosition)
            // This prevents the "teleportation" effect when user clicks on the edge of the thumb
            // (not the exact center). DragDelta provides deltas from the mouse capture position,
            // so we must initialize _currentDragPosition to the actual mouse position.
            var actualMousePosition = Mouse.GetPosition(AdornedElement);

            Debug.WriteLine($"[ConnectionEditAdorner] === TAIL DRAG STARTED ===");
            Debug.WriteLine($"[ConnectionEditAdorner] _tailPosition (thumb center): ({_tailPosition.X:F1}, {_tailPosition.Y:F1})");
            Debug.WriteLine($"[ConnectionEditAdorner] Mouse.GetPosition(AdornedElement): ({actualMousePosition.X:F1}, {actualMousePosition.Y:F1})");
            Debug.WriteLine($"[ConnectionEditAdorner] DragStartedEventArgs Offset: ({e.HorizontalOffset:F1}, {e.VerticalOffset:F1})");
            Debug.WriteLine($"[ConnectionEditAdorner] Difference (Mouse - TailPos): ({actualMousePosition.X - _tailPosition.X:F1}, {actualMousePosition.Y - _tailPosition.Y:F1})");

            _dragStartPosition = actualMousePosition;
            _currentDragPosition = actualMousePosition;

            Debug.WriteLine($"[ConnectionEditAdorner] _currentDragPosition set to: ({_currentDragPosition.X:F1}, {_currentDragPosition.Y:F1})");
            Debug.WriteLine($"[ConnectionEditAdorner] === END TAIL DRAG STARTED ===\n");

            TailDragStarted?.Invoke(this, new ConnectionThumbDragEventArgs(actualMousePosition));
            e.Handled = true;
        }

        private void OnTailThumbDragDelta(object sender, DragDeltaEventArgs e)
        {
            var oldPosition = _currentDragPosition;

            // Calculate new position based on current drag position (not the visual position)
            // This ensures smooth dragging even when visual position is adjusted for routing
            _currentDragPosition = new Point(
                _currentDragPosition.X + e.HorizontalChange,
                _currentDragPosition.Y + e.VerticalChange);

            Debug.WriteLine($"[ConnectionEditAdorner] TAIL DRAG DELTA: delta=({e.HorizontalChange:F1}, {e.VerticalChange:F1}), old=({oldPosition.X:F1}, {oldPosition.Y:F1}), new=({_currentDragPosition.X:F1}, {_currentDragPosition.Y:F1})");

            TailDragDelta?.Invoke(this, new ConnectionThumbDragEventArgs(_currentDragPosition));
            e.Handled = true;
        }

        private void OnTailThumbDragCompleted(object sender, DragCompletedEventArgs e)
        {
            _isDraggingTail = false;
            TailDragCompleted?.Invoke(this, new ConnectionThumbDragEventArgs(_tailPosition));
            e.Handled = true;
        }

        #endregion

        #region Visual Management

        /// <summary>
        /// Updates the thumb positions on the canvas.
        /// </summary>
        private void UpdateThumbPositions()
        {
            // STRUCTURAL FIX: Derive positions from route endpoints (single source of truth)
            // This ensures thumb CANNOT separate from arrow by design.
            // Route endpoints ARE the arrow endpoints - using them directly guarantees alignment.

            Point headVisualPosition;
            Point tailVisualPosition;

            if (_routePoints != null && _routePoints.Count >= 2)
            {
                // Route endpoints ARE the arrow endpoints - use them directly
                tailVisualPosition = _routePoints[0];                      // Arrow start (tail)
                headVisualPosition = _routePoints[_routePoints.Count - 1]; // Arrow end (head)
            }
            else
            {
                // Fallback when no route available (initial state or edge case)
                headVisualPosition = _headPosition;
                tailVisualPosition = _tailPosition;
            }

            Debug.WriteLine($"[ConnectionEditAdorner] UpdateThumbPositions called:");
            Debug.WriteLine($"[ConnectionEditAdorner]   _isDraggingHead={_isDraggingHead}, _isHeadSnapped={_isHeadSnapped}");
            Debug.WriteLine($"[ConnectionEditAdorner]   _headPosition: ({_headPosition.X:F1}, {_headPosition.Y:F1})");
            Debug.WriteLine($"[ConnectionEditAdorner]   _currentDragPosition: ({_currentDragPosition.X:F1}, {_currentDragPosition.Y:F1})");
            Debug.WriteLine($"[ConnectionEditAdorner]   RoutePoints count: {_routePoints?.Count ?? 0}");
            Debug.WriteLine($"[ConnectionEditAdorner]   HeadThumb visual position: ({headVisualPosition.X:F1}, {headVisualPosition.Y:F1})");
            Debug.WriteLine($"[ConnectionEditAdorner]   TailThumb visual position: ({tailVisualPosition.X:F1}, {tailVisualPosition.Y:F1})");

            // Position head thumb centered on visual position
            _headThumb.SetValue(System.Windows.Controls.Canvas.LeftProperty, headVisualPosition.X - ThumbSize / 2);
            _headThumb.SetValue(System.Windows.Controls.Canvas.TopProperty, headVisualPosition.Y - ThumbSize / 2);

            // Position tail thumb centered on visual position
            _tailThumb.SetValue(System.Windows.Controls.Canvas.LeftProperty, tailVisualPosition.X - ThumbSize / 2);
            _tailThumb.SetValue(System.Windows.Controls.Canvas.TopProperty, tailVisualPosition.Y - ThumbSize / 2);
        }

        /// <summary>
        /// Renders the connection line (during drag, shows preview).
        /// </summary>
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            // Draw the connection line if we have route points
            if (_routePoints != null && _routePoints.Count >= 2)
            {
                var pen = new Pen(LineBrushSelected, LineThickness);

                for (int i = 0; i < _routePoints.Count - 1; i++)
                {
                    drawingContext.DrawLine(pen, _routePoints[i], _routePoints[i + 1]);
                }

                // Draw arrow head at the end point
                DrawArrowHead(drawingContext);
            }

            // Draw visual indicators for snap state on thumbs
            DrawThumbIndicator(drawingContext, _headPosition, _isHeadSnapped || !_isDraggingHead);
            DrawThumbIndicator(drawingContext, _tailPosition, _isTailSnapped || !_isDraggingTail);
        }

        /// <summary>
        /// Draws an arrow head at the end of the connection line.
        /// </summary>
        private void DrawArrowHead(DrawingContext dc)
        {
            if (_routePoints == null || _routePoints.Count < 2) return;

            var endPoint = _routePoints[_routePoints.Count - 1];
            var previousPoint = _routePoints[_routePoints.Count - 2];

            // Calculate direction
            var dx = endPoint.X - previousPoint.X;
            var dy = endPoint.Y - previousPoint.Y;
            var length = Math.Sqrt(dx * dx + dy * dy);

            if (length < 0.1) return;

            // Normalize direction
            dx /= length;
            dy /= length;

            // Calculate perpendicular
            var px = -dy;
            var py = dx;

            // Arrow head points (triangle)
            var tip = endPoint;
            var baseLeft = new Point(
                endPoint.X - ArrowHeadLength * dx + ArrowHeadWidth / 2 * px,
                endPoint.Y - ArrowHeadLength * dy + ArrowHeadWidth / 2 * py);
            var baseRight = new Point(
                endPoint.X - ArrowHeadLength * dx - ArrowHeadWidth / 2 * px,
                endPoint.Y - ArrowHeadLength * dy - ArrowHeadWidth / 2 * py);

            // Create and draw the triangle
            var geometry = new StreamGeometry();
            using (var ctx = geometry.Open())
            {
                ctx.BeginFigure(tip, true, true);
                ctx.LineTo(baseLeft, true, false);
                ctx.LineTo(baseRight, true, false);
            }
            geometry.Freeze();

            dc.DrawGeometry(LineBrushSelected, null, geometry);
        }

        /// <summary>
        /// Draws a visual indicator around the thumb position.
        /// </summary>
        private void DrawThumbIndicator(DrawingContext dc, Point position, bool isSnapped)
        {
            if (isSnapped)
            {
                // Draw a small dot in the center when snapped
                dc.DrawEllipse(
                    LineBrushSelected,
                    null,
                    position,
                    3,
                    3);
            }
        }

        #endregion

        #region Visual Tree Overrides

        /// <summary>
        /// Gets the number of visual children.
        /// </summary>
        protected override int VisualChildrenCount => _visuals.Count;

        /// <summary>
        /// Gets a visual child at the specified index.
        /// </summary>
        protected override Visual GetVisualChild(int index) => _visuals[index];

        /// <summary>
        /// Measures the adorner.
        /// </summary>
        protected override Size MeasureOverride(Size constraint)
        {
            _headThumb.Measure(constraint);
            _tailThumb.Measure(constraint);
            return AdornedElement.RenderSize;
        }

        /// <summary>
        /// Arranges the adorner and its child thumbs.
        /// </summary>
        protected override Size ArrangeOverride(Size finalSize)
        {
            // Use same route-based positioning logic as UpdateThumbPositions for consistency
            // Route endpoints ARE the arrow endpoints - using them directly guarantees alignment.
            Point headVisualPosition;
            Point tailVisualPosition;

            if (_routePoints != null && _routePoints.Count >= 2)
            {
                // Route endpoints ARE the arrow endpoints - use them directly
                tailVisualPosition = _routePoints[0];                      // Arrow start (tail)
                headVisualPosition = _routePoints[_routePoints.Count - 1]; // Arrow end (head)
            }
            else
            {
                // Fallback when no route available (initial state or edge case)
                headVisualPosition = _headPosition;
                tailVisualPosition = _tailPosition;
            }

            // Arrange head thumb
            _headThumb.Arrange(new Rect(
                headVisualPosition.X - ThumbSize / 2,
                headVisualPosition.Y - ThumbSize / 2,
                ThumbSize,
                ThumbSize));

            // Arrange tail thumb
            _tailThumb.Arrange(new Rect(
                tailVisualPosition.X - ThumbSize / 2,
                tailVisualPosition.Y - ThumbSize / 2,
                ThumbSize,
                ThumbSize));

            return finalSize;
        }

        #endregion
    }

    #region Event Args

    /// <summary>
    /// Event arguments for connection thumb drag events.
    /// </summary>
    public class ConnectionThumbDragEventArgs : EventArgs
    {
        /// <summary>
        /// The current position of the thumb.
        /// </summary>
        public Point Position { get; }

        /// <summary>
        /// Creates new drag event arguments.
        /// </summary>
        /// <param name="position">The thumb position.</param>
        public ConnectionThumbDragEventArgs(Point position)
        {
            Position = position;
        }
    }

    #endregion
}