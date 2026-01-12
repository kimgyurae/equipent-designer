using System;
using System.Collections.Generic;
using System.Linq;
using EquipmentDesigner.Models;

namespace EquipmentDesigner.ViewModels
{
    /// <summary>
    /// Partial class containing Process object management and state synchronization.
    /// Handles loading/saving UMLWorkflow from/to Process.Processes dictionary.
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
        /// Initialize with Process input. Null creates new Process with GUID.
        /// </summary>
        /// <param name="process">Existing Process or null to create new.</param>
        private void InitializeProcess(Process process)
        {
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
            Elements.Clear();
            _nextZIndex = 1;

            if (_process?.Processes == null) return;

            if (_process.Processes.TryGetValue(SelectedState, out var workflow) 
                && workflow?.Steps != null)
            {
                foreach (var step in workflow.Steps)
                {
                    LoadStepToCanvas(step);
                }
            }
        }

        /// <summary>
        /// Load DrawingElement from UMLStep and add to canvas.
        /// Uses direct reference so modifications auto-sync to Process.
        /// </summary>
        /// <param name="step">The UMLStep containing DrawingElement.</param>
        private void LoadStepToCanvas(UMLStep step)
        {
            if (step?.DrawingElement == null) return;

            var element = step.DrawingElement;

            // Reset UI state (selection is transient, not persisted)
            element.IsSelected = false;

            // Track max ZIndex to ensure new elements are placed on top
            if (element.ZIndex >= _nextZIndex)
            {
                _nextZIndex = element.ZIndex + 1;
            }

            // Add the SAME object to canvas - modifications will auto-sync
            Elements.Add(element);
        }

        /// <summary>
        /// Add element to current workflow in Process.
        /// Creates workflow if it doesn't exist for current state.
        /// </summary>
        /// <param name="element">The DrawingElement to add.</param>
        private void AddStepToCurrentWorkflow(DrawingElement element)
        {
            if (_process == null) return;

            var workflow = GetOrCreateWorkflowForCurrentState();
            var step = new UMLStep
            {
                Id = Guid.NewGuid().ToString(),
                DrawingElement = element
            };

            workflow.Steps.Add(step);
        }

        /// <summary>
        /// Remove element from current workflow in Process.
        /// </summary>
        /// <param name="element">The DrawingElement to remove.</param>
        private void RemoveStepFromCurrentWorkflow(DrawingElement element)
        {
            if (_process?.Processes == null) return;
            if (!_process.Processes.TryGetValue(SelectedState, out var workflow)) return;
            if (workflow?.Steps == null) return;

            var stepToRemove = workflow.Steps.FirstOrDefault(s => s.DrawingElement?.Id == element.Id);
            if (stepToRemove != null)
            {
                workflow.Steps.Remove(stepToRemove);
            }
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
                    Steps = new List<UMLStep>(),
                    Connections = new List<UMLConnection>()
                };
                _process.Processes[SelectedState] = workflow;
            }

            workflow.Steps ??= new List<UMLStep>();
            return workflow;
        }
    }
}