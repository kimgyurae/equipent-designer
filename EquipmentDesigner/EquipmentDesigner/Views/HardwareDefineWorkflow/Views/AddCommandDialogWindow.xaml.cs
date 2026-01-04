using System.Windows;

namespace EquipmentDesigner.Views.HardwareDefineWorkflow
{
    /// <summary>
    /// Code-behind for AddCommandDialogWindow.xaml
    /// </summary>
    public partial class AddCommandDialogWindow : Window
    {
        private CommandViewModel _result;

        public AddCommandDialogWindow()
        {
            InitializeComponent();
        }

        public AddCommandDialogWindow(AddCommandDialogViewModel viewModel) : this()
        {
            DataContext = viewModel;
            viewModel.RequestClose += OnRequestClose;
        }

        private void OnRequestClose(object sender, CommandViewModel result)
        {
            _result = result;
            DialogResult = result != null;
            Close();
        }

        /// <summary>
        /// Gets the command result from the dialog.
        /// </summary>
        public CommandViewModel Result => _result;
    }
}
