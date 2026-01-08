using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using EquipmentDesigner.Models;
using Microsoft.Win32;

namespace EquipmentDesigner.ViewModels
{
    /// <summary>
    /// ViewModel for Equipment definition form.
    /// </summary>
    public class EquipmentDefineViewModel : ViewModelBase, IHardwareDefineViewModel
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
            AddDocumentCommand = new RelayCommand(ExecuteAddDocument);
            RemoveDocumentCommand = new RelayCommand<string>(ExecuteRemoveDocument);
        }

        /// <summary>
        /// Equipment type (required in UI but not for navigation).
        /// </summary>
        public string EquipmentType
        {
            get => _equipmentType;
            set
            {
                if (SetProperty(ref _equipmentType, value))
                {
                    OnPropertyChanged(nameof(FilledFieldCount));
                }
            }
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
                {
                    OnPropertyChanged(nameof(CanProceedToNext));
                    OnPropertyChanged(nameof(FilledFieldCount));
                }
            }
        }

        /// <summary>
        /// Display name (optional).
        /// </summary>
        public string DisplayName
        {
            get => _displayName;
            set
            {
                if (SetProperty(ref _displayName, value))
                {
                    OnPropertyChanged(nameof(FilledFieldCount));
                }
            }
        }

        /// <summary>
        /// Subname (optional).
        /// </summary>
        public string Subname
        {
            get => _subname;
            set
            {
                if (SetProperty(ref _subname, value))
                {
                    OnPropertyChanged(nameof(FilledFieldCount));
                }
            }
        }

        /// <summary>
        /// Description (optional).
        /// </summary>
        public string Description
        {
            get => _description;
            set
            {
                if (SetProperty(ref _description, value))
                {
                    OnPropertyChanged(nameof(FilledFieldCount));
                }
            }
        }

        /// <summary>
        /// Customer (optional).
        /// </summary>
        public string Customer
        {
            get => _customer;
            set
            {
                if (SetProperty(ref _customer, value))
                {
                    OnPropertyChanged(nameof(FilledFieldCount));
                }
            }
        }

        /// <summary>
        /// Process (optional).
        /// </summary>
        public string Process
        {
            get => _process;
            set
            {
                if (SetProperty(ref _process, value))
                {
                    OnPropertyChanged(nameof(FilledFieldCount));
                }
            }
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
        /// Total number of fields in this form.
        /// </summary>
        public int TotalFieldCount => 7;

        /// <summary>
        /// Number of fields that have been filled.
        /// </summary>
        public int FilledFieldCount
        {
            get
            {
                int count = 0;
                if (!string.IsNullOrWhiteSpace(EquipmentType)) count++;
                if (!string.IsNullOrWhiteSpace(Name)) count++;
                if (!string.IsNullOrWhiteSpace(DisplayName)) count++;
                if (!string.IsNullOrWhiteSpace(Subname)) count++;
                if (!string.IsNullOrWhiteSpace(Description)) count++;
                if (!string.IsNullOrWhiteSpace(Customer)) count++;
                if (!string.IsNullOrWhiteSpace(Process)) count++;
                return count;
            }
        }

        /// <summary>
        /// Command to load equipment data from server.
        /// </summary>
        public ICommand LoadFromServerCommand { get; }

        /// <summary>
        /// Command to add a document via file dialog.
        /// </summary>
        public ICommand AddDocumentCommand { get; }

        /// <summary>
        /// Command to remove a document from the list.
        /// </summary>
        public ICommand RemoveDocumentCommand { get; }

        private void ExecuteLoadFromServer()
        {
            // TODO: Implement server loading logic
        }

        private void ExecuteAddDocument()
        {
            var dialog = new OpenFileDialog
            {
                Title = "문서 선택",
                Filter = "모든 파일 (*.*)|*.*",
                Multiselect = true
            };

            if (dialog.ShowDialog() == true)
            {
                foreach (var fileName in dialog.FileNames)
                {
                    if (!AttachedDocuments.Contains(fileName))
                    {
                        AttachedDocuments.Add(fileName);
                    }
                }
            }
        }

        private void ExecuteRemoveDocument(string document)
        {
            if (!string.IsNullOrEmpty(document) && AttachedDocuments.Contains(document))
            {
                AttachedDocuments.Remove(document);
            }
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