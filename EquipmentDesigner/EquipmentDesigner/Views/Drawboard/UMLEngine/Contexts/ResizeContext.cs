using System.Windows;
using EquipmentDesigner.Models;

namespace EquipmentDesigner.Views.Drawboard.UMLEngine.Contexts
{
    /// <summary>
    /// Immutable context for resize operations.
    /// Contains both initial state and flip tracking.
    /// </summary>
    public readonly struct ResizeContext
    {
        /// <summary>Original element bounds at drag start.</summary>
        public Rect TrueOriginalBounds { get; }

        /// <summary>Original mouse position at drag start.</summary>
        public Point TrueOriginalStartPoint { get; }

        /// <summary>Handle type at drag start (determines which edges move).</summary>
        public ResizeHandleType InitialHandle { get; }

        /// <summary>Aspect ratio at drag start (never changes during drag).</summary>
        public double OriginalAspectRatio { get; }

        /// <summary>Whether element was flipped vertically.</summary>
        public bool WasVerticallyFlipped { get; }

        /// <summary>Whether element was flipped horizontally.</summary>
        public bool WasHorizontallyFlipped { get; }

        public ResizeContext(
            Rect trueOriginalBounds,
            Point trueOriginalStartPoint,
            ResizeHandleType initialHandle,
            double originalAspectRatio,
            bool wasVerticallyFlipped = false,
            bool wasHorizontallyFlipped = false)
        {
            TrueOriginalBounds = trueOriginalBounds;
            TrueOriginalStartPoint = trueOriginalStartPoint;
            InitialHandle = initialHandle;
            OriginalAspectRatio = originalAspectRatio;
            WasVerticallyFlipped = wasVerticallyFlipped;
            WasHorizontallyFlipped = wasHorizontallyFlipped;
        }

        /// <summary>
        /// Creates a new context with updated flip state and reference points.
        /// </summary>
        public ResizeContext WithFlipUpdate(
            Rect newBounds,
            Point newStartPoint,
            ResizeHandleType newHandle,
            bool wasVerticallyFlipped,
            bool wasHorizontallyFlipped)
        {
            return new ResizeContext(
                newBounds,
                newStartPoint,
                newHandle,
                OriginalAspectRatio,
                wasVerticallyFlipped,
                wasHorizontallyFlipped);
        }
    }
}
