using System;

namespace LaserBurrMachine.Presentation.Components.Flowchart
{
    /// <summary>
    /// Represents a node for layout purposes.
    /// This is a UI-independent representation of workflow nodes.
    /// </summary>
    public class LayoutNode
    {
        /// <summary>
        /// Unique identifier for the node.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Width of the node.
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Height of the node.
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Whether this node is the initial (entry) node of the workflow.
        /// </summary>
        public bool IsInitial { get; set; }

        /// <summary>
        /// Whether this node is a conditional (branching) node.
        /// </summary>
        public bool IsConditional { get; set; }

        /// <summary>
        /// Whether this node is a terminal (exit) node of the workflow.
        /// </summary>
        public bool IsTerminal { get; set; }

        /// <summary>
        /// Whether this node is a merge (convergence) node of the workflow.
        /// </summary>
        public bool IsMerge { get; set; }

        public LayoutNode(Guid id, int width = 100, int height = 40)
        {
            Id = id;
            Width = width;
            Height = height;
            IsInitial = false;
            IsConditional = false;
            IsTerminal = false;
            IsMerge = false;
        }
    }
}
