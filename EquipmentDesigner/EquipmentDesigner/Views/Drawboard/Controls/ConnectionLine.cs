using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using EquipmentDesigner.Models;
using EquipmentDesigner.Views.Drawboard.UMLEngine;

namespace EquipmentDesigner.Views.Drawboard.Controls
{
    /// <summary>
    /// A custom control that renders a connection line between two UML elements.
    /// Uses orthogonal routing with 90-degree turns and displays an optional label at midpoint.
    /// Supports left-click selection for editing connection endpoints.
    /// Updated for adjacency list pattern where source element owns the connection.
    /// </summary>
    public class ConnectionLine : Canvas
    {
        #region Constants

        private const double LineThickness = 2.0;
        private const double ArrowHeadLength = 10.0;
        private const double ArrowHeadWidth = 8.0;
        private const double LabelPadding = 4.0;
        private const double HitTestTolerance = 20.0; // Increased hit-test area for better usability

        private static readonly Brush LineBrush;
        private static readonly Brush LineBrushSelected;
        private static readonly Pen LinePen;
        private static readonly Pen LinePenSelected;

        #endregion

        #region Static Constructor

        static ConnectionLine()
        {
            LineBrush = new SolidColorBrush(Color.FromRgb(0x33, 0x33, 0x33));
            LineBrush.Freeze();
            LinePen = new Pen(LineBrush, LineThickness);
            LinePen.Freeze();

            // Selected state: blue line (primary color)
            LineBrushSelected = new SolidColorBrush(Color.FromRgb(0x60, 0xA5, 0xFA)); // Primary.400
            LineBrushSelected.Freeze();
            LinePenSelected = new Pen(LineBrushSelected, LineThickness);
            LinePenSelected.Freeze();
        }

        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty TailIdProperty =
            DependencyProperty.Register(
                nameof(TailId),
                typeof(string),
                typeof(ConnectionLine),
                new PropertyMetadata(null, OnConnectionPropertyChanged));

        public static readonly DependencyProperty HeadIdProperty =
            DependencyProperty.Register(
                nameof(HeadId),
                typeof(string),
                typeof(ConnectionLine),
                new PropertyMetadata(null, OnConnectionPropertyChanged));

        public static readonly DependencyProperty TailPortProperty =
            DependencyProperty.Register(
                nameof(TailPort),
                typeof(PortPosition),
                typeof(ConnectionLine),
                new PropertyMetadata(PortPosition.Right, OnConnectionPropertyChanged));

        public static readonly DependencyProperty HeadPortProperty =
            DependencyProperty.Register(
                nameof(HeadPort),
                typeof(PortPosition),
                typeof(ConnectionLine),
                new PropertyMetadata(PortPosition.Left, OnConnectionPropertyChanged));

        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register(
                nameof(Label),
                typeof(string),
                typeof(ConnectionLine),
                new PropertyMetadata(string.Empty, OnLabelPropertyChanged));

        public static readonly DependencyProperty ElementsProperty =
            DependencyProperty.Register(
                nameof(Elements),
                typeof(ObservableCollection<DrawingElement>),
                typeof(ConnectionLine),
                new PropertyMetadata(null, OnElementsPropertyChanged));

        public static readonly DependencyProperty ConnectionProperty =
            DependencyProperty.Register(
                nameof(Connection),
                typeof(UMLConnection2),
                typeof(ConnectionLine),
                new PropertyMetadata(null));

        public static readonly DependencyProperty SourceElementProperty =
            DependencyProperty.Register(
                nameof(SourceElement),
                typeof(DrawingElement),
                typeof(ConnectionLine),
                new PropertyMetadata(null, OnSourceElementPropertyChanged));

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register(
                nameof(IsSelected),
                typeof(bool),
                typeof(ConnectionLine),
                new PropertyMetadata(false, OnIsSelectedPropertyChanged));

        public static readonly DependencyProperty IsEditingProperty =
            DependencyProperty.Register(
                nameof(IsEditing),
                typeof(bool),
                typeof(ConnectionLine),
                new PropertyMetadata(false, OnIsEditingPropertyChanged));

        #endregion

        #region Routed Events

        /// <summary>
        /// Event raised when this connection line is selected via left-click.
        /// </summary>
        public static readonly RoutedEvent ConnectionSelectedEvent =
            EventManager.RegisterRoutedEvent(
                "ConnectionSelected",
                RoutingStrategy.Bubble,
                typeof(RoutedEventHandler),
                typeof(ConnectionLine));

