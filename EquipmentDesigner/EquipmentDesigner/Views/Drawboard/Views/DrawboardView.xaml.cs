using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using EquipmentDesigner.Models;
using EquipmentDesigner.ViewModels;
using EquipmentDesigner.Views.Drawboard.Adorners;

namespace EquipmentDesigner.Views
{
    /// <summary>
    /// Interaction logic for DrawboardView.xaml
    /// </summary>
    public partial class DrawboardView : UserControl
    {
        /// <summary>
        /// Exponential zoom factor for mouse scroll (Figma/Excalidraw style).
        /// Each scroll step multiplies/divides by this factor (~10% change per step).
        /// </summary>
        private const double ScrollZoomFactor = 1.1;

        /// <summary>
        /// Linear zoom step for low zoom levels (10-100 range).
        /// </summary>
        private const int LinearZoomStep = 10;

        /// <summary>
        /// Threshold below which linear zoom is used instead of exponential.
        /// </summary>
        private const int LinearZoomThreshold = 100;

        private SelectionAdorner _selectionAdorner;
        private AdornerLayer _adornerLayer;
        private bool _isShiftPressed;
        private DrawboardViewModel _viewModel;
        private DrawingElement _editingElement;

        public DrawboardView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
            PreviewKeyDown += OnDrawboardPreviewKeyDown;
            Loaded += OnDrawboardLoaded;
        }

        /// <summary>
        /// Sets keyboard focus when the view is loaded.
        /// </summary>
        private void OnDrawboardLoaded(object sender, RoutedEventArgs e)
        {
            Focusable = true;
            Focus();
        }

        /// <summary>
        /// Forces layout recalculation when DataContext changes.
        /// This ensures the toolbar sizes correctly based on bound tool data.
        /// </summary>
        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // Unsubscribe from old ViewModel
            if (_viewModel != null)
            {
                _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
            }

