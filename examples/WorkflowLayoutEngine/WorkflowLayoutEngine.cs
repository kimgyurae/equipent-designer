using System;
using System.Collections.Generic;
using System.Linq;

namespace LaserBurrMachine.Presentation.Components.Flowchart
{
    /// <summary>
    /// Engine for calculating hierarchical vertical layout of workflow nodes.
    /// Implements a Sugiyama-style layout algorithm with:
    /// - BFS-based layer assignment
    /// - Branch-aware column assignment
    /// - Merge point detection
    /// - Collision resolution
    /// </summary>
    public class WorkflowLayoutEngine
    {
        /// <summary>
        /// Padding from the edge of the content area.
        /// </summary>
        public const int ContentPadding = 40;

        /// <summary>
        /// Vertical spacing between layers.
        /// </summary>
        public const int LayerSpacing = 100;

        /// <summary>
        /// Horizontal spacing between columns.
        /// </summary>
        public const int ColumnWidth = 200;

        /// <summary>
        /// Vertical spacing between nodes when resolving collisions.
        /// </summary>
        public const int NodeSpacingY = 20;

        /// <summary>
        /// Assigns layer (depth) values to each node using BFS traversal.
        /// Layer 0 is the initial node, increasing by 1 for each subsequent depth level.
        /// The initial node is always guaranteed to be at layer 0.
        /// </summary>
        /// <param name="nodes">List of nodes to layout</param>
        /// <param name="connections">List of connections between nodes</param>
        /// <returns>Dictionary mapping node IDs to layer values</returns>
        public Dictionary<Guid, int> AssignLayers(IList<LayoutNode> nodes, IList<LayoutConnection> connections)
        {
            var layerMap = new Dictionary<Guid, int>();

            if (nodes == null || nodes.Count == 0)
            {
                return layerMap;
            }

            // Find initial node - first try explicit IsInitial flag
            var initialNode = nodes.FirstOrDefault(n => n.IsInitial);
            
            // Fallback: find node with no incoming connections
            if (initialNode == null)
            {
                var nodesWithIncoming = new HashSet<Guid>(connections.Select(c => c.ToNodeId));
                initialNode = nodes.FirstOrDefault(n => !nodesWithIncoming.Contains(n.Id));
            }
            
            // Still no initial node found - return empty
            if (initialNode == null)
            {
                return layerMap;
            }

            var initialNodeId = initialNode.Id;

            // BFS traversal
            var queue = new Queue<Guid>();
            queue.Enqueue(initialNodeId);
            layerMap[initialNodeId] = 0;

            while (queue.Count > 0)
            {
                var currentId = queue.Dequeue();
                var currentLayer = layerMap[currentId];

                // Find outgoing connections from current node
                var outgoing = connections.Where(c => c.FromNodeId == currentId).ToList();

                foreach (var connection in outgoing)
                {
                    var targetId = connection.ToNodeId;

                    // Never adjust layer for initial node (protect from back references)
                    if (targetId == initialNodeId)
                    {
                        continue;
                    }

                    if (!layerMap.ContainsKey(targetId))
                    {
                        // First visit - assign layer
                        layerMap[targetId] = currentLayer + 1;
                        queue.Enqueue(targetId);
                    }
                    else if (layerMap[targetId] < currentLayer + 1)
                    {
                        // Already visited but at shallower depth - adjust to deeper layer
                        layerMap[targetId] = currentLayer + 1;
                    }
                }
            }

            return layerMap;
        }

