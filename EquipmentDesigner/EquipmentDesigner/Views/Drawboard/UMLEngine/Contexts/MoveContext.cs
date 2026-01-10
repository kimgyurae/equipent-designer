using System.Windows;

namespace EquipmentDesigner.Views.Drawboard.UMLEngine.Contexts
{
    /// <summary>
    /// Immutable context for move operations.
    /// </summary>
    public readonly struct MoveContext
    {
        public Point DragStartPoint { get; }
        public Rect OriginalBounds { get; }

        public MoveContext(Point dragStartPoint, Rect originalBounds)
        {
            DragStartPoint = dragStartPoint;
            OriginalBounds = originalBounds;
        }
    }
}
