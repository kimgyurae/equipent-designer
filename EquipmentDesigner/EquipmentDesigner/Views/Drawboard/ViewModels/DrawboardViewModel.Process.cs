using System;
using System.Collections.Generic;
using System.Linq;
using EquipmentDesigner.Models;

namespace EquipmentDesigner.ViewModels
{
    /// <summary>
    /// Partial class containing Process object management and state synchronization.
    /// Handles loading/saving UMLWorkflow from/to Process.Processes dictionary.
    /// 대안 A: ObservableCollection CurrentSteps 래퍼를 사용하여 UI 반응성 유지.
    /// </summary>
    public partial class DrawboardViewModel
    {
        private Process _process;

        /// <summary>
        /// The Process object containing UMLWorkflow for each PackML state.
        /// </summary>
        public Process Process
        {
            get => _process;
            private set => SetProperty(ref _process, value);
        }

        /// <summary>
        /// Initialize with Process input. Loads from repository by ID or creates new Process.
        /// </summary>
        /// <param name="processId">Process ID to load, or null/empty to create new.</param>
        private async void InitializeProcessAsync(string processId)
        {
            var process = await LoadProcessByIdAsync(processId);

            if (process == null)
            {
                _process = new Process
                {
                    Id = Guid.NewGuid().ToString(),
                    Processes = new Dictionary<PackMlState, UMLWorkflow>()
                };
            }
            else
            {
                _process = process;
                _process.Processes ??= new Dictionary<PackMlState, UMLWorkflow>();
            }

            LoadWorkflowForCurrentState();
        }

        /// <summary>
        /// Load workflow for current SelectedState and populate canvas.
        /// Clears canvas completely if no workflow exists for the state.
        /// </summary>
        private void LoadWorkflowForCurrentState()
        {
            ClearAllSelections();
            ClearConnectionSelection();
            CurrentSteps.Clear();
            _nextZIndex = 1;

            if (_process?.Processes == null) return;

            if (_process.Processes.TryGetValue(SelectedState, out var workflow)
                && workflow?.Steps != null)
            {
                foreach (var element in workflow.Steps)
                {
                    LoadElementToCanvas(element);
                }

                // 로드 후 IncomingSourceIds 재구축 (연결 무결성 보장)
                RebuildIncomingSourceIds();
            }
        }

        /// <summary>
        /// Load DrawingElement directly to canvas.
        /// Uses direct reference so modifications auto-sync to Process.
        /// </summary>
        /// <param name="element">The DrawingElement to load.</param>
        private void LoadElementToCanvas(DrawingElement element)
        {
            if (element == null) return;

            // Reset UI state (selection is transient, not persisted)
            element.IsSelected = false;

            // Track max ZIndex to ensure new elements are placed on top
            if (element.ZIndex >= _nextZIndex)
            {
                _nextZIndex = element.ZIndex + 1;
            }

            // Add the SAME object to canvas - modifications will auto-sync
            CurrentSteps.Add(element);
        }

        /// <summary>
        /// Rebuilds IncomingSourceIds for all elements based on OutgoingArrows.
        /// Called after loading workflow to ensure data consistency.
        /// </summary>
        private void RebuildIncomingSourceIds()
        {
            // Clear all IncomingSourceIds first
            foreach (var element in CurrentSteps)
            {
                element.IncomingSourceIds.Clear();
            }

            // Rebuild from OutgoingArrows
            foreach (var sourceElement in CurrentSteps)
            {
                foreach (var arrow in sourceElement.OutgoingArrows)
                {
                    var targetElement = CurrentSteps.FirstOrDefault(e => e.Id == arrow.TargetId);
                    if (targetElement != null)
                    {
                        targetElement.IncomingSourceIds.Add(sourceElement.Id);
                    }
                }
            }
        }

        /// <summary>
        /// Add element to current workflow in Process.
        /// Creates workflow if it doesn't exist for current state.
        /// </summary>
        /// <param name="element">The DrawingElement to add.</param>
        private void AddElementToCurrentWorkflow(DrawingElement element)
        {
            if (_process == null) return;

            var workflow = GetOrCreateWorkflowForCurrentState();
            workflow.Steps.Add(element);
        }

        /// <summary>
        /// Remove element from current workflow in Process.
        /// Also cleans up all connections referencing this element.
        /// </summary>
        /// <param name="element">The DrawingElement to remove.</param>
        private void RemoveElementFromCurrentWorkflow(DrawingElement element)
        {
            if (_process?.Processes == null) return;
            if (!_process.Processes.TryGetValue(SelectedState, out var workflow)) return;
            if (workflow?.Steps == null) return;

            // 1. 이 element를 가리키는 모든 연결 정리 (IncomingSourceIds에서 source 찾기)
            foreach (var sourceId in element.IncomingSourceIds.ToList())
            {
                var sourceElement = CurrentSteps.FirstOrDefault(e => e.Id == sourceId);
                if (sourceElement != null)
                {
                    var arrowsToRemove = sourceElement.OutgoingArrows
                        .Where(a => a.TargetId == element.Id)
                        .ToList();
                    foreach (var arrow in arrowsToRemove)
                    {
                        sourceElement.OutgoingArrows.Remove(arrow);
                    }
                }
            }

            // 2. 이 element에서 나가는 연결의 target들 정리
            foreach (var arrow in element.OutgoingArrows.ToList())
            {
                var targetElement = CurrentSteps.FirstOrDefault(e => e.Id == arrow.TargetId);
                if (targetElement != null)
                {
                    targetElement.IncomingSourceIds.Remove(element.Id);
                }
            }

            // 3. Workflow에서 element 제거
            workflow.Steps.Remove(element);
        }

        /// <summary>
        /// Get or create workflow for current SelectedState.
        /// </summary>
        /// <returns>The UMLWorkflow for the current state.</returns>
        private UMLWorkflow GetOrCreateWorkflowForCurrentState()
        {
            _process.Processes ??= new Dictionary<PackMlState, UMLWorkflow>();

            if (!_process.Processes.TryGetValue(SelectedState, out var workflow))
            {
                workflow = new UMLWorkflow
                {
                    Id = Guid.NewGuid().ToString(),
                    State = SelectedState,
                    Name = string.Empty,
                    Description = string.Empty,
                    ImplementationInstructions = Array.Empty<string>(),
                    Steps = new List<DrawingElement>()
                };
                _process.Processes[SelectedState] = workflow;
            }

            workflow.Steps ??= new List<DrawingElement>();
            return workflow;
        }

        #region Legacy Method Aliases (Backward Compatibility)

        /// <summary>
        /// Alias for AddElementToCurrentWorkflow for backward compatibility.
        /// </summary>
        private void AddStepToCurrentWorkflow(DrawingElement element) => AddElementToCurrentWorkflow(element);

        /// <summary>
        /// Alias for RemoveElementFromCurrentWorkflow for backward compatibility.
        /// </summary>
        private void RemoveStepFromCurrentWorkflow(DrawingElement element) => RemoveElementFromCurrentWorkflow(element);

        #endregion
    }
}
