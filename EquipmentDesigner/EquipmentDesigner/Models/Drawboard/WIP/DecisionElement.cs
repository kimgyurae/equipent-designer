namespace EquipmentDesigner.Models
{
    /// <summary>
    /// Decision node element - diamond shape representing conditional branching
    /// </summary>
    public class DecisionElement : DrawingElement
    {
        /// <inheritdoc />
        public override DrawingShapeType ShapeType => DrawingShapeType.Decision;

        /// <inheritdoc />
        public override int OutgoingArrowCount => 2;

        /// <inheritdoc />
        public override int IncomingArrowCount => -1;
    }
}