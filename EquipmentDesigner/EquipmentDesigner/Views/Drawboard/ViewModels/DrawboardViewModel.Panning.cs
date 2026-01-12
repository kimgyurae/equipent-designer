using System;
using System.Windows;
using EquipmentDesigner.Models;
using EquipmentDesigner.Views.Drawboard.UMLEngine;
using EquipmentDesigner.Views.Drawboard.UMLEngine.Contexts;

namespace EquipmentDesigner.ViewModels
{
    /// <summary>
    /// Partial class containing canvas panning operations for Hand tool.
    /// </summary>
    public partial class DrawboardViewModel
    {
        #region Fields

        private PanContext _panContext;

        #endregion

        #region Properties

        /// <summary>
        /// Callback to apply scroll offset changes to the View.
        /// Set by the View during initialization.
        /// </summary>
        public Action<double, double> ApplyScrollOffset { get; set; }

        /// <summary>
        /// Checks if the Hand tool is currently active.
        /// </summary>
        public bool IsHandToolActive => SelectedTool?.Id == "Hand";

        /// <summary>
        /// Checks if a pan operation is in progress.
        /// </summary>
        public bool IsPanning => EditModeState == EditModeState.Panning;

        #endregion

        #region Pan Operations

        /// <summary>
        /// Starts a pan operation when Hand tool is active or when forced (e.g., middle mouse button).
        /// </summary>
        /// <param name="startPoint">Mouse position at pan start (viewport coordinates).</param>
        /// <param name="force">If true, bypasses Hand tool check (used for middle mouse button panning).</param>
        public void StartPan(Point startPoint, bool force = false)
        {
            if (!force && !IsHandToolActive) return;

            _panContext = CanvasPanEngine.CreatePanContext(
                startPoint,
                new Point(ScrollOffsetX, ScrollOffsetY),
                ZoomScale);

            EditModeState = EditModeState.Panning;
        }

        /// <summary>
        /// Updates the pan position during drag.
        /// </summary>
        /// <param name="currentPoint">Current mouse position (viewport coordinates).</param>
        public void UpdatePan(Point currentPoint)
        {
            if (EditModeState != EditModeState.Panning) return;

            var result = CanvasPanEngine.CalculatePan(_panContext, currentPoint);

            // Request View to apply scroll offset
            ApplyScrollOffset?.Invoke(result.NewScrollOffsetX, result.NewScrollOffsetY);
        }

        /// <summary>
        /// Ends the pan operation.
        /// </summary>
        public void EndPan()
        {
            if (EditModeState != EditModeState.Panning) return;
            EditModeState = EditModeState.None;
        }

        #endregion
    }
}