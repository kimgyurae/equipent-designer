namespace LaserBurrMachine.Presentation.Components.Flowchart
{
    /// <summary>
    /// Represents the rectangular bounds of a node for connection point calculation.
    /// </summary>
    public struct NodeRect
    {
        /// <summary>
        /// X coordinate of the top-left corner.
        /// </summary>
        public int X { get; }

        /// <summary>
        /// Y coordinate of the top-left corner.
        /// </summary>
        public int Y { get; }

        /// <summary>
        /// Width of the node.
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// Height of the node.
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// X coordinate of the center point.
        /// </summary>
        public int CenterX => X + Width / 2;

        /// <summary>
        /// Y coordinate of the center point.
        /// </summary>
        public int CenterY => Y + Height / 2;

        /// <summary>
        /// Creates a new NodeRect with the specified position and size.
        /// </summary>
        public NodeRect(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
    }
}
