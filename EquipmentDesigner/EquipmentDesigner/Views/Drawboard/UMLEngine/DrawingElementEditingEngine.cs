using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using EquipmentDesigner.Models;
using EquipmentDesigner.Views.Drawboard.UMLEngine.Contexts;
using EquipmentDesigner.Views.Drawboard.UMLEngine.Results;

namespace EquipmentDesigner.Views.Drawboard.UMLEngine
{
    /// <summary>
    /// Stateless engine for calculating element editing transformations.
    /// All methods are pure functions with no side effects.
    /// </summary>
    public static class DrawingElementEditingEngine
    {
        public const double MinSize = 1.0;

        #region Utility Methods

        /// <summary>
        /// Gets the handle type after applying horizontal and/or vertical flip.
        /// </summary>
        public static ResizeHandleType GetFlippedHandle(
            ResizeHandleType initial,
            bool horizontalFlip,
            bool verticalFlip)
        {
            var result = initial;

            if (horizontalFlip)
            {
                result = result switch
                {
                    ResizeHandleType.Left => ResizeHandleType.Right,
                    ResizeHandleType.Right => ResizeHandleType.Left,
                    ResizeHandleType.TopLeft => ResizeHandleType.TopRight,
                    ResizeHandleType.TopRight => ResizeHandleType.TopLeft,
                    ResizeHandleType.BottomLeft => ResizeHandleType.BottomRight,
                    ResizeHandleType.BottomRight => ResizeHandleType.BottomLeft,
                    _ => result
                };
            }

            if (verticalFlip)
            {
                result = result switch
                {
                    ResizeHandleType.Top => ResizeHandleType.Bottom,
                    ResizeHandleType.Bottom => ResizeHandleType.Top,
                    ResizeHandleType.TopLeft => ResizeHandleType.BottomLeft,
                    ResizeHandleType.TopRight => ResizeHandleType.BottomRight,
                    ResizeHandleType.BottomLeft => ResizeHandleType.TopLeft,
                    ResizeHandleType.BottomRight => ResizeHandleType.TopRight,
                    _ => result
                };
            }

            return result;
        }

        /// <summary>
        /// Returns true if the handle is a corner handle.
        /// </summary>
        public static bool IsCornerHandle(ResizeHandleType handle) =>
            handle is ResizeHandleType.TopLeft or ResizeHandleType.TopRight
                   or ResizeHandleType.BottomLeft or ResizeHandleType.BottomRight;

        /// <summary>
        /// Returns true if the handle affects the left edge.
        /// </summary>
        public static bool IsLeftHandle(ResizeHandleType handle) =>
            handle is ResizeHandleType.TopLeft or ResizeHandleType.BottomLeft or ResizeHandleType.Left;

        /// <summary>
        /// Returns true if the handle affects the top edge.
        /// </summary>
        public static bool IsTopHandle(ResizeHandleType handle) =>
            handle is ResizeHandleType.TopLeft or ResizeHandleType.TopRight or ResizeHandleType.Top;

        #endregion

        #region Move Operations

        /// <summary>
        /// Creates a move context from element bounds and start point.
        /// </summary>
        public static MoveContext CreateMoveContext(Rect elementBounds, Point startPoint)
        {
            return new MoveContext(startPoint, elementBounds);
        }

        /// <summary>
        /// Calculates new position based on move delta.
        /// </summary>
        public static MoveResult CalculateMove(MoveContext context, Point currentPoint)
        {
            double deltaX = currentPoint.X - context.DragStartPoint.X;
            double deltaY = currentPoint.Y - context.DragStartPoint.Y;

            return new MoveResult(
                context.OriginalBounds.X + deltaX,
                context.OriginalBounds.Y + deltaY
            );
        }

        #endregion

        #region Drawing Operations

        /// <summary>
        /// Creates a drawing context from start point and shape type.
        /// </summary>
        public static DrawingContext CreateDrawingContext(Point startPoint, DrawingShapeType shapeType)
        {
            return new DrawingContext(startPoint, shapeType);
        }

        /// <summary>
        /// Calculates drawing bounds from start point to current point.
        /// Normalizes coordinates so bounds are always positive.
        /// </summary>
        public static DrawingBounds CalculateDrawingBounds(DrawingContext context, Point currentPoint)
        {
            double x = Math.Min(context.StartPoint.X, currentPoint.X);
            double y = Math.Min(context.StartPoint.Y, currentPoint.Y);
            double width = Math.Abs(currentPoint.X - context.StartPoint.X);
            double height = Math.Abs(currentPoint.Y - context.StartPoint.Y);

            // Ensure minimum size
            width = Math.Max(MinSize, width);
            height = Math.Max(MinSize, height);

            return new DrawingBounds(x, y, width, height);
        }

