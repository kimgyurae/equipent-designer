namespace EquipmentDesigner.Models
{
    /// <summary>
    /// Textbox element for text annotations on the drawing canvas.
    /// This element has no arrow rules as it is not part of the workflow logic.
    /// </summary>
    public class TextboxElement : DrawingElement
    {
        /// <inheritdoc />
        public override DrawingShapeType ShapeType => DrawingShapeType.Textbox;

        /// <inheritdoc />
        public override int OutgoingArrowCount => 0;

        /// <inheritdoc />
        public override int IncomingArrowCount => 0;
    }
}