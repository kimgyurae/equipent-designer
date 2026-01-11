namespace EquipmentDesigner.Models
{
    /// <summary>
    /// Represents the current state of element editing in the drawboard.
    /// </summary>
    public enum EditModeState
    {
        /// <summary>
        /// No element is selected.
        /// </summary>
        None,

        /// <summary>
        /// An element is selected and ready for editing.
        /// </summary>
        Selected,

        /// <summary>
        /// An element is being moved (dragged to new position).
        /// </summary>
        Moving,

        /// <summary>
        /// An element is being resized via a resize handle.
        /// </summary>
        Resizing,

        /// <summary>
        /// Multiple elements are selected and ready for group editing.
        /// </summary>
        MultiSelected,

        /// <summary>
        /// A rubberband (drag) selection is in progress.
        /// </summary>
        RubberbandSelecting,

        /// <summary>
        /// Multiple elements are being moved together.
        /// </summary>
        MultiMoving,

        /// <summary>
        /// Multiple elements are being resized together proportionally.
        /// </summary>
        MultiResizing
    }
}