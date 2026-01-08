using System.Collections.Generic;

namespace EquipmentDesigner.Models
{
    public class UMLWorkflow: IIdentifiable
    {
        public string Id { get; set; }
        public PackMlState State { get; set; }   
        public string Name { get; set; }
        public string Description { get; set; }
        public string[] ImplementationInstructions  { get; set; }

        public List<UMLStep> Steps { get; set; }
        public List<UMLConnection> Connections { get; set; }
    }
}