        /// <summary>
        /// Occurs when this connection line is selected.
        /// </summary>
        public event RoutedEventHandler ConnectionSelectedChanged
        {
            add => AddHandler(ConnectionSelectedEvent, value);
            remove => RemoveHandler(ConnectionSelectedEvent, value);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the ID of the source (tail) element.
        /// </summary>
        public string TailId
        {
            get => (string)GetValue(TailIdProperty);
            set => SetValue(TailIdProperty, value);
        }

        /// <summary>
        /// Gets or sets the ID of the target (head) element.
        /// </summary>
        public string HeadId
        {
            get => (string)GetValue(HeadIdProperty);
            set => SetValue(HeadIdProperty, value);
        }

        /// <summary>
        /// Gets or sets the port position on the source element.
        /// </summary>
        public PortPosition TailPort
        {
            get => (PortPosition)GetValue(TailPortProperty);
            set => SetValue(TailPortProperty, value);
        }

        /// <summary>
        /// Gets or sets the port position on the target element.
        /// </summary>
        public PortPosition HeadPort
        {
            get => (PortPosition)GetValue(HeadPortProperty);
            set => SetValue(HeadPortProperty, value);
        }

        /// <summary>
        /// Gets or sets the label displayed at the midpoint of the connection.
        /// </summary>
        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        /// <summary>
        /// Gets or sets the collection of drawing elements (for position tracking).
        /// </summary>
        public ObservableCollection<DrawingElement> Elements
        {
            get => (ObservableCollection<DrawingElement>)GetValue(ElementsProperty);
            set => SetValue(ElementsProperty, value);
        }

        /// <summary>
        /// Gets or sets the UMLConnection2 data model this line represents.
        /// </summary>
        public UMLConnection2 Connection
        {
            get => (UMLConnection2)GetValue(ConnectionProperty);
            set => SetValue(ConnectionProperty, value);
        }

        /// <summary>
        /// Gets or sets the source element that owns this connection.
        /// Used to get TailId in adjacency list pattern where UMLConnection2 only stores TargetId.
        /// </summary>
        public DrawingElement SourceElement
        {
            get => (DrawingElement)GetValue(SourceElementProperty);
            set => SetValue(SourceElementProperty, value);
        }

        /// <summary>
        /// Gets or sets whether this connection line is selected.
        /// </summary>
        public bool IsSelected
        {
            get => (bool)GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }

        /// <summary>
        /// Gets or sets whether this connection line is being edited (dragging endpoint).
        /// When true, the visual elements are hidden so only the adorner preview is visible.
        /// </summary>
        public bool IsEditing
        {
            get => (bool)GetValue(IsEditingProperty);
            set => SetValue(IsEditingProperty, value);
        }

        /// <summary>
        /// Gets the current route points for the connection line.
        /// Used by ConnectionEditAdorner to position edit handles.
        /// </summary>
        public IReadOnlyList<Point> RoutePoints => _routePoints;

        /// <summary>
        /// Gets the head (arrow tip) position of the connection.
        /// </summary>
        public Point HeadPosition => _routePoints.Count > 0 ? _routePoints[_routePoints.Count - 1] : new Point();

        /// <summary>
        /// Gets the tail (start) position of the connection.
        /// </summary>
        public Point TailPosition => _routePoints.Count > 0 ? _routePoints[0] : new Point();

        #endregion

        #region Fields

        private readonly Path _linePath;
        private readonly Path _hitTestPath;  // Invisible wide path for hit testing
        private readonly Path _arrowPath;
        private readonly Border _labelBorder;
        private readonly TextBlock _labelText;
        private DrawingElement _tailElement;
        private DrawingElement _headElement;
        private List<Point> _routePoints = new List<Point>();

        #endregion

        #region Constructor

        public ConnectionLine()
        {
            // Create invisible hit-test path (wider than visible line for easier clicking)
            _hitTestPath = new Path
            {
                Stroke = Brushes.Transparent,
                StrokeThickness = HitTestTolerance * 2,  // Wide transparent stroke for hit testing
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                StrokeLineJoin = PenLineJoin.Round,
                IsHitTestVisible = true
            };
            Children.Add(_hitTestPath);

            // Create the visible line path
            _linePath = new Path
            {
                Stroke = LineBrush,
                StrokeThickness = LineThickness,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                StrokeLineJoin = PenLineJoin.Round,
                IsHitTestVisible = false  // Don't use for hit testing
            };
            Children.Add(_linePath);

            // Create the arrow head path
            _arrowPath = new Path
            {
                Fill = LineBrush,
                Stroke = null,
                IsHitTestVisible = false  // Don't use for hit testing
            };
            Children.Add(_arrowPath);

            // Create the label
            _labelText = new TextBlock
            {
                FontSize = 11,
                Foreground = LineBrush,
                TextAlignment = System.Windows.TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            _labelBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(230, 255, 255, 255)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(0xCC, 0xCC, 0xCC)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(2),
                Padding = new Thickness(LabelPadding, 2, LabelPadding, 2),
                Child = _labelText,
                Visibility = Visibility.Collapsed
            };
            Children.Add(_labelBorder);

            // Enable hit testing for selection
            IsHitTestVisible = true;
            Background = Brushes.Transparent; // Required for hit-test on empty areas

            // Wire up mouse events for selection
            MouseLeftButtonDown += OnMouseLeftButtonDown;
        }

        #endregion

        #region Mouse Event Handlers

        /// <summary>
        /// Handles left mouse button down to select this connection.
        /// </summary>
        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // _routePoints are stored in absolute ZoomableGrid coordinates
            // We need to get the click point directly in ZoomableGrid space
            
            Point clickPoint;
            
            // Find the ZoomableGrid by walking up the visual tree
            var zoomableGrid = FindZoomableGrid() as IInputElement;
            
            if (zoomableGrid != null)
            {
                // Get click position directly relative to ZoomableGrid
                // This avoids coordinate transformation issues in nested ItemsControl structure
                clickPoint = e.GetPosition(zoomableGrid);
                
                Debug.WriteLine($"[ConnectionLine.OnMouseLeftButtonDown] Using direct GetPosition(ZoomableGrid)");
                Debug.WriteLine($"[ConnectionLine.OnMouseLeftButtonDown]   clickPoint: ({clickPoint.X:F1}, {clickPoint.Y:F1})");
            }
            else
            {
                // Fallback: get position relative to this (may not work correctly)
                clickPoint = e.GetPosition(this);
                Debug.WriteLine($"[ConnectionLine.OnMouseLeftButtonDown] WARNING: ZoomableGrid not found, using fallback");
                Debug.WriteLine($"[ConnectionLine.OnMouseLeftButtonDown]   clickPoint: ({clickPoint.X:F1}, {clickPoint.Y:F1})");
            }
            
            Debug.WriteLine($"[ConnectionLine.OnMouseLeftButtonDown]   _routePoints.Count: {_routePoints.Count}");
            for (int i = 0; i < _routePoints.Count; i++)
            {
                Debug.WriteLine($"[ConnectionLine.OnMouseLeftButtonDown]     RoutePoint[{i}]: ({_routePoints[i].X:F1}, {_routePoints[i].Y:F1})");
            }
            
            if (IsPointNearLine(clickPoint))
            {
                Debug.WriteLine($"[ConnectionLine.OnMouseLeftButtonDown]   HIT! Raising selection event");
                // Raise the selection event
                RaiseEvent(new RoutedEventArgs(ConnectionSelectedEvent, this));
                e.Handled = true;
            }
            else
            {
                Debug.WriteLine($"[ConnectionLine.OnMouseLeftButtonDown]   MISS - point not near any segment");
                // Log distances to each segment for debugging
                for (int i = 0; i < _routePoints.Count - 1; i++)
                {
                    var distance = DistanceToLineSegment(clickPoint, _routePoints[i], _routePoints[i + 1]);
                    Debug.WriteLine($"[ConnectionLine.OnMouseLeftButtonDown]     Distance to segment[{i}]: {distance:F1} (tolerance: {HitTestTolerance + LineThickness / 2})");
                }
            }
        }

