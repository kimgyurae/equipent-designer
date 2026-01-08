using System.Collections.Generic;

namespace EquipmentDesigner.Models
{
    /// <summary>
    /// 17 PackML State to Process Workflow mapping
    /// </summary>
    public class StateProcessMapDto
    {
        /// <summary>
        /// PackML State to Process Workflow dictionary
        /// </summary>
        public Dictionary<PackMlState, ProcessWorkflowDto> Processes { get; set; }
            = new Dictionary<PackMlState, ProcessWorkflowDto>();
    }
}
