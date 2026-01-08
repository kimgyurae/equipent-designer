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
        public ProcessEditButton()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles click on the main edit button - navigates to ProcessDefineView.
        /// </summary>
        private void OnMainButtonClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Instance.NavigateToProcessDefine();
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
        /// Opens the ProcessDefineView in a new window.
        /// </summary>
        private void OpenInNewWindow()
        {
            var viewModel = new ProcessDefineViewModel(showBackButton: false);
            var view = new ProcessDefineView
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

            window.Show();
        }
    }
}
