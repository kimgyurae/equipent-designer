namespace EquipmentDesigner.Models.ProcessEditor
{
    /// <summary>
    /// Cursor types for process editor tools.
    /// </summary>
    public enum DrawboardToolCursorType
    {
        /// <summary>
        /// Default arrow cursor.
        /// </summary>
        Default,

        /// <summary>
        /// Hand cursor for panning/grabbing.
        /// </summary>
        Hand,

        /// <summary>
        /// Cross/crosshair cursor for precise placement.
        /// </summary>
        Cross,

        /// <summary>
        /// Eraser cursor (small white circle with black stroke).
        /// </summary>
        Eraser
    }
}
