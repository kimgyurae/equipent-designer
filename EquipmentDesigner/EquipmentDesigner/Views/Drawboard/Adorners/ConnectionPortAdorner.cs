using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using EquipmentDesigner.Models;

namespace EquipmentDesigner.Views.Drawboard.Adorners
{
    /// <summary>
    /// Adorner that displays connection ports at the center of each side of a drawing element.
    /// Each port is a clickable button with an arrow icon pointing outward.
    /// Ports maintain constant visual size regardless of zoom level.
    /// </summary>
    public class ConnectionPortAdorner : Adorner
    {
        private const double PortSize = 24.0;
        private const double PortCornerRadius = 4.0;
        private const double IconSize = 12.0;
        private const double VisualPortOffsetFromEdge = 8.0; // Visual distance from element edge to port center (in screen pixels, zoom-independent)

        private static readonly Brush PortBackgroundBrush;
        private static readonly Brush PortBackgroundHoverBrush;
        private static readonly Brush IconFillBrush;

        private readonly DrawingElement _element;
        private readonly VisualCollection _visualChildren;
        private readonly Border[] _portBorders;
        private readonly Canvas _portContainer;
        private readonly ScaleTransform[] _portScaleTransforms; // Individual scale transforms for each port

        private PortPosition? _hoveredPort;

        /// <summary>
        /// Raised when a connection port is clicked.
        /// </summary>
        public event EventHandler<PortClickedEventArgs> PortClicked;

        static ConnectionPortAdorner()
        {
            // Semi-transparent black background: rgba(0,0,0,0.3) = #4D000000
            PortBackgroundBrush = new SolidColorBrush(Color.FromArgb(0x4D, 0x00, 0x00, 0x00));
            PortBackgroundBrush.Freeze();

            // Slightly more opaque on hover
            PortBackgroundHoverBrush = new SolidColorBrush(Color.FromArgb(0x80, 0x00, 0x00, 0x00));
            PortBackgroundHoverBrush.Freeze();

            // White arrow icon
            IconFillBrush = Brushes.White;
        }

        /// <summary>
        /// Initializes a new instance of the ConnectionPortAdorner class.
        /// </summary>
        /// <param name="adornedElement">The UI element to adorn.</param>
        /// <param name="element">The DrawingElement data model.</param>
        public ConnectionPortAdorner(UIElement adornedElement, DrawingElement element) : base(adornedElement)
        {
            _element = element ?? throw new ArgumentNullException(nameof(element));
            _visualChildren = new VisualCollection(this);
            _portBorders = new Border[4];
            _portScaleTransforms = new ScaleTransform[4];

            // Create a container canvas for the ports (no transform on container)
            _portContainer = new Canvas
            {
                IsHitTestVisible = true,
                ClipToBounds = false // Allow ports to render outside element bounds
            };

            // Create port buttons for all four sides
            CreatePortButton(PortPosition.Top, 0);
            CreatePortButton(PortPosition.Right, 1);
            CreatePortButton(PortPosition.Bottom, 2);
            CreatePortButton(PortPosition.Left, 3);

            _visualChildren.Add(_portContainer);

            // Subscribe to element property changes
            _element.PropertyChanged += Element_PropertyChanged;

            IsHitTestVisible = true;
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
        /// Creates a port button for the specified position.
        /// </summary>
        private void CreatePortButton(PortPosition position, int index)
        {
            // Create arrow icon (always pointing UP - rotation applied to Border)
            var arrowPath = CreateArrowIcon();

            // Calculate rotation angle based on port position
            var rotateAngle = position switch
            {
                PortPosition.Top => 0,      // Arrow pointing up (out of element)
                PortPosition.Right => 90,   // Arrow pointing right
                PortPosition.Bottom => 180, // Arrow pointing down
                PortPosition.Left => 270,   // Arrow pointing left
                _ => 0
            };

            // Create combined transform: Scale + Rotate
            // Scale is applied first, then rotation around Border center
            var scaleTransform = new ScaleTransform(1, 1);
            var rotateTransform = new RotateTransform(rotateAngle);
            var transformGroup = new TransformGroup();
            transformGroup.Children.Add(scaleTransform);
            transformGroup.Children.Add(rotateTransform);

            _portScaleTransforms[index] = scaleTransform;

            // Create port border (button-like appearance)
            var border = new Border
            {
                Width = PortSize,
                Height = PortSize,
                CornerRadius = new CornerRadius(PortCornerRadius),
                Background = PortBackgroundBrush,
                Child = arrowPath,
                Cursor = Cursors.Hand,
                Tag = position,
                RenderTransform = transformGroup,
                RenderTransformOrigin = new Point(0.5, 0.5) // Rotate around Border center
            };

            // Wire up events
            border.MouseEnter += OnPortMouseEnter;
            border.MouseLeave += OnPortMouseLeave;
            border.MouseLeftButtonUp += OnPortMouseLeftButtonUp;

            _portBorders[index] = border;
            _portContainer.Children.Add(border);
        }

        /// <summary>
        /// Creates an arrow icon path pointing upward (forward direction).
        /// The arrow is always created pointing UP; rotation is applied to the parent Border.
        /// </summary>
        private Path CreateArrowIcon()
        {
            // Arrow triangle geometry pointing UP (forward direction)
            // Triangle: M 0,-4 L 4,4 L -4,4 Z (centered at origin, symmetric bounding box)
            var geometry = new PathGeometry();
            var figure = new PathFigure { StartPoint = new Point(0, -4), IsClosed = true };
            figure.Segments.Add(new LineSegment(new Point(4, 4), true));
            figure.Segments.Add(new LineSegment(new Point(-4, 4), true));
            geometry.Figures.Add(figure);

            return new Path
            {
                Data = geometry,
                Fill = IconFillBrush,
                Width = IconSize,
                Height = IconSize,
                Stretch = Stretch.Uniform,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
                // No rotation here - rotation is applied to the parent Border
            };
        }

        #region Port Event Handlers

        private void OnPortMouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Border border && border.Tag is PortPosition position)
            {
                _hoveredPort = position;
                border.Background = PortBackgroundHoverBrush;
            }
        }

