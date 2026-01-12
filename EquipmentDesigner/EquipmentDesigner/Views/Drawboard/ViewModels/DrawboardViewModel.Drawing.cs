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
        /// Minimum drag distance (in pixels) required to create a drawing element.
        /// If the user clicks without sufficient dragging, the element creation is cancelled.
        /// </summary>
        private const double MinimumDragThreshold = 5.0;

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
            
            // Textbox uses fixed single-line height; other shapes start with height 1
            if (shapeType.Value == DrawingShapeType.Textbox)
            {
                PreviewElement.Height = TextEditingEngine.GetSingleLineElementHeight(PreviewElement.FontSize);
            }
            else
            {
                PreviewElement.Height = 1;
            }
            
            PreviewElement.ZIndex = _nextZIndex;

            return true;
        }

        /// <summary>
        /// Updates preview element during mouse drag.
        /// Delegates bounds calculation to DrawingElementEditingEngine.
        /// For Textbox elements, only width changes (height is fixed for single-line text).
        /// </summary>
        /// <param name="currentPosition">The current mouse position.</param>
        public void UpdateDrawing(Point currentPosition)
        {
            if (!IsDrawing || PreviewElement == null) return;

            var bounds = DrawingElementEditingEngine.CalculateDrawingBounds(_drawingContext, currentPosition);

            PreviewElement.X = bounds.X;
            PreviewElement.Y = bounds.Y;
            PreviewElement.Width = bounds.Width;
            
            // Textbox maintains fixed single-line height; other shapes use drag-determined height
            if (PreviewElement.ShapeType != DrawingShapeType.Textbox)
            {
                PreviewElement.Height = bounds.Height;
            }
        }

        /// <summary>
        /// Completes drawing and adds element to collection.
        /// For Textbox elements, automatically enters text editing mode.
        /// For Textbox with insufficient drag, creates default-sized Textbox at start position
        /// (same behavior as double-clicking on empty canvas).
        /// Cancels other shapes if drag distance is below MinimumDragThreshold.
        /// </summary>
        public void FinishDrawing()
        {
            if (!IsDrawing || PreviewElement == null) return;

            // Special handling for Textbox: create default-sized Textbox when drag is insufficient
            // This matches the behavior of double-clicking on empty canvas
            if (PreviewElement.ShapeType == DrawingShapeType.Textbox &&
                PreviewElement.Width < MinimumDragThreshold)
            {
                // Save start position before canceling (CancelDrawing clears PreviewElement)
                var startPoint = _drawingContext.StartPoint;
                CancelDrawing();
                CreateTextboxAtPosition(startPoint);
                return;
            }

            // For other shapes, check if drag distance meets minimum threshold
            bool isDragSufficient = PreviewElement.Width >= MinimumDragThreshold ||
                                    PreviewElement.Height >= MinimumDragThreshold;

            if (!isDragSufficient)
            {
                // Drag distance too small, cancel drawing
                CancelDrawing();
                return;
            }

            var newElement = PreviewElement;
            newElement.Opacity = 1.0;
            Elements.Add(newElement);
            AddStepToCurrentWorkflow(newElement);
            _nextZIndex++;

            PreviewElement = null;
            IsDrawing = false;

            // For Textbox, select the element and enter text editing mode immediately
            if (newElement.ShapeType == DrawingShapeType.Textbox)
            {
                SelectElement(newElement);
                TryStartTextEditing(newElement);
            }

            // Switch to Selection tool after creating element (unless ToolLock is enabled)
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

        /// <summary>
        /// Default width for Textbox created by double-click on empty canvas.
        /// </summary>
        private const double DefaultTextboxWidth = 150.0;

        /// <summary>
        /// Creates a TextboxElement at the specified position with default size.
        /// Used when double-clicking on empty canvas space.
        /// Automatically selects the element and enters text editing mode.
        /// </summary>
        /// <param name="position">The position where the Textbox should be created.</param>
        /// <returns>True if the Textbox was created and text editing started, false otherwise.</returns>
        public bool CreateTextboxAtPosition(Point position)
        {
            var textbox = new TextboxElement
            {
                X = position.X,
                Y = position.Y,
                Width = DefaultTextboxWidth,
                Height = TextEditingEngine.GetSingleLineElementHeight(TextFontSize.Base),
                Opacity = 1.0,
                ZIndex = _nextZIndex
            };

            Elements.Add(textbox);
            AddStepToCurrentWorkflow(textbox);
            _nextZIndex++;

            // Select and enter text editing mode
            SelectElement(textbox);
            var textEditingStarted = TryStartTextEditing(textbox);

            // Switch to Selection tool after creating Textbox (unless ToolLock is enabled)
            if (!IsToolLockEnabled)
            {
                SelectToolById("Selection");
            }

            return textEditingStarted;
        }

        #endregion
    }
}