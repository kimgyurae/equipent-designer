using EquipmentDesigner.Models;
using EquipmentDesigner.Views.Drawboard.UMLEngine.Contexts;

namespace EquipmentDesigner.Views.Drawboard.UMLEngine.Results
{
    /// <summary>
    /// Result of a resize calculation.
    /// Includes updated context because flip operations reset reference points.
    /// </summary>
    public readonly struct ResizeResult
    {
        public double NewX { get; }
        public double NewY { get; }
        public double NewWidth { get; }
        public double NewHeight { get; }
        public ResizeHandleType ActiveHandle { get; }
        public ResizeContext UpdatedContext { get; }

        public ResizeResult(
            double newX,
            double newY,
            double newWidth,
            double newHeight,
            ResizeHandleType activeHandle,
            ResizeContext updatedContext)
        {
            NewX = newX;
            NewY = newY;
            NewWidth = newWidth;
            NewHeight = newHeight;
            ActiveHandle = activeHandle;
            UpdatedContext = updatedContext;
        }
    }
}
