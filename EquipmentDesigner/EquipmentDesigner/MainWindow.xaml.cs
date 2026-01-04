using System;
using System.Windows;
using EquipmentDesigner.Services;
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
            MainContent.Content = _dashboardView;
        }

        protected override void OnClosed(EventArgs e)
        {
            // Unsubscribe from events to prevent memory leaks
            NavigationService.Instance.NavigationRequested -= OnNavigationRequested;
            NavigationService.Instance.NavigateToDashboardRequested -= OnNavigateToDashboard;
            base.OnClosed(e);
        }
    }
}