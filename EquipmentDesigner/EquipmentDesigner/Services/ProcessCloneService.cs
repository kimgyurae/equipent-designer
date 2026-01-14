using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using EquipmentDesigner.Models;

namespace EquipmentDesigner.Services
{
    /// <summary>
    /// Service interface for cloning Process data during hardware copy operations.
    /// Handles ProcessId regeneration and deep cloning of Process workflows.
    /// </summary>
    public interface IProcessCloneService
    {
        /// <summary>
        /// Regenerates ProcessIds for all non-Device nodes in the hardware tree.
        /// Device nodes will have their ProcessId set to null.
        /// </summary>
        /// <param name="rootNode">Root HardwareDefinition node.</param>
        /// <returns>Dictionary mapping oldProcessId → newProcessId.</returns>
        Dictionary<string, string> RegenerateProcessIds(HardwareDefinition rootNode);

        /// <summary>
        /// Deep clones all processes based on the ProcessId mapping.
        /// Fetches original processes, clones with new IDs, and saves to local storage.
        /// </summary>
        /// <param name="processIdMapping">Mapping of old ProcessId to new ProcessId.</param>
        Task CloneAllProcessesAsync(Dictionary<string, string> processIdMapping);
    }

    /// <summary>
    /// Implementation of IProcessCloneService for deep cloning Process data.
    /// </summary>
    public class ProcessCloneService : IProcessCloneService
    {
        private readonly ILocalProcessRepositoryManager _localProcessManager;
        private readonly IProcessApiService _remoteProcessService;

        public ProcessCloneService(
            ILocalProcessRepositoryManager localProcessManager,
            IProcessApiService remoteProcessService)
        {
            _localProcessManager = localProcessManager ?? throw new ArgumentNullException(nameof(localProcessManager));
            _remoteProcessService = remoteProcessService ?? throw new ArgumentNullException(nameof(remoteProcessService));
        }

        /// <inheritdoc />
        public Dictionary<string, string> RegenerateProcessIds(HardwareDefinition rootNode)
        {
            var mapping = new Dictionary<string, string>();
            RegenerateProcessIdsRecursive(rootNode, mapping);
            return mapping;
        }

        /// <inheritdoc />
        public async Task CloneAllProcessesAsync(Dictionary<string, string> processIdMapping)
        {
            if (processIdMapping == null || processIdMapping.Count == 0)
                return;

            var processes = await _localProcessManager.LoadAsync() ?? new List<Process>();

            foreach (var kvp in processIdMapping)
            {
                var oldProcessId = kvp.Key;
                var newProcessId = kvp.Value;

                // Skip if already cloned (shouldn't happen but safety check)
                if (processes.Any(p => p.Id == newProcessId))
                    continue;

                var originalProcess = await FetchProcessAsync(oldProcessId);

                if (originalProcess != null)
                {
                    var clonedProcess = CloneProcess(originalProcess, newProcessId);
                    processes.Add(clonedProcess);
                }
                else
                {
                    // Create empty process if original not found
                    processes.Add(new Process
                    {
                        Id = newProcessId,
                        Processes = new Dictionary<PackMlState, UMLWorkflow>()
                    });
                }
            }

            await _localProcessManager.SaveAsync(processes);
        }

        #region Private Methods - ProcessId Regeneration

        /// <summary>
        /// Recursively regenerates ProcessIds for all non-Device nodes.
        /// </summary>
        private void RegenerateProcessIdsRecursive(
            HardwareDefinition node,
            Dictionary<string, string> mapping)
        {
            if (node == null)
                return;

            // Device type nodes should have null/empty ProcessId
            if (node.HardwareType == HardwareType.Device)
            {
                node.ProcessId = null;
            }
            else
            {
                var oldProcessId = node.ProcessId;
                if (!string.IsNullOrEmpty(oldProcessId))
                {
                    // Check if already mapped (shared ProcessId case)
                    if (!mapping.ContainsKey(oldProcessId))
                    {
                        mapping[oldProcessId] = Guid.NewGuid().ToString();
                    }
                    node.ProcessId = mapping[oldProcessId];
                }
                else
                {
                    // Generate new ProcessId if none exists
                    node.ProcessId = Guid.NewGuid().ToString();
                }
            }

            // Process children recursively
            if (node.Children != null)
            {
                foreach (var child in node.Children)
                {
                    RegenerateProcessIdsRecursive(child, mapping);
                }
            }
        }

        #endregion

        #region Private Methods - Process Fetching

