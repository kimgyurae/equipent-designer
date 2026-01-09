using System;

namespace LaserBurrMachine.Presentation.Components.Flowchart
{
    /// <summary>
    /// Contains layout information for a single node in a workflow visualization.
    /// Stores the calculated layer, column, branch context, and final coordinates.
    /// </summary>
    public class NodeLayoutInfo
    {
        /// <summary>
        /// Unique identifier for the node.
        /// </summary>
        public Guid NodeId { get; set; }

        /// <summary>
        /// Vertical layer (depth) in the workflow graph.
        /// Layer 0 is the initial node, increasing downward.
        /// </summary>
        public int Layer { get; set; }

        /// <summary>
        /// Horizontal column position.
        /// 0 = center, -1 = left (True branch), 1 = right (False branch).
        /// </summary>
        public int Column { get; set; }

        /// <summary>
        /// The branch context this node belongs to.
        /// </summary>
        public BranchContext Branch { get; set; }

        /// <summary>
        /// Calculated X coordinate for the node position.
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// Calculated Y coordinate for the node position.
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        /// Width of the node (used for centering calculations).
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Height of the node (used for collision resolution).
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Creates a new NodeLayoutInfo with the specified node ID.
        /// </summary>
        public NodeLayoutInfo(Guid nodeId)
        {
            NodeId = nodeId;
            Layer = 0;
            Column = 0;
            Branch = BranchContext.Main;
            X = 0;
            Y = 0;
            Width = 100;
            Height = 40;
        }
    }
}
