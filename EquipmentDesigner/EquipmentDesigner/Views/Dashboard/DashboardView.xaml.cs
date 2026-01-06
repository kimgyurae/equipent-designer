using System.Windows.Controls;
using System.Windows.Input;

namespace EquipmentDesigner.Views.Dashboard
{
    /// <summary>
    /// Interaction logic for DashboardView.xaml
    /// </summary>
    public partial class DashboardView : UserControl
    {
        public DashboardView()
        {
            InitializeComponent();
            DataContext = new DashboardViewModel();

            // Register keyboard shortcut for Admin mode toggle
            PreviewKeyDown += DashboardView_PreviewKeyDown;
        }

        /// <summary>
        /// Handles PreviewKeyDown to toggle Admin mode with Ctrl+Shift+F10.
        /// </summary>
        private void DashboardView_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // F10 is a system key in Windows, so we need to check SystemKey instead of Key
            var key = e.Key == Key.System ? e.SystemKey : e.Key;

            if (key == Key.F10 &&
                Keyboard.Modifiers.HasFlag(ModifierKeys.Control) &&
                Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                if (DataContext is DashboardViewModel viewModel)
                {
                    viewModel.ToggleAdminMode();
                    e.Handled = true;
                }
            }
        }
    }
}