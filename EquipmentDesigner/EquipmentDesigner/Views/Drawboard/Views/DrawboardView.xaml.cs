using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using EquipmentDesigner.Models;
using EquipmentDesigner.ViewModels;
using EquipmentDesigner.Views.Drawboard.Adorners;
using EquipmentDesigner.Views.Drawboard.UMLEngine;
using EquipmentDesigner.Views.Drawboard.UMLEngine.Contexts;

namespace EquipmentDesigner.Views
{
    /// <summary>
    /// Interaction logic for DrawboardView.xaml
    /// </summary>
    public partial class DrawboardView : UserControl
    {
        private SelectionAdorner _selectionAdorner;
        private MultiSelectionAdorner _multiSelectionAdorner;
        private AdornerLayer _adornerLayer;
        private bool _isShiftPressed;
        private bool _isSpaceHeld;
        private DrawboardViewModel _viewModel;

        public DrawboardView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
            PreviewKeyDown += OnDrawboardPreviewKeyDown;
            Loaded += OnDrawboardLoaded;
        }

        /// <summary>
        /// Sets keyboard focus and centers the canvas when the view is loaded.
        /// Uses Dispatcher.BeginInvoke to ensure ScrollViewer layout is complete.
        /// </summary>
        private void OnDrawboardLoaded(object sender, RoutedEventArgs e)
        {
            Focusable = true;
            Focus();

            // Center canvas after layout is complete
            // Must use Dispatcher to ensure ScrollViewer has valid ViewportWidth/Height
            Dispatcher.BeginInvoke(new Action(CenterCanvas), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        /// <summary>
        /// Centers the canvas in the viewport.
        /// Should be called after ScrollViewer layout is complete.
        /// </summary>
        private void CenterCanvas()
        {
            if (_viewModel == null) return;

            // Get viewport size from ScrollViewer
            var viewportSize = new Size(
                CanvasScrollViewer.ViewportWidth,
                CanvasScrollViewer.ViewportHeight);

            // Skip if viewport not yet laid out (safety check)
            if (viewportSize.Width <= 0 || viewportSize.Height <= 0) return;

            // Get canvas size from ViewModel
            var canvasSize = new Size(_viewModel.CanvasWidth, _viewModel.CanvasHeight);

            // Calculate centered scroll offset
            var centerOffset = ZoomControlEngine.CalculateCenterScrollOffset(
                canvasSize,
                viewportSize,
                _viewModel.ZoomScale);

            // Apply scroll offset
            CanvasScrollViewer.ScrollToHorizontalOffset(centerOffset.X);
            CanvasScrollViewer.ScrollToVerticalOffset(centerOffset.Y);
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
                _viewModel.TextEditingStarted -= OnTextEditingStarted;
                _viewModel.TextEditingEnded -= OnTextEditingEnded;
                _viewModel.TextBoxBoundsChanged -= OnTextBoxBoundsChanged;
            }

            // Force layout update to ensure toolbar resizes correctly
            // when DataContext is set after view creation
            if (e.NewValue is DrawboardViewModel viewModel)
            {
                _viewModel = viewModel;
                _viewModel.PropertyChanged += ViewModel_PropertyChanged;
                _viewModel.TextEditingStarted += OnTextEditingStarted;
                _viewModel.TextEditingEnded += OnTextEditingEnded;
                _viewModel.TextBoxBoundsChanged += OnTextBoxBoundsChanged;

                // Set up scroll offset callback for pan operations
                _viewModel.ApplyScrollOffset = ApplyScrollOffset;

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
            if (e.PropertyName == nameof(DrawboardViewModel.SelectedElement) ||
                e.PropertyName == nameof(DrawboardViewModel.IsMultiSelectionMode))
            {
                UpdateAdorners();
            }
            else if (e.PropertyName == nameof(DrawboardViewModel.IsRubberbandSelecting))
            {
                UpdateRubberbandVisibility();
            }
        }

        /// <summary>
        /// Updates adorners based on current selection state.
        /// </summary>
        private void UpdateAdorners()
        {
            if (_viewModel == null) return;

            if (_viewModel.IsMultiSelectionMode)
            {
                // Multi-selection mode: use MultiSelectionAdorner
                RemoveSelectionAdorner();
                UpdateMultiSelectionAdorner();
            }
            else if (_viewModel.SelectedElement != null)
            {
                // Single selection mode: use SelectionAdorner
                RemoveMultiSelectionAdorner();
                UpdateSelectionAdorner(_viewModel.SelectedElement);
            }
            else
            {
                // No selection: remove all adorners
                // IMPORTANT: RemoveMultiSelectionAdorner must be called first because
                // RemoveSelectionAdorner sets _adornerLayer = null, which would cause
                // RemoveMultiSelectionAdorner's null check to fail
                RemoveMultiSelectionAdorner();
                RemoveSelectionAdorner();
            }
        }

        /// <summary>
        /// Updates the rubberband rectangle visibility.
        /// </summary>
        private void UpdateRubberbandVisibility()
        {
            if (_viewModel == null) return;
            RubberbandRect.Visibility = _viewModel.IsRubberbandSelecting
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        #region Selection Event Handlers

        /// <summary>
        /// Handles Ctrl+Mouse Wheel for zoom control with focus on mouse position.
        /// Uses ZoomControlEngine for calculation.
        /// </summary>
        private void OnCanvasPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers != ModifierKeys.Control) return;
            if (_viewModel == null) return;

            // Create zoom context from current viewport state
            var context = ZoomControlEngine.CreateZoomContext(
                currentZoomLevel: _viewModel.ZoomLevel,
                mousePosition: e.GetPosition(CanvasScrollViewer),
                scrollOffset: new Point(CanvasScrollViewer.HorizontalOffset, CanvasScrollViewer.VerticalOffset),
                viewportSize: new Size(CanvasScrollViewer.ViewportWidth, CanvasScrollViewer.ViewportHeight));

            // Calculate zoom result
            var result = ZoomControlEngine.CalculateMouseWheelZoom(context, zoomIn: e.Delta > 0);

            // If zoom level didn't change, exit
            if (!result.ZoomChanged)
            {
                e.Handled = true;
                return;
            }

            // Apply zoom level
            _viewModel.ZoomLevel = result.NewZoomLevel;

            // Force layout update to ensure ScrollViewer extent is recalculated
            CanvasScrollViewer.UpdateLayout();

            // Apply scroll offset
            CanvasScrollViewer.ScrollToHorizontalOffset(result.NewScrollOffset.X);
            CanvasScrollViewer.ScrollToVerticalOffset(result.NewScrollOffset.Y);

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
        /// Handles Zoom Level TextBlock click to reset zoom to 100%.
        /// Maintains viewport center as focus point.
        /// </summary>
        private void OnZoomLevelClick(object sender, MouseButtonEventArgs e)
        {
            ResetZoomToDefault();
            e.Handled = true;
        }

        /// <summary>
        /// Resets zoom level to 100% while maintaining viewport center position.
        /// Uses ZoomControlEngine for calculation.
        /// </summary>
        private void ResetZoomToDefault()
        {
            if (_viewModel == null) return;

            // Create zoom context from current viewport state
            var context = ZoomControlEngine.CreateZoomContext(
                currentZoomLevel: _viewModel.ZoomLevel,
                mousePosition: new Point(0, 0), // Not used for reset
                scrollOffset: new Point(CanvasScrollViewer.HorizontalOffset, CanvasScrollViewer.VerticalOffset),
                viewportSize: new Size(CanvasScrollViewer.ViewportWidth, CanvasScrollViewer.ViewportHeight));

            // Calculate zoom reset result
            var result = ZoomControlEngine.CalculateZoomReset(context);

            // If zoom level didn't change, exit
            if (!result.ZoomChanged) return;

            // Apply zoom level
            _viewModel.ZoomLevel = result.NewZoomLevel;

            // Force layout update to ensure ScrollViewer extent is recalculated
            CanvasScrollViewer.UpdateLayout();

            // Apply scroll offset
            CanvasScrollViewer.ScrollToHorizontalOffset(result.NewScrollOffset.X);
            CanvasScrollViewer.ScrollToVerticalOffset(result.NewScrollOffset.Y);
        }

        /// <summary>
        /// Applies zoom while maintaining viewport center position.
        /// Uses ZoomControlEngine for calculation.
        /// </summary>
        private void ZoomAtViewportCenter(bool zoomIn)
        {
            if (_viewModel == null) return;

            // Create zoom context from current viewport state
            var context = ZoomControlEngine.CreateZoomContext(
                currentZoomLevel: _viewModel.ZoomLevel,
                mousePosition: new Point(0, 0), // Not used for viewport center zoom
                scrollOffset: new Point(CanvasScrollViewer.HorizontalOffset, CanvasScrollViewer.VerticalOffset),
                viewportSize: new Size(CanvasScrollViewer.ViewportWidth, CanvasScrollViewer.ViewportHeight));

            // Calculate zoom result
            var result = ZoomControlEngine.CalculateViewportCenterZoom(context, zoomIn);

            // If zoom level didn't change, exit
            if (!result.ZoomChanged) return;

            // Apply zoom level
            _viewModel.ZoomLevel = result.NewZoomLevel;

            // Force layout update to ensure ScrollViewer extent is recalculated
            CanvasScrollViewer.UpdateLayout();

            // Apply scroll offset
            CanvasScrollViewer.ScrollToHorizontalOffset(result.NewScrollOffset.X);
            CanvasScrollViewer.ScrollToVerticalOffset(result.NewScrollOffset.Y);
        }

        /// <summary>
        /// Handles keyboard shortcuts for tool selection.
        /// </summary>
        private void OnDrawboardPreviewKeyDown(object sender, KeyEventArgs e)
        {
            // If text editing is active, let the TextBox handle ESC
            if (e.Key == Key.Escape && _viewModel?.IsTextEditing == true)
            {
                return;  // OnInlineEditKeyDown will handle this
            }

            // ESC key: Exit edit mode (clear selection) - works even in TextBox
            if (e.Key == Key.Escape)
            {
                if (DataContext is DrawboardViewModel viewModel)
                {
                    // Handle both single selection and multi-selection
                    if (viewModel.SelectedElement != null || viewModel.IsMultiSelectionMode)
                    {
                        viewModel.ClearAllSelections();
                        e.Handled = true;
                    }
                }
                return;
            }

            // Ignore other keys when typing in TextBox
            if (e.OriginalSource is TextBox)
                return;

            // Space key: Enable space panning mode
            if (e.Key == Key.Space)
            {
                _isSpaceHeld = true;
                e.Handled = true;  // Prevent default scroll behavior
                return;
            }

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
            if (_viewModel == null)
            {
                return;
            }

            // Handle double-click for text editing (Grid doesn't support MouseDoubleClick event)
            if (e.ClickCount == 2 && _viewModel.IsSelectionToolActive)
            {
                OnCanvasMouseDoubleClick(sender, e);
                if (e.Handled) return;
            }

            // Note: Unlock button is now outside ZoomableGrid, so no need to check for it here

            // Handle Hand tool panning
            if (_viewModel.IsHandToolActive)
            {
                // Get position relative to ScrollViewer (viewport coordinates)
                var panPosition = e.GetPosition(CanvasScrollViewer);
                _viewModel.StartPan(panPosition);
                // Capture on ZoomableGrid (same as other edit modes) to ensure MouseLeftButtonUp is received
                ZoomableGrid.CaptureMouse();
                e.Handled = true;
                return;
            }

            // Only handle selection when Selection tool is active
            if (!_viewModel.IsSelectionToolActive)
            {
                return;
            }

            var position = e.GetPosition(ZoomableGrid);
            HandleSelectionClick(position, e);
        }

        /// <summary>
        /// Handles preview mouse right button down for deselection (per spec: Button Down triggers deselection).
        /// </summary>
        private void OnCanvasPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_viewModel == null) return;

            // Only handle selection when Selection tool is active
            if (!_viewModel.IsSelectionToolActive) return;

            var position = e.GetPosition(ZoomableGrid);
            var hitElement = _viewModel.FindElementAtPoint(position);

            // Track Shift key state
            _isShiftPressed = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);

