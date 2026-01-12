using System;
using System.Linq;
using System.Windows;
using EquipmentDesigner.Models;
using EquipmentDesigner.Services;
using EquipmentDesigner.ViewModels;
using EquipmentDesigner.Views;

namespace EquipmentDesigner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly DashboardView _dashboardView;
        private object _previousViewBeforeDrawboard;

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
            NavigationService.Instance.WorkflowCompleteRequested += OnWorkflowCompleteRequested;
            NavigationService.Instance.ShowDashboardRequested += OnShowDrawboardRequested;
            NavigationService.Instance.NavigateBackFromShowDashboardRequested += OnNavigateBackFromDrawboard;
            
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
            else if (target.TargetType == NavigationTargetType.CreateNewComponent)
            {
                var view = new CreateNewComponentView();
                MainContent.Content = view;
            }
            else if (target.TargetType == NavigationTargetType.ComponentList)
            {
                var view = new ComponentListView();
                MainContent.Content = view;
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
                var repository = ServiceLocator.GetService<IWorkflowRepository>();
                var sessions = await repository.LoadAsync();

                var sessionDto = sessions?.FirstOrDefault(s => s.Id == target.WorkflowId);

                if (sessionDto != null)
                {
                    var workflowViewModel = HardwareDefineWorkflowViewModel.FromHardwareDefinition(sessionDto);
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
                // Load from API service (ComponentId is WorkflowId)
                var apiService = ServiceLocator.GetService<IHardwareApiService>();
                var response = await apiService.GetSessionByIdAsync(target.ComponentId);

                if (response.Success && response.Data != null)
                {
                    // Use FromHardwareDefinition to rebuild the workflow
                    var workflowViewModel = HardwareDefineWorkflowViewModel.FromHardwareDefinition(response.Data);
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

        private void OnWorkflowCompleteRequested(NavigationTarget target)
        {
            var viewModel = new WorkflowCompleteViewModel(target.SessionDto);
            var view = new WorkflowCompleteView
            {
                DataContext = viewModel
            };
            MainContent.Content = view;
        }

        private void OnShowDrawboardRequested(NavigationTarget target)
        {
            // Save current view for back navigation
            _previousViewBeforeDrawboard = MainContent.Content;

            var viewModel = new DrawboardViewModel(showBackButton: true);
            var view = new DrawboardView
            {
                DataContext = viewModel
            };
            MainContent.Content = view;
        }

        private void OnNavigateBackFromDrawboard()
        {
            // Restore previous view
            if (_previousViewBeforeDrawboard != null)
            {
                MainContent.Content = _previousViewBeforeDrawboard;
                _previousViewBeforeDrawboard = null;
            }
            else
            {
                // Fallback to dashboard if no previous view
                OnNavigateToDashboard();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            // Unsubscribe from events to prevent memory leaks
            NavigationService.Instance.NavigationRequested -= OnNavigationRequested;
            NavigationService.Instance.NavigateToDashboardRequested -= OnNavigateToDashboard;
            NavigationService.Instance.ResumeWorkflowRequested -= OnResumeWorkflowRequested;
            NavigationService.Instance.ViewComponentRequested -= OnViewComponentRequested;
            NavigationService.Instance.WorkflowCompleteRequested -= OnWorkflowCompleteRequested;
            NavigationService.Instance.ShowDashboardRequested -= OnShowDrawboardRequested;
            NavigationService.Instance.NavigateBackFromShowDashboardRequested -= OnNavigateBackFromDrawboard;
            base.OnClosed(e);
        }
    }
}