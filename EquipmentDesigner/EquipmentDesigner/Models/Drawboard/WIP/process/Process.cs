using System.Collections.Generic;

namespace EquipmentDesigner.Models
{
    public class Process : IIdentifiable
    {
        public string Id { get; set; }
        public Dictionary<PackMlState, UMLWorkflow> Processes { get; set; } = new Dictionary<PackMlState, UMLWorkflow>();
    }
}