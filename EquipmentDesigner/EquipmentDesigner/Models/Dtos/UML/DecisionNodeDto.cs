namespace EquipmentDesigner.Models
{
    /// <summary>
    /// Decision node - conditional branching (True/False)
    /// Inbound: 1+ | Outbound: exactly 2
    /// </summary>
    public class DecisionNodeDto : ProcessNodeBase
    {
        public override ProcessNodeType NodeType => ProcessNodeType.Decision;

        /// <summary>
        /// Branch condition expression
        /// </summary>
        public string Condition { get; set; }

        /// <summary>
        /// Connection when condition is True
        /// </summary>
        public ProcessConnectionDto TrueBranch { get; set; }

        /// <summary>
        /// Connection when condition is False
        /// </summary>
        public ProcessConnectionDto FalseBranch { get; set; }

        /// <summary>
        /// Validates that the Decision node has a condition and both True/False branches
        /// </summary>
        public override NodeValidationResult Validate()
        {
            var result = new NodeValidationResult
            {
                NodeId = Id,
                NodeType = NodeType
            };

            if (string.IsNullOrWhiteSpace(Condition))
            {
                result.AddError("Decision node must have a condition expression.");
            }

            if (TrueBranch == null)
            {
                result.AddError("Decision node must have a TrueBranch connection.");
            }
            else if (string.IsNullOrWhiteSpace(TrueBranch.TargetNodeId))
            {
                result.AddError("TrueBranch connection must have a valid TargetNodeId.");
            }

            if (FalseBranch == null)
            {
                result.AddError("Decision node must have a FalseBranch connection.");
            }
            else if (string.IsNullOrWhiteSpace(FalseBranch.TargetNodeId))
            {
                result.AddError("FalseBranch connection must have a valid TargetNodeId.");
            }

            return result;
        }
    }
}