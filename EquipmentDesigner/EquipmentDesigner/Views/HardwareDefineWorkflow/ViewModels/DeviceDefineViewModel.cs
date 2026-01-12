using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Input;
using EquipmentDesigner.Models;

namespace EquipmentDesigner.ViewModels
{
    /// <summary>
    /// ViewModel for Device definition form.
    /// </summary>
    public class DeviceDefineViewModel : ViewModelBase, IHardwareDefineViewModel
    {
        private string _parentUnitId;
        private string _name = string.Empty;
        private string _displayName = string.Empty;
        private string _description = string.Empty;
        private string _implementationGuidelines = string.Empty;
        private Func<bool> _allStepsRequiredFieldsFilledCheck;

        public DeviceDefineViewModel()
        {
            Commands = new ObservableCollection<CommandViewModel>();
            Commands.CollectionChanged += OnCommandsCollectionChanged;
            IoConfigurations = new ObservableCollection<IoConfigurationViewModel>();
            IoConfigurations.CollectionChanged += OnIoConfigurationsCollectionChanged;
            AttachedDocuments = new ObservableCollection<string>();

            LoadFromServerCommand = new RelayCommand(ExecuteLoadFromServer);
            AddCommandCommand = new RelayCommand(ExecuteAddCommand);
            RemoveCommandCommand = new RelayCommand<CommandViewModel>(ExecuteRemoveCommand);
            AddIoCommand = new RelayCommand(ExecuteAddIo);
            RemoveIoCommand = new RelayCommand<IoConfigurationViewModel>(ExecuteRemoveIo);
            AddAnotherCommand = new RelayCommand(ExecuteAddAnother, CanExecuteAddAnother);
            CompleteWorkflowCommand = new RelayCommand(ExecuteCompleteWorkflow, CanExecuteCompleteWorkflow);
        }

        /// <summary>
        /// Sets the callback function to check if all steps have required fields filled.
        /// </summary>
        public void SetAllStepsRequiredFieldsFilledCheck(Func<bool> check)
        {
            _allStepsRequiredFieldsFilledCheck = check;
        }

        /// <summary>
        /// Notifies that CanCompleteWorkflow should be re-evaluated.
        /// </summary>
        public void RaiseCanCompleteWorkflowChanged()
        {
            OnPropertyChanged(nameof(CanCompleteWorkflow));
        }

        /// <summary>
        /// Parent Unit ID.
        /// </summary>
        public string ParentUnitId
        {
            get => _parentUnitId;
            set
            {
                if (SetProperty(ref _parentUnitId, value))
                {
                    OnPropertyChanged(nameof(FilledFieldCount));
                }
            }
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
        /// Collection of commands for this device.
        /// </summary>
        public ObservableCollection<CommandViewModel> Commands { get; }

        /// <summary>
        /// Returns true if there are no commands in the collection.
        /// </summary>
        public bool HasNoCommands => Commands.Count == 0;

        /// <summary>
        /// Collection of IO configurations for this device.
        /// </summary>
        public ObservableCollection<IoConfigurationViewModel> IoConfigurations { get; }

        /// <summary>
        /// Collection of attached design documents.
        /// </summary>
        public ObservableCollection<string> AttachedDocuments { get; }

        /// <summary>
        /// Returns true if there are no IO configurations in the collection.
        /// </summary>
        public bool HasNoIoConfigurations => IoConfigurations.Count == 0;

        /// <summary>
        /// Returns true if required properties are provided for navigation.
        /// </summary>
        public bool CanProceedToNext => !string.IsNullOrWhiteSpace(Name);

        /// <summary>
        /// Total number of fields in this form.
        /// </summary>
        public int TotalFieldCount => 6;

        /// <summary>
        /// Number of fields that have been filled.
        /// </summary>
        public int FilledFieldCount
        {
            get
            {
                int count = 0;
                if (!string.IsNullOrWhiteSpace(ParentUnitId)) count++;
                if (!string.IsNullOrWhiteSpace(Name)) count++;
                if (!string.IsNullOrWhiteSpace(DisplayName)) count++;
                if (!string.IsNullOrWhiteSpace(Description)) count++;
                if (!string.IsNullOrWhiteSpace(ImplementationGuidelines)) count++;
                return count;
            }
        }

        /// <summary>
        /// Returns true if the workflow can be completed.
        /// All required fields in all steps must be filled and form must be editable.
        /// </summary>
        public bool CanCompleteWorkflow => IsEditable && (_allStepsRequiredFieldsFilledCheck?.Invoke() ?? CanProceedToNext);

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

        /// <summary>
        /// Event raised when the Add IO dialog should be shown.
        /// </summary>
        public event EventHandler ShowAddIoDialogRequested;

        /// <summary>
        /// Event raised when the Add Command dialog should be shown.
        /// </summary>
        public event EventHandler ShowAddCommandDialogRequested;

        /// <summary>
        /// Event raised when workflow completion is requested.
        /// </summary>
        public event EventHandler WorkflowCompletedRequest;

        /// <summary>
        /// Gets or sets whether the form is editable.
        /// Overridden to also notify CanCompleteWorkflow when editability changes.
        /// </summary>
        public override bool IsEditable
        {
            get => base.IsEditable;
            set
            {
                if (base.IsEditable != value)
                {
                    base.IsEditable = value;
                    OnPropertyChanged(nameof(CanCompleteWorkflow));
                }
            }
        }

        private void OnCommandsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(HasNoCommands));
        }

