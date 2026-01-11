using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using EquipmentDesigner.Models;
using EquipmentDesigner.Views.Drawboard.UMLEngine;

namespace EquipmentDesigner.Views.Drawboard.Adorners
{
    /// <summary>
    /// Adorner that displays a group selection box with dotted line border and resize handles.
    /// Used when multiple elements are selected for group editing.
    /// </summary>
    public class MultiSelectionAdorner : Adorner
    {
        private const double SelectionPadding = 4.0;
        private const double HandleSize = 8.0;

        private static readonly Brush HandleFillBrush = Brushes.White;
        private static readonly Brush SelectionBorderBrush;
        private static readonly Pen SelectionPen;
        private static readonly Pen HandlePen;

        private readonly ObservableCollection<DrawingElement> _selectedElements;
        private readonly VisualCollection _visualChildren;
        private readonly Dictionary<ResizeHandleType, Thumb> _handles;

        // Computed group bounds
        private Rect _groupBounds;

        // Drag state tracking
        private Point _dragStartPoint;

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

        /// <summary>
        /// Raised when a move operation should begin (click on group surface).
        /// </summary>
        public event EventHandler<Point> MoveStarted;

        static MultiSelectionAdorner()
        {
            SelectionBorderBrush = new SolidColorBrush(Color.FromRgb(0x60, 0xA5, 0xFA)); // #60A5FA
            SelectionBorderBrush.Freeze();

            // Dotted line for multi-selection (different from single selection solid line)
            SelectionPen = new Pen(SelectionBorderBrush, 1.0)
            {
                DashStyle = DashStyles.Dash
            };
            SelectionPen.Freeze();

            HandlePen = new Pen(SelectionBorderBrush, 1.5);
            HandlePen.Freeze();
        }

        /// <summary>
        /// Initializes a new instance of the MultiSelectionAdorner class.
        /// </summary>
        /// <param name="adornedElement">The UI element to adorn (typically the canvas).</param>
        /// <param name="selectedElements">The collection of selected elements.</param>
        public MultiSelectionAdorner(UIElement adornedElement, ObservableCollection<DrawingElement> selectedElements)
            : base(adornedElement)
        {
            _selectedElements = selectedElements ?? throw new ArgumentNullException(nameof(selectedElements));
            _visualChildren = new VisualCollection(this);
            _handles = new Dictionary<ResizeHandleType, Thumb>();

            // Subscribe to collection changes
            _selectedElements.CollectionChanged += OnSelectedElementsChanged;

            // Subscribe to each element's property changes
            foreach (var element in _selectedElements)
            {
                element.PropertyChanged += OnElementPropertyChanged;
            }

            // Calculate initial group bounds
            UpdateGroupBounds();

            // Adorner must be hit-testable for child Thumbs to receive events
            IsHitTestVisible = true;

            // Create Thumb controls for all resize handles
            CreateResizeHandles();
        }

        /// <summary>
        /// Gets the computed group bounds.
        /// </summary>
        public Rect GroupBounds => _groupBounds;

        /// <summary>
        /// Gets the number of visual children.
        /// </summary>
        protected override int VisualChildrenCount => _visualChildren.Count;

        /// <summary>
        /// Gets the visual child at the specified index.
        /// </summary>
        protected override Visual GetVisualChild(int index) => _visualChildren[index];

        /// <summary>
        /// Updates the computed group bounds from all selected elements.
        /// </summary>
        public void UpdateGroupBounds()
        {
            _groupBounds = DrawingElementEditingEngine.ComputeGroupBounds(_selectedElements);
            InvalidateMeasure();
            InvalidateArrange();
            InvalidateVisual();
        }

        /// <summary>
        /// Creates Thumb controls for corner and edge resize handles.
        /// </summary>
        private void CreateResizeHandles()
        {
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
            // Get mouse position relative to AdornedElement (ZoomableGrid)
            _dragStartPoint = Mouse.GetPosition(AdornedElement);

            ResizeStarted?.Invoke(this, new ResizeEventArgs(handleType, _dragStartPoint));
        }