        /// <summary>
        /// Assigns column values to each node based on branch context.
        /// Main flow: column 0 (center)
        /// True branch: column -1 (left)
        /// False branch: column 1 (right)
        /// </summary>
        /// <param name="nodes">List of nodes to layout</param>
        /// <param name="connections">List of connections between nodes</param>
        /// <param name="layerMap">Layer assignments from AssignLayers</param>
        /// <returns>Tuple of (columnMap, branchContextMap)</returns>
        public (Dictionary<Guid, int> ColumnMap, Dictionary<Guid, BranchContext> BranchContextMap) AssignColumns(
            IList<LayoutNode> nodes,
            IList<LayoutConnection> connections,
            Dictionary<Guid, int> layerMap)
        {
            var columnMap = new Dictionary<Guid, int>();
            var branchContextMap = new Dictionary<Guid, BranchContext>();

            if (nodes == null || nodes.Count == 0)
            {
                return (columnMap, branchContextMap);
            }

            // Find initial node
            var initialNode = nodes.FirstOrDefault(n => n.IsInitial);
            if (initialNode == null)
            {
                return (columnMap, branchContextMap);
            }

            // Initialize initial node
            columnMap[initialNode.Id] = 0;
            branchContextMap[initialNode.Id] = BranchContext.Main;

            // Get max layer for iteration
            int maxLayer = layerMap.Values.DefaultIfEmpty(0).Max();

            // Process nodes layer by layer
            for (int layer = 0; layer <= maxLayer; layer++)
            {
                var nodesInLayer = nodes.Where(n => layerMap.ContainsKey(n.Id) && layerMap[n.Id] == layer).ToList();

                foreach (var node in nodesInLayer)
                {
                    // Skip if already assigned (initial node)
                    if (columnMap.ContainsKey(node.Id))
                    {
                        continue;
                    }

                    // Find incoming connections
                    var incoming = connections.Where(c => c.ToNodeId == node.Id).ToList();

                    if (incoming.Count == 0)
                    {
                        // No incoming - treat as main flow
                        columnMap[node.Id] = 0;
                        branchContextMap[node.Id] = BranchContext.Main;
                        continue;
                    }

                    // Get the first incoming connection
                    var firstIncoming = incoming.First();
                    var parentId = firstIncoming.FromNodeId;

                    // Check if parent is a conditional node and this is a branch
                    if (firstIncoming.IsFromConditional)
                    {
                        if (firstIncoming.BranchType == BranchType.True)
                        {
                            columnMap[node.Id] = -1;
                            branchContextMap[node.Id] = BranchContext.TrueBranch;
                        }
                        else if (firstIncoming.BranchType == BranchType.False)
                        {
                            columnMap[node.Id] = 1;
                            branchContextMap[node.Id] = BranchContext.FalseBranch;
                        }
                        else
                        {
                            // No specific branch type - inherit from parent
                            columnMap[node.Id] = columnMap.ContainsKey(parentId) ? columnMap[parentId] : 0;
                            branchContextMap[node.Id] = branchContextMap.ContainsKey(parentId) ? branchContextMap[parentId] : BranchContext.Main;
                        }
                    }
                    else
                    {
                        // Not from conditional - inherit from parent
                        columnMap[node.Id] = columnMap.ContainsKey(parentId) ? columnMap[parentId] : 0;
                        branchContextMap[node.Id] = branchContextMap.ContainsKey(parentId) ? branchContextMap[parentId] : BranchContext.Main;
                    }
                }
            }

            return (columnMap, branchContextMap);
        }

        /// <summary>
        /// Detects merge points where True and False branches converge.
        /// </summary>
        /// <param name="nodes">List of nodes</param>
        /// <param name="connections">List of connections</param>
        /// <param name="branchContextMap">Branch context assignments</param>
        /// <returns>Set of node IDs that are merge points</returns>
        public HashSet<Guid> DetectMergePoints(
            IList<LayoutNode> nodes,
            IList<LayoutConnection> connections,
            Dictionary<Guid, BranchContext> branchContextMap)
        {
            var mergePoints = new HashSet<Guid>();

            foreach (var node in nodes)
            {
                var incoming = connections.Where(c => c.ToNodeId == node.Id).ToList();

                if (incoming.Count < 2)
                {
                    continue;
                }

                // Collect branch contexts of all incoming connections' source nodes
                var sourceContexts = new HashSet<BranchContext>();
                foreach (var conn in incoming)
                {
                    if (branchContextMap.TryGetValue(conn.FromNodeId, out var context))
                    {
                        sourceContexts.Add(context);
                    }
                }

                // A merge point is where both TrueBranch and FalseBranch converge
                if (sourceContexts.Contains(BranchContext.TrueBranch) &&
                    sourceContexts.Contains(BranchContext.FalseBranch))
                {
                    mergePoints.Add(node.Id);
                }
            }

            return mergePoints;
        }

        /// <summary>
        /// Applies merge point columns - assigns column 0 to all merge points.
        /// </summary>
        /// <param name="columnMap">Column assignments to update</param>
        /// <param name="branchContextMap">Branch context assignments to update</param>
        /// <param name="mergePoints">Set of merge point node IDs</param>
        public void ApplyMergePointColumns(
            Dictionary<Guid, int> columnMap,
            Dictionary<Guid, BranchContext> branchContextMap,
            HashSet<Guid> mergePoints)
        {
            foreach (var mergeId in mergePoints)
            {
                columnMap[mergeId] = 0;
                branchContextMap[mergeId] = BranchContext.Main;
            }
        }