        #endregion

        #region Resize Operations

        /// <summary>
        /// Creates a resize context from element bounds, handle type, and start point.
        /// </summary>
        public static ResizeContext CreateResizeContext(Rect elementBounds, ResizeHandleType handle, Point startPoint)
        {
            double aspectRatio = elementBounds.Width / Math.Max(1, elementBounds.Height);
            return new ResizeContext(
                elementBounds,
                startPoint,
                handle,
                aspectRatio,
                wasVerticallyFlipped: false,
                wasHorizontallyFlipped: false);
        }

        /// <summary>
        /// Calculates new bounds during a resize operation using absolute position calculation.
        /// Handles flip detection and aspect ratio constraints.
        /// </summary>
        public static ResizeResult CalculateResize(ResizeContext context, Point currentPoint, bool maintainAspectRatio)
        {
            // ABSOLUTE delta from TRUE original start point
            double totalDeltaX = currentPoint.X - context.TrueOriginalStartPoint.X;
            double totalDeltaY = currentPoint.Y - context.TrueOriginalStartPoint.Y;

            // Start from TRUE original bounds
            double left = context.TrueOriginalBounds.Left;
            double top = context.TrueOriginalBounds.Top;
            double right = context.TrueOriginalBounds.Right;
            double bottom = context.TrueOriginalBounds.Bottom;

            // Apply delta based on INITIAL handle type (determines which edges move)
            switch (context.InitialHandle)
            {
                case ResizeHandleType.TopLeft:
                    left += totalDeltaX;
                    top += totalDeltaY;
                    break;
                case ResizeHandleType.TopRight:
                    right += totalDeltaX;
                    top += totalDeltaY;
                    break;
                case ResizeHandleType.BottomLeft:
                    left += totalDeltaX;
                    bottom += totalDeltaY;
                    break;
                case ResizeHandleType.BottomRight:
                    right += totalDeltaX;
                    bottom += totalDeltaY;
                    break;
                case ResizeHandleType.Top:
                    top += totalDeltaY;
                    break;
                case ResizeHandleType.Bottom:
                    bottom += totalDeltaY;
                    break;
                case ResizeHandleType.Left:
                    left += totalDeltaX;
                    break;
                case ResizeHandleType.Right:
                    right += totalDeltaX;
                    break;
            }

            // Calculate raw dimensions (may be negative if flipped)
            double rawWidth = right - left;
            double rawHeight = bottom - top;

            // Detect flips via negative dimensions
            bool horizontalFlip = rawWidth < 0;
            bool verticalFlip = rawHeight < 0;

            // Normalize: ensure positive dimensions and correct position
            double newX, newY, newWidth, newHeight;

            if (horizontalFlip)
            {
                newX = right;  // Swap: right becomes new left
                newWidth = Math.Max(MinSize, -rawWidth);
            }
            else
            {
                newWidth = Math.Max(MinSize, rawWidth);
                // When width is clamped to MinSize, anchor the opposite edge to prevent drift
                if (rawWidth < MinSize && IsLeftHandle(context.InitialHandle))
                {
                    // Left handle being dragged - anchor right edge
                    newX = right - newWidth;
                }
                else
                {
                    newX = left;
                }
            }

            if (verticalFlip)
            {
                newY = bottom;  // Swap: bottom becomes new top
                newHeight = Math.Max(MinSize, -rawHeight);
            }
            else
            {
                newHeight = Math.Max(MinSize, rawHeight);
                // When height is clamped to MinSize, anchor the opposite edge to prevent drift
                if (rawHeight < MinSize && IsTopHandle(context.InitialHandle))
                {
                    // Top handle being dragged - anchor bottom edge
                    newY = bottom - newHeight;
                }
                else
                {
                    newY = top;
                }
            }

            // Detect flip TRANSITIONS (not-flipped -> flipped)
            bool justFlippedHorizontally = horizontalFlip && !context.WasHorizontallyFlipped;
            bool justFlippedVertically = verticalFlip && !context.WasVerticallyFlipped;

            // Prepare updated context
            ResizeContext updatedContext;
            ResizeHandleType currentInitialHandle = context.InitialHandle;

            // Reset reference points ONLY on flip (not unflip)
            if (justFlippedHorizontally || justFlippedVertically)
            {
                // Reset to current calculated state
                var newBounds = new Rect(newX, newY, newWidth, newHeight);
                currentInitialHandle = GetFlippedHandle(
                    context.InitialHandle,
                    justFlippedHorizontally,
                    justFlippedVertically);

                updatedContext = context.WithFlipUpdate(
                    newBounds,
                    currentPoint,
                    currentInitialHandle,
                    verticalFlip,
                    horizontalFlip);
            }
            else
            {
                // Update flip state tracking without resetting bounds
                updatedContext = new ResizeContext(
                    context.TrueOriginalBounds,
                    context.TrueOriginalStartPoint,
                    context.InitialHandle,
                    context.OriginalAspectRatio,
                    horizontalFlip,
                    verticalFlip);
            }

            // Update active handle for cursor display (UX feedback)
            // After a flip transition, currentInitialHandle already reflects the new handle
            // No need to apply GetFlippedHandle again - it would flip it back incorrectly
            var activeHandle = currentInitialHandle;

            // Maintain aspect ratio if requested
            if (maintainAspectRatio)
            {
                ApplyAspectRatioConstraint(
                    ref newX, ref newY, ref newWidth, ref newHeight,
                    context.OriginalAspectRatio,
                    activeHandle);
            }

            return new ResizeResult(newX, newY, newWidth, newHeight, activeHandle, updatedContext);
        }

