using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using EquipmentDesigner.Models;
using EquipmentDesigner.Views.Drawboard.UMLEngine;

namespace EquipmentDesigner.Views.Drawboard.Controls
{
    /// <summary>
    /// A custom control that renders a connection line between two UML elements.
    /// Uses orthogonal routing with 90-degree turns and displays an optional label at midpoint.
    /// </summary>
    public class ConnectionLine : Canvas
    {
        #region Constants

        private const double LineThickness = 2.0;
        private const double ArrowHeadLength = 10.0;
        private const double ArrowHeadWidth = 8.0;
        private const double LabelPadding = 4.0;

        private static readonly Brush LineBrush;
        private static readonly Pen LinePen;

        #endregion

        #region Static Constructor

        static ConnectionLine()
        {
            LineBrush = new SolidColorBrush(Color.FromRgb(0x33, 0x33, 0x33));
            LineBrush.Freeze();
            LinePen = new Pen(LineBrush, LineThickness);
            LinePen.Freeze();
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

        #endregion

        #region Fields

        private readonly Path _linePath;
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
            // Create the line path
            _linePath = new Path
            {
                Stroke = LineBrush,
                StrokeThickness = LineThickness,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                StrokeLineJoin = PenLineJoin.Round
            };
            Children.Add(_linePath);

            // Create the arrow head path
            _arrowPath = new Path
            {
                Fill = LineBrush,
                Stroke = null
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

            // Set IsHitTestVisible to false for now (no interaction)
            IsHitTestVisible = false;
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

            // Calculate port positions
            var tailBounds = _tailElement.Bounds;
            var headBounds = _headElement.Bounds;

            var startPoint = ConnectionRoutingEngine.CalculatePortPosition(tailBounds, TailPort);
            var endPoint = ConnectionRoutingEngine.CalculatePortPosition(headBounds, HeadPort);

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
    }
}
