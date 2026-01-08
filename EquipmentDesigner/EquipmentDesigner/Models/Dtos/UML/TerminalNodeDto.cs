namespace EquipmentDesigner.Models
{
    /// <summary>
    /// Terminal node - workflow termination point
    /// Inbound: 1+ | Outbound: 0 (no outgoing connections)
    /// </summary>
    public class TerminalNodeDto : ProcessNodeBase
    {
        public override ProcessNodeType NodeType => ProcessNodeType.Terminal;

        // No OutgoingConnection property - terminal nodes cannot have outgoing connections

        /// <summary>
        /// Validates the Terminal node configuration.
        /// Terminal nodes are always valid as they have no outgoing connections by design.
        /// </summary>
        public override NodeValidationResult Validate()
        {
            return NodeValidationResult.Valid(Id, NodeType);
        }
    }
}