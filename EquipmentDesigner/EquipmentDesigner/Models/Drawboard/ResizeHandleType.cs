namespace EquipmentDesigner.Models
{
    /// <summary>
    /// Represents the type of resize handle being interacted with.
    /// </summary>
    public enum ResizeHandleType
    {
        /// <summary>
        /// No handle is being interacted with.
        /// </summary>
        None,

        /// <summary>
        /// Top-left corner handle (resizes width and height).
        /// </summary>
        TopLeft,

        /// <summary>
        /// Top-right corner handle (resizes width and height).
        /// </summary>
        TopRight,

        /// <summary>
        /// Bottom-left corner handle (resizes width and height).
        /// </summary>
        BottomLeft,

        /// <summary>
        /// Bottom-right corner handle (resizes width and height).
        /// </summary>
        BottomRight,

        /// <summary>
        /// Top edge handle (resizes height only).
        /// </summary>
        Top,

        /// <summary>
        /// Right edge handle (resizes width only).
        /// </summary>
        Right,

        /// <summary>
        /// Bottom edge handle (resizes height only).
        /// </summary>
        Bottom,

        /// <summary>
        /// Left edge handle (resizes width only).
        /// </summary>
        Left
    }
}
