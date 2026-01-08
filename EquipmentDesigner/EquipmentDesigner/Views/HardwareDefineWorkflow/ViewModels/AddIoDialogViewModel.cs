using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace EquipmentDesigner.ViewModels
{
    /// <summary>
    /// ViewModel for the Add IO dialog window.
    /// </summary>
    public class AddIoDialogViewModel : ViewModelBase
    {
        private string _ioName = string.Empty;
        private string _address = string.Empty;
        private string _ioType;

        /// <summary>
        /// Event raised when the dialog should close.
        /// </summary>
        public event EventHandler<IoConfigurationViewModel> RequestClose;

        public AddIoDialogViewModel()
        {
            AvailableIoTypes = new List<string>
            {
                "Digital Input",
                "Digital Output",
                "Analog Input",
                "Analog Output"
            };

            AddIoCommand = new RelayCommand(ExecuteAddIo, CanExecuteAddIo);
            CancelCommand = new RelayCommand(ExecuteCancel);
        }

        /// <summary>
        /// IO name (required).
        /// </summary>
        public string IoName
        {
            get => _ioName;
            set
            {
                if (SetProperty(ref _ioName, value))
                {
                    OnPropertyChanged(nameof(CanAddIo));
                }
            }
        }

        /// <summary>
        /// IO address (required).
        /// </summary>
        public string Address
        {
            get => _address;
            set
            {
                if (SetProperty(ref _address, value))
                {
                    OnPropertyChanged(nameof(CanAddIo));
                }
            }
        }

        /// <summary>
        /// IO type (required).
        /// </summary>
        public string IoType
        {
            get => _ioType;
            set
            {
                if (SetProperty(ref _ioType, value))
                {
                    OnPropertyChanged(nameof(CanAddIo));
                }
            }
        }

        /// <summary>
        /// Available IO types for selection.
        /// </summary>
        public IReadOnlyList<string> AvailableIoTypes { get; }

        /// <summary>
        /// Returns true when all required fields are valid.
        /// </summary>
        public bool CanAddIo =>
            !string.IsNullOrWhiteSpace(IoName) &&
            !string.IsNullOrWhiteSpace(Address) &&
            !string.IsNullOrWhiteSpace(IoType);

        /// <summary>
        /// Command to add the IO and close the dialog.
        /// </summary>
        public ICommand AddIoCommand { get; }

        /// <summary>
        /// Command to cancel and close the dialog.
        /// </summary>
        public ICommand CancelCommand { get; }

        private void ExecuteAddIo()
        {
            var ioConfiguration = new IoConfigurationViewModel
            {
                Name = IoName,
                Address = Address,
                IoType = IoType
            };

            RequestClose?.Invoke(this, ioConfiguration);
        }

        private bool CanExecuteAddIo()
        {
            return CanAddIo;
        }

        private void ExecuteCancel()
        {
            RequestClose?.Invoke(this, null);
        }
    }
}