        /// <summary>
        /// Applies aspect ratio constraint to the given dimensions.
        /// </summary>
        internal static void ApplyAspectRatioConstraint(
            ref double newX, ref double newY,
            ref double newWidth, ref double newHeight,
            double aspectRatio,
            ResizeHandleType activeHandle)
        {
            // Ensure valid aspect ratio
            if (aspectRatio <= 0 || double.IsNaN(aspectRatio) || double.IsInfinity(aspectRatio))
                aspectRatio = 1.0;

            double scaledWidth, scaledHeight;

            // For side handles, use the primary axis scale; for corners, use max
            bool isSideHandle = activeHandle is ResizeHandleType.Top or ResizeHandleType.Bottom
                                               or ResizeHandleType.Left or ResizeHandleType.Right;

            if (isSideHandle)
            {
                // Side handles: use the primary axis dimension to calculate the secondary axis
                bool isVerticalPrimary = activeHandle is ResizeHandleType.Top or ResizeHandleType.Bottom;

                if (isVerticalPrimary)
                {
                    // Top/Bottom: height is primary, calculate width from aspect ratio
                    scaledHeight = Math.Max(MinSize, newHeight);
                    scaledWidth = scaledHeight * aspectRatio;
                }
                else
                {
                    // Left/Right: width is primary, calculate height from aspect ratio
                    scaledWidth = Math.Max(MinSize, newWidth);
                    scaledHeight = scaledWidth / aspectRatio;
                }
            }
            else
            {
                // Corner handles: use the larger dimension to ensure element grows in larger direction
                double widthBasedHeight = newWidth / aspectRatio;
                double heightBasedWidth = newHeight * aspectRatio;

                // Use the dimension that results in the larger overall size
                if (newWidth >= heightBasedWidth)
                {
                    scaledWidth = Math.Max(MinSize, newWidth);
                    scaledHeight = scaledWidth / aspectRatio;
                }
                else
                {
                    scaledHeight = Math.Max(MinSize, newHeight);
                    scaledWidth = scaledHeight * aspectRatio;
                }
            }

            // Ensure minimum size is respected
            scaledWidth = Math.Max(MinSize, scaledWidth);
            scaledHeight = Math.Max(MinSize, scaledHeight);

            // Adjust position based on which edge/corner is anchored (opposite of active handle)
            switch (activeHandle)
            {
                // Corner handles - anchor opposite corner
                case ResizeHandleType.TopLeft:
                    newX = (newX + newWidth) - scaledWidth;
                    newY = (newY + newHeight) - scaledHeight;
                    break;
                case ResizeHandleType.TopRight:
                    newY = (newY + newHeight) - scaledHeight;
                    break;
                case ResizeHandleType.BottomLeft:
                    newX = (newX + newWidth) - scaledWidth;
                    break;
                case ResizeHandleType.BottomRight:
                    // top-left anchor - no position adjustment needed
                    break;

                // Side handles - anchor opposite edge, center complementary axis
                case ResizeHandleType.Top:
                    // Anchor bottom edge, center width horizontally
                    newY = (newY + newHeight) - scaledHeight;
                    newX = newX + (newWidth - scaledWidth) / 2;
                    break;
                case ResizeHandleType.Bottom:
                    // Anchor top edge (no Y adjustment), center width horizontally
                    newX = newX + (newWidth - scaledWidth) / 2;
                    break;
                case ResizeHandleType.Left:
                    // Anchor right edge, center height vertically
                    newX = (newX + newWidth) - scaledWidth;
                    newY = newY + (newHeight - scaledHeight) / 2;
                    break;
                case ResizeHandleType.Right:
                    // Anchor left edge (no X adjustment), center height vertically
                    newY = newY + (newHeight - scaledHeight) / 2;
                    break;
            }

            newWidth = scaledWidth;
            newHeight = scaledHeight;
        }

