namespace EquipmentDesigner.Models
{
    /// <summary>
    /// Action node - performs actual work
    /// Inbound: 1+ | Outbound: exactly 1
    /// </summary>
    public class UMLNodeTypeToDrawingShapeMapper
    {
        UMLNodeType? ConvertToUMLNodeType(DrawingShapeType drawingShapeType) {
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

        DrawingShapeType? ConvertToDrawingShapeTypee(UMLNodeType drawingShapeType)
        {
            switch (drawingShapeType)
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