namespace EquipmentDesigner.Models.Dtos.Process
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
    }
}