        /// <summary>
        /// Calculates Y coordinate for a given layer.
        /// Formula: CONTENT_PADDING + layer * LAYER_SPACING
        /// </summary>
        public int CalculateYCoordinate(int layer)
        {
            return ContentPadding + layer * LayerSpacing;
        }

        /// <summary>
        /// Calculates X coordinate for a given column and node width.
        /// Formula: centerX + column * COLUMN_WIDTH - nodeWidth / 2
        /// </summary>
        public int CalculateXCoordinate(int column, int nodeWidth, int panelWidth)
        {
            int centerX = panelWidth / 2;
            return centerX + column * ColumnWidth - nodeWidth / 2;
        }

        /// <summary>
        /// Calculates final coordinates for all nodes.
        /// </summary>
        /// <param name="layoutInfos">Layout infos with layer/column already set</param>
        /// <param name="panelWidth">Width of the panel for centering</param>
        public void CalculateCoordinates(IList<NodeLayoutInfo> layoutInfos, int panelWidth)
        {
            foreach (var info in layoutInfos)
            {
                info.Y = CalculateYCoordinate(info.Layer);
                info.X = CalculateXCoordinate(info.Column, info.Width, panelWidth);
            }
        }

        /// <summary>
        /// Resolves collisions when multiple nodes are at the same layer-column.
        /// Distributes nodes vertically with NODE_SPACING_Y gap.
        /// </summary>
        public void ResolveCollisions(IList<NodeLayoutInfo> layoutInfos)
        {
            // Group nodes by (layer, column)
            var groups = layoutInfos
                .GroupBy(n => (n.Layer, n.Column))
                .Where(g => g.Count() > 1)
                .ToList();

            foreach (var group in groups)
            {
                var nodesInCell = group.ToList();
                
                // Calculate total height needed
                int totalHeight = nodesInCell.Sum(n => n.Height) + (nodesInCell.Count - 1) * NodeSpacingY;
                
                // Get original Y (center of distribution)
                int originalY = nodesInCell.First().Y;
                int startY = originalY - totalHeight / 2 + nodesInCell.First().Height / 2;

                // Distribute nodes vertically
                int currentY = startY;
                foreach (var node in nodesInCell)
                {
                    node.Y = currentY;
                    currentY += node.Height + NodeSpacingY;
                }
            }
        }

        /// <summary>
        /// Performs complete layout calculation for a workflow.
        /// </summary>
        /// <param name="nodes">List of nodes to layout</param>
        /// <param name="connections">List of connections between nodes</param>
        /// <param name="panelWidth">Width of the panel for centering</param>
        /// <returns>List of layout info with calculated positions</returns>
        public IList<NodeLayoutInfo> LayoutNodes(
            IList<LayoutNode> nodes,
            IList<LayoutConnection> connections,
            int panelWidth)
        {
            var result = new List<NodeLayoutInfo>();

            if (nodes == null || nodes.Count == 0)
            {
                return result;
            }

            // Phase 1: Assign layers
            var layerMap = AssignLayers(nodes, connections);

            // Phase 2: Assign columns
            var (columnMap, branchContextMap) = AssignColumns(nodes, connections, layerMap);

            // Phase 2.5: Detect and apply merge points
            var mergePoints = DetectMergePoints(nodes, connections, branchContextMap);
            ApplyMergePointColumns(columnMap, branchContextMap, mergePoints);

            // Create layout infos
            foreach (var node in nodes)
            {
                if (!layerMap.ContainsKey(node.Id))
                {
                    continue; // Skip unreachable nodes
                }

                var info = new NodeLayoutInfo(node.Id)
                {
                    Layer = layerMap[node.Id],
                    Column = columnMap.ContainsKey(node.Id) ? columnMap[node.Id] : 0,
                    Branch = branchContextMap.ContainsKey(node.Id) ? branchContextMap[node.Id] : BranchContext.Main,
                    Width = node.Width,
                    Height = node.Height
                };

                result.Add(info);
            }

            // Phase 3: Calculate coordinates
            CalculateCoordinates(result, panelWidth);

            // Phase 3.5: Resolve collisions
            ResolveCollisions(result);

            return result;
        }
    }
}