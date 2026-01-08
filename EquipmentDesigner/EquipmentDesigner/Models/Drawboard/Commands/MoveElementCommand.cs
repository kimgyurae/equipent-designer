using System;

namespace EquipmentDesigner.Models.Commands
{
    /// <summary>
    /// Command to move a drawing element to a new position
    /// </summary>
    public class MoveElementCommand : IEditCommand
    {
        private readonly StateWorkspace _workspace;
        private readonly string _elementId;
        private readonly double _originalX;
        private readonly double _originalY;
        private readonly double _newX;
        private readonly double _newY;

        public MoveElementCommand(StateWorkspace workspace, string elementId,
            double originalX, double originalY, double newX, double newY)
        {
            _workspace = workspace;
            _elementId = elementId;
            _originalX = originalX;
            _originalY = originalY;
            _newX = newX;
            _newY = newY;
        }

        public string Description
        {
            get
            {
                var element = _workspace.GetElementById(_elementId);
                return $"Move {element?.ShapeType}";
            }
        }

        public void Execute()
        {
            var element = _workspace.GetElementById(_elementId);
            if (element == null)
            {
                throw new InvalidOperationException($"Element with Id '{_elementId}' not found in workspace.");
            }

            element.X = _newX;
            element.Y = _newY;
        }

        public void Undo()
        {
            var element = _workspace.GetElementById(_elementId);
            if (element != null)
            {
                element.X = _originalX;
                element.Y = _originalY;
            }
        }
    }
}
