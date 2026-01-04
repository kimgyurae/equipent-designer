using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using EquipmentDesigner.Models.Dtos;

namespace EquipmentDesigner.Views.HardwareDefineWorkflow
{
    /// <summary>
    /// ViewModel for Equipment definition form.
    /// </summary>
    public class EquipmentDefineViewModel : ViewModelBase
    {
        private string _equipmentType = string.Empty;
        private string _name = string.Empty;
        private string _displayName = string.Empty;
        private string _subname = string.Empty;
        private string _description = string.Empty;
        private string _customer = string.Empty;
        private string _process = string.Empty;

        public EquipmentDefineViewModel()
        {
            AttachedDocuments = new ObservableCollection<string>();
            LoadFromServerCommand = new RelayCommand(ExecuteLoadFromServer);
        }

        /// <summary>
        /// Equipment type (required in UI but not for navigation).
        /// </summary>
        public string EquipmentType
        {
            get => _equipmentType;
            set => SetProperty(ref _equipmentType, value);
        }

        /// <summary>
        /// Equipment name (required).
        /// </summary>
        public string Name
        {
            get => _name;
            set
            {
                if (SetProperty(ref _name, value))
                    OnPropertyChanged(nameof(CanProceedToNext));
            }
        }

        /// <summary>
        /// Display name (optional).
        /// </summary>
        public string DisplayName
        {
            get => _displayName;
            set => SetProperty(ref _displayName, value);
        }

        /// <summary>
        /// Subname (optional).
        /// </summary>
        public string Subname
        {
            get => _subname;
            set => SetProperty(ref _subname, value);
        }

        /// <summary>
        /// Description (optional).
        /// </summary>
        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        /// <summary>
        /// Customer (optional).
        /// </summary>
        public string Customer
        {
            get => _customer;
            set => SetProperty(ref _customer, value);
        }

        /// <summary>
        /// Process (optional).
        /// </summary>
        public string Process
        {
            get => _process;
            set => SetProperty(ref _process, value);
        }

        /// <summary>
        /// Attached documents file paths.
        /// Supported extensions: PDF, PPT, MD, DRAWIO.
        /// </summary>
        public ObservableCollection<string> AttachedDocuments { get; }

        /// <summary>
        /// Returns true if required properties are provided for navigation.
        /// </summary>
        public bool CanProceedToNext => !string.IsNullOrWhiteSpace(Name);

        /// <summary>
        /// Command to load equipment data from server.
        /// </summary>
        public ICommand LoadFromServerCommand { get; }

        private void ExecuteLoadFromServer()
        {
            // TODO: Implement server loading logic
        }

        /// <summary>
        /// Converts this ViewModel to a DTO.
        /// </summary>
        public EquipmentDto ToDto()
        {
            return new EquipmentDto
            {
                EquipmentType = EquipmentType,
                Name = Name,
                DisplayName = DisplayName,
                Subname = Subname,
                Description = Description,
                Customer = Customer,
                Process = Process,
                AttachedDocuments = AttachedDocuments.ToList()
            };
        }

        /// <summary>
        /// Creates a ViewModel from a DTO.
        /// </summary>
        public static EquipmentDefineViewModel FromDto(EquipmentDto dto)
        {
            var viewModel = new EquipmentDefineViewModel
            {
                EquipmentType = dto.EquipmentType ?? string.Empty,
                Name = dto.Name ?? string.Empty,
                DisplayName = dto.DisplayName ?? string.Empty,
                Subname = dto.Subname ?? string.Empty,
                Description = dto.Description ?? string.Empty,
                Customer = dto.Customer ?? string.Empty,
                Process = dto.Process ?? string.Empty
            };

            if (dto.AttachedDocuments != null)
            {
                foreach (var doc in dto.AttachedDocuments)
                    viewModel.AttachedDocuments.Add(doc);
            }

            return viewModel;
        }
    }
}
