namespace EquipmentDesigner.Models
{
    /// <summary>
    /// Terminal node element - double-bordered ellipse representing workflow termination
    /// </summary>
    public class TerminalElement : DrawingElement
    {
        /// <inheritdoc />
        public override DrawingShapeType ShapeType => DrawingShapeType.Terminal;

        public override int OutgoingArrowCount => 0;

        public override int IncomingArrowCount => 1;
    }
}