        #endregion

        #region Group Operations

        /// <summary>
        /// Computes the bounding box that contains all element bounds.
        /// </summary>
        public static Rect ComputeGroupBounds(IEnumerable<DrawingElement> elements)
        {
            var elementList = elements.ToList();
            if (elementList.Count == 0)
                return Rect.Empty;

            double minX = double.MaxValue;
            double minY = double.MaxValue;
            double maxX = double.MinValue;
            double maxY = double.MinValue;

            foreach (var element in elementList)
            {
                minX = Math.Min(minX, element.X);
                minY = Math.Min(minY, element.Y);
                maxX = Math.Max(maxX, element.X + element.Width);
                maxY = Math.Max(maxY, element.Y + element.Height);
            }

            return new Rect(minX, minY, maxX - minX, maxY - minY);
        }

        /// <summary>
        /// Calculates new positions for multiple elements during group move.
        /// </summary>
        public static IReadOnlyList<MoveResult> CalculateGroupMove(
            IReadOnlyList<Rect> originalBounds,
            Point dragStartPoint,
            Point currentPoint)
        {
            double deltaX = currentPoint.X - dragStartPoint.X;
            double deltaY = currentPoint.Y - dragStartPoint.Y;

            var results = new List<MoveResult>(originalBounds.Count);
            foreach (var bounds in originalBounds)
            {
                results.Add(new MoveResult(bounds.X + deltaX, bounds.Y + deltaY));
            }

            return results;
        }

        /// <summary>
        /// Creates context for group resize operation.
        /// </summary>
        public static GroupResizeContext CreateGroupResizeContext(
            IEnumerable<DrawingElement> elements,
            ResizeHandleType handle,
            Point startPoint)
        {
            var elementList = elements.ToList();
            var snapshots = elementList.Select(e => new ElementSnapshot(e)).ToList();
            var groupBounds = ComputeGroupBounds(elementList);
            double aspectRatio = groupBounds.Width / Math.Max(1, groupBounds.Height);

            return new GroupResizeContext(
                groupBounds,
                startPoint,
                handle,
                snapshots,
                aspectRatio,
                wasVerticallyFlipped: false,
                wasHorizontallyFlipped: false);
        }

