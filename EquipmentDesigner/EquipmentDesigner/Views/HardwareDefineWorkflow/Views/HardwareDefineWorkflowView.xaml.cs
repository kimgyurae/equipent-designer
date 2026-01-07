using System.Windows;
using System.Windows.Controls;

namespace EquipmentDesigner.Views.HardwareDefineWorkflow
{
    /// <summary>
    /// Interaction logic for HardwareDefineWorkflowView.xaml
    /// </summary>
    public partial class HardwareDefineWorkflowView : UserControl
    {
        public HardwareDefineWorkflowView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        /// <summary>
        /// Enables autosave when the view is loaded.
        /// </summary>
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is HardwareDefineWorkflowViewModel viewModel && !viewModel.IsReadOnly)
            {
                viewModel.EnableAutosave();
            }
        }

        /// <summary>
        /// Disables autosave when the view is unloaded.
        /// </summary>
        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is HardwareDefineWorkflowViewModel viewModel)
            {
                viewModel.DisableAutosave();
            }
        }
    }
}
