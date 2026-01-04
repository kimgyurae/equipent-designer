namespace EquipmentDesigner.Models.Dtos.Process
{
    /// <summary>
    /// Action node - performs actual work
    /// Inbound: 1+ | Outbound: exactly 1
    /// </summary>
    public class ActionNodeDto : ProcessNodeBase
    {
        public override ProcessNodeType NodeType => ProcessNodeType.Action;

        /// <summary>
        /// Outgoing connection (exactly 1 required)
        /// </summary>
        public ProcessConnectionDto OutgoingConnection { get; set; }
    }
}
