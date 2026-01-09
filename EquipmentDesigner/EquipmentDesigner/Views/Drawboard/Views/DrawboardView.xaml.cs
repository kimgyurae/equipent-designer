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
        private SelectionAdorner _selectionAdorner;
        private AdornerLayer _adornerLayer;
        private bool _isShiftPressed;
        private DrawboardViewModel _viewModel;

        public DrawboardView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
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
        /// Handles preview mouse left button down for selection logic.
        /// This runs before the regular MouseLeftButtonDown to handle selection first.
        /// </summary>
        private void OnCanvasPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_viewModel == null) return;

            // Only handle selection when Selection tool is active
            if (!_viewModel.IsSelectionToolActive) return;

            var position = e.GetPosition(MainCanvasArea);
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

            var position = e.GetPosition(MainCanvasArea);
            var hitElement = _viewModel.FindElementAtPoint(position);

            if (hitElement != null)
            {
                // Right-click on element: select it
                _viewModel.SelectElement(hitElement);
                MainCanvasArea.Focus();
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
                // Check if clicking on a resize handle
                if (_selectionAdorner != null)
                {
                    var adornerPoint = MainCanvasArea.TranslatePoint(position, _selectionAdorner.AdornedElement);
                    var handleType = _selectionAdorner.HitTestHandle(adornerPoint);

                    if (handleType != ResizeHandleType.None)
                    {
                        // Start resize operation
                        _viewModel.StartResize(handleType, position);
                        MainCanvasArea.CaptureMouse();
                        e.Handled = true;
                        return;
                    }

                    // Check if on element surface - start move
                    if (_selectionAdorner.IsOnElementSurface(adornerPoint))
                    {
                        _viewModel.StartMove(position);
                        MainCanvasArea.CaptureMouse();
                        e.Handled = true;
                        return;
                    }
                }
            }
            else
            {
                // Clicked on different element - select it
                _viewModel.SelectElement(hitElement);
                MainCanvasArea.Focus();
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
                    _viewModel.UpdateResize(position, _isShiftPressed);
                    return;
            }

            // Handle drawing preview
            if (_viewModel.IsDrawing)
            {
                _viewModel.UpdateDrawing(position);
            }
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
        /// Creates the adorner on the specified container.
        /// </summary>
        private void CreateAdorner(UIElement container, DrawingElement element)
        {
            _adornerLayer = AdornerLayer.GetAdornerLayer(container);
            if (_adornerLayer == null) return;

            _selectionAdorner = new SelectionAdorner(container, element);
            _adornerLayer.Add(_selectionAdorner);
        }

        /// <summary>
        /// Removes the current selection adorner.
        /// </summary>
        private void RemoveSelectionAdorner()
        {
            if (_selectionAdorner != null && _adornerLayer != null)
            {
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
    }
}
