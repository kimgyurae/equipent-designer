using System;
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
        /// Converts this ViewModel to a HardwareDefinition.
        /// </summary>
        public HardwareDefinition ToHardwareDefinition()
        {
            return new HardwareDefinition
            {
                HardwareType = HardwareType.Equipment,
                Name = Name,
                DisplayName = DisplayName,
                Description = Description,
                EquipmentType = EquipmentType,
                Customer = Customer,
                ProcessId = ProcessId,
                ProcessInfo = Process,
                AttachedDocumentsIds = AttachedDocuments.ToList(),
                Version = Version,
                HardwareKey = HardwareKey
            };
        }

        /// <summary>
        /// Creates a ViewModel from a HardwareDefinition.
        /// </summary>
        /// <param name="hw">The HardwareDefinition to convert from.</param>
        /// <exception cref="ArgumentNullException">Thrown when hw is null.</exception>
        public static EquipmentDefineViewModel FromHardwareDefinition(HardwareDefinition hw)
        {
            if (hw == null)
                throw new ArgumentNullException(nameof(hw));

            var viewModel = new EquipmentDefineViewModel
            {
                Name = hw.Name ?? string.Empty,
                DisplayName = hw.DisplayName ?? string.Empty,
                Description = hw.Description ?? string.Empty,
                EquipmentType = hw.EquipmentType ?? string.Empty,
                Customer = hw.Customer ?? string.Empty,
                Process = hw.ProcessInfo ?? string.Empty,
                Version = hw.Version ?? "undefined",
                HardwareKey = hw.HardwareKey,
                ProcessId = hw.ProcessId ?? System.Guid.NewGuid().ToString()
            };

            if (hw.AttachedDocumentsIds != null)
            {
                foreach (var doc in hw.AttachedDocumentsIds)
                    viewModel.AttachedDocuments.Add(doc);
            }

            return viewModel;
        }
    }
}