        /// <summary>
        /// Fetches a Process by ID, trying local storage first then remote.
        /// </summary>
        private async Task<Process> FetchProcessAsync(string processId)
        {
            if (string.IsNullOrEmpty(processId))
                return null;

            // Try local first
            var localProcess = await _localProcessManager.GetProcessByIdAsync(processId);
            if (localProcess != null)
                return localProcess;

            // Try remote
            try
            {
                var response = await _remoteProcessService.GetProcessByIdAsync(processId);
                if (response != null && response.Success && response.Data != null)
                    return response.Data;
            }
            catch
            {
                // Ignore remote fetch errors, return null
            }

            return null;
        }

        #endregion

        #region Private Methods - Process Cloning

        /// <summary>
        /// Deep clones a Process with new IDs and remapped connections.
        /// </summary>
        private Process CloneProcess(Process original, string newProcessId)
        {
            var cloned = new Process
            {
                Id = newProcessId,
                Processes = new Dictionary<PackMlState, UMLWorkflow>()
            };

            if (original.Processes == null)
                return cloned;

            foreach (var kvp in original.Processes)
            {
                var clonedWorkflow = CloneWorkflow(kvp.Value);
                cloned.Processes[kvp.Key] = clonedWorkflow;
            }

            return cloned;
        }

        /// <summary>
        /// Deep clones a UMLWorkflow with new element IDs and remapped connections.
        /// </summary>
        private UMLWorkflow CloneWorkflow(UMLWorkflow original)
        {
            if (original == null)
                return null;

            var elementIdMapping = new Dictionary<string, string>();

            var cloned = new UMLWorkflow
            {
                Id = Guid.NewGuid().ToString(),
                State = original.State,
                ImplementationInstructions = original.ImplementationInstructions?.ToArray(),
                Steps = new List<DrawingElement>()
            };

            if (original.Steps == null || original.Steps.Count == 0)
                return cloned;

            // First pass: Clone all elements and build ID mapping
            foreach (var element in original.Steps)
            {
                var clonedElement = CloneElementWithConnections(element);
                elementIdMapping[element.Id] = clonedElement.Id;
                cloned.Steps.Add(clonedElement);
            }

            // Second pass: Remap all connection TargetIds
            foreach (var element in cloned.Steps)
            {
                RemapConnectionTargets(element, elementIdMapping);
            }

            return cloned;
        }

        /// <summary>
        /// Clones a DrawingElement including its OutgoingArrows connections.
        /// </summary>
        private DrawingElement CloneElementWithConnections(DrawingElement original)
        {
            if (original == null)
                return null;

            // Create new instance of the same type
            DrawingElement clone = original.GetType().Name switch
            {
                nameof(InitialElement) => new InitialElement(),
                nameof(ActionElement) => new ActionElement(),
                nameof(DecisionElement) => new DecisionElement(),
                nameof(TerminalElement) => new TerminalElement(),
                nameof(PredefinedActionElement) => new PredefinedActionElement(),
                nameof(TextboxElement) => new TextboxElement(),
                _ => throw new InvalidOperationException($"Unknown DrawingElement type: {original.GetType().Name}")
            };

            // Copy all base properties (Id is already a new GUID from constructor)
            clone.X = original.X;
            clone.Y = original.Y;
            clone.Width = original.Width;
            clone.Height = original.Height;
            clone.Opacity = original.Opacity;
            clone.ZIndex = original.ZIndex;
            clone.IsLocked = original.IsLocked;
            clone.Text = original.Text;
            clone.FontSize = original.FontSize;
            clone.TextAlign = original.TextAlign;
            clone.TextColor = original.TextColor;
            clone.TextOpacity = original.TextOpacity;

            // Copy connections (TargetIds will be remapped in second pass)
            if (original.OutgoingArrows != null)
            {
                foreach (var arrow in original.OutgoingArrows)
                {
                    clone.OutgoingArrows.Add(new UMLConnection2
                    {
                        Label = arrow.Label,
                        TargetId = arrow.TargetId, // Will be remapped later
                        TailPort = arrow.TailPort,
                        HeadPort = arrow.HeadPort
                    });
                }
            }

            return clone;
        }

        /// <summary>
        /// Remaps all OutgoingArrows TargetIds to the new element IDs.
        /// </summary>
        private void RemapConnectionTargets(
            DrawingElement element,
            Dictionary<string, string> elementIdMapping)
        {
            if (element?.OutgoingArrows == null)
                return;

            foreach (var arrow in element.OutgoingArrows)
            {
                if (!string.IsNullOrEmpty(arrow.TargetId) &&
                    elementIdMapping.TryGetValue(arrow.TargetId, out var newTargetId))
                {
                    arrow.TargetId = newTargetId;
                }
            }
        }

        #endregion
    }
}
