using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using EquipmentDesigner.Models.Dtos;

namespace EquipmentDesigner.Views.HardwareDefineWorkflow
{
    /// <summary>
    /// ViewModel for Device definition form.
    /// </summary>
    public class DeviceDefineViewModel : ViewModelBase
    {
        private string _parentUnitId;
        private string _name = string.Empty;
        private string _displayName = string.Empty;
        private string _subname = string.Empty;
        private string _description = string.Empty;
        private string _implementationGuidelines = string.Empty;

        public DeviceDefineViewModel()
        {
            Commands = new ObservableCollection<CommandViewModel>();
            IoConfigurations = new ObservableCollection<IoConfigurationViewModel>();

            LoadFromServerCommand = new RelayCommand(ExecuteLoadFromServer);
            AddCommandCommand = new RelayCommand(ExecuteAddCommand);
            RemoveCommandCommand = new RelayCommand<CommandViewModel>(ExecuteRemoveCommand);
            AddIoCommand = new RelayCommand(ExecuteAddIo);
            RemoveIoCommand = new RelayCommand<IoConfigurationViewModel>(ExecuteRemoveIo);
            AddAnotherCommand = new RelayCommand(ExecuteAddAnother, CanExecuteAddAnother);
            CompleteWorkflowCommand = new RelayCommand(ExecuteCompleteWorkflow, CanExecuteCompleteWorkflow);
        }

        /// <summary>
        /// Parent Unit ID.
        /// </summary>
        public string ParentUnitId
        {
            get => _parentUnitId;
            set => SetProperty(ref _parentUnitId, value);
        }

        /// <summary>
        /// Device name (required).
        /// </summary>
        public string Name
        {
            get => _name;
            set
            {
                if (SetProperty(ref _name, value))
                {
                    OnPropertyChanged(nameof(CanProceedToNext));
                    OnPropertyChanged(nameof(CanCompleteWorkflow));
                }
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
        /// Collection of commands for this device.
        /// </summary>
        public ObservableCollection<CommandViewModel> Commands { get; }

        /// <summary>
        /// Collection of IO configurations for this device.
        /// </summary>
        public ObservableCollection<IoConfigurationViewModel> IoConfigurations { get; }

        /// <summary>
        /// Returns true if required properties are provided for navigation.
        /// </summary>
        public bool CanProceedToNext => !string.IsNullOrWhiteSpace(Name);

        /// <summary>
        /// Returns true if the workflow can be completed.
        /// </summary>
        public bool CanCompleteWorkflow => CanProceedToNext;

        /// <summary>
        /// Command to load device data from server.
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
        /// Command to add a new IO configuration.
        /// </summary>
        public ICommand AddIoCommand { get; }

        /// <summary>
        /// Command to remove an IO configuration.
        /// </summary>
        public ICommand RemoveIoCommand { get; }

        /// <summary>
        /// Command to add another device (reset form).
        /// </summary>
        public ICommand AddAnotherCommand { get; }

        /// <summary>
        /// Command to complete the workflow.
        /// </summary>
        public ICommand CompleteWorkflowCommand { get; }

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

        private void ExecuteAddIo()
        {
            IoConfigurations.Add(new IoConfigurationViewModel());
        }

        private void ExecuteRemoveIo(IoConfigurationViewModel io)
        {
            if (io != null)
                IoConfigurations.Remove(io);
        }

        private void ExecuteAddAnother()
        {
            // Reset form for creating another device
            Name = string.Empty;
            DisplayName = string.Empty;
            Subname = string.Empty;
            Description = string.Empty;
            ImplementationGuidelines = string.Empty;
            Commands.Clear();
            IoConfigurations.Clear();
        }

        private bool CanExecuteAddAnother()
        {
            return CanProceedToNext;
        }

        private void ExecuteCompleteWorkflow()
        {
            // TODO: Implement workflow completion logic
        }

        private bool CanExecuteCompleteWorkflow()
        {
            return CanCompleteWorkflow;
        }

        /// <summary>
        /// Converts this ViewModel to a DTO.
        /// </summary>
        public DeviceDto ToDto()
        {
            var dto = new DeviceDto
            {
                UnitId = ParentUnitId,
                Name = Name,
                DisplayName = DisplayName,
                Subname = Subname,
                Description = Description,
                Commands = Commands.Select(c => c.ToDto()).ToList(),
                IoInfo = IoConfigurations.Select(io => io.ToDto()).ToList()
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
        public static DeviceDefineViewModel FromDto(DeviceDto dto)
        {
            var viewModel = new DeviceDefineViewModel
            {
                ParentUnitId = dto.UnitId,
                Name = dto.Name ?? string.Empty,
                DisplayName = dto.DisplayName ?? string.Empty,
                Subname = dto.Subname ?? string.Empty,
                Description = dto.Description ?? string.Empty
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

            if (dto.IoInfo != null)
            {
                foreach (var ioDto in dto.IoInfo)
                    viewModel.IoConfigurations.Add(IoConfigurationViewModel.FromDto(ioDto));
            }

            return viewModel;
        }
    }
}
