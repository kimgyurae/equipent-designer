using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Input;
using EquipmentDesigner.Models;

namespace EquipmentDesigner.ViewModels
{
    /// <summary>
    /// ViewModel for System definition form.
    /// </summary>
    public class SystemDefineViewModel : ViewModelBase, IHardwareDefineViewModel
    {
        private string _parentEquipmentId;
        private string _name = string.Empty;
        private string _displayName = string.Empty;
        private string _description = string.Empty;
        private string _implementationGuidelines = string.Empty;
        private string _process = string.Empty;

        public SystemDefineViewModel()
        {
            Commands = new ObservableCollection<CommandViewModel>();
            Commands.CollectionChanged += OnCommandsCollectionChanged;
            AttachedDocuments = new ObservableCollection<string>();

            LoadFromServerCommand = new RelayCommand(ExecuteLoadFromServer);
            AddCommandCommand = new RelayCommand(ExecuteAddCommand);
            RemoveCommandCommand = new RelayCommand<CommandViewModel>(ExecuteRemoveCommand);
            AddAnotherCommand = new RelayCommand(ExecuteAddAnother, CanExecuteAddAnother);
        }

        /// <summary>
        /// Event raised when the Add Command dialog should be shown.
        /// </summary>
        public event EventHandler ShowAddCommandDialogRequested;

        /// <summary>
        /// Parent Equipment ID.
        /// </summary>
        public string ParentEquipmentId
        {
            get => _parentEquipmentId;
            set
            {
                if (SetProperty(ref _parentEquipmentId, value))
                {
                    OnPropertyChanged(nameof(FilledFieldCount));
                }
            }
        }

        /// <summary>
        /// System name (required).
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
        /// Implementation guidelines (optional).
        /// </summary>
        public string ImplementationGuidelines
        {
            get => _implementationGuidelines;
            set
            {
                if (SetProperty(ref _implementationGuidelines, value))
                {
                    OnPropertyChanged(nameof(FilledFieldCount));
                }
            }
        }

        /// <summary>
        /// Process information (optional).
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
        /// Collection of commands for this system.
        /// </summary>
        public ObservableCollection<CommandViewModel> Commands { get; }

        /// <summary>
        /// Collection of attached design documents.
        /// </summary>
        public ObservableCollection<string> AttachedDocuments { get; }

        /// <summary>
        /// Returns true if the commands collection is empty.
        /// </summary>
        public bool HasNoCommands => Commands.Count == 0;

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
                if (!string.IsNullOrWhiteSpace(ParentEquipmentId)) count++;
                if (!string.IsNullOrWhiteSpace(Name)) count++;
                if (!string.IsNullOrWhiteSpace(DisplayName)) count++;
                if (!string.IsNullOrWhiteSpace(Description)) count++;
                if (!string.IsNullOrWhiteSpace(ImplementationGuidelines)) count++;
                if (!string.IsNullOrWhiteSpace(Process)) count++;
                return count;
            }
        }

        /// <summary>
        /// Command to load system data from server.
        /// </summary>
        public ICommand LoadFromServerCommand { get; }

        /// <summary>
        /// Command to add a new command.
        /// </summary>
        public ICommand AddCommandCommand { get; }

        /// <summary>
        /// Command to remove a command.
        /// </summary>
        public ICommand RemoveCommandCommand { get; }

        /// <summary>
        /// Command to add another system (reset form).
        /// </summary>
        public ICommand AddAnotherCommand { get; }

        private void ExecuteLoadFromServer()
        {
            // TODO: Implement server loading logic
        }

        private void OnCommandsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(HasNoCommands));
        }

        private void ExecuteAddCommand()
        {
            ShowAddCommandDialogRequested?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Processes the result from the Add Command dialog.
        /// </summary>
        public void ProcessCommandDialogResult(CommandViewModel command)
        {
            if (command != null)
            {
                Commands.Add(command);
            }
        }

        private void ExecuteRemoveCommand(CommandViewModel command)
        {
            if (command != null)
                Commands.Remove(command);
        }

        private void ExecuteAddAnother()
        {
            // Reset form for creating another system
            Name = string.Empty;
            DisplayName = string.Empty;
            Description = string.Empty;
            ImplementationGuidelines = string.Empty;
            Process = string.Empty;
            Commands.Clear();
            AttachedDocuments.Clear();
        }

        private bool CanExecuteAddAnother()
        {
            return CanProceedToNext;
        }

        /// <summary>
        /// Converts this ViewModel to a DTO.
        /// </summary>
        public SystemDto ToDto()
        {
            var dto = new SystemDto
            {
                EquipmentId = ParentEquipmentId,
                Name = Name,
                DisplayName = DisplayName,
                Description = Description,
                ProcessInfo = Process,
                Commands = Commands.Select(c => c.ToDto()).ToList()
            };

            if (!string.IsNullOrEmpty(ImplementationGuidelines))
            {
                dto.ImplementationInstructions = ImplementationGuidelines
                    .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                    .ToList();
            }

            return dto;
        }

        /// <summary>
        /// Creates a ViewModel from a DTO.
        /// </summary>
        public static SystemDefineViewModel FromDto(SystemDto dto)
        {
            var viewModel = new SystemDefineViewModel
            {
                ParentEquipmentId = dto.EquipmentId,
                Name = dto.Name ?? string.Empty,
                DisplayName = dto.DisplayName ?? string.Empty,
                Description = dto.Description ?? string.Empty,
                Process = dto.ProcessInfo ?? string.Empty
            };

            if (dto.ImplementationInstructions != null && dto.ImplementationInstructions.Any())
            {
                viewModel.ImplementationGuidelines = string.Join("\n", dto.ImplementationInstructions);
            }

            if (dto.Commands != null)
            {
                foreach (var cmdDto in dto.Commands)
                    viewModel.Commands.Add(CommandViewModel.FromDto(cmdDto));
            }

            return viewModel;
        }
    }
}