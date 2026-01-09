using System;

namespace LaserBurrMachine.Presentation.Components.Flowchart
{
    /// <summary>
    /// Represents a connection between two nodes for layout purposes.
    /// This is a UI-independent representation of workflow connections.
    /// </summary>
    public class LayoutConnection
    {
        /// <summary>
        /// The source node ID.
        /// </summary>
        public Guid FromNodeId { get; set; }

        /// <summary>
        /// The target node ID.
        /// </summary>
        public Guid ToNodeId { get; set; }

        /// <summary>
        /// The branch type for this connection.
        /// </summary>
        public BranchType BranchType { get; set; }

        /// <summary>
        /// Whether the source node is a conditional node.
        /// </summary>
        public bool IsFromConditional { get; set; }

        public LayoutConnection(Guid fromNodeId, Guid toNodeId, BranchType branchType = BranchType.None, bool isFromConditional = false)
        {
            FromNodeId = fromNodeId;
            ToNodeId = toNodeId;
            BranchType = branchType;
            IsFromConditional = isFromConditional;
        }
    }
}
