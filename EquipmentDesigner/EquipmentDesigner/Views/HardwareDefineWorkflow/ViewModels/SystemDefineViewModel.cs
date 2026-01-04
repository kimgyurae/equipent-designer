using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using EquipmentDesigner.Models.Dtos;

namespace EquipmentDesigner.Views.HardwareDefineWorkflow
{
    /// <summary>
    /// ViewModel for System definition form.
    /// </summary>
    public class SystemDefineViewModel : ViewModelBase
    {
        private string _parentEquipmentId;
        private string _name = string.Empty;
        private string _displayName = string.Empty;
        private string _subname = string.Empty;
        private string _description = string.Empty;
        private string _implementationGuidelines = string.Empty;
        private string _process = string.Empty;

        public SystemDefineViewModel()
        {
            Commands = new ObservableCollection<CommandViewModel>();

            LoadFromServerCommand = new RelayCommand(ExecuteLoadFromServer);
            AddCommandCommand = new RelayCommand(ExecuteAddCommand);
            RemoveCommandCommand = new RelayCommand<CommandViewModel>(ExecuteRemoveCommand);
            AddAnotherCommand = new RelayCommand(ExecuteAddAnother, CanExecuteAddAnother);
        }

        /// <summary>
        /// Parent Equipment ID.
        /// </summary>
        public string ParentEquipmentId
        {
            get => _parentEquipmentId;
            set => SetProperty(ref _parentEquipmentId, value);
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
        /// Implementation guidelines (optional).
        /// </summary>
        public string ImplementationGuidelines
        {
            get => _implementationGuidelines;
            set => SetProperty(ref _implementationGuidelines, value);
        }

        /// <summary>
        /// Process information (optional).
        /// </summary>
        public string Process
        {
            get => _process;
            set => SetProperty(ref _process, value);
        }

        /// <summary>
        /// Collection of commands for this system.
        /// </summary>
        public ObservableCollection<CommandViewModel> Commands { get; }

        /// <summary>
        /// Returns true if required properties are provided for navigation.
        /// </summary>
        public bool CanProceedToNext => !string.IsNullOrWhiteSpace(Name);

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

        private void ExecuteAddCommand()
        {
            Commands.Add(new CommandViewModel());
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
            Subname = string.Empty;
            Description = string.Empty;
            ImplementationGuidelines = string.Empty;
            Process = string.Empty;
            Commands.Clear();
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
                Subname = Subname,
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
                Subname = dto.Subname ?? string.Empty,
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
