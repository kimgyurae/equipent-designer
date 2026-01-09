namespace LaserBurrMachine.Presentation.Components.Flowchart
{
    /// <summary>
    /// Defines the four cardinal directions for node connection points.
    /// Values are ordered clockwise starting from Top (0).
    /// </summary>
    public enum ConnectionDirection
    {
        /// <summary>
        /// Connection point at the top center of the node.
        /// </summary>
        Top = 0,

        /// <summary>
        /// Connection point at the right center of the node.
        /// </summary>
        Right = 1,

        /// <summary>
        /// Connection point at the bottom center of the node.
        /// </summary>
        Bottom = 2,

        /// <summary>
        /// Connection point at the left center of the node.
        /// </summary>
        Left = 3
    }
}
