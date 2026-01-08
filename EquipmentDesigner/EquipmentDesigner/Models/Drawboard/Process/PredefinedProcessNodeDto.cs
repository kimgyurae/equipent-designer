using System.Collections.Generic;
using System.Linq;

namespace EquipmentDesigner.Models
{
    /// <summary>
    /// Predefined process node - a complete subprocess containing its own workflow.
    /// Contains Initial, Terminal, Action, Decision, and nested PredefinedProcess nodes.
    /// Inbound: 1+ | Outbound: 0 or 1
    /// </summary>
    public class PredefinedProcessNodeDto : ProcessNodeBase
    {
        public override UMLNodeType NodeType => UMLNodeType.PredefinedAction;

        /// <summary>
        /// All nodes in this predefined process (polymorphic storage).
        /// Must include at least one Initial node and one Terminal node,
        /// with one or more Action, Decision, or nested PredefinedProcess nodes.
        /// </summary>
        public List<ProcessNodeBase> Nodes { get; set; } = new List<ProcessNodeBase>();

        /// <summary>
        /// Initial node ID (entry point for this subprocess)
        /// </summary>
        public string InitialNodeId { get; set; }

        /// <summary>
        /// Outgoing connection after subprocess completes.
        /// null means this node acts as a terminal point in the parent workflow.
        /// </summary>
        public ProcessConnectionDto OutgoingConnection { get; set; }

        /// <summary>
        /// Validates the PredefinedProcess node configuration.
        /// Requirements:
        /// - Must have at least one Initial node
        /// - Must have at least one Terminal node
        /// - Must have at least one intermediate node (Action, Decision, or nested PredefinedProcess)
        /// - InitialNodeId must reference a valid Initial node
        /// - All child nodes must be valid
        /// </summary>
        public override NodeValidationResult Validate()
        {
            var result = new NodeValidationResult
            {
                NodeId = Id,
                NodeType = NodeType
            };

            if (Nodes == null || Nodes.Count == 0)
            {
                result.AddError("PredefinedProcess must contain at least one node.");
                return result;
            }

            // Count node types
            var initialNodes = Nodes.OfType<InitialNodeDto>().ToList();
            var terminalNodes = Nodes.OfType<TerminalNodeDto>().ToList();
            var actionNodes = Nodes.OfType<ActionNodeDto>().ToList();
            var decisionNodes = Nodes.OfType<DecisionNodeDto>().ToList();
            var predefinedNodes = Nodes.OfType<PredefinedProcessNodeDto>().ToList();

            // Validate: Must have at least one Initial node
            if (initialNodes.Count == 0)
            {
                result.AddError("PredefinedProcess must have at least one Initial node.");
            }

            // Validate: Must have at least one Terminal node
            if (terminalNodes.Count == 0)
            {
                result.AddError("PredefinedProcess must have at least one Terminal node.");
            }

            // Validate: Must have at least one intermediate node (Action, Decision, or nested PredefinedProcess)
            int intermediateNodeCount = actionNodes.Count + decisionNodes.Count + predefinedNodes.Count;
            if (intermediateNodeCount == 0)
            {
                result.AddError("PredefinedProcess must have at least one Action, Decision, or nested PredefinedProcess node.");
            }

            // Validate: InitialNodeId must be set and reference a valid Initial node
            if (string.IsNullOrWhiteSpace(InitialNodeId))
            {
                result.AddError("PredefinedProcess must have InitialNodeId set.");
            }
            else
            {
                var referencedInitial = initialNodes.FirstOrDefault(n => n.Id == InitialNodeId);
                if (referencedInitial == null)
                {
                    result.AddError($"InitialNodeId '{InitialNodeId}' does not reference a valid Initial node.");
                }
            }

            // Validate all child nodes
            foreach (var node in Nodes)
            {
                var childResult = node.Validate();
                if (!childResult.IsValid)
                {
                    foreach (var error in childResult.Errors)
                    {
                        result.AddError($"[{node.NodeType}:{node.Id}] {error}");
                    }
                }
            }

            return result;
        }
    }
}