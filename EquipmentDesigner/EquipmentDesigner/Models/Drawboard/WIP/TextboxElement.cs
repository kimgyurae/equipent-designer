namespace EquipmentDesigner.Models
{
    /// <summary>
    /// Textbox element for text annotations on the drawing canvas
    /// </summary>
    public class TextboxElement : DrawingElement
    {
        /// <inheritdoc />
        public override DrawingShapeType ShapeType => DrawingShapeType.Textbox;

        public override int OutgoingArrowCount => 0;

        public override int IncomingArrowCount => 0;
    }
}