        private void OnThumbDragDelta(ResizeHandleType handleType, DragDeltaEventArgs e)
        {
            // Get mouse position relative to AdornedElement (ZoomableGrid)
            var mousePosition = Mouse.GetPosition(AdornedElement);
            ResizeDelta?.Invoke(this, new ResizeEventArgs(handleType, mousePosition));
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
            if (_groupBounds.IsEmpty) return finalSize;

            foreach (var kvp in _handles)
            {
                var handleType = kvp.Key;
                var thumb = kvp.Value;
                var center = GetHandleCenter(handleType);

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
        /// Gets the center point for a handle in canvas coordinates.
        /// Handles are positioned on the selection box which is 4px outside the group bounds.
        /// </summary>
        private Point GetHandleCenter(ResizeHandleType handleType)
        {
            double padding = SelectionPadding;
            var bounds = _groupBounds;

            return handleType switch
            {
                // Corners
                ResizeHandleType.TopLeft => new Point(bounds.Left - padding, bounds.Top - padding),
                ResizeHandleType.TopRight => new Point(bounds.Right + padding, bounds.Top - padding),
                ResizeHandleType.BottomLeft => new Point(bounds.Left - padding, bounds.Bottom + padding),
                ResizeHandleType.BottomRight => new Point(bounds.Right + padding, bounds.Bottom + padding),

                // Edges
                ResizeHandleType.Top => new Point(bounds.Left + bounds.Width / 2, bounds.Top - padding),
                ResizeHandleType.Right => new Point(bounds.Right + padding, bounds.Top + bounds.Height / 2),
                ResizeHandleType.Bottom => new Point(bounds.Left + bounds.Width / 2, bounds.Bottom + padding),
                ResizeHandleType.Left => new Point(bounds.Left - padding, bounds.Top + bounds.Height / 2),

                _ => new Point(0, 0)
            };
        }

        #endregion

        #region Rendering

        /// <summary>
        /// Renders the dotted selection box border.
        /// </summary>
        protected override void OnRender(DrawingContext drawingContext)
        {
            if (_groupBounds.IsEmpty) return;

            // Calculate selection box bounds (4px padding around group)
            var selectionRect = new Rect(
                _groupBounds.Left - SelectionPadding,
                _groupBounds.Top - SelectionPadding,
                _groupBounds.Width + (SelectionPadding * 2),
                _groupBounds.Height + (SelectionPadding * 2)
            );

            // Draw dotted selection box border (Thumbs draw themselves)
            drawingContext.DrawRectangle(null, SelectionPen, selectionRect);
        }

        #endregion

        #region Hit Testing

        /// <summary>
        /// Checks if a point is within the group surface (for move operations).
        /// </summary>
        /// <param name="point">The point in canvas coordinates.</param>
        /// <returns>True if the point is on the group surface.</returns>
        public bool IsOnGroupSurface(Point point)
        {
            return _groupBounds.Contains(point);
        }

        #endregion

        #region Event Handlers

        private void OnSelectedElementsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Unsubscribe from removed elements
            if (e.OldItems != null)
            {
                foreach (DrawingElement element in e.OldItems)
                {
                    element.PropertyChanged -= OnElementPropertyChanged;
                }
            }

            // Subscribe to new elements
            if (e.NewItems != null)
            {
                foreach (DrawingElement element in e.NewItems)
                {
                    element.PropertyChanged += OnElementPropertyChanged;
                }
            }

            UpdateGroupBounds();
        }

        private void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Update bounds when element geometry changes
            if (e.PropertyName == nameof(DrawingElement.X) ||
                e.PropertyName == nameof(DrawingElement.Y) ||
                e.PropertyName == nameof(DrawingElement.Width) ||
                e.PropertyName == nameof(DrawingElement.Height))
            {
                UpdateGroupBounds();
            }
        }

        #endregion

        #region Cleanup

        /// <summary>
        /// Cleans up resources when the adorner is no longer needed.
        /// </summary>
        public void Detach()
        {
            _selectedElements.CollectionChanged -= OnSelectedElementsChanged;

            foreach (var element in _selectedElements)
            {
                element.PropertyChanged -= OnElementPropertyChanged;
            }

            _handles.Clear();
            _visualChildren.Clear();
        }

        #endregion
    }
}