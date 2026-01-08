using System.Windows;


namespace EquipmentDesigner.Models
{
    public class UMLConnection: IIdentifiable
    {
        public string Id { get; set; }

        public string Label { get; set; }
        public string TailId { get; set; }   
        public string HeadId { get; set; }
        public DrawingElement DrawingElement { get; set; }
    }
}