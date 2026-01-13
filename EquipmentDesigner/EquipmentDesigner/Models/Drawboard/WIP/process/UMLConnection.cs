namespace EquipmentDesigner.Models
{
    /// <summary>
    /// Represents a connection (arrow) between two UML elements in a workflow.
    /// </summary>
    public class UMLConnection
    {
        /// <summary>
        /// Optional label displayed at the midpoint of the connection line.
        /// </summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// The ID of the source (tail) element.
        /// </summary>
        public string TailId { get; set; }

        /// <summary>
        /// The port position on the source element where the connection starts.
        /// </summary>
        public PortPosition TailPort { get; set; }

        /// <summary>
        /// The ID of the target (head) element.
        /// </summary>
        public string HeadId { get; set; }

        /// <summary>
        /// The port position on the target element where the connection ends.
        /// </summary>
        public PortPosition HeadPort { get; set; }
    }
}