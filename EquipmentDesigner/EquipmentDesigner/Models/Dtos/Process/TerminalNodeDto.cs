namespace EquipmentDesigner.Models.Dtos.Process
{
    /// <summary>
    /// Terminal node - workflow termination point
    /// Inbound: 1+ | Outbound: 0 (no outgoing connections)
    /// </summary>
    public class TerminalNodeDto : ProcessNodeBase
    {
        public override ProcessNodeType NodeType => ProcessNodeType.Terminal;

        // No OutgoingConnection property - terminal nodes cannot have outgoing connections
    }
}
