using System;
using System.Windows;
using EquipmentDesigner.Models;
using EquipmentDesigner.Views.Drawboard.UMLEngine;

namespace EquipmentDesigner.ViewModels
{
    /// <summary>
    /// Partial class containing element creation and drawing logic.
    /// </summary>
    public partial class DrawboardViewModel
    {
        #region Element Creation

        /// <summary>
        /// Starts drawing operation when mouse button is pressed.
        /// </summary>
        /// <param name="position">The mouse down position on the canvas.</param>
        /// <returns>True if drawing started, false otherwise.</returns>
        public bool TryStartDrawing(Point position)
        {
            if (SelectedTool == null) return false;

            var shapeType = GetShapeTypeFromTool(SelectedTool.Id);
            if (shapeType == null) return false;

            _drawingContext = DrawingElementEditingEngine.CreateDrawingContext(position, shapeType.Value);
            IsDrawing = true;

            PreviewElement = CreateElementForShapeType(shapeType.Value);
            PreviewElement.X = position.X;
            PreviewElement.Y = position.Y;
            PreviewElement.Width = 1;
            PreviewElement.Height = 1;
            PreviewElement.ZIndex = _nextZIndex;

            return true;
        }

        /// <summary>
        /// Updates preview element during mouse drag.
        /// Delegates bounds calculation to DrawingElementEditingEngine.
        /// </summary>
        /// <param name="currentPosition">The current mouse position.</param>
        public void UpdateDrawing(Point currentPosition)
        {
            if (!IsDrawing || PreviewElement == null) return;

            var bounds = DrawingElementEditingEngine.CalculateDrawingBounds(_drawingContext, currentPosition);

            PreviewElement.X = bounds.X;
            PreviewElement.Y = bounds.Y;
            PreviewElement.Width = bounds.Width;
            PreviewElement.Height = bounds.Height;
        }

        /// <summary>
        /// Completes drawing and adds element to collection.
        /// </summary>
        public void FinishDrawing()
        {
            if (!IsDrawing || PreviewElement == null) return;

            PreviewElement.Opacity = 1.0;
            Elements.Add(PreviewElement);
            _nextZIndex++;

            PreviewElement = null;
            IsDrawing = false;

            if (!IsToolLockEnabled)
            {
                SelectToolById("Selection");
            }
        }

        /// <summary>
        /// Cancels current drawing operation.
        /// </summary>
        public void CancelDrawing()
        {
            PreviewElement = null;
            IsDrawing = false;
        }

        /// <summary>
        /// Maps tool ID to DrawingShapeType for shape tools (shortcuts 1-5).
        /// </summary>
        private DrawingShapeType? GetShapeTypeFromTool(string toolId)
        {
            return toolId switch
            {
                "InitialNode" => DrawingShapeType.Initial,
                "ActionNode" => DrawingShapeType.Action,
                "DecisionNode" => DrawingShapeType.Decision,
                "TerminalNode" => DrawingShapeType.Terminal,
                "PredefinedActionNode" => DrawingShapeType.PredefinedAction,
                "Textbox" => DrawingShapeType.Textbox,
                _ => null
            };
        }

        /// <summary>
        /// Creates the appropriate DrawingElement subclass for the given shape type.
        /// </summary>
        private DrawingElement CreateElementForShapeType(DrawingShapeType shapeType)
        {
            return shapeType switch
            {
                DrawingShapeType.Initial => new InitialElement(),
                DrawingShapeType.Action => new ActionElement(),
                DrawingShapeType.Decision => new DecisionElement(),
                DrawingShapeType.Terminal => new TerminalElement(),
                DrawingShapeType.PredefinedAction => new PredefinedActionElement(),
                DrawingShapeType.Textbox => new TextboxElement(),
                _ => throw new ArgumentOutOfRangeException(nameof(shapeType), shapeType, "Unknown shape type")
            };
        }

        #endregion
    }
}