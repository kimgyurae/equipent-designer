using System.Windows;
using System.Windows.Input;
using EquipmentDesigner.Models;
using EquipmentDesigner.ViewModels;

namespace EquipmentDesigner.Views
{
    /// <summary>
    /// Dialog for selecting a hardware version from the list of versions with the same HardwareKey.
    /// </summary>
    public partial class HardwareVersionSelectionDialog : Window
    {
        private readonly HardwareVersionSelectionDialogViewModel _viewModel;

        /// <summary>
        /// Gets the selected workflow ID after dialog closes.
        /// </summary>
        public string SelectedWorkflowId { get; private set; }

        /// <summary>
        /// Gets or sets the hardware key to filter versions.
        /// </summary>
        public string HardwareKey
        {
            get => _viewModel.HardwareKey;
            set => _viewModel.HardwareKey = value;
        }

        /// <summary>
        /// Gets or sets the hardware type.
        /// </summary>
        public HardwareType HardwareType
        {
            get => _viewModel.HardwareType;
            set => _viewModel.HardwareType = value;
        }

        /// <summary>
        /// Gets or sets the display name for the hardware.
        /// </summary>
        public string DisplayName
        {
            get => _viewModel.DisplayName;
            set => _viewModel.DisplayName = value;
        }

        public HardwareVersionSelectionDialog()
        {
            InitializeComponent();

            _viewModel = new HardwareVersionSelectionDialogViewModel();
            _viewModel.RequestClose += OnViewModelRequestClose;
            DataContext = _viewModel;

            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _viewModel.Initialize();
        }

        private void OnViewModelRequestClose(object sender, string workflowId)
        {
            SelectedWorkflowId = workflowId;
            DialogResult = !string.IsNullOrEmpty(workflowId);
            Close();
        }

        private void Backdrop_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
