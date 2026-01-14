using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using EquipmentDesigner.Models;
using EquipmentDesigner.Views.Drawboard.UMLEngine;
using EquipmentDesigner.Views.Drawboard.UMLEngine.Contexts;

namespace EquipmentDesigner.ViewModels
{
    /// <summary>
    /// Partial class containing move and resize editing operations.
    /// </summary>
    public partial class DrawboardViewModel
    {
        #region Move Operations

        /// <summary>
        /// Starts a move operation on the selected element.
        /// </summary>
        public void StartMove(Point startPoint)
        {
            if (_selectedElement == null || _selectedElement.IsLocked) return;

            _moveContext = DrawingElementEditingEngine.CreateMoveContext(_selectedElement.Bounds, startPoint);
            EditModeState = EditModeState.Moving;
        }

        /// <summary>
        /// Updates the element position during a move operation.
        /// </summary>
        public void UpdateMove(Point currentPoint)
        {
            if (EditModeState != EditModeState.Moving || _selectedElement == null) return;

            var result = DrawingElementEditingEngine.CalculateMove(_moveContext, currentPoint);
            _selectedElement.X = result.NewX;
            _selectedElement.Y = result.NewY;
        }

        /// <summary>
        /// Ends the move operation.
        /// </summary>
        public void EndMove()
        {
            if (EditModeState != EditModeState.Moving) return;
            EditModeState = EditModeState.Selected;

            // Save process after move completes
            SaveProcessAsync();
        }

        #endregion

        #region Resize Operations

        /// <summary>
        /// Starts a resize operation on the selected element.
        /// </summary>
        public void StartResize(ResizeHandleType handle, Point startPoint)
        {
            if (_selectedElement == null || _selectedElement.IsLocked || handle == ResizeHandleType.None) return;

            _resizeContext = DrawingElementEditingEngine.CreateResizeContext(
                _selectedElement.Bounds, handle, startPoint);

            ActiveResizeHandle = handle;
            EditModeState = EditModeState.Resizing;
        }

        /// <summary>
        /// Updates the element size during a resize operation.
        /// Delegates calculation to DrawingElementEditingEngine for consistent behavior.
        /// </summary>
        public void UpdateResize(Point currentPoint, bool maintainAspectRatio)
        {
            if (EditModeState != EditModeState.Resizing || _selectedElement == null) return;

            var result = DrawingElementEditingEngine.CalculateResize(
                _resizeContext, currentPoint, maintainAspectRatio);

            _selectedElement.X = result.NewX;
            _selectedElement.Y = result.NewY;
            _selectedElement.Width = result.NewWidth;
            _selectedElement.Height = result.NewHeight;

            _resizeContext = result.UpdatedContext;
            ActiveResizeHandle = result.ActiveHandle;
        }

        /// <summary>
        /// Ends the resize operation.
        /// </summary>
        public void EndResize()
        {
            if (EditModeState != EditModeState.Resizing) return;
            EditModeState = EditModeState.Selected;
            ActiveResizeHandle = ResizeHandleType.None;

            // Save process after resize completes
            SaveProcessAsync();
        }

        #endregion

        #region Multi-Element Move Operations

        /// <summary>
        /// Starts moving all selected elements.
        /// </summary>
        public void StartMultiMove(Point startPoint)
        {
            if (_selectedElements.Count < 2) return;

            _groupDragStartPoint = startPoint;
            _originalElementBounds = _selectedElements.Select(e => e.Bounds).ToList();
            EditModeState = EditModeState.MultiMoving;
        }

        /// <summary>
        /// Updates positions of all selected elements.
        /// </summary>
        public void UpdateMultiMove(Point currentPoint)
        {
            if (EditModeState != EditModeState.MultiMoving) return;

            var results = DrawingElementEditingEngine.CalculateGroupMove(
                _originalElementBounds,
                _groupDragStartPoint,
                currentPoint);

            for (int i = 0; i < _selectedElements.Count && i < results.Count; i++)
            {
                _selectedElements[i].X = results[i].NewX;
                _selectedElements[i].Y = results[i].NewY;
            }

            OnPropertyChanged(nameof(GroupBounds));
        }

        /// <summary>
        /// Ends multi-element move operation.
        /// </summary>
        public void EndMultiMove()
        {
            if (EditModeState != EditModeState.MultiMoving) return;
            EditModeState = EditModeState.MultiSelected;
            OnPropertyChanged(nameof(GroupBounds));

            // Save process after multi-move completes
            SaveProcessAsync();
        }

        #endregion

        #region Multi-Element Resize Operations

        /// <summary>
        /// Starts resizing all selected elements proportionally.
        /// </summary>
        public void StartMultiResize(ResizeHandleType handle, Point startPoint)
        {
            if (_selectedElements.Count < 2 || handle == ResizeHandleType.None) return;

            _groupResizeContext = DrawingElementEditingEngine.CreateGroupResizeContext(
                _selectedElements,
                handle,
                startPoint);

            ActiveResizeHandle = handle;
            EditModeState = EditModeState.MultiResizing;
        }

        /// <summary>
        /// Updates sizes and positions of all selected elements.
        /// </summary>
        public void UpdateMultiResize(Point currentPoint, bool maintainAspectRatio)
        {
            if (EditModeState != EditModeState.MultiResizing) return;

            var result = DrawingElementEditingEngine.CalculateGroupResize(
                _groupResizeContext,
                currentPoint,
                maintainAspectRatio);

            // Apply transforms
            result.ApplyAll();

            // Update context for next calculation
            _groupResizeContext = result.UpdatedContext;
            ActiveResizeHandle = result.ActiveHandle;

            OnPropertyChanged(nameof(GroupBounds));
        }

        /// <summary>
        /// Ends multi-element resize operation.
        /// </summary>
        public void EndMultiResize()
        {
            if (EditModeState != EditModeState.MultiResizing) return;
            EditModeState = EditModeState.MultiSelected;
            ActiveResizeHandle = ResizeHandleType.None;
            OnPropertyChanged(nameof(GroupBounds));

            // Save process after multi-resize completes
            SaveProcessAsync();
        }

        #endregion
    }
}