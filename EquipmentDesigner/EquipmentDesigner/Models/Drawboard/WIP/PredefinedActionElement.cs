namespace EquipmentDesigner.Models
{
    /// <summary>
    /// Predefined action node element - double-bordered rectangle representing reusable subprocess
    /// </summary>
    public class PredefinedActionElement : DrawingElement
    {
        /// <inheritdoc />
        public override DrawingShapeType ShapeType => DrawingShapeType.PredefinedAction;

        /// <inheritdoc />
        public override int OutgoingArrowCount => 1;

        /// <inheritdoc />
        public override int IncomingArrowCount => -1;
    }
}