        private void OnPortMouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Border border)
            {
                _hoveredPort = null;
                border.Background = PortBackgroundBrush;
            }
        }

        private void OnPortMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is PortPosition position)
            {
                PortClicked?.Invoke(this, new PortClickedEventArgs(position));
                e.Handled = true;
            }
        }

        #endregion

        #region Layout

        /// <summary>
        /// Gets the current zoom scale from the visual tree.
        /// </summary>
        private double GetCurrentZoomScale()
        {
            // Walk up the visual tree to find the scale transform
            DependencyObject current = AdornedElement;
            int depth = 0;
            while (current != null)
            {
                if (current is FrameworkElement fe)
                {
                    Debug.WriteLine($"[ConnectionPortAdorner] Visual Tree[{depth}]: {fe.GetType().Name}, " +
                        $"LayoutTransform={fe.LayoutTransform?.GetType().Name}, " +
                        $"RenderTransform={fe.RenderTransform?.GetType().Name}");
                    
                    if (fe.LayoutTransform is ScaleTransform scaleTransform)
                    {
                        Debug.WriteLine($"[ConnectionPortAdorner] Found ScaleTransform at depth {depth}: ScaleX={scaleTransform.ScaleX}, ScaleY={scaleTransform.ScaleY}");
                        return scaleTransform.ScaleX;
                    }
                }
                current = VisualTreeHelper.GetParent(current);
                depth++;
            }

            Debug.WriteLine("[ConnectionPortAdorner] No ScaleTransform found in visual tree, using 1.0");
            return 1.0;
        }

        /// <summary>
        /// Measures the adorner.
        /// </summary>
        protected override Size MeasureOverride(Size constraint)
        {
            _portContainer.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            return base.MeasureOverride(constraint);
        }

        /// <summary>
        /// Arranges the port buttons at their positions around the element.
        /// Ports are positioned at the center of each side, offset outside the element.
        /// Applies inverse scale transform to maintain constant visual size.
        /// </summary>
        protected override Size ArrangeOverride(Size finalSize)
        {
            // Get element dimensions from the DrawingElement (canvas coordinates)
            var elementWidth = _element.Width;
            var elementHeight = _element.Height;

            // Get AdornedElement actual size for comparison
            var adornedElement = AdornedElement as FrameworkElement;
            var adornedActualWidth = adornedElement?.ActualWidth ?? 0;
            var adornedActualHeight = adornedElement?.ActualHeight ?? 0;

            Debug.WriteLine($"[ConnectionPortAdorner] === ArrangeOverride ===");
            Debug.WriteLine($"[ConnectionPortAdorner] finalSize: {finalSize.Width:F1} x {finalSize.Height:F1}");
            Debug.WriteLine($"[ConnectionPortAdorner] DrawingElement: Width={elementWidth:F1}, Height={elementHeight:F1}");
            Debug.WriteLine($"[ConnectionPortAdorner] AdornedElement.ActualSize: {adornedActualWidth:F1} x {adornedActualHeight:F1}");

            // Get the current zoom scale
            var zoomScale = GetCurrentZoomScale();
            var inverseScale = 1.0 / zoomScale;

            Debug.WriteLine($"[ConnectionPortAdorner] ZoomScale: {zoomScale:F3}, InverseScale: {inverseScale:F3}");

            // Apply inverse scale to each port individually
            // This makes ports maintain constant VISUAL size regardless of zoom
            for (int i = 0; i < 4; i++)
            {
                _portScaleTransforms[i].ScaleX = inverseScale;
                _portScaleTransforms[i].ScaleY = inverseScale;
            }

            // Calculate port offset in canvas coordinates to achieve constant visual gap
            // The port is scaled by inverseScale, so its canvas half-size varies with zoom
            // Visual gap = 8px, Port center offset = gap + scaled port half-size
            var portCanvasHalfSize = (PortSize / 2) * inverseScale;
            var canvasOffset = VisualPortOffsetFromEdge + portCanvasHalfSize;

            // Calculate port positions using FIXED PortSize and dynamic canvasOffset
            // Positions are in element's coordinate system (before zoom transform)
            // Port center should be at: edge center + offset distance outside
            var centerX = elementWidth / 2;
            var centerY = elementHeight / 2;
            var halfPort = PortSize / 2;

            // Port positions (top-left corner of each port)
            // The port's RenderTransformOrigin is (0.5, 0.5), so it scales around its center
            var positions = new[]
            {
                // Top: port center at (centerX, -canvasOffset)
                new Point(centerX - halfPort, -canvasOffset - halfPort),
                // Right: port center at (elementWidth + canvasOffset, centerY)
                new Point(elementWidth + canvasOffset - halfPort, centerY - halfPort),
                // Bottom: port center at (centerX, elementHeight + canvasOffset)
                new Point(centerX - halfPort, elementHeight + canvasOffset - halfPort),
                // Left: port center at (-canvasOffset, centerY)
                new Point(-canvasOffset - halfPort, centerY - halfPort)
            };

            Debug.WriteLine($"[ConnectionPortAdorner] Element Center: ({centerX:F1}, {centerY:F1})");
            Debug.WriteLine($"[ConnectionPortAdorner] Canvas Offset: {canvasOffset:F1} (visual: {VisualPortOffsetFromEdge:F1}px)");
            Debug.WriteLine($"[ConnectionPortAdorner] Port Positions (canvas coords, scaled offset):");
            Debug.WriteLine($"[ConnectionPortAdorner]   Top:    ({positions[0].X:F1}, {positions[0].Y:F1}) -> center ({centerX:F1}, {-canvasOffset:F1})");
            Debug.WriteLine($"[ConnectionPortAdorner]   Right:  ({positions[1].X:F1}, {positions[1].Y:F1}) -> center ({elementWidth + canvasOffset:F1}, {centerY:F1})");
            Debug.WriteLine($"[ConnectionPortAdorner]   Bottom: ({positions[2].X:F1}, {positions[2].Y:F1}) -> center ({centerX:F1}, {elementHeight + canvasOffset:F1})");
            Debug.WriteLine($"[ConnectionPortAdorner]   Left:   ({positions[3].X:F1}, {positions[3].Y:F1}) -> center ({-canvasOffset:F1}, {centerY:F1})");

            for (int i = 0; i < 4; i++)
            {
                Canvas.SetLeft(_portBorders[i], positions[i].X);
                Canvas.SetTop(_portBorders[i], positions[i].Y);
            }

            // Arrange container at (0, 0) - ClipToBounds=false allows negative coordinates to render
            // Container size matches element size, ports outside will still be visible
            Debug.WriteLine($"[ConnectionPortAdorner] Container Arrange: (0, 0, {elementWidth:F1}, {elementHeight:F1})");

            _portContainer.Arrange(new Rect(0, 0, elementWidth, elementHeight));

            Debug.WriteLine($"[ConnectionPortAdorner] === End ArrangeOverride ===\n");

            return new Size(elementWidth, elementHeight);
        }

        #endregion

        #region Property Change Handling

        private void Element_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DrawingElement.Width) ||
                e.PropertyName == nameof(DrawingElement.Height))
            {
                InvalidateMeasure();
                InvalidateArrange();
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

            foreach (var border in _portBorders)
            {
                if (border != null)
                {
                    border.MouseEnter -= OnPortMouseEnter;
                    border.MouseLeave -= OnPortMouseLeave;
                    border.MouseLeftButtonUp -= OnPortMouseLeftButtonUp;
                }
            }

            _portContainer.Children.Clear();
            _visualChildren.Clear();
        }

        #endregion
    }

    /// <summary>
    /// Event arguments for port click events.
    /// </summary>
    public class PortClickedEventArgs : EventArgs
    {
        /// <summary>
        /// The position of the clicked port.
        /// </summary>
        public PortPosition Port { get; }

        public PortClickedEventArgs(PortPosition port)
        {
            Port = port;
        }
    }
}