namespace EquipmentDesigner.Models
{
    /// <summary>
    /// Action node element - rectangle shape representing work execution
    /// </summary>
    public class ActionElement : DrawingElement
    {
        /// <inheritdoc />
        public override DrawingShapeType ShapeType => DrawingShapeType.Action;
    }
}
