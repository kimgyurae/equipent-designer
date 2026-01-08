namespace EquipmentDesigner.Models
{
    /// <summary>
    /// Action node - performs actual work
    /// Inbound: 1+ | Outbound: exactly 1
    /// </summary>
    public class ActionNodeDto : ProcessNodeBase
    {
        public override UMLNodeType NodeType => UMLNodeType.Action;

        /// <summary>
        /// Outgoing connection (exactly 1 required)
        /// </summary>
        public ProcessConnectionDto OutgoingConnection { get; set; }

        /// <summary>
        /// Validates that the Action node has exactly 1 outgoing connection with valid target
        /// </summary>
        public override NodeValidationResult Validate()
        {
            var result = new NodeValidationResult
            {
                NodeId = Id,
                NodeType = NodeType
            };

            if (OutgoingConnection == null)
            {
                result.AddError("Action node must have exactly 1 outgoing connection.");
            }
            else if (string.IsNullOrWhiteSpace(OutgoingConnection.TargetNodeId))
            {
                result.AddError("Outgoing connection must have a valid TargetNodeId.");
            }

            return result;
        }
    }
}