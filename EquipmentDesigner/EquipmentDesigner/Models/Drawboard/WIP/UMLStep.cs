using System.Windows;


namespace EquipmentDesigner.Models
{
    public class UMLStep: IIdentifiable
    {
        public string Id { get; set; }
        public UMLNodeType NodeType { get; set; }   
        public string Label { get; set; }
        public string Description { get; set; }
        public string[] ImplementationInstructions  { get; set; }

        DrawingElement DrawingElement { get; set; }
    }
}