        /// <summary>
        /// Finds the ZoomableGrid ancestor by walking up the visual tree.
        /// </summary>
        private Visual FindZoomableGrid()
        {
            DependencyObject current = this;
            
            while (current != null)
            {
                // Check for Grid with Name="ZoomableGrid"
                if (current is FrameworkElement fe && fe.Name == "ZoomableGrid")
                {
                    return fe as Visual;
                }
                
                current = VisualTreeHelper.GetParent(current);
            }
            
            return null;
        }

        /// <summary>
        /// Determines if a point is near the connection line path.
        /// </summary>
        private bool IsPointNearLine(Point point)
        {
            if (_routePoints.Count < 2) return false;

            // Check distance to each line segment
            for (int i = 0; i < _routePoints.Count - 1; i++)
            {
                var distance = DistanceToLineSegment(point, _routePoints[i], _routePoints[i + 1]);
                if (distance <= HitTestTolerance + LineThickness / 2)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Calculates the distance from a point to a line segment.
        /// </summary>
        private static double DistanceToLineSegment(Point point, Point lineStart, Point lineEnd)
        {
            var dx = lineEnd.X - lineStart.X;
            var dy = lineEnd.Y - lineStart.Y;
            var lengthSquared = dx * dx + dy * dy;

            if (lengthSquared < 0.001)
            {
                // Line segment is essentially a point
                return Math.Sqrt(Math.Pow(point.X - lineStart.X, 2) + Math.Pow(point.Y - lineStart.Y, 2));
            }

            // Project point onto line segment
            var t = Math.Max(0, Math.Min(1, ((point.X - lineStart.X) * dx + (point.Y - lineStart.Y) * dy) / lengthSquared));
            var projectionX = lineStart.X + t * dx;
            var projectionY = lineStart.Y + t * dy;

            return Math.Sqrt(Math.Pow(point.X - projectionX, 2) + Math.Pow(point.Y - projectionY, 2));
        }

        #endregion

        #region Property Changed Handlers

        private static void OnConnectionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ConnectionLine control)
            {
                control.ResolveElements();
                control.UpdateRoute();
            }
        }

