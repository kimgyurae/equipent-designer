namespace LaserBurrMachine.Presentation.Components.Flowchart
{
    /// <summary>
    /// Represents the branch context for a node in a workflow layout.
    /// Used to track which branch path a node belongs to during vertical layout calculation.
    /// </summary>
    public enum BranchContext
    {
        /// <summary>
        /// Main flow - nodes before any conditional branch or after merge points
        /// </summary>
        Main,

        /// <summary>
        /// True branch path from a ConditionalNode
        /// </summary>
        TrueBranch,

        /// <summary>
        /// False branch path from a ConditionalNode
        /// </summary>
        FalseBranch
    }
}