        /// <summary>
        /// Calculates resize transformations for all elements in a group.
        /// Applies proportional scaling based on group bounding box changes.
        /// </summary>
        public static GroupResizeResult CalculateGroupResize(
            GroupResizeContext context,
            Point currentPoint,
            bool maintainAspectRatio)
        {
            // ABSOLUTE delta from TRUE original start point (never reset)
            double totalDeltaX = currentPoint.X - context.TrueOriginalStartPoint.X;
            double totalDeltaY = currentPoint.Y - context.TrueOriginalStartPoint.Y;

            // Start from TRUE original group bounds (never reset)
            double left = context.TrueOriginalBounds.Left;
            double top = context.TrueOriginalBounds.Top;
            double right = context.TrueOriginalBounds.Right;
            double bottom = context.TrueOriginalBounds.Bottom;

            // Apply delta based on TRUE initial handle type (never changes)
            switch (context.TrueInitialHandle)
            {
                case ResizeHandleType.TopLeft:
                    left += totalDeltaX;
                    top += totalDeltaY;
                    break;
                case ResizeHandleType.TopRight:
                    right += totalDeltaX;
                    top += totalDeltaY;
                    break;
                case ResizeHandleType.BottomLeft:
                    left += totalDeltaX;
                    bottom += totalDeltaY;
                    break;
                case ResizeHandleType.BottomRight:
                    right += totalDeltaX;
                    bottom += totalDeltaY;
                    break;
                case ResizeHandleType.Top:
                    top += totalDeltaY;
                    break;
                case ResizeHandleType.Bottom:
                    bottom += totalDeltaY;
                    break;
                case ResizeHandleType.Left:
                    left += totalDeltaX;
                    break;
                case ResizeHandleType.Right:
                    right += totalDeltaX;
                    break;
            }

            // Calculate raw dimensions (may be negative if flipped)
            double rawWidth = right - left;
            double rawHeight = bottom - top;

            // Detect flips via negative dimensions
            bool horizontalFlip = rawWidth < 0;
            bool verticalFlip = rawHeight < 0;

            // Normalize to positive dimensions
            double newGroupX, newGroupY, newGroupWidth, newGroupHeight;

            if (horizontalFlip)
            {
                newGroupX = right;
                newGroupWidth = Math.Max(MinSize, -rawWidth);
            }
            else
            {
                newGroupWidth = Math.Max(MinSize, rawWidth);
                // When width is clamped to MinSize, anchor the opposite edge to prevent drift
                if (rawWidth < MinSize && IsLeftHandle(context.TrueInitialHandle))
                {
                    // Left handle being dragged - anchor right edge
                    newGroupX = right - newGroupWidth;
                }
                else
                {
                    newGroupX = left;
                }
            }

            if (verticalFlip)
            {
                newGroupY = bottom;
                newGroupHeight = Math.Max(MinSize, -rawHeight);
            }
            else
            {
                newGroupHeight = Math.Max(MinSize, rawHeight);
                // When height is clamped to MinSize, anchor the opposite edge to prevent drift
                if (rawHeight < MinSize && IsTopHandle(context.TrueInitialHandle))
                {
                    // Top handle being dragged - anchor bottom edge
                    newGroupY = bottom - newGroupHeight;
                }
                else
                {
                    newGroupY = top;
                }
            }

            // Detect flip TRANSITIONS (for context tracking only, not for calculation reset)
            bool justFlippedHorizontally = horizontalFlip && !context.WasHorizontallyFlipped;
            bool justFlippedVertically = verticalFlip && !context.WasVerticallyFlipped;
            bool justUnflippedHorizontally = !horizontalFlip && context.WasHorizontallyFlipped;
            bool justUnflippedVertically = !verticalFlip && context.WasVerticallyFlipped;

            // Reference frame tracking (for visual handle display only)
            bool needsReferenceReset = justFlippedHorizontally || justFlippedVertically
                                    || justUnflippedHorizontally || justUnflippedVertically;

            // Calculate the current active handle for visual feedback
            ResizeHandleType activeHandle = GetFlippedHandle(
                context.TrueInitialHandle,
                horizontalFlip,
                verticalFlip);

            // Calculate scale factors from TRUE original bounds
            double scaleX = newGroupWidth / Math.Max(MinSize, context.TrueOriginalBounds.Width);
            double scaleY = newGroupHeight / Math.Max(MinSize, context.TrueOriginalBounds.Height);

            // Determine edge handle types for axis-specific behavior
            bool isHorizontalEdge = context.TrueInitialHandle is ResizeHandleType.Top or ResizeHandleType.Bottom;
            bool isVerticalEdge = context.TrueInitialHandle is ResizeHandleType.Left or ResizeHandleType.Right;

            // Maintain aspect ratio if Shift is held
            if (maintainAspectRatio)
            {
                double scale;

                if (isHorizontalEdge)
                {
                    // Top/Bottom: height is primary, use scaleY for both axes
                    scale = scaleY;
                }
                else if (isVerticalEdge)
                {
                    // Left/Right: width is primary, use scaleX for both axes
                    scale = scaleX;
                }
                else
                {
                    // Corner handles: use the larger scale factor
                    scale = Math.Max(Math.Abs(scaleX), Math.Abs(scaleY));
                }

                scaleX = scale;
                scaleY = scale;

                // Recalculate group dimensions with uniform scale
                newGroupWidth = context.TrueOriginalBounds.Width * scale;
                newGroupHeight = context.TrueOriginalBounds.Height * scale;

                // Adjust position based on anchor point using TRUE original bounds
                AdjustGroupPositionForAspectRatio(
                    ref newGroupX, ref newGroupY,
                    context.TrueOriginalBounds,
                    newGroupWidth, newGroupHeight,
                    activeHandle,
                    context.TrueOriginalBounds);
            }
            else
            {
                // For edge handles without aspect ratio lock, only scale in that axis
                if (isHorizontalEdge)
                {
                    scaleX = 1.0;
                }
                else if (isVerticalEdge)
                {
                    scaleY = 1.0;
                }
            }

            var newGroupBounds = new Rect(newGroupX, newGroupY, newGroupWidth, newGroupHeight);

            // Calculate element transforms from TRUE original snapshots
            var transforms = new List<ElementTransform>(context.TrueElementSnapshots.Count);
            foreach (var snapshot in context.TrueElementSnapshots)
            {
                var origBounds = snapshot.OriginalBounds;
                var origGroupBounds = context.TrueOriginalBounds;

                // Calculate new size
                double newWidth = Math.Max(MinSize, origBounds.Width * scaleX);
                double newHeight = Math.Max(MinSize, origBounds.Height * scaleY);

                // Calculate new position relative to new group origin
                double relativeX = (origBounds.X - origGroupBounds.X) * scaleX;
                double relativeY = (origBounds.Y - origGroupBounds.Y) * scaleY;
                double newX = newGroupX + relativeX;
                double newY = newGroupY + relativeY;

                transforms.Add(new ElementTransform(snapshot.Element, newX, newY, newWidth, newHeight));
            }

            // Update context with current flip state (for tracking transitions only)
            GroupResizeContext updatedContext;
            if (needsReferenceReset)
            {
                var newSnapshots = transforms
                    .Select(t => new ElementSnapshot(t.Element, new Rect(t.NewX, t.NewY, t.NewWidth, t.NewHeight)))
                    .ToList();

                updatedContext = context.WithFlipUpdate(
                    newGroupBounds,
                    currentPoint,
                    activeHandle,
                    newSnapshots,
                    verticalFlip,
                    horizontalFlip);
            }
            else
            {
                updatedContext = new GroupResizeContext(
                    context.OriginalGroupBounds,
                    context.OriginalStartPoint,
                    context.InitialHandle,
                    context.ElementSnapshots,
                    context.OriginalAspectRatio,
                    horizontalFlip,
                    verticalFlip,
                    context.TrueInitialHandle,
                    context.TrueOriginalBounds,
                    context.TrueOriginalStartPoint,
                    context.TrueElementSnapshots);
            }

            return new GroupResizeResult(transforms, activeHandle, updatedContext, newGroupBounds);
        }

