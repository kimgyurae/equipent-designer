using System.Collections.Generic;

namespace EquipmentDesigner.Models.Dtos.Process
{
    /// <summary>
    /// Initial node - workflow entry point
    /// Inbound: 0 | Outbound: 1+
    /// </summary>
    public class InitialNodeDto : ProcessNodeBase
    {
        public override ProcessNodeType NodeType => ProcessNodeType.Initial;

        /// <summary>
        /// Outgoing connections (1 or more required)
        /// </summary>
        public List<ProcessConnectionDto> OutgoingConnections { get; set; } = new List<ProcessConnectionDto>();
    }
}