        private void OnIoConfigurationsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(HasNoIoConfigurations));
        }

        private void ExecuteLoadFromServer()
        {
            // TODO: Implement server loading logic
        }

        private void ExecuteAddCommand()
        {
            ShowAddCommandDialogRequested?.Invoke(this, EventArgs.Empty);
        }

        private void ExecuteRemoveCommand(CommandViewModel command)
        {
            if (command != null)
                Commands.Remove(command);
        }

        private void ExecuteAddIo()
        {
            ShowAddIoDialogRequested?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Processes the result from the Add IO dialog.
        /// </summary>
        /// <param name="result">The IO configuration from the dialog, or null if cancelled.</param>
        public void ProcessDialogResult(IoConfigurationViewModel result)
        {
            if (result != null)
            {
                IoConfigurations.Add(result);
            }
        }

        /// <summary>
        /// Processes the result from the Add Command dialog.
        /// </summary>
        /// <param name="command">The command from the dialog, or null if cancelled.</param>
        public void ProcessCommandDialogResult(CommandViewModel command)
        {
            if (command != null)
            {
                Commands.Add(command);
            }
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
            Description = string.Empty;
            ImplementationGuidelines = string.Empty;
            Commands.Clear();
            IoConfigurations.Clear();
            AttachedDocuments.Clear();
        }

        private bool CanExecuteAddAnother()
        {
            return CanProceedToNext;
        }

        private void ExecuteCompleteWorkflow()
        {
            WorkflowCompletedRequest?.Invoke(this, EventArgs.Empty);
        }

        private bool CanExecuteCompleteWorkflow()
        {
            return CanCompleteWorkflow;
        }

        /// <summary>
        /// Converts this ViewModel to a HardwareDefinition.
        /// </summary>
        public HardwareDefinition ToHardwareDefinition()
        {
            var hw = new HardwareDefinition
            {
                HardwareType = HardwareType.Device,
                Name = Name,
                DisplayName = DisplayName,
                Description = Description,
                Commands = Commands.Select(c => c.ToDto()).ToList(),
                IoInfo = IoConfigurations.Select(io => io.ToDto()).ToList(),
                Version = Version,
                HardwareKey = HardwareKey
            };

            if (!string.IsNullOrEmpty(ImplementationGuidelines))
            {
                hw.ImplementationInstructions = ImplementationGuidelines
                    .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                    .ToList();
            }

            return hw;
        }

        /// <summary>
        /// Creates a ViewModel from a HardwareDefinition.
        /// </summary>
        /// <param name="hw">The HardwareDefinition to convert from.</param>
        /// <exception cref="ArgumentNullException">Thrown when hw is null.</exception>
        public static DeviceDefineViewModel FromHardwareDefinition(HardwareDefinition hw)
        {
            if (hw == null)
                throw new ArgumentNullException(nameof(hw));

            var viewModel = new DeviceDefineViewModel
            {
                Name = hw.Name ?? string.Empty,
                DisplayName = hw.DisplayName ?? string.Empty,
                Description = hw.Description ?? string.Empty,
                Version = hw.Version ?? "undefined",
                HardwareKey = hw.HardwareKey
            };

            if (hw.ImplementationInstructions != null && hw.ImplementationInstructions.Any())
            {
                viewModel.ImplementationGuidelines = string.Join("\n", hw.ImplementationInstructions);
            }

            if (hw.Commands != null)
            {
                foreach (var cmdDto in hw.Commands)
                    viewModel.Commands.Add(CommandViewModel.FromDto(cmdDto));
            }

            if (hw.IoInfo != null)
            {
                foreach (var ioDto in hw.IoInfo)
                    viewModel.IoConfigurations.Add(IoConfigurationViewModel.FromDto(ioDto));
            }

            return viewModel;
        }
    }
}