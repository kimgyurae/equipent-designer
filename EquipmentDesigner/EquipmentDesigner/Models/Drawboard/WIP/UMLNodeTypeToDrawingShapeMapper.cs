namespace EquipmentDesigner.Models
{
    /// <summary>
    /// Mapper class for converting between UMLNodeType and DrawingShapeType.
    /// </summary>
    public static class UMLNodeTypeToDrawingShapeMapper
    {
        public static UMLNodeType? ConvertToUMLNodeType(DrawingShapeType drawingShapeType)
        {
            switch (drawingShapeType)
            {
                case DrawingShapeType.Initial:
                    return UMLNodeType.Initial;
                case DrawingShapeType.Action:
                    return UMLNodeType.Action;
                case DrawingShapeType.Decision:
                    return UMLNodeType.Decision;
                case DrawingShapeType.PredefinedAction:
                    return UMLNodeType.PredefinedAction;
                case DrawingShapeType.Terminal:
                    return UMLNodeType.Terminal;
                default:
                    return null;
            }
        }

        public static DrawingShapeType? ConvertToDrawingShapeType(UMLNodeType umlNodeType)
        {
            switch (umlNodeType)
            {
                case UMLNodeType.Initial:
                    return DrawingShapeType.Initial;
                case UMLNodeType.Action:
                    return DrawingShapeType.Action;
                case UMLNodeType.Decision:
                    return DrawingShapeType.Decision;
                case UMLNodeType.PredefinedAction:
                    return DrawingShapeType.PredefinedAction;
                case UMLNodeType.Terminal:
                    return DrawingShapeType.Terminal;
            }
            return null;
        }
    }
}