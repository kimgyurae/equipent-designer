using System.Windows;
using System.Windows.Controls;
using EquipmentDesigner.Resources;
using EquipmentDesigner.Services;
using EquipmentDesigner.ViewModels;
using EquipmentDesigner.Views;
using CustomContextMenuService = EquipmentDesigner.Controls.ContextMenuService;

namespace EquipmentDesigner.Controls
{
    /// <summary>
    /// Process edit button with context menu support.
    /// </summary>
    public partial class ProcessEditButton : UserControl
    {
        /// <summary>
        /// Tracks the currently open Process editor window (if any).
        /// </summary>
        private Window _openProcessEditorWindow;

        public ProcessEditButton()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles click on the main edit button - navigates to DrawboardView.
        /// </summary>
        private void OnMainButtonClick(object sender, RoutedEventArgs e)
        {
            // Check if a Process editor window is already open
            if (_openProcessEditorWindow != null)
            {
                ToastService.Instance.ShowWarning(
                    Strings.Toast_ProcessEditorAlreadyOpen_Title,
                    Strings.Toast_ProcessEditorAlreadyOpen_Description);
                return;
            }

            NavigationService.Instance.NavigateToDrawboard();
        }

        /// <summary>
        /// Handles click on the more options (ellipsis) button - shows context menu.
        /// </summary>
        private void OnMoreOptionsClick(object sender, RoutedEventArgs e)
        {
            CustomContextMenuService.Instance.Create()
                .AddItem(Strings.Process_OpenInNewWindow, OpenInNewWindow)
                .Show();
        }

        /// <summary>
        /// Opens the DrawboardView in a new window.
        /// </summary>
        private void OpenInNewWindow()
        {
            // Check if a Process editor window is already open
            if (_openProcessEditorWindow != null)
            {
                ToastService.Instance.ShowWarning(
                    Strings.Toast_ProcessEditorAlreadyOpen_Title,
                    Strings.Toast_ProcessEditorAlreadyOpen_Description);
                _openProcessEditorWindow.Activate();
                return;
            }

            var viewModel = new DrawboardViewModel(showBackButton: false);
            var view = new DrawboardView
            {
                DataContext = viewModel
            };

            var window = new Window
            {
                Title = Strings.Process_Title,
                Content = view,
                Width = 800,
                Height = 600,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Owner = Application.Current.MainWindow
            };

            // Track the window and clear reference when closed
            _openProcessEditorWindow = window;
            window.Closed += (s, args) => _openProcessEditorWindow = null;

            window.Show();
        }
    }
}