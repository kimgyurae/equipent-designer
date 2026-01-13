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
        /// Event raised when viewing an existing component is requested.
        /// </summary>
        public event Action<NavigationTarget> ViewComponentRequested;

        /// <summary>
        /// Event raised when navigating to workflow complete view is requested.
        /// </summary>
        public event Action<NavigationTarget> WorkflowCompleteRequested;

        /// <summary>
        /// Event raised when navigating to process define view is requested.
        /// </summary>
        public event Action<NavigationTarget> ShowDashboardRequested;

        /// <summary>
        /// Event raised when navigating back from process define view is requested.
        /// </summary>
        public event Action NavigateBackFromShowDashboardRequested;

        /// <summary>
        /// Navigate to the Hardware Define Workflow with the specified start type.
        /// </summary>
        public void NavigateToHardwareDefineWorkflow(HardwareType startType)
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
        /// <param name="hardwareType">The type of the component.</param>
        public void ViewComponent(string componentId, HardwareType hardwareType)
        {
            ViewComponentRequested?.Invoke(new NavigationTarget
            {
                TargetType = NavigationTargetType.HardwareDefineWorkflow,
                ComponentId = componentId,
                HardwareType = hardwareType,
                IsReadOnly = true
            });
        }

        /// <summary>
        /// Navigate to the workflow complete view.
        /// </summary>
        /// <param name="sessionDto">The workflow session DTO containing the completed workflow data.</param>
        public void NavigateToWorkflowComplete(HardwareDefinition sessionDto)
        {
            WorkflowCompleteRequested?.Invoke(new NavigationTarget
            {
                TargetType = NavigationTargetType.WorkflowComplete,
                SessionDto = sessionDto
            });
        }

        /// <summary>
        /// Navigate to the process define view.
        /// </summary>
        /// <param name="processId">The process ID to load in the drawboard.</param>
        public void NavigateToDrawboard(string processId)
        {
            ShowDashboardRequested?.Invoke(new NavigationTarget
            {
                TargetType = NavigationTargetType.Drawboard,
                ProcessId = processId
            });
        }

        /// <summary>
        /// Navigate back from process define view to the previous view (hardware define workflow).
        /// </summary>
        public void NavigateBackFromDrawboard()
        {
            NavigateBackFromShowDashboardRequested?.Invoke();
        }

        /// <summary>
        /// Navigate to the Create New Component view.
        /// </summary>
        public void NavigateToCreateNewComponent()
        {
            NavigationRequested?.Invoke(new NavigationTarget
            {
                TargetType = NavigationTargetType.CreateNewComponent
            });
        }

        /// <summary>
        /// Navigate to the Component List view.
        /// </summary>
        public void NavigateToComponentList()
        {
            NavigationRequested?.Invoke(new NavigationTarget
            {
                TargetType = NavigationTargetType.ComponentList
            });
        }
    }

    /// <summary>
    /// Represents a navigation target.
    /// </summary>
    public class NavigationTarget
    {
        public NavigationTargetType TargetType { get; set; }
        public HardwareType StartType { get; set; }
        
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
        public HardwareType HardwareType { get; set; }

        /// <summary>
        /// Whether to open in read-only mode.
        /// </summary>
        public bool IsReadOnly { get; set; }

        /// <summary>
        /// Workflow session data for workflow complete scenarios.
        /// </summary>
        public HardwareDefinition SessionDto { get; set; }

        /// <summary>
        /// Process ID for drawboard navigation.
        /// </summary>
        public string ProcessId { get; set; }
    }

    /// <summary>
    /// Available navigation targets.
    /// </summary>
    public enum NavigationTargetType
    {
        Dashboard,
        HardwareDefineWorkflow,
        WorkflowComplete,
        Drawboard,
        CreateNewComponent,
        ComponentList
    }
}