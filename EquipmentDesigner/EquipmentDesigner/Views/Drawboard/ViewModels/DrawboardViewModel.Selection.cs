using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using EquipmentDesigner.Models;

namespace EquipmentDesigner.ViewModels
{
    /// <summary>
    /// Partial class containing selection and multi-selection logic.
    /// </summary>
    public partial class DrawboardViewModel
    {
        #region Selection Operations

        /// <summary>
        /// Selects an element for editing.
        /// Locked elements can be selected (to allow unlocking via context menu).
        /// </summary>
        public void SelectElement(DrawingElement element)
        {
            if (element == null) return;

            // Clear previous selection
            if (_selectedElement != null && _selectedElement != element)
            {
                _selectedElement.IsSelected = false;
            }

            element.IsSelected = true;
            SelectedElement = element;
            EditModeState = EditModeState.Selected;
            DeferNotifyUnlockButtonPropertiesChanged();

            // Show connection ports for single selection
            UpdatePortDisplayForSelection();
        }

        /// <summary>
        /// Clears the current selection.
        /// </summary>
        public void ClearSelection()
        {
            // Hide connection ports before clearing selection
            HideConnectionPorts();

            if (_selectedElement != null)
            {
                _selectedElement.IsSelected = false;
                SelectedElement = null;
            }
            EditModeState = EditModeState.None;
            ActiveResizeHandle = ResizeHandleType.None;
            DeferNotifyUnlockButtonPropertiesChanged();
        }

        /// <summary>
        /// Deletes the currently selected element.
        /// </summary>
        public void DeleteSelectedElement()
        {
            if (_selectedElement == null || _selectedElement.IsLocked) return;

            RemoveStepFromCurrentWorkflow(_selectedElement);
            Elements.Remove(_selectedElement);
            ClearSelection();
        }

        /// <summary>
        /// Finds an element at the specified canvas point.
        /// </summary>
        /// <param name="point">The point in canvas coordinates.</param>
        /// <returns>The topmost element at the point, or null if none found.</returns>
        public DrawingElement FindElementAtPoint(Point point)
        {
            // Search in reverse ZIndex order (topmost first)
            return Elements
                .OrderByDescending(e => e.ZIndex)
                .FirstOrDefault(e => e.Bounds.Contains(point));
        }

        #endregion

        #region Multi-Selection Methods

        /// <summary>
        /// Adds or removes element from selection (Shift+Click toggle).
        /// Locked elements can be toggled for removal but cannot be added to multi-selection.
        /// </summary>
        public void ToggleSelection(DrawingElement element)
        {
            if (element == null) return;

            if (_selectedElements.Contains(element))
            {
                // Remove from selection
                element.IsSelected = false;
                _selectedElements.Remove(element);

                // Update state based on remaining selection
                if (_selectedElements.Count == 0)
                {
                    SelectedElement = null;
                    EditModeState = EditModeState.None;
                    // Hide ports when no selection
                    HideConnectionPorts();
                }
                else if (_selectedElements.Count == 1)
                {
                    SelectedElement = _selectedElements[0];
                    EditModeState = EditModeState.Selected;
                    // Show ports for single selection
                    UpdatePortDisplayForSelection();
                }
            }
            else
            {
                // Locked elements cannot be added to multi-selection
                if (element.IsLocked)
                {
                    return;
                }

                // Bridge: If there's an existing single selection not in the list, add it first
                if (_selectedElement != null && !_selectedElements.Contains(_selectedElement))
                {
                    _selectedElements.Add(_selectedElement);
                }

                // Add to selection
                element.IsSelected = true;
                _selectedElements.Add(element);

                if (_selectedElements.Count == 1)
                {
                    SelectedElement = element;
                    EditModeState = EditModeState.Selected;
                    // Show ports for single selection
                    UpdatePortDisplayForSelection();
                }
                else
                {
                    SelectedElement = null;
                    EditModeState = EditModeState.MultiSelected;
                    // Hide ports in multi-selection mode
                    HideConnectionPorts();
                }
            }

            OnPropertyChanged(nameof(IsMultiSelectionMode));
            OnPropertyChanged(nameof(GroupBounds));
        }

