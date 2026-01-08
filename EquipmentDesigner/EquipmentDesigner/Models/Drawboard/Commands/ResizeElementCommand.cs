using System;

namespace EquipmentDesigner.Models.Commands
{
    /// <summary>
    /// Command to resize a drawing element
    /// </summary>
    public class ResizeElementCommand : IEditCommand
    {
        private readonly StateWorkspace _workspace;
        private readonly string _elementId;
        private readonly double _originalWidth;
        private readonly double _originalHeight;
        private readonly double _newWidth;
        private readonly double _newHeight;

        public ResizeElementCommand(StateWorkspace workspace, string elementId,
            double originalWidth, double originalHeight, double newWidth, double newHeight)
        {
            _workspace = workspace;
            _elementId = elementId;
            _originalWidth = originalWidth;
            _originalHeight = originalHeight;
            _newWidth = newWidth;
            _newHeight = newHeight;
        }

        public string Description
        {
            get
            {
                var element = _workspace.GetElementById(_elementId);
                return $"Resize {element?.ShapeType}";
            }
        }

        public void Execute()
        {
            var element = _workspace.GetElementById(_elementId);
            if (element == null)
            {
                throw new InvalidOperationException($"Element with Id '{_elementId}' not found in workspace.");
            }

            element.Width = _newWidth;
            element.Height = _newHeight;
        }

        public void Undo()
        {
            var element = _workspace.GetElementById(_elementId);
            if (element != null)
            {
                element.Width = _originalWidth;
                element.Height = _originalHeight;
            }
        }
    }
}
