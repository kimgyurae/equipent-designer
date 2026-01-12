using System.Windows;


namespace EquipmentDesigner.Models
{
    public class UMLStep: IIdentifiable
    {
        public string Id { get; set; }

        public DrawingElement DrawingElement { get; set; }
    }
}