namespace EquipmentDesigner.Models
{
    /// <summary>
    /// Maps DrawingShapeType to ProcessNodeType for UML node compatibility
    /// </summary>
    public static class DrawingShapeTypeMapper
    {
        /// <summary>
        /// Converts a DrawingShapeType to its corresponding ProcessNodeType.
        /// Returns null for Textbox as it has no process node mapping.
        /// </summary>
        public static UMLNodeType? ToProcessNodeType(DrawingShapeType shapeType)
        {
            return shapeType switch
            {
                DrawingShapeType.Initial => UMLNodeType.Initial,
                DrawingShapeType.Action => UMLNodeType.Action,
                DrawingShapeType.Decision => UMLNodeType.Decision,
                DrawingShapeType.Terminal => UMLNodeType.Terminal,
                DrawingShapeType.PredefinedAction => UMLNodeType.PredefinedAction,
                DrawingShapeType.Textbox => null,
                _ => null
            };
        }
    }
}
