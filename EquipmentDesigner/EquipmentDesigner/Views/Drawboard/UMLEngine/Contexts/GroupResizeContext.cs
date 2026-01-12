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
        /// This is reset during flip/unflip transitions.
        /// </summary>
        public Rect OriginalGroupBounds { get; }

        /// <summary>
        /// The mouse position at the start of resize.
        /// </summary>
        public Point OriginalStartPoint { get; }

        /// <summary>
        /// The resize handle type at the start of resize.
        /// This changes during flip transitions.
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

        /// <summary>
        /// The TRUE original handle type at the very start of the drag operation.
        /// Never changes during flip/unflip - used to determine anchor edge.
        /// </summary>
        public ResizeHandleType TrueInitialHandle { get; }

        /// <summary>
        /// The TRUE original bounds at the very start of the drag operation.
        /// Never changes during flip/unflip - used to calculate true anchor edges and centers.
        /// </summary>
        public Rect TrueOriginalBounds { get; }

        /// <summary>
        /// The TRUE original start point at the very start of the drag operation.
        /// Never changes during flip/unflip - used to calculate absolute delta.
        /// </summary>
        public Point TrueOriginalStartPoint { get; }

        /// <summary>
        /// The TRUE original element snapshots at the very start of the drag operation.
        /// Never changes during flip/unflip - used to calculate element transforms from absolute positions.
        /// </summary>
        public IReadOnlyList<ElementSnapshot> TrueElementSnapshots { get; }

        public GroupResizeContext(
            Rect originalGroupBounds,
            Point originalStartPoint,
            ResizeHandleType initialHandle,
            IReadOnlyList<ElementSnapshot> elementSnapshots,
            double originalAspectRatio,
            bool wasVerticallyFlipped = false,
            bool wasHorizontallyFlipped = false,
            ResizeHandleType? trueInitialHandle = null,
            Rect? trueOriginalBounds = null,
            Point? trueOriginalStartPoint = null,
            IReadOnlyList<ElementSnapshot> trueElementSnapshots = null)
        {
            OriginalGroupBounds = originalGroupBounds;
            OriginalStartPoint = originalStartPoint;
            InitialHandle = initialHandle;
            ElementSnapshots = elementSnapshots;
            OriginalAspectRatio = originalAspectRatio;
            WasVerticallyFlipped = wasVerticallyFlipped;
            WasHorizontallyFlipped = wasHorizontallyFlipped;
            // Preserve true original values - use provided values or defaults
            TrueInitialHandle = trueInitialHandle ?? initialHandle;
            TrueOriginalBounds = trueOriginalBounds ?? originalGroupBounds;
            TrueOriginalStartPoint = trueOriginalStartPoint ?? originalStartPoint;
            TrueElementSnapshots = trueElementSnapshots ?? elementSnapshots;
        }

        /// <summary>
        /// Gets the TRUE original horizontal center (never changes during flip/unflip).
        /// </summary>
        public double TrueOriginalCenterX => TrueOriginalBounds.X + TrueOriginalBounds.Width / 2;

        /// <summary>
        /// Gets the TRUE original vertical center (never changes during flip/unflip).
        /// </summary>
        public double TrueOriginalCenterY => TrueOriginalBounds.Y + TrueOriginalBounds.Height / 2;

        /// <summary>
        /// Creates a new context with updated flip state and reset reference points.
        /// Used when a flip transition occurs during resize.
        /// Preserves TrueInitialHandle and TrueOriginalBounds values.
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
                OriginalAspectRatio,    // Keep original aspect ratio
                wasVerticallyFlipped,
                wasHorizontallyFlipped,
                TrueInitialHandle,      // Preserve TRUE initial handle
                TrueOriginalBounds,     // Preserve TRUE original bounds
                TrueOriginalStartPoint, // Preserve TRUE original start point
                TrueElementSnapshots);  // Preserve TRUE original element snapshots
        }
    }
}