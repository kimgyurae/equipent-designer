using System.Collections.Generic;

namespace EquipmentDesigner.Models
{
    /// <summary>
    /// Initial node - workflow entry point
    /// Inbound: 0 | Outbound: 1+
    /// </summary>
    public class InitialNodeDto : ProcessNodeBase
    {
        public override UMLNodeType NodeType => UMLNodeType.Initial;

        /// <summary>
        /// Outgoing connections (1 or more required)
        /// </summary>
        public List<ProcessConnectionDto> OutgoingConnections { get; set; } = new List<ProcessConnectionDto>();

        /// <summary>
        /// Validates that the Initial node has at least 1 outgoing connection
        /// </summary>
        public override NodeValidationResult Validate()
        {
            var result = new NodeValidationResult
            {
                NodeId = Id,
                NodeType = NodeType
            };

            if (OutgoingConnections == null || OutgoingConnections.Count == 0)
            {
                result.AddError("Initial node must have at least 1 outgoing connection.");
            }
            else
            {
                foreach (var connection in OutgoingConnections)
                {
                    if (string.IsNullOrWhiteSpace(connection?.TargetNodeId))
                    {
                        result.AddError("Outgoing connection must have a valid TargetNodeId.");
                    }
                }
            }

            return result;
        }
    }
}