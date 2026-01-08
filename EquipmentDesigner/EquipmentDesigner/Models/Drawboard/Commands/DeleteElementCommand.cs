using System;

namespace EquipmentDesigner.Models.Commands
{
    /// <summary>
    /// Command to delete a drawing element from a workspace
    /// </summary>
    public class DeleteElementCommand : IEditCommand
    {
        private readonly StateWorkspace _workspace;
        private readonly string _elementId;
        private DrawingElement _deletedElement;

        public DeleteElementCommand(StateWorkspace workspace, string elementId)
        {
            _workspace = workspace;
            _elementId = elementId;

            // Store reference for Description (before deletion)
            _deletedElement = _workspace.GetElementById(elementId);
        }

        public string Description => $"Delete {_deletedElement?.ShapeType}";

        public void Execute()
        {
            _deletedElement = _workspace.GetElementById(_elementId);
            if (_deletedElement == null)
            {
                throw new InvalidOperationException($"Element with Id '{_elementId}' not found in workspace.");
            }

            _workspace.RemoveElement(_elementId);
        }

        public void Undo()
        {
            if (_deletedElement != null)
            {
                // Re-add without changing ZIndex (preserve original)
                _workspace.Elements.Add(_deletedElement);
            }
        }
    }
}
