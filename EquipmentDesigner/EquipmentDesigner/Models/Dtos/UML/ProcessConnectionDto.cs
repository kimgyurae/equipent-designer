namespace EquipmentDesigner.Models
{
    /// <summary>
    /// Connection information between nodes
    /// </summary>
    public class ProcessConnectionDto
    {
        /// <summary>
        /// Target node ID
        /// </summary>
        public string TargetNodeId { get; set; }

        /// <summary>
        /// Connection label (optional, displayed on arrow)
        /// </summary>
        public string Label { get; set; }
    }
}