            // Force layout update to ensure toolbar resizes correctly
            // when DataContext is set after view creation
            if (e.NewValue is DrawboardViewModel viewModel)
            {
                _viewModel = viewModel;
                _viewModel.PropertyChanged += ViewModel_PropertyChanged;

                InvalidateMeasure();
                InvalidateArrange();
                UpdateLayout();
            }
        }

        /// <summary>
        /// Handles ViewModel property changes to update adorner.
        /// </summary>
        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DrawboardViewModel.SelectedElement))
            {
                if (_viewModel?.SelectedElement != null)
                {
                    UpdateSelectionAdorner(_viewModel.SelectedElement);
                }
                else
                {
                    RemoveSelectionAdorner();
                }
            }
        }

        #region Selection Event Handlers

        /// <summary>
        /// Handles Ctrl+Mouse Wheel for zoom control with focus on mouse position.
        /// Uses exponential scaling for consistent zoom feel across all zoom levels.
        /// </summary>
        private void OnCanvasPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers != ModifierKeys.Control) return;
            if (_viewModel == null) return;

            // 1. Get mouse position relative to viewport (before zoom)
            Point mouseViewport = e.GetPosition(CanvasScrollViewer);

            // 2. Calculate content coordinates at mouse position
            double oldScale = _viewModel.ZoomScale;
            Point mouseContent = new Point(
                (CanvasScrollViewer.HorizontalOffset + mouseViewport.X) / oldScale,
                (CanvasScrollViewer.VerticalOffset + mouseViewport.Y) / oldScale
            );

            // 3. Apply zoom level change (linear for 10-100, exponential for >100)
            int oldZoom = _viewModel.ZoomLevel;
            if (e.Delta > 0)
            {
                // Zoom In
                int newZoom;
                if (_viewModel.ZoomLevel < LinearZoomThreshold)
                {
                    // Linear scaling for 10-100 range
                    newZoom = Math.Min(_viewModel.ZoomLevel + LinearZoomStep, 3000);
                }
                else
                {
                    // Exponential scaling for >100
                    newZoom = (int)Math.Round(Math.Min(_viewModel.ZoomLevel * ScrollZoomFactor, 3000));
                }
                _viewModel.ZoomLevel = newZoom;
            }
            else
            {
                // Zoom Out
                int newZoom;
                if (_viewModel.ZoomLevel <= LinearZoomThreshold)
                {
                    // Linear scaling for 10-100 range
                    newZoom = Math.Max(_viewModel.ZoomLevel - LinearZoomStep, 10);
                }
                else
                {
                    // Exponential scaling for >100
                    double expZoom = _viewModel.ZoomLevel / ScrollZoomFactor;
                    newZoom = (int)Math.Round(Math.Max(expZoom, 10));
                }
                _viewModel.ZoomLevel = newZoom;
            }

            // If zoom level didn't change, exit
            if (_viewModel.ZoomLevel == oldZoom)
            {
                e.Handled = true;
                return;
            }

            // 4. Calculate new scroll offset after zoom
            double newScale = _viewModel.ZoomScale;
            double newHOffset = mouseContent.X * newScale - mouseViewport.X;
            double newVOffset = mouseContent.Y * newScale - mouseViewport.Y;

            // 5. Force layout update to ensure ScrollViewer extent is recalculated
            CanvasScrollViewer.UpdateLayout();

            // 6. Apply scroll offset synchronously (no Dispatcher needed after UpdateLayout)
            CanvasScrollViewer.ScrollToHorizontalOffset(Math.Max(0, newHOffset));
            CanvasScrollViewer.ScrollToVerticalOffset(Math.Max(0, newVOffset));

            e.Handled = true;
        }

        /// <summary>
        /// Handles Zoom In button click with focus on viewport center.
        /// </summary>
        private void OnZoomInClick(object sender, RoutedEventArgs e)
        {
            ZoomAtViewportCenter(zoomIn: true);
        }

        /// <summary>
        /// Handles Zoom Out button click with focus on viewport center.
        /// </summary>
        private void OnZoomOutClick(object sender, RoutedEventArgs e)
        {
            ZoomAtViewportCenter(zoomIn: false);
        }

        /// <summary>
        /// Applies zoom while maintaining viewport center position.
        /// </summary>
        private void ZoomAtViewportCenter(bool zoomIn)
        {
            if (_viewModel == null) return;

            // 1. Calculate viewport center position
            Point viewportCenter = new Point(
                CanvasScrollViewer.ViewportWidth / 2,
                CanvasScrollViewer.ViewportHeight / 2
            );

            // 2. Calculate content coordinates at viewport center
            double oldScale = _viewModel.ZoomScale;
            Point contentCenter = new Point(
                (CanvasScrollViewer.HorizontalOffset + viewportCenter.X) / oldScale,
                (CanvasScrollViewer.VerticalOffset + viewportCenter.Y) / oldScale
            );

            // 3. Apply zoom level change
            int oldZoom = _viewModel.ZoomLevel;
            if (zoomIn)
                _viewModel.ZoomLevel = Math.Min(_viewModel.ZoomLevel + 10, 3000);
            else
                _viewModel.ZoomLevel = Math.Max(_viewModel.ZoomLevel - 10, 10);

            // If zoom level didn't change, exit
            if (_viewModel.ZoomLevel == oldZoom) return;

            // 4. Calculate new scroll offset after zoom
            double newScale = _viewModel.ZoomScale;
            double newHOffset = contentCenter.X * newScale - viewportCenter.X;
            double newVOffset = contentCenter.Y * newScale - viewportCenter.Y;

            // 5. Force layout update to ensure ScrollViewer extent is recalculated
            CanvasScrollViewer.UpdateLayout();

            // 6. Apply scroll offset synchronously (no Dispatcher needed after UpdateLayout)
            CanvasScrollViewer.ScrollToHorizontalOffset(Math.Max(0, newHOffset));
            CanvasScrollViewer.ScrollToVerticalOffset(Math.Max(0, newVOffset));
        }

        /// <summary>
        /// Handles keyboard shortcuts for tool selection.
        /// </summary>
        private void OnDrawboardPreviewKeyDown(object sender, KeyEventArgs e)
        {
            // If text editing is active, let the TextBox handle ESC
            if (e.Key == Key.Escape && _editingElement != null)
            {
                return;  // OnInlineEditKeyDown will handle this
            }

            // ESC key: Exit edit mode (clear selection) - works even in TextBox
            if (e.Key == Key.Escape)
            {
                if (DataContext is DrawboardViewModel viewModel && viewModel.SelectedElement != null)
                {
                    viewModel.ClearSelection();
                    e.Handled = true;
                }
                return;
            }

            // Ignore other keys when typing in TextBox
            if (e.OriginalSource is TextBox)
                return;

            // Ignore when modifier keys are pressed (allow Ctrl+C, Ctrl+V, etc.)
            if (Keyboard.Modifiers != ModifierKeys.None)
                return;

            var toolId = GetToolIdFromKey(e.Key);
            if (toolId != null && DataContext is DrawboardViewModel vm)
            {
                vm.SelectToolById(toolId);
                e.Handled = true;
            }
        }

        /// <summary>
        /// Maps keyboard keys to tool IDs.
        /// </summary>
        private static string GetToolIdFromKey(Key key)
        {
            return key switch
            {
                Key.L => "ToolLock",
                Key.H => "Hand",
                Key.V => "Selection",
                Key.D1 or Key.NumPad1 => "InitialNode",
                Key.D2 or Key.NumPad2 => "ActionNode",
                Key.D3 or Key.NumPad3 => "DecisionNode",
                Key.D4 or Key.NumPad4 => "TerminalNode",
                Key.D5 or Key.NumPad5 => "PredefinedActionNode",
                Key.T => "Textbox",
                Key.E => "Eraser",
                Key.I => "Image",
                _ => null
            };
        }

        /// <summary>
        /// Handles preview mouse left button down for selection logic.
        /// This runs before the regular MouseLeftButtonDown to handle selection first.
        /// </summary>
        private void OnCanvasPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_viewModel == null) return;

            // Only handle selection when Selection tool is active
            if (!_viewModel.IsSelectionToolActive) return;

            var position = e.GetPosition(ZoomableGrid);
            HandleSelectionClick(position, e);
        }

        /// <summary>
        /// Handles preview mouse right button up for selection (per spec: right-click Button Up activates edit mode).
        /// </summary>
        private void OnCanvasPreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_viewModel == null) return;

            // Only handle selection when Selection tool is active
            if (!_viewModel.IsSelectionToolActive) return;

            var position = e.GetPosition(ZoomableGrid);
            var hitElement = _viewModel.FindElementAtPoint(position);

            if (hitElement != null)
            {
                // Right-click on element: select it
                _viewModel.SelectElement(hitElement);
                ZoomableGrid.Focus();
                e.Handled = true;
            }
            else
            {
                // Right-click on empty space: clear selection
                _viewModel.ClearSelection();
            }
        }

        /// <summary>
        /// Common logic for handling selection clicks.
        /// Note: Resize operations are now handled directly by Thumb drag events in SelectionAdorner.
        /// </summary>
        private void HandleSelectionClick(Point position, MouseButtonEventArgs e)
        {
            var hitElement = _viewModel.FindElementAtPoint(position);

            if (hitElement == null)
            {
                // Clicked on empty space - clear selection
                _viewModel.ClearSelection();
                return;
            }

            // Check if we clicked on already selected element
            if (hitElement == _viewModel.SelectedElement)
            {
                // Check if on element surface - start move
                if (_selectionAdorner != null)
                {
                    var adornerPoint = ZoomableGrid.TranslatePoint(position, _selectionAdorner);
                    if (_selectionAdorner.IsOnElementSurface(adornerPoint))
                    {
                        _viewModel.StartMove(position);
                        ZoomableGrid.CaptureMouse();
                        e.Handled = true;
                        return;
                    }
                }
            }
            else
            {
                // Clicked on different element - select it and start move immediately
                _viewModel.SelectElement(hitElement);
                _viewModel.StartMove(position);
                ZoomableGrid.CaptureMouse();
                ZoomableGrid.Focus();
                e.Handled = true;
            }
        }

        /// <summary>
        /// Handles keyboard input for delete and modifier keys.
        /// </summary>
        private void OnCanvasKeyDown(object sender, KeyEventArgs e)
        {
            if (_viewModel == null) return;

            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
            {
                _isShiftPressed = true;
            }
            else if (e.Key == Key.Delete)
            {
                if (_viewModel.SelectedElement != null)
                {
                    _viewModel.DeleteSelectedElement();
                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// Handles key up to track Shift key state.
        /// </summary>
        protected override void OnPreviewKeyUp(KeyEventArgs e)
        {
            base.OnPreviewKeyUp(e);

            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
            {
                _isShiftPressed = false;
            }
        }

        #endregion

        #region Drawing and Edit Event Handlers

        /// <summary>
        /// Handles mouse left button down on the canvas to start drawing.
        /// </summary>
        private void OnCanvasMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_viewModel == null) return;

            // If we're in Selection tool mode, selection is handled by Preview event
            if (_viewModel.IsSelectionToolActive) return;

            var position = e.GetPosition((IInputElement)sender);
            if (_viewModel.TryStartDrawing(position))
            {
                ((UIElement)sender).CaptureMouse();
                e.Handled = true;
            }
        }

        /// <summary>
        /// Handles mouse move on the canvas for drawing preview or edit operations.
        /// Note: Cursor changes for resize handles are now handled automatically by Thumb.Cursor property.
        /// </summary>
        private void OnCanvasMouseMove(object sender, MouseEventArgs e)
        {
            if (_viewModel == null) return;

            var position = e.GetPosition((IInputElement)sender);

            // Handle edit mode operations
            switch (_viewModel.EditModeState)
            {
                case EditModeState.Moving:
                    _viewModel.UpdateMove(position);
                    return;

                case EditModeState.Resizing:
                    // Resize is handled exclusively by SelectionAdorner.OnThumbDragDelta
                    // Do NOT call UpdateResize here to prevent dual invocation bug
                    // that causes incorrect sizing when mouse events and Thumb events conflict
                    return;
            }

            // Handle drawing preview
            if (_viewModel.IsDrawing)
            {
                _viewModel.UpdateDrawing(position);
                return;
            }

            // Note: Cursor updates for resize handles are now automatic via Thumb.Cursor property
        }

        /// <summary>
        /// Handles mouse left button up on the canvas to finish drawing or edit operations.
        /// </summary>
        private void OnCanvasMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_viewModel == null) return;

            // Handle edit mode operations
            switch (_viewModel.EditModeState)
            {
                case EditModeState.Moving:
                    _viewModel.EndMove();
                    ((UIElement)sender).ReleaseMouseCapture();
                    e.Handled = true;
                    return;

                case EditModeState.Resizing:
                    _viewModel.EndResize();
                    ((UIElement)sender).ReleaseMouseCapture();
                    e.Handled = true;
                    return;
            }

            // Handle drawing completion
            if (_viewModel.IsDrawing)
            {
                _viewModel.FinishDrawing();
                ((UIElement)sender).ReleaseMouseCapture();
                e.Handled = true;
            }
        }

        #endregion

        #region Adorner Management

        /// <summary>
        /// Updates or creates the selection adorner for the specified element.
        /// </summary>
        private void UpdateSelectionAdorner(DrawingElement element)
        {
            // Remove existing adorner
            RemoveSelectionAdorner();

            // Find the visual container for the element
            var container = FindContainerForElement(element);
            if (container == null)
            {
                // Element might not be rendered yet, schedule for later
                Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    if (_viewModel?.SelectedElement == element)
                    {
                        var delayedContainer = FindContainerForElement(element);
                        if (delayedContainer != null)
                        {
                            CreateAdorner(delayedContainer, element);
                        }
                    }
                }), System.Windows.Threading.DispatcherPriority.Loaded);
                return;
            }

            CreateAdorner(container, element);
        }

        /// <summary>
        /// Creates the adorner on the specified container and connects resize events.
        /// </summary>
        private void CreateAdorner(UIElement container, DrawingElement element)
        {
            _adornerLayer = AdornerLayer.GetAdornerLayer(container);
            if (_adornerLayer == null) return;

            _selectionAdorner = new SelectionAdorner(container, element);

            // Connect adorner resize events to ViewModel
            _selectionAdorner.ResizeStarted += OnAdornerResizeStarted;
            _selectionAdorner.ResizeDelta += OnAdornerResizeDelta;
            _selectionAdorner.ResizeCompleted += OnAdornerResizeCompleted;

            _adornerLayer.Add(_selectionAdorner);
        }

        /// <summary>
        /// Handles resize start event from adorner Thumb.
        /// </summary>
        private void OnAdornerResizeStarted(object sender, ResizeEventArgs e)
        {
            if (_viewModel == null) return;
            _viewModel.StartResize(e.HandleType, e.Position);
        }

        /// <summary>
        /// Handles resize delta event from adorner Thumb.
        /// </summary>
        private void OnAdornerResizeDelta(object sender, ResizeEventArgs e)
        {
            if (_viewModel == null) return;
            _viewModel.UpdateResize(e.Position, _isShiftPressed);
        }

        /// <summary>
        /// Handles resize completed event from adorner Thumb.
        /// </summary>
        private void OnAdornerResizeCompleted(object sender, ResizeEventArgs e)
        {
            if (_viewModel == null) return;
            _viewModel.EndResize();
        }

        /// <summary>
        /// Removes the current selection adorner.
        /// </summary>
        private void RemoveSelectionAdorner()
        {
            if (_selectionAdorner != null && _adornerLayer != null)
            {
                // Disconnect events
                _selectionAdorner.ResizeStarted -= OnAdornerResizeStarted;
                _selectionAdorner.ResizeDelta -= OnAdornerResizeDelta;
                _selectionAdorner.ResizeCompleted -= OnAdornerResizeCompleted;

                _selectionAdorner.Detach();
                _adornerLayer.Remove(_selectionAdorner);
            }

            _selectionAdorner = null;
            _adornerLayer = null;
        }

        /// <summary>
        /// Finds the visual container (ContentPresenter) for a DrawingElement.
        /// </summary>
        private FrameworkElement FindContainerForElement(DrawingElement element)
        {
            if (element == null) return null;

            // Get the ItemContainerGenerator from the ItemsControl
            var generator = ElementsItemsControl.ItemContainerGenerator;
            if (generator == null) return null;

            // Get the container for this item
            var container = generator.ContainerFromItem(element) as ContentPresenter;
            if (container == null) return null;

            // We need the actual visual child (the shape template content)
            if (VisualTreeHelper.GetChildrenCount(container) > 0)
            {
                return VisualTreeHelper.GetChild(container, 0) as FrameworkElement ?? container;
            }

            return container;
        }

        #endregion

        #region Inline Text Editing

        /// <summary>
        /// Handles double-click to enter text editing mode.
        /// </summary>
        private void OnCanvasMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_viewModel == null) return;
            if (!_viewModel.IsSelectionToolActive) return;

            var position = e.GetPosition(ZoomableGrid);
            var hitElement = _viewModel.FindElementAtPoint(position);

            if (hitElement != null)
            {
                StartTextEditing(hitElement);
                e.Handled = true;
            }
        }

        /// <summary>
        /// Starts inline text editing for the specified element.
        /// </summary>
        private void StartTextEditing(DrawingElement element)
        {
            _editingElement = element;

            // Position and size the TextBox to match the element
            InlineEditTextBox.Width = element.Width;
            InlineEditTextBox.Height = element.Height;
            Canvas.SetLeft(InlineEditTextBox, element.X);
            Canvas.SetTop(InlineEditTextBox, element.Y);

            // Set text and style
            InlineEditTextBox.Text = element.Text;
            InlineEditTextBox.FontSize = (int)element.FontSize;
            
            // Convert Models.TextAlignment to System.Windows.TextAlignment
            InlineEditTextBox.TextAlignment = element.TextAlign switch
            {
                Models.TextAlignment.Left => System.Windows.TextAlignment.Left,
                Models.TextAlignment.Center => System.Windows.TextAlignment.Center,
                Models.TextAlignment.Right => System.Windows.TextAlignment.Right,
                _ => System.Windows.TextAlignment.Center
            };

            // Show and focus
            InlineEditTextBox.Visibility = Visibility.Visible;
            InlineEditTextBox.Focus();
            InlineEditTextBox.SelectAll();
        }

        /// <summary>
        /// Commits the text changes and exits editing mode.
        /// </summary>
        private void CommitTextEditing()
        {
            if (_editingElement != null)
            {
                _editingElement.Text = InlineEditTextBox.Text;
                _editingElement = null;
            }
            InlineEditTextBox.Visibility = Visibility.Collapsed;
            MainCanvasArea.Focus();
        }

        /// <summary>
        /// Cancels text editing without saving changes.
        /// </summary>
        private void CancelTextEditing()
        {
            _editingElement = null;
            InlineEditTextBox.Visibility = Visibility.Collapsed;
            MainCanvasArea.Focus();
        }

        /// <summary>
        /// Handles focus loss on the inline edit TextBox.
        /// </summary>
        private void OnInlineEditLostFocus(object sender, RoutedEventArgs e)
        {
            CommitTextEditing();
        }

        /// <summary>
        /// Handles key events in the inline edit TextBox.
        /// </summary>
        private void OnInlineEditKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                CancelTextEditing();
                e.Handled = true;
            }
            // Enter allows line breaks (AcceptsReturn=True)
        }

        #endregion
    }
}