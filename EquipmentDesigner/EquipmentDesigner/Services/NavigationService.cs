using System;
using EquipmentDesigner.Models;
using EquipmentDesigner.Models.Storage;

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
        /// Event raised when viewing an existing component is requested.
        /// </summary>
        public event Action<NavigationTarget> ViewComponentRequested;

        /// <summary>
        /// Event raised when navigating to workflow complete view is requested.
        /// </summary>
        public event Action<NavigationTarget> WorkflowCompleteRequested;

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

        /// <summary>
        /// View an existing component in read-only mode.
        /// </summary>
        /// <param name="componentId">The unique identifier of the component to view.</param>
        /// <param name="hardwareLayer">The type of the component.</param>
        public void ViewComponent(string componentId, HardwareLayer hardwareLayer)
        {
            ViewComponentRequested?.Invoke(new NavigationTarget
            {
                TargetType = NavigationTargetType.HardwareDefineWorkflow,
                ComponentId = componentId,
                HardwareLayer = hardwareLayer,
                IsReadOnly = true
            });
        }

        /// <summary>
        /// Navigate to the workflow complete view.
        /// </summary>
        /// <param name="sessionDto">The workflow session DTO containing the completed workflow data.</param>
        public void NavigateToWorkflowComplete(WorkflowSessionDto sessionDto)
        {
            WorkflowCompleteRequested?.Invoke(new NavigationTarget
            {
                TargetType = NavigationTargetType.WorkflowComplete,
                SessionDto = sessionDto
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

        /// <summary>
        /// Component ID for view scenarios.
        /// </summary>
        public string ComponentId { get; set; }

        /// <summary>
        /// Component type for view scenarios.
        /// </summary>
        public HardwareLayer HardwareLayer { get; set; }

        /// <summary>
        /// Whether to open in read-only mode.
        /// </summary>
        public bool IsReadOnly { get; set; }

        /// <summary>
        /// Workflow session data for workflow complete scenarios.
        /// </summary>
        public WorkflowSessionDto SessionDto { get; set; }
    }

    /// <summary>
    /// Available navigation targets.
    /// </summary>
    public enum NavigationTargetType
    {
        Dashboard,
        HardwareDefineWorkflow,
        WorkflowComplete
    }
}