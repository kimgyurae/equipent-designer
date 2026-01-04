namespace EquipmentDesigner.Models.Dtos.Process
{
    /// <summary>
    /// Abstract base class for all process nodes
    /// </summary>
    public abstract class ProcessNodeBase
    {
        /// <summary>
        /// Node unique identifier
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Node type (determined by derived class)
        /// </summary>
        public abstract ProcessNodeType NodeType { get; }

        /// <summary>
        /// Node name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Node description (optional)
        /// </summary>
        public string Description { get; set; }
    }
}