        private static void OnLabelPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ConnectionLine control)
            {
                control.UpdateLabelVisibility();
                control.UpdateLabelPosition();
            }
        }

        private static void OnIsSelectedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ConnectionLine control)
            {
                control.UpdateVisualStyle();
            }
        }

        private static void OnIsEditingPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ConnectionLine control)
            {
                control.UpdateEditingVisibility();
            }
        }

        private static void OnElementsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ConnectionLine control)
            {
                // Unsubscribe from old collection
                if (e.OldValue is ObservableCollection<DrawingElement> oldCollection)
                {
                    oldCollection.CollectionChanged -= control.OnElementsCollectionChanged;
                    control.UnsubscribeFromElementChanges(oldCollection);
                }

                // Subscribe to new collection
                if (e.NewValue is ObservableCollection<DrawingElement> newCollection)
                {
                    newCollection.CollectionChanged += control.OnElementsCollectionChanged;
                    control.SubscribeToElementChanges(newCollection);
                }

                control.ResolveElements();
                control.UpdateRoute();
            }
        }

        private void OnElementsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Handle added elements
            if (e.NewItems != null)
            {
                foreach (DrawingElement element in e.NewItems)
                {
                    element.PropertyChanged += OnElementPropertyChanged;
                }
            }

            // Handle removed elements
            if (e.OldItems != null)
            {
                foreach (DrawingElement element in e.OldItems)
                {
                    element.PropertyChanged -= OnElementPropertyChanged;
                }
            }

            ResolveElements();
            UpdateRoute();
        }

        private void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Update route when element position or size changes
            if (e.PropertyName == nameof(DrawingElement.X) ||
                e.PropertyName == nameof(DrawingElement.Y) ||
                e.PropertyName == nameof(DrawingElement.Width) ||
                e.PropertyName == nameof(DrawingElement.Height))
            {
                var element = sender as DrawingElement;
                if (element != null && (element == _tailElement || element == _headElement))
                {
                    UpdateRoute();
                }
            }
        }

        private void SubscribeToElementChanges(IEnumerable<DrawingElement> elements)
        {
            foreach (var element in elements)
            {
                element.PropertyChanged += OnElementPropertyChanged;
            }
        }

        private void UnsubscribeFromElementChanges(IEnumerable<DrawingElement> elements)
        {
            foreach (var element in elements)
            {
                element.PropertyChanged -= OnElementPropertyChanged;
            }
        }

        private static void OnSourceElementPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ConnectionLine control)
            {
                // When SourceElement changes, update TailId from it
                if (e.NewValue is DrawingElement sourceElement)
                {
                    control.TailId = sourceElement.Id;
                }
                control.ResolveElements();
                control.UpdateRoute();
            }
        }

        #endregion

        #region Element Resolution

        private void ResolveElements()
        {
            _tailElement = null;
            _headElement = null;

            if (Elements == null) return;

            if (!string.IsNullOrEmpty(TailId))
            {
                _tailElement = Elements.FirstOrDefault(e => e.Id == TailId);
            }

            if (!string.IsNullOrEmpty(HeadId))
            {
                _headElement = Elements.FirstOrDefault(e => e.Id == HeadId);
            }
        }

        #endregion

        #region Route Calculation

        private void UpdateRoute()
        {
            if (_tailElement == null || _headElement == null)
            {
                _linePath.Data = null;
                _arrowPath.Data = null;
                _labelBorder.Visibility = Visibility.Collapsed;
                return;
            }

            // Calculate edge positions (where arrows attach directly to element boundaries)
            var tailBounds = _tailElement.Bounds;
            var headBounds = _headElement.Bounds;

            var startPoint = ConnectionRoutingEngine.CalculateEdgePosition(tailBounds, TailPort);
            var endPoint = ConnectionRoutingEngine.CalculateEdgePosition(headBounds, HeadPort);

            // Calculate orthogonal route
            _routePoints = CalculateRoute(startPoint, TailPort, endPoint, HeadPort);

            // Update the path geometry
            UpdatePathGeometry();
            UpdateArrowHead();
            UpdateLabelPosition();
        }

        private List<Point> CalculateRoute(Point start, PortPosition startPort, Point end, PortPosition endPort)
        {
            var points = new List<Point> { start };

            // Simple L-shaped or Z-shaped routing based on port directions
            var dx = end.X - start.X;
            var dy = end.Y - start.Y;

            bool isHorizontalStart = startPort == PortPosition.Left || startPort == PortPosition.Right;
            bool isHorizontalEnd = endPort == PortPosition.Left || endPort == PortPosition.Right;

            if (isHorizontalStart && isHorizontalEnd)
            {
                // Both horizontal - use vertical midpoint
                var midX = start.X + dx / 2;
                points.Add(new Point(midX, start.Y));
                points.Add(new Point(midX, end.Y));
            }
            else if (!isHorizontalStart && !isHorizontalEnd)
            {
                // Both vertical - use horizontal midpoint
                var midY = start.Y + dy / 2;
                points.Add(new Point(start.X, midY));
                points.Add(new Point(end.X, midY));
            }
            else if (isHorizontalStart)
            {
                // Start horizontal, end vertical - L-shape
                points.Add(new Point(end.X, start.Y));
            }
            else
            {
                // Start vertical, end horizontal - L-shape
                points.Add(new Point(start.X, end.Y));
            }

            points.Add(end);

            return RemoveDuplicatePoints(points);
        }

        private List<Point> RemoveDuplicatePoints(List<Point> points)
        {
            if (points.Count <= 1) return points;

            var result = new List<Point> { points[0] };

            for (int i = 1; i < points.Count; i++)
            {
                var dx = points[i].X - result[result.Count - 1].X;
                var dy = points[i].Y - result[result.Count - 1].Y;
                if (Math.Sqrt(dx * dx + dy * dy) > 0.5)
                {
                    result.Add(points[i]);
                }
            }

            return result;
        }

        private void UpdatePathGeometry()
        {
            if (_routePoints.Count < 2)
            {
                _linePath.Data = null;
                _hitTestPath.Data = null;
                return;
            }

            var geometry = new PathGeometry();
            var figure = new PathFigure { StartPoint = _routePoints[0] };

            for (int i = 1; i < _routePoints.Count; i++)
            {
                figure.Segments.Add(new LineSegment(_routePoints[i], true));
            }

            geometry.Figures.Add(figure);
            _linePath.Data = geometry;
            
            // Use same geometry for hit-test path (it has wider stroke)
            _hitTestPath.Data = geometry.Clone();
        }

        private void UpdateArrowHead()
        {
            if (_routePoints.Count < 2)
            {
                _arrowPath.Data = null;
                return;
            }

            var endPoint = _routePoints[_routePoints.Count - 1];
            var previousPoint = _routePoints[_routePoints.Count - 2];

            // Calculate direction
            var dx = endPoint.X - previousPoint.X;
            var dy = endPoint.Y - previousPoint.Y;
            var length = Math.Sqrt(dx * dx + dy * dy);

            if (length < 0.1)
            {
                _arrowPath.Data = null;
                return;
            }

            // Normalize direction
            dx /= length;
            dy /= length;

            // Calculate perpendicular
            var px = -dy;
            var py = dx;

            // Arrow head points
            var tip = endPoint;
            var baseLeft = new Point(
                endPoint.X - ArrowHeadLength * dx + ArrowHeadWidth / 2 * px,
                endPoint.Y - ArrowHeadLength * dy + ArrowHeadWidth / 2 * py);
            var baseRight = new Point(
                endPoint.X - ArrowHeadLength * dx - ArrowHeadWidth / 2 * px,
                endPoint.Y - ArrowHeadLength * dy - ArrowHeadWidth / 2 * py);

            // Create triangle geometry
            var geometry = new PathGeometry();
            var figure = new PathFigure
            {
                StartPoint = tip,
                IsClosed = true,
                IsFilled = true
            };
            figure.Segments.Add(new LineSegment(baseLeft, true));
            figure.Segments.Add(new LineSegment(baseRight, true));
            geometry.Figures.Add(figure);

            _arrowPath.Data = geometry;
        }

        #endregion

        #region Label Management

        private void UpdateLabelVisibility()
        {
            _labelText.Text = Label ?? string.Empty;
            _labelBorder.Visibility = string.IsNullOrEmpty(Label)
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        private void UpdateLabelPosition()
        {
            if (string.IsNullOrEmpty(Label) || _routePoints.Count < 2)
            {
                _labelBorder.Visibility = Visibility.Collapsed;
                return;
            }

            // Calculate the midpoint of the route
            var midpoint = CalculateRouteMidpoint();

            // Update label text and measure
            _labelText.Text = Label;
            _labelBorder.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            var labelSize = _labelBorder.DesiredSize;

            // Position label centered on midpoint
            SetLeft(_labelBorder, midpoint.X - labelSize.Width / 2);
            SetTop(_labelBorder, midpoint.Y - labelSize.Height / 2);

            _labelBorder.Visibility = Visibility.Visible;
        }

        private Point CalculateRouteMidpoint()
        {
            if (_routePoints.Count < 2)
            {
                return new Point(0, 0);
            }

            // Calculate total route length
            double totalLength = 0;
            var segmentLengths = new List<double>();

            for (int i = 0; i < _routePoints.Count - 1; i++)
            {
                var dx = _routePoints[i + 1].X - _routePoints[i].X;
                var dy = _routePoints[i + 1].Y - _routePoints[i].Y;
                var length = Math.Sqrt(dx * dx + dy * dy);
                segmentLengths.Add(length);
                totalLength += length;
            }

            // Find the midpoint
            double halfLength = totalLength / 2;
            double currentLength = 0;

            for (int i = 0; i < segmentLengths.Count; i++)
            {
                if (currentLength + segmentLengths[i] >= halfLength)
                {
                    // Midpoint is in this segment
                    double remainingLength = halfLength - currentLength;
                    double t = remainingLength / segmentLengths[i];

                    return new Point(
                        _routePoints[i].X + t * (_routePoints[i + 1].X - _routePoints[i].X),
                        _routePoints[i].Y + t * (_routePoints[i + 1].Y - _routePoints[i].Y));
                }

                currentLength += segmentLengths[i];
            }

            // Fallback to last point
            return _routePoints[_routePoints.Count - 1];
        }

        #endregion

        #region Visual Style

        /// <summary>
        /// Updates the visual appearance based on selection state.
        /// </summary>
        private void UpdateVisualStyle()
        {
            var brush = IsSelected ? LineBrushSelected : LineBrush;
            _linePath.Stroke = brush;
            _arrowPath.Fill = brush;
        }

        /// <summary>
        /// Updates visibility of visual elements based on editing state.
        /// When editing, elements are hidden so only the adorner preview is visible.
        /// </summary>
        private void UpdateEditingVisibility()
        {
            var visibility = IsEditing ? Visibility.Collapsed : Visibility.Visible;
            _linePath.Visibility = visibility;
            _arrowPath.Visibility = visibility;
            _hitTestPath.Visibility = visibility;
            
            // Label visibility depends on both editing state and label content
            if (IsEditing)
            {
                _labelBorder.Visibility = Visibility.Collapsed;
            }
            else
            {
                UpdateLabelVisibility();
            }
        }

        #endregion
    }
}