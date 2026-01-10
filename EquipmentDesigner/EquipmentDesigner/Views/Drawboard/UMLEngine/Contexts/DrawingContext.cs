using System.Windows;
using EquipmentDesigner.Models;

namespace EquipmentDesigner.Views.Drawboard.UMLEngine.Contexts
{
    /// <summary>
    /// Immutable context for drawing operations.
    /// </summary>
    public readonly struct DrawingContext
    {
        public Point StartPoint { get; }
        public DrawingShapeType ShapeType { get; }

        public DrawingContext(Point startPoint, DrawingShapeType shapeType)
        {
            StartPoint = startPoint;
            ShapeType = shapeType;
        }
    }
}