        /// <summary>
        /// Adds element to current selection without clearing.
        /// Handles transition from single-selection (via SelectElement) to multi-selection.
        /// Locked elements cannot be added to multi-selection.
        /// </summary>
        public void AddToSelection(DrawingElement element)
        {
            if (element == null)
            {
                return;
            }
            if (_selectedElements.Contains(element))
            {
                return;
            }
            // Locked elements cannot be added to multi-selection
            if (element.IsLocked)
            {
                return;
            }

            // Bridge single-selection to multi-selection:
            // If there's an existing single selection that's not in the list, add it first
            if (_selectedElement != null && !_selectedElements.Contains(_selectedElement))
            {
                _selectedElements.Add(_selectedElement);
            }

            element.IsSelected = true;
            _selectedElements.Add(element);

            if (_selectedElements.Count == 1)
            {
                SelectedElement = element;
                EditModeState = EditModeState.Selected;
                // Show ports for single selection
                UpdatePortDisplayForSelection();
            }
            else
            {
                SelectedElement = null;
                EditModeState = EditModeState.MultiSelected;
                // Hide ports in multi-selection mode
                HideConnectionPorts();
            }

            OnPropertyChanged(nameof(IsMultiSelectionMode));
            OnPropertyChanged(nameof(GroupBounds));
        }

        /// <summary>
        /// Clears all selections.
        /// </summary>
        public void ClearAllSelections()
        {
            // Hide connection ports before clearing
            HideConnectionPorts();

            foreach (var element in _selectedElements)
            {
                element.IsSelected = false;
            }
            _selectedElements.Clear();

            if (_selectedElement != null)
            {
                _selectedElement.IsSelected = false;
                SelectedElement = null;
            }

            EditModeState = EditModeState.None;
            ActiveResizeHandle = ResizeHandleType.None;

            OnPropertyChanged(nameof(IsMultiSelectionMode));
            OnPropertyChanged(nameof(GroupBounds));
            DeferNotifyUnlockButtonPropertiesChanged();
        }

        /// <summary>
        /// Finds all elements fully contained within the given rectangle.
        /// </summary>
        public IEnumerable<DrawingElement> FindElementsInRect(Rect selectionRect)
        {
            return Elements.Where(e => selectionRect.Contains(e.Bounds));
        }

        #endregion

        #region Rubberband Selection

        /// <summary>
        /// Starts rubberband selection from empty space.
        /// </summary>
        public void StartRubberbandSelection(Point startPoint)
        {
            _rubberbandStartPoint = startPoint;
            RubberbandRect = new Rect(startPoint, new Size(0, 0));
            IsRubberbandSelecting = true;
            EditModeState = EditModeState.RubberbandSelecting;
        }

        /// <summary>
        /// Updates rubberband rectangle during drag.
        /// </summary>
        public void UpdateRubberbandSelection(Point currentPoint)
        {
            if (!_isRubberbandSelecting) return;

            double x = Math.Min(_rubberbandStartPoint.X, currentPoint.X);
            double y = Math.Min(_rubberbandStartPoint.Y, currentPoint.Y);
            double width = Math.Abs(currentPoint.X - _rubberbandStartPoint.X);
            double height = Math.Abs(currentPoint.Y - _rubberbandStartPoint.Y);

            RubberbandRect = new Rect(x, y, width, height);
        }

        /// <summary>
        /// Finishes rubberband selection and selects contained elements.
        /// Locked elements are excluded from rubberband selection.
        /// </summary>
        public void FinishRubberbandSelection()
        {
            if (!_isRubberbandSelecting) return;

            // Find elements fully contained in the rubberband (excluding locked elements)
            var containedElements = FindElementsInRect(_rubberbandRect)
                .Where(e => !e.IsLocked)
                .ToList();

            // Clear rubberband state
            IsRubberbandSelecting = false;
            RubberbandRect = new Rect(0, 0, 0, 0);

            if (containedElements.Count == 0)
            {
                EditModeState = EditModeState.None;
                return;
            }

            // Select all contained elements
            _selectedElements.Clear();
            foreach (var element in containedElements)
            {
                element.IsSelected = true;
                _selectedElements.Add(element);
            }

            if (_selectedElements.Count == 1)
            {
                SelectedElement = _selectedElements[0];
                EditModeState = EditModeState.Selected;
            }
            else
            {
                SelectedElement = null;
                EditModeState = EditModeState.MultiSelected;
            }

            OnPropertyChanged(nameof(IsMultiSelectionMode));
            OnPropertyChanged(nameof(GroupBounds));
        }

        /// <summary>
        /// Cancels rubberband selection.
        /// </summary>
        public void CancelRubberbandSelection()
        {
            IsRubberbandSelecting = false;
            RubberbandRect = new Rect(0, 0, 0, 0);
            EditModeState = EditModeState.None;
        }

        #endregion
    }
}