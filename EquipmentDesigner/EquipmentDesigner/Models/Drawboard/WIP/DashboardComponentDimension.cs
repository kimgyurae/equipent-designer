using System.Windows;


namespace EquipmentDesigner.Models
{
    /// <summary>
    /// Action node - performs actual work
    /// Inbound: 1+ | Outbound: exactly 1
    /// </summary>
    public class DashboardComponentDimension
    {
        public double Height { get; set; }
        public double Width { get; set; }
        public Point Coordinate { get; set; }
    }
}