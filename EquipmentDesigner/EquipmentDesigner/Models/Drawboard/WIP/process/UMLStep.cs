using System.Windows;


namespace EquipmentDesigner.Models
{
    public class UMLStep: IIdentifiable
    {
        public string Id { get; set; }

        DrawingElement DrawingElement { get; set; }
    }
}