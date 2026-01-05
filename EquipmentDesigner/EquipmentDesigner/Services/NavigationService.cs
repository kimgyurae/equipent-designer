using System;
using EquipmentDesigner.Models;

namespace EquipmentDesigner.Services
{
    /// <summary>
    /// Simple navigation service for managing page navigation.
    /// </summary>
    public class NavigationService
    {
        private static NavigationService _instance;
        public static NavigationService Instance => _instance ??= new NavigationService();

        /// <summary>
        /// Event raised when navigation is requested.
        /// </summary>
        public event Action<NavigationTarget> NavigationRequested;

        /// <summary>
        /// Event raised when navigation back to dashboard is requested.
        /// </summary>
        public event Action NavigateToDashboardRequested;

        /// <summary>
        /// Event raised when resuming an existing workflow is requested.
        /// </summary>
        public event Action<NavigationTarget> ResumeWorkflowRequested;

        /// <summary>
        /// Navigate to the Hardware Define Workflow with the specified start type.
        /// </summary>
        public void NavigateToHardwareDefineWorkflow(HardwareLayer startType)
        {
            NavigationRequested?.Invoke(new NavigationTarget
            {
                TargetType = NavigationTargetType.HardwareDefineWorkflow,
                StartType = startType
            });
        }

        /// <summary>
        /// Navigate back to the dashboard.
        /// </summary>
        public void NavigateToDashboard()
        {
            NavigateToDashboardRequested?.Invoke();
        }

        /// <summary>
        /// Resume an existing workflow from saved state.
        /// </summary>
        /// <param name="workflowId">The unique identifier of the workflow to resume.</param>
        public void ResumeWorkflow(string workflowId)
        {
            ResumeWorkflowRequested?.Invoke(new NavigationTarget
            {
                TargetType = NavigationTargetType.HardwareDefineWorkflow,
                WorkflowId = workflowId
            });
        }
    }

    /// <summary>
    /// Represents a navigation target.
    /// </summary>
    public class NavigationTarget
    {
        public NavigationTargetType TargetType { get; set; }
        public HardwareLayer StartType { get; set; }
        
        /// <summary>
        /// Workflow ID for resume scenarios (null for new workflows).
        /// </summary>
        public string WorkflowId { get; set; }
    }

    /// <summary>
    /// Available navigation targets.
    /// </summary>
    public enum NavigationTargetType
    {
        Dashboard,
        HardwareDefineWorkflow
    }
}