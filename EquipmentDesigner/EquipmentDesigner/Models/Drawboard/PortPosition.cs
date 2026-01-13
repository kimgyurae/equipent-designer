namespace EquipmentDesigner.Models
{
    /// <summary>
    /// Represents the position of a connection port on a drawing element.
    /// Ports are located at the center of each side of the element.
    /// </summary>
    public enum PortPosition
    {
        /// <summary>
        /// Port at the top center of the element.
        /// </summary>
        Top,

        /// <summary>
        /// Port at the right center of the element.
        /// </summary>
        Right,

        /// <summary>
        /// Port at the bottom center of the element.
        /// </summary>
        Bottom,

        /// <summary>
        /// Port at the left center of the element.
        /// </summary>
        Left
    }
}
