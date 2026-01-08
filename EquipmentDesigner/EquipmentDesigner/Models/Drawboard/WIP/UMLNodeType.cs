namespace EquipmentDesigner.Models
{
    /// <summary>
    /// Process node types for UML Activity Diagram workflow
    /// </summary>
    public enum UMLNodeType
    {
        /// <summary>
        /// Start node - workflow entry point
        /// Inbound: 0 | Outbound: 1+
        /// </summary>
        Initial,

        /// <summary>
        /// End node - workflow termination point
        /// Inbound: 1+ | Outbound: 0
        /// </summary>
        Terminal,

        /// <summary>
        /// Action node - performs actual work
        /// Inbound: 1+ | Outbound: exactly 1
        /// </summary>
        Action,

        /// <summary>
        /// Decision node - conditional branching
        /// Inbound: 1+ | Outbound: exactly 2
        /// </summary>
        Decision,

        /// <summary>
        /// Predefined process node - reusable subprocess reference
        /// Inbound: 1+ | Outbound: 0 or 1
        /// </summary>
        PredefinedAction
    }
}