            if (hitElement == null)
            {
                // Right-click on empty space: clear all selections
                _viewModel.ClearAllSelections();
                e.Handled = true;
                return;
            }

            // Shift+Click: Toggle selection (same as left-click)
            if (_isShiftPressed)
            {
                _viewModel.ToggleSelection(hitElement);
                ZoomableGrid.Focus();
                e.Handled = true;
                return;
            }

            // Handle multi-selection mode
            if (_viewModel.IsMultiSelectionMode)
            {
                if (!_viewModel.SelectedElements.Contains(hitElement))
                {
                    // Right-click on non-selected element: clear all and select new one
                    _viewModel.ClearAllSelections();
                    _viewModel.SelectElement(hitElement);
                }
                // If clicking on a selected element in multi-select, keep multi-selection
            }
            else if (hitElement != _viewModel.SelectedElement)
            {
                // Right-click on different element in single selection: select the new one
                _viewModel.SelectElement(hitElement);
            }
            // If clicking on already selected element, keep selection

            ZoomableGrid.Focus();
            // Don't set e.Handled so context menu can show on ButtonUp if needed
        }

        /// <summary>
        /// Handles preview mouse right button up for context menu (selection already handled on ButtonDown).
        /// </summary>
        private void OnCanvasPreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_viewModel == null) return;

            // Only show context menu when Selection tool is active
            if (!_viewModel.IsSelectionToolActive) return;

            var position = e.GetPosition(ZoomableGrid);
            var hitElement = _viewModel.FindElementAtPoint(position);

            // Only show context menu if right-clicked on a shape (not empty space)
            if (hitElement != null && _viewModel.CanShowContextMenu)
            {
                _viewModel.ShowContextMenu();
                e.Handled = true;
            }
        }

        /// <summary>
        /// Common logic for handling selection clicks.
        /// Supports Shift+Click for multi-selection and rubberband selection.
        /// Note: Resize operations are now handled directly by Thumb drag events in SelectionAdorner.
        /// </summary>
        private void HandleSelectionClick(Point position, MouseButtonEventArgs e)
        {            
            // Check Shift key state at click time, not relying on cached KeyDown state
            bool keyboardShiftState = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
            
            _isShiftPressed = keyboardShiftState;

            var hitElement = _viewModel.FindElementAtPoint(position);

            if (hitElement == null)
            {
                // Clicked on empty space
                if (!_isShiftPressed)
                {
                    // Clear selection and start rubberband selection
                    _viewModel.ClearAllSelections();
                }
                // Start rubberband selection (works with or without Shift)
                _viewModel.StartRubberbandSelection(position);
                UpdateRubberbandVisibility();
                ZoomableGrid.CaptureMouse();
                e.Handled = true;
                return;
            }

            // Shift+Click: Toggle selection (add/remove from multi-selection)
            if (_isShiftPressed)
            {
                _viewModel.ToggleSelection(hitElement);
                ZoomableGrid.Focus();
                e.Handled = true;
                return;
            }

            // Check if we clicked on already selected element (single or multi-selection)
            if (_viewModel.IsMultiSelectionMode)
            {
                // In multi-selection mode, check if clicked on any selected element
                if (_viewModel.SelectedElements.Contains(hitElement))
                {
                    // Check if on group surface - start multi-move
                    if (_multiSelectionAdorner != null && _multiSelectionAdorner.IsOnGroupSurface(position))
                    {
                        _viewModel.StartMultiMove(position);
                        ZoomableGrid.CaptureMouse();
                        e.Handled = true;
                        return;
                    }
                }
                else
                {
                    // Clicked on non-selected element - clear and select new one
                    _viewModel.ClearAllSelections();
                    _viewModel.SelectElement(hitElement);
                    // Only start move and capture mouse for non-locked elements
                    if (!hitElement.IsLocked)
                    {
                        _viewModel.StartMove(position);
                        ZoomableGrid.CaptureMouse();
                    }
                    ZoomableGrid.Focus();
                    e.Handled = true;
                }
            }
            else if (hitElement == _viewModel.SelectedElement)
            {
                // Check if on element surface - start move (only for non-locked elements)
                if (_selectionAdorner != null && !hitElement.IsLocked)
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
                // Only start move and capture mouse for non-locked elements
                if (!hitElement.IsLocked)
                {
                    _viewModel.StartMove(position);
                    ZoomableGrid.CaptureMouse();
                }
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
                // Delete all selected elements (multi-selection or single)
                if (_viewModel.IsMultiSelectionMode)
                {
                    // Delete all elements in multi-selection
                    var elementsToDelete = _viewModel.SelectedElements.ToArray();
                    _viewModel.ClearAllSelections();
                    foreach (var element in elementsToDelete)
                    {
                        if (!element.IsLocked)
                        {
                            _viewModel.Elements.Remove(element);
                        }
                    }
                    e.Handled = true;
                }
                else if (_viewModel.SelectedElement != null)
                {
                    _viewModel.DeleteSelectedElement();
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.L && Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift))
            {
                // Ctrl+Shift+L: Toggle lock/unlock for selected elements
                if (_viewModel.SelectedElement != null || _viewModel.IsMultiSelectionMode)
                {
                    _viewModel.ToggleLock();
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.C && Keyboard.Modifiers == ModifierKeys.Control)
            {
                // Ctrl+C: Copy selected elements
                _viewModel.CopyToClipboard();
                e.Handled = true;
            }
            else if (e.Key == Key.V && Keyboard.Modifiers == ModifierKeys.Control)
            {
                // Ctrl+V: Paste elements from clipboard
                _viewModel.PasteFromClipboard();
                e.Handled = true;
            }
        }

        /// <summary>
        /// Handles key up to track Shift and Space key state.
        /// </summary>
        protected override void OnPreviewKeyUp(KeyEventArgs e)
        {
            base.OnPreviewKeyUp(e);

            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
            {
                _isShiftPressed = false;
            }
            else if (e.Key == Key.Space)
            {
                _isSpaceHeld = false;
                // End panning if it was active
                if (_viewModel?.EditModeState == EditModeState.Panning)
                {
                    _viewModel.EndPan();
                    ZoomableGrid.ReleaseMouseCapture();
                }
            }
        }

        #endregion

        #region Drawing and Edit Event Handlers

        /// <summary>
        /// Handles preview mouse down for middle button panning.
        /// Middle mouse button enables panning regardless of current tool.
        /// </summary>
        private void OnCanvasPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Middle) return;
            if (_viewModel == null) return;

            // Get position relative to ScrollViewer (viewport coordinates)
            var panPosition = e.GetPosition(CanvasScrollViewer);
            _viewModel.StartPan(panPosition, force: true);
            ZoomableGrid.CaptureMouse();
            e.Handled = true;
        }

        /// <summary>
        /// Handles mouse up for middle button panning completion.
        /// </summary>
        private void OnCanvasMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Middle) return;
            if (_viewModel == null) return;
            if (_viewModel.EditModeState != EditModeState.Panning) return;

            _viewModel.EndPan();
            ((UIElement)sender).ReleaseMouseCapture();
            e.Handled = true;
        }

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

            // Handle Space key panning: Start on first mouse move while Space is held
            if (_isSpaceHeld && _viewModel.EditModeState != EditModeState.Panning)
            {
                var panPosition = e.GetPosition(CanvasScrollViewer);
                _viewModel.StartPan(panPosition, force: true);
                ZoomableGrid.CaptureMouse();
                return;
            }

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

                case EditModeState.RubberbandSelecting:
                    _viewModel.UpdateRubberbandSelection(position);
                    return;

                case EditModeState.MultiMoving:
                    _viewModel.UpdateMultiMove(position);
                    return;

                case EditModeState.MultiResizing:
                    // Multi-resize is handled by MultiSelectionAdorner.OnThumbDragDelta
                    return;

                case EditModeState.Panning:
                    // Get position relative to ScrollViewer for pan calculations
                    var viewportPosition = e.GetPosition(CanvasScrollViewer);
                    _viewModel.UpdatePan(viewportPosition);
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

                case EditModeState.RubberbandSelecting:
                    _viewModel.FinishRubberbandSelection();
                    UpdateRubberbandVisibility();
                    ((UIElement)sender).ReleaseMouseCapture();
                    ZoomableGrid.Focus();  // Restore keyboard focus for Delete key handling
                    e.Handled = true;
                    return;

                case EditModeState.MultiMoving:
                    _viewModel.EndMultiMove();
                    ((UIElement)sender).ReleaseMouseCapture();
                    e.Handled = true;
                    return;

                case EditModeState.MultiResizing:
                    _viewModel.EndMultiResize();
                    ((UIElement)sender).ReleaseMouseCapture();
                    e.Handled = true;
                    return;

                case EditModeState.Panning:
                    _viewModel.EndPan();
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

            // Commit text editing before resize (exit text edit mode, save text changes)
            if (_viewModel.IsTextEditing)
            {
                _viewModel.CommitTextEditing(InlineEditTextBox.Text);
            }

            _viewModel.StartResize(e.HandleType, e.Position);
        }

        /// <summary>
        /// Handles resize delta event from adorner Thumb.
        /// </summary>
        private void OnAdornerResizeDelta(object sender, ResizeEventArgs e)
        {
            if (_viewModel == null) return;
            bool isShiftHeld = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
            _viewModel.UpdateResize(e.Position, isShiftHeld);
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
        /// Updates or creates the multi-selection adorner for the selected elements.
        /// </summary>
        private void UpdateMultiSelectionAdorner()
        {
            if (_viewModel == null || _viewModel.SelectedElements.Count < 2)
            {
                RemoveMultiSelectionAdorner();
                return;
            }

            // If adorner already exists, just update its bounds
            if (_multiSelectionAdorner != null)
            {
                _multiSelectionAdorner.UpdateGroupBounds();
                return;
            }

            // Create new adorner attached to ZoomableGrid
            _adornerLayer = AdornerLayer.GetAdornerLayer(ZoomableGrid);
            if (_adornerLayer == null) return;

            _multiSelectionAdorner = new MultiSelectionAdorner(ZoomableGrid, _viewModel.SelectedElements);

            // Connect adorner resize events to ViewModel
            _multiSelectionAdorner.ResizeStarted += OnMultiAdornerResizeStarted;
            _multiSelectionAdorner.ResizeDelta += OnMultiAdornerResizeDelta;
            _multiSelectionAdorner.ResizeCompleted += OnMultiAdornerResizeCompleted;

            _adornerLayer.Add(_multiSelectionAdorner);
        }

        /// <summary>
        /// Handles resize start event from multi-selection adorner Thumb.
        /// </summary>
        private void OnMultiAdornerResizeStarted(object sender, ResizeEventArgs e)
        {
            if (_viewModel == null) return;

            // Commit text editing before resize (exit text edit mode, save text changes)
            if (_viewModel.IsTextEditing)
            {
                _viewModel.CommitTextEditing(InlineEditTextBox.Text);
            }

            _viewModel.StartMultiResize(e.HandleType, e.Position);
        }

        /// <summary>
        /// Handles resize delta event from multi-selection adorner Thumb.
        /// </summary>
        private void OnMultiAdornerResizeDelta(object sender, ResizeEventArgs e)
        {
            if (_viewModel == null) return;
            bool isShiftHeld = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
            _viewModel.UpdateMultiResize(e.Position, isShiftHeld);
        }

        /// <summary>
        /// Handles resize completed event from multi-selection adorner Thumb.
        /// </summary>
        private void OnMultiAdornerResizeCompleted(object sender, ResizeEventArgs e)
        {
            if (_viewModel == null) return;
            _viewModel.EndMultiResize();
        }

        /// <summary>
        /// Removes the current multi-selection adorner.
        /// </summary>
        private void RemoveMultiSelectionAdorner()
        {
            if (_multiSelectionAdorner == null) return;

            // Get adorner layer directly from ZoomableGrid to avoid dependency on shared _adornerLayer
            // which may point to a different element's layer (SelectionAdorner uses element container)
            var adornerLayer = AdornerLayer.GetAdornerLayer(ZoomableGrid);
            if (adornerLayer != null)
            {
                // Disconnect events
                _multiSelectionAdorner.ResizeStarted -= OnMultiAdornerResizeStarted;
                _multiSelectionAdorner.ResizeDelta -= OnMultiAdornerResizeDelta;
                _multiSelectionAdorner.ResizeCompleted -= OnMultiAdornerResizeCompleted;

                _multiSelectionAdorner.Detach();
                adornerLayer.Remove(_multiSelectionAdorner);
            }

            _multiSelectionAdorner = null;
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
        /// Handles double-click to enter text editing mode or create Textbox on empty space.
        /// </summary>
        private void OnCanvasMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_viewModel == null) return;
            if (!_viewModel.IsSelectionToolActive) return;

            var position = e.GetPosition(ZoomableGrid);
            var hitElement = _viewModel.FindElementAtPoint(position);

            if (hitElement != null)
            {
                // Double-click on existing element: enter text editing mode
                if (_viewModel.TryStartTextEditing(hitElement))
                {
                    e.Handled = true;
                }
            }
            else
            {
                // Double-click on empty space: create Textbox at position
                if (_viewModel.CreateTextboxAtPosition(position))
                {
                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// Handles text editing started event from ViewModel.
        /// </summary>
        private void OnTextEditingStarted(object sender, TextEditingEventArgs e)
        {
            // Adjust TextBox position and size to account for its border and padding.
            // TextSafeBounds represents where text content should render.
            // TextBox must be positioned so its internal text area aligns with TextSafeBounds.
            double offset = TextEditingEngine.InlineTextBoxContentOffset;

            Canvas.SetLeft(InlineEditTextBox, e.TextBoxBounds.X - offset);
            Canvas.SetTop(InlineEditTextBox, e.TextBoxBounds.Y - offset);
            InlineEditTextBox.Width = e.TextBoxBounds.Width + 2 * offset;
            InlineEditTextBox.Height = e.TextBoxBounds.Height + 2 * offset;

            // Set text and style
            InlineEditTextBox.Text = e.InitialText;
            InlineEditTextBox.FontSize = e.FontSize;
            InlineEditTextBox.TextAlignment = e.TextAlignment;

            // Show and focus
            InlineEditTextBox.Visibility = Visibility.Visible;
            InlineEditTextBox.Focus();
            
            // Use Dispatcher to ensure SelectAll works after focus is fully established
            // This fixes the issue where existing text is not selected on edit start
            Dispatcher.BeginInvoke(new Action(() =>
            {
                InlineEditTextBox.SelectAll();
            }), System.Windows.Threading.DispatcherPriority.Input);
        }

        /// <summary>
        /// Handles text editing ended event from ViewModel.
        /// </summary>
        private void OnTextEditingEnded(object sender, TextEditingEventArgs e)
        {
            InlineEditTextBox.Visibility = Visibility.Collapsed;
            MainCanvasArea.Focus();
        }

        /// <summary>
        /// Handles TextBox bounds changed event from ViewModel (when element resizes).
        /// </summary>
        private void OnTextBoxBoundsChanged(object sender, TextEditingEventArgs e)
        {
            // Apply same offset adjustment as OnTextEditingStarted
            double offset = TextEditingEngine.InlineTextBoxContentOffset;

            Canvas.SetLeft(InlineEditTextBox, e.TextBoxBounds.X - offset);
            Canvas.SetTop(InlineEditTextBox, e.TextBoxBounds.Y - offset);
            InlineEditTextBox.Width = e.TextBoxBounds.Width + 2 * offset;
            InlineEditTextBox.Height = e.TextBoxBounds.Height + 2 * offset;
        }

        /// <summary>
        /// Handles text changed in the inline edit TextBox.
        /// Notifies ViewModel to check if element resize is needed.
        /// </summary>
        private void OnInlineEditTextChanged(object sender, TextChangedEventArgs e)
        {
            _viewModel?.UpdateTextContent(InlineEditTextBox.Text);
        }

        /// <summary>
        /// Handles focus loss on the inline edit TextBox.
        /// </summary>
        private void OnInlineEditLostFocus(object sender, RoutedEventArgs e)
        {
            if (_viewModel?.IsTextEditing == true)
            {
                _viewModel.CommitTextEditing(InlineEditTextBox.Text);
            }
        }

        /// <summary>
        /// Handles key events in the inline edit TextBox.
        /// ESC commits the current text and ends editing.
        /// </summary>
        private void OnInlineEditKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                _viewModel?.CommitTextEditing(InlineEditTextBox.Text);
                e.Handled = true;
            }
            // Enter allows line breaks (AcceptsReturn=True)
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Applies scroll offset from ViewModel pan calculations.
        /// </summary>
        private void ApplyScrollOffset(double horizontalOffset, double verticalOffset)
        {
            CanvasScrollViewer.ScrollToHorizontalOffset(horizontalOffset);
            CanvasScrollViewer.ScrollToVerticalOffset(verticalOffset);
        }

        /// <summary>
        /// Handles ScrollViewer scroll changes to update unlock button position.
        /// </summary>
        private void OnScrollViewerScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            _viewModel?.UpdateScrollOffset(e.HorizontalOffset, e.VerticalOffset);
        }

        /// <summary>
        /// Handles the floating unlock button click event.
        /// </summary>
        private void OnUnlockFloatingButtonClick(object sender, RoutedEventArgs e)
        {
            _viewModel?.UnlockSingleSelectedElement();
            MainCanvasArea.Focus();  // Restore focus to enable keyboard shortcuts (Ctrl+Shift+L)
            e.Handled = true;  // Prevent event bubbling
        }

        #endregion
    }
}