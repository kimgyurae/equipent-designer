using System;
using EquipmentDesigner.Views.HardwareDefineWorkflow;

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
        /// Navigate to the Hardware Define Workflow with the specified start type.
        /// </summary>
        public void NavigateToHardwareDefineWorkflow(WorkflowStartType startType)
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
    }

    /// <summary>
    /// Represents a navigation target.
    /// </summary>
    public class NavigationTarget
    {
        public NavigationTargetType TargetType { get; set; }
        public WorkflowStartType StartType { get; set; }
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
