namespace EquipmentDesigner.Models
{
    /// <summary>
    /// Initial node element - ellipse shape representing workflow entry point
    /// </summary>
    public class InitialElement : DrawingElement
    {
        /// <inheritdoc />
        public override DrawingShapeType ShapeType => DrawingShapeType.Initial;
    }
}
