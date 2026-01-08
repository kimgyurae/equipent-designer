using System.Collections.Generic;

namespace EquipmentDesigner.Models
{
    /// <summary>
    /// Single PackML State workflow definition
    /// </summary>
    public class ProcessWorkflowDto
    {
        /// <summary>
        /// PackML State type
        /// </summary>
        public PackMlState StateType { get; set; }

        /// <summary>
        /// All nodes in the workflow (polymorphic storage)
        /// </summary>
        public List<ProcessNodeBase> Nodes { get; set; } = new List<ProcessNodeBase>();

        /// <summary>
        /// Initial node ID (entry point)
        /// </summary>
        public string InitialNodeId { get; set; }
    }
}
