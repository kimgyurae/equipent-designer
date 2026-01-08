namespace EquipmentDesigner.Models
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

        /// <summary>
        /// Validates the node configuration.
        /// Each node type has specific validation rules:
        /// - Initial: Must have at least 1 outgoing connection
        /// - Terminal: No outgoing connections (inherently valid)
        /// - Action: Must have exactly 1 outgoing connection
        /// - Decision: Must have condition and both True/False branches
        /// - PredefinedProcess: Must have valid workflow (Initial -> nodes -> Terminal)
        /// </summary>
        /// <returns>Validation result with any error messages</returns>
        public abstract NodeValidationResult Validate();
    }
}