        /// <summary>
        /// Adjusts group position when maintaining aspect ratio.
        /// </summary>
        /// <param name="trueOriginalBounds">
        /// The TRUE original bounds from the start of the drag operation.
        /// Used for secondary axis centering to prevent drift during flip/unflip cycles.
        /// If null, uses originalBounds for centering.
        /// </param>
        private static void AdjustGroupPositionForAspectRatio(
            ref double newX, ref double newY,
            Rect originalBounds,
            double newWidth, double newHeight,
            ResizeHandleType activeHandle,
            Rect? trueOriginalBounds = null)
        {
            // Use true original for centering if provided, otherwise use current original
            var centeringBounds = trueOriginalBounds ?? originalBounds;
            double trueOriginalCenterX = centeringBounds.X + centeringBounds.Width / 2;
            double trueOriginalCenterY = centeringBounds.Y + centeringBounds.Height / 2;

            switch (activeHandle)
            {
                case ResizeHandleType.TopLeft:
                    newX = originalBounds.Right - newWidth;
                    newY = originalBounds.Bottom - newHeight;
                    break;
                case ResizeHandleType.TopRight:
                    newY = originalBounds.Bottom - newHeight;
                    break;
                case ResizeHandleType.BottomLeft:
                    newX = originalBounds.Right - newWidth;
                    break;
                case ResizeHandleType.BottomRight:
                    // Top-left anchor, no adjustment needed
                    break;
                case ResizeHandleType.Top:
                    // Anchor bottom edge, center width around TRUE original center
                    newY = originalBounds.Bottom - newHeight;
                    newX = trueOriginalCenterX - newWidth / 2;
                    break;
                case ResizeHandleType.Bottom:
                    // Anchor top edge, center width around TRUE original center
                    newX = trueOriginalCenterX - newWidth / 2;
                    break;
                case ResizeHandleType.Left:
                    // Anchor right edge, center height around TRUE original center
                    newX = originalBounds.Right - newWidth;
                    newY = trueOriginalCenterY - newHeight / 2;
                    break;
                case ResizeHandleType.Right:
                    // Anchor left edge, center height around TRUE original center
                    newY = trueOriginalCenterY - newHeight / 2;
                    break;
            }
        }

        #endregion
    }
}