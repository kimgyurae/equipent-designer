using System;
using System.Collections.Generic;

namespace EquipmentDesigner.Models
{
    /// <summary>
    /// Manages workspaces for all PackML states with state switching capability
    /// </summary>
    public class WorkspaceManager
    {
        private readonly Dictionary<PackMlState, StateWorkspace> _workspaces = new Dictionary<PackMlState, StateWorkspace>();

        /// <summary>
        /// Currently active PackML state
        /// </summary>
        public PackMlState CurrentState { get; private set; } = PackMlState.Idle;

        /// <summary>
        /// Workspace for the current state
        /// </summary>
        public StateWorkspace CurrentWorkspace => _workspaces[CurrentState];

        public WorkspaceManager()
        {
            // Initialize workspaces for all 17 PackML states
            foreach (PackMlState state in Enum.GetValues(typeof(PackMlState)))
            {
                _workspaces[state] = new StateWorkspace(state);
            }
        }

        /// <summary>
        /// Switches to a different PackML state and returns its workspace.
        /// Preserves all elements in the previous state.
        /// </summary>
        public StateWorkspace SwitchState(PackMlState newState)
        {
            CurrentState = newState;
            return _workspaces[newState];
        }

        /// <summary>
        /// Gets the workspace for a specific PackML state.
        /// </summary>
        public StateWorkspace GetWorkspace(PackMlState state)
        {
            return _workspaces[state];
        }
    }
}
