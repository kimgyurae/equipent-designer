using System;
using System.Linq;
using System.Windows;
using EquipmentDesigner.Models;
using EquipmentDesigner.Models.Storage;
using EquipmentDesigner.Services;
using EquipmentDesigner.Services.Storage;
using EquipmentDesigner.Views.Dashboard;
using EquipmentDesigner.Views.HardwareDefineWorkflow;

namespace EquipmentDesigner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly DashboardView _dashboardView;

        public MainWindow()
        {
            InitializeComponent();
            
            // Initialize dashboard view
            _dashboardView = new DashboardView();
            
            // Subscribe to navigation events
            NavigationService.Instance.NavigationRequested += OnNavigationRequested;
            NavigationService.Instance.NavigateToDashboardRequested += OnNavigateToDashboard;
            NavigationService.Instance.ResumeWorkflowRequested += OnResumeWorkflowRequested;
            NavigationService.Instance.ViewComponentRequested += OnViewComponentRequested;
            
            // Show dashboard by default
            MainContent.Content = _dashboardView;
        }

        /// <summary>
        /// Shows the full-screen backdrop overlay.
        /// </summary>
        public void ShowBackdrop()
        {
            BackdropOverlay.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Hides the full-screen backdrop overlay.
        /// </summary>
        public void HideBackdrop()
        {
            BackdropOverlay.Visibility = Visibility.Collapsed;
        }

        private void OnNavigationRequested(NavigationTarget target)
        {
            if (target.TargetType == NavigationTargetType.HardwareDefineWorkflow)
            {
                var workflowViewModel = new HardwareDefineWorkflowViewModel(target.StartType);
                var workflowView = new HardwareDefineWorkflowView
                {
                    DataContext = workflowViewModel
                };
                MainContent.Content = workflowView;
            }
        }

        private void OnNavigateToDashboard()
        {
            // Refresh dashboard data before showing
            if (_dashboardView.DataContext is DashboardViewModel vm)
            {
                vm.RefreshAsync();
            }
            MainContent.Content = _dashboardView;
        }

        private async void OnResumeWorkflowRequested(NavigationTarget target)
        {
            if (string.IsNullOrEmpty(target.WorkflowId))
                return;

            try
            {
                var repository = ServiceLocator.GetService<ITypedDataRepository<IncompleteWorkflowDataStore>>();
                var dataStore = await repository.LoadAsync();

                var sessionDto = dataStore?.WorkflowSessions?
                    .FirstOrDefault(s => s.WorkflowId == target.WorkflowId);

                if (sessionDto != null)
                {
                    var workflowViewModel = HardwareDefineWorkflowViewModel.FromWorkflowSessionDto2(sessionDto);
                    var workflowView = new HardwareDefineWorkflowView
                    {
                        DataContext = workflowViewModel
                    };
                    MainContent.Content = workflowView;
                }
            }
            catch
            {
                // If resume fails, stay on current view
            }
        }

        private async void OnViewComponentRequested(NavigationTarget target)
        {
            if (string.IsNullOrEmpty(target.ComponentId))
                return;

            try
            {
                // Load from UploadedWorkflowDataStore (ComponentId is WorkflowId)
                var repository = ServiceLocator.GetService<ITypedDataRepository<UploadedWorkflowDataStore>>();
                var dataStore = await repository.LoadAsync();

                var sessionDto = dataStore?.WorkflowSessions?
                    .FirstOrDefault(s => s.WorkflowId == target.ComponentId);

                if (sessionDto != null)
                {
                    // Use FromWorkflowSessionDto2 to rebuild the workflow
                    var workflowViewModel = HardwareDefineWorkflowViewModel.FromWorkflowSessionDto2(sessionDto);
                    workflowViewModel.IsReadOnly = true;  // Set read-only mode

                    var workflowView = new HardwareDefineWorkflowView
                    {
                        DataContext = workflowViewModel
                    };
                    MainContent.Content = workflowView;
                }
            }
            catch
            {
                // If loading fails, stay on current view
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            // Unsubscribe from events to prevent memory leaks
            NavigationService.Instance.NavigationRequested -= OnNavigationRequested;
            NavigationService.Instance.NavigateToDashboardRequested -= OnNavigateToDashboard;
            NavigationService.Instance.ResumeWorkflowRequested -= OnResumeWorkflowRequested;
            NavigationService.Instance.ViewComponentRequested -= OnViewComponentRequested;
            base.OnClosed(e);
        }
    }
}