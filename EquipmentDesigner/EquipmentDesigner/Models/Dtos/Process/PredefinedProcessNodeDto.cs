namespace EquipmentDesigner.Models.Dtos.Process
{
    /// <summary>
    /// Predefined process node - reusable subprocess reference
    /// Inbound: 1+ | Outbound: 0 or 1
    /// </summary>
    public class PredefinedProcessNodeDto : ProcessNodeBase
    {
        public override ProcessNodeType NodeType => ProcessNodeType.PredefinedProcess;

        /// <summary>
        /// Referenced predefined process ID
        /// </summary>
        public string PredefinedProcessId { get; set; }

        /// <summary>
        /// Outgoing connection (0 or 1, nullable)
        /// null means this node acts as a terminal point
        /// </summary>
        public ProcessConnectionDto OutgoingConnection { get; set; }
    }
}
