namespace EquipmentDesigner.Models
{
    /// <summary>
    /// Shape types for drawing elements in the process editor
    /// </summary>
    public enum DrawingShapeType
    {
        /// <summary>
        /// Initial node - ellipse shape (workflow entry point)
        /// </summary>
        Initial,

        /// <summary>
        /// Action node - rectangle shape (performs actual work)
        /// </summary>
        Action,

        /// <summary>
        /// Decision node - diamond shape (conditional branching)
        /// </summary>
        Decision,

        /// <summary>
        /// Terminal node - double-bordered ellipse (workflow termination)
        /// </summary>
        Terminal,

        /// <summary>
        /// Predefined action node - double-bordered rectangle (reusable subprocess)
        /// </summary>
        PredefinedAction,

        /// <summary>
        /// Textbox - text annotation (no process node mapping)
        /// </summary>
        Textbox
    }
}
