using System;
using System.Linq;
using System.Windows;
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
                var repository = ServiceLocator.GetService<IDataRepository>();
                var dataStore = await repository.LoadAsync();

                var sessionDto = dataStore?.WorkflowSessions?
                    .FirstOrDefault(s => s.WorkflowId == target.WorkflowId);

                if (sessionDto != null)
                {
                    var workflowViewModel = HardwareDefineWorkflowViewModel.FromWorkflowSessionDto(sessionDto);
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

        protected override void OnClosed(EventArgs e)
        {
            // Unsubscribe from events to prevent memory leaks
            NavigationService.Instance.NavigationRequested -= OnNavigationRequested;
            NavigationService.Instance.NavigateToDashboardRequested -= OnNavigateToDashboard;
            NavigationService.Instance.ResumeWorkflowRequested -= OnResumeWorkflowRequested;
            base.OnClosed(e);
        }
    }
}