using System.Windows;
using EquipmentDesigner.ViewModels;

namespace EquipmentDesigner.Views
{
    /// <summary>
    /// Code-behind for AddIoDialogWindow.xaml
    /// </summary>
    public partial class AddIoDialogWindow : Window
    {
        public AddIoDialogWindow()
        {
            InitializeComponent();
        }

        public AddIoDialogWindow(AddIoDialogViewModel viewModel) : this()
        {
            DataContext = viewModel;
            viewModel.RequestClose += OnRequestClose;
        }

        private void OnRequestClose(object sender, IoConfigurationViewModel result)
        {
            DialogResult = result != null;
            Close();
        }

        /// <summary>
        /// Gets the IO configuration result from the dialog.
        /// </summary>
        public IoConfigurationViewModel Result =>
            DataContext is AddIoDialogViewModel vm && DialogResult == true
                ? new IoConfigurationViewModel
                {
                    Name = vm.IoName,
                    Address = vm.Address,
                    IoType = vm.IoType
                }
                : null;

        private void ComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }
    }
}
