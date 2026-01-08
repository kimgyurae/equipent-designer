namespace EquipmentDesigner.Models.Commands
{
    /// <summary>
    /// Command to add a drawing element to a workspace
    /// </summary>
    public class AddElementCommand : IEditCommand
    {
        private readonly StateWorkspace _workspace;
        private readonly DrawingElement _element;

        public AddElementCommand(StateWorkspace workspace, DrawingElement element)
        {
            _workspace = workspace;
            _element = element;
        }

        public string Description => $"Add {_element.ShapeType}";

        public void Execute()
        {
            _workspace.AddElement(_element);
        }

        public void Undo()
        {
            _workspace.RemoveElement(_element.Id);
        }
    }
}
