using System.Collections.Generic;
using System.Windows;
using EquipmentDesigner.Models;
using EquipmentDesigner.Views.Drawboard.UMLEngine.Contexts;

namespace EquipmentDesigner.Views.Drawboard.UMLEngine.Results
{
    /// <summary>
    /// Represents the new position and size for a single element after group resize calculation.
    /// </summary>
    public readonly struct ElementTransform
    {
        /// <summary>
        /// The element to transform.
        /// </summary>
        public DrawingElement Element { get; }

        /// <summary>
        /// The new X coordinate.
        /// </summary>
        public double NewX { get; }

        /// <summary>
        /// The new Y coordinate.
        /// </summary>
        public double NewY { get; }

        /// <summary>
        /// The new width.
        /// </summary>
        public double NewWidth { get; }

        /// <summary>
        /// The new height.
        /// </summary>
        public double NewHeight { get; }

        public ElementTransform(DrawingElement element, double newX, double newY, double newWidth, double newHeight)
        {
            Element = element;
            NewX = newX;
            NewY = newY;
            NewWidth = newWidth;
            NewHeight = newHeight;
        }

        /// <summary>
        /// Applies this transform to the element.
        /// </summary>
        public void Apply()
        {
            Element.X = NewX;
            Element.Y = NewY;
            Element.Width = NewWidth;
            Element.Height = NewHeight;
        }
    }

    /// <summary>
    /// Result of a group resize calculation.
    /// Contains transforms for all elements and the updated context.
    /// </summary>
    public readonly struct GroupResizeResult
    {
        /// <summary>
        /// Transforms to apply to each element.
        /// </summary>
        public IReadOnlyList<ElementTransform> Transforms { get; }

        /// <summary>
        /// The currently active resize handle (may change during flip).
        /// </summary>
        public ResizeHandleType ActiveHandle { get; }

        /// <summary>
        /// Updated context with current flip state (for next calculation).
        /// </summary>
        public GroupResizeContext UpdatedContext { get; }

        /// <summary>
        /// The new group bounding box after resize.
        /// </summary>
        public Rect NewGroupBounds { get; }

        public GroupResizeResult(
            IReadOnlyList<ElementTransform> transforms,
            ResizeHandleType activeHandle,
            GroupResizeContext updatedContext,
            Rect newGroupBounds)
        {
            Transforms = transforms;
            ActiveHandle = activeHandle;
            UpdatedContext = updatedContext;
            NewGroupBounds = newGroupBounds;
        }

        /// <summary>
        /// Applies all transforms to their respective elements.
        /// </summary>
        public void ApplyAll()
        {
            foreach (var transform in Transforms)
            {
                transform.Apply();
            }
        }
    }
}