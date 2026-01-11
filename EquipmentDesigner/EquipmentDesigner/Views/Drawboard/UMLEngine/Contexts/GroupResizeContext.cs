using System.Collections.Generic;
using System.Windows;
using EquipmentDesigner.Models;

namespace EquipmentDesigner.Views.Drawboard.UMLEngine.Contexts
{
    /// <summary>
    /// Immutable snapshot of an element's bounds at the start of a group resize operation.
    /// </summary>
    public readonly struct ElementSnapshot
    {
        /// <summary>
        /// The drawing element being tracked.
        /// </summary>
        public DrawingElement Element { get; }

        /// <summary>
        /// The element's bounds at the start of the resize operation.
        /// </summary>
        public Rect OriginalBounds { get; }

        public ElementSnapshot(DrawingElement element)
        {
            Element = element;
            OriginalBounds = element.Bounds;
        }

        public ElementSnapshot(DrawingElement element, Rect originalBounds)
        {
            Element = element;
            OriginalBounds = originalBounds;
        }
    }

    /// <summary>
    /// Immutable context for group resize operations.
    /// Stores the initial state of the group and all elements at the start of resize.
    /// </summary>
    public readonly struct GroupResizeContext
    {
        /// <summary>
        /// The bounding box of all selected elements at the start of resize.
        /// </summary>
        public Rect OriginalGroupBounds { get; }

        /// <summary>
        /// The mouse position at the start of resize.
        /// </summary>
        public Point OriginalStartPoint { get; }

        /// <summary>
        /// The resize handle type at the start of resize.
        /// </summary>
        public ResizeHandleType InitialHandle { get; }

        /// <summary>
        /// Snapshots of all elements at the start of resize.
        /// </summary>
        public IReadOnlyList<ElementSnapshot> ElementSnapshots { get; }

        /// <summary>
        /// The original aspect ratio of the group bounding box.
        /// </summary>
        public double OriginalAspectRatio { get; }

        /// <summary>
        /// Whether the group was vertically flipped during this operation.
        /// </summary>
        public bool WasVerticallyFlipped { get; }

        /// <summary>
        /// Whether the group was horizontally flipped during this operation.
        /// </summary>
        public bool WasHorizontallyFlipped { get; }

        public GroupResizeContext(
            Rect originalGroupBounds,
            Point originalStartPoint,
            ResizeHandleType initialHandle,
            IReadOnlyList<ElementSnapshot> elementSnapshots,
            double originalAspectRatio,
            bool wasVerticallyFlipped = false,
            bool wasHorizontallyFlipped = false)
        {
            OriginalGroupBounds = originalGroupBounds;
            OriginalStartPoint = originalStartPoint;
            InitialHandle = initialHandle;
            ElementSnapshots = elementSnapshots;
            OriginalAspectRatio = originalAspectRatio;
            WasVerticallyFlipped = wasVerticallyFlipped;
            WasHorizontallyFlipped = wasHorizontallyFlipped;
        }

        /// <summary>
        /// Creates a new context with updated flip state and reset reference points.
        /// Used when a flip transition occurs during resize.
        /// </summary>
        public GroupResizeContext WithFlipUpdate(
            Rect newGroupBounds,
            Point newStartPoint,
            ResizeHandleType newHandle,
            IReadOnlyList<ElementSnapshot> newSnapshots,
            bool wasVerticallyFlipped,
            bool wasHorizontallyFlipped)
        {
            return new GroupResizeContext(
                newGroupBounds,
                newStartPoint,
                newHandle,
                newSnapshots,
                OriginalAspectRatio, // Keep original aspect ratio
                wasVerticallyFlipped,
                wasHorizontallyFlipped);
        }
    }
}
