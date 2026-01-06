using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using EquipmentDesigner.Models;
using EquipmentDesigner.Models.Dtos;
using EquipmentDesigner.Models.Storage;
using EquipmentDesigner.Services;
using EquipmentDesigner.Services.Storage;

namespace EquipmentDesigner.Views.HardwareDefineWorkflow
{
    /// <summary>
    /// Main ViewModel for the Hardware Define Workflow.
    /// Orchestrates navigation between Equipment, System, Unit, and Device definition steps.
    /// </summary>
    public class HardwareDefineWorkflowViewModel : ViewModelBase
    {
        private int _currentStepIndex;
        private bool _isReadOnly;
        private string _loadedComponentId;
        private HardwareLayer? _loadedHardwareLayer;
        private EquipmentDefineViewModel _equipmentViewModel;
        private SystemDefineViewModel _systemViewModel;
        private UnitDefineViewModel _unitViewModel;
        private DeviceDefineViewModel _deviceViewModel;

        /// <summary>
        /// Creates a new workflow with a unique ID.
        /// </summary>
        public HardwareDefineWorkflowViewModel(HardwareLayer startType)
            : this(startType, Guid.NewGuid().ToString())
        {
        }

        /// <summary>
        /// Creates a workflow with a specific ID (for resume scenarios).
        /// </summary>
        private HardwareDefineWorkflowViewModel(HardwareLayer startType, string workflowId)
        {
            WorkflowId = workflowId;
            StartType = startType;
            WorkflowSteps = new ObservableCollection<WorkflowStepViewModel>();

            InitializeWorkflowSteps();
            InitializeViewModels();
            InitializeCommands();

            UpdateStepStates();
        }

        /// <summary>
        /// Unique identifier for this workflow session.
        /// </summary>
        public string WorkflowId { get; }

        /// <summary>
        /// The starting type of the workflow.
        /// </summary>
        public HardwareLayer StartType { get; }

        /// <summary>
        /// Collection of workflow steps.
        /// </summary>
        public ObservableCollection<WorkflowStepViewModel> WorkflowSteps { get; }

        /// <summary>
        /// Current step index (0-based).
        /// </summary>
        public int CurrentStepIndex
        {
            get => _currentStepIndex;
            private set
            {
                if (SetProperty(ref _currentStepIndex, value))
                {
                    OnPropertyChanged(nameof(CurrentStep));
                    OnPropertyChanged(nameof(IsFirstStep));
                    OnPropertyChanged(nameof(IsLastStep));
                    OnPropertyChanged(nameof(CanGoToNext));
                    UpdateStepStates();
                }
            }
        }

        /// <summary>
        /// Current workflow step.
        /// </summary>
        public WorkflowStepViewModel CurrentStep => WorkflowSteps[CurrentStepIndex];

        /// <summary>
        /// Returns true if at the first step.
        /// </summary>
        public bool IsFirstStep => CurrentStepIndex == 0;

        /// <summary>
        /// Returns true if at the last step.
        /// </summary>
        public bool IsLastStep => CurrentStepIndex == WorkflowSteps.Count - 1;

        /// <summary>
        /// Returns true if navigation to the next step is allowed.
        /// Navigation is always allowed unless at the last step.
        /// </summary>
        public bool CanGoToNext => !IsLastStep;

        /// <summary>
        /// Returns true if all required fields in all steps are filled.
        /// </summary>
        public bool AllStepsRequiredFieldsFilled
        {
            get
            {
                foreach (var step in WorkflowSteps)
                {
                    var vm = GetViewModelForStep(step.StepName);
                    if (vm != null && !vm.CanProceedToNext)
                        return false;
                }
                return true;
            }
        }

        /// <summary>
        /// Gets or sets whether the view is in read-only mode.
        /// </summary>
        public bool IsReadOnly
        {
            get => _isReadOnly;
            set
            {
                if (SetProperty(ref _isReadOnly, value))
                {
                    OnPropertyChanged(nameof(IsEditButtonVisible));
                    UpdateChildViewModelsEditability(!value);
                }
            }
        }

        /// <summary>
        /// Returns true if the Edit button should be visible (only in read-only mode).
        /// </summary>
        public bool IsEditButtonVisible => IsReadOnly;

        /// <summary>
        /// The ID of the loaded component (when viewing an existing component).
        /// </summary>
        public string LoadedComponentId
        {
            get => _loadedComponentId;
            private set => SetProperty(ref _loadedComponentId, value);
        }

        /// <summary>
        /// The type of the loaded component (when viewing an existing component).
        /// </summary>
        public HardwareLayer? LoadedHardwareLayer
        {
            get => _loadedHardwareLayer;
            private set => SetProperty(ref _loadedHardwareLayer, value);
        }

        /// <summary>
        /// Equipment definition ViewModel.
        /// </summary>
        public EquipmentDefineViewModel EquipmentViewModel
        {
            get => _equipmentViewModel;
            private set => SetProperty(ref _equipmentViewModel, value);
        }

        /// <summary>
        /// System definition ViewModel.
        /// </summary>
        public SystemDefineViewModel SystemViewModel
        {
            get => _systemViewModel;
            private set => SetProperty(ref _systemViewModel, value);
        }

        /// <summary>
        /// Unit definition ViewModel.
        /// </summary>
        public UnitDefineViewModel UnitViewModel
        {
            get => _unitViewModel;
            private set => SetProperty(ref _unitViewModel, value);
        }

        /// <summary>
        /// Device definition ViewModel.
        /// </summary>
        public DeviceDefineViewModel DeviceViewModel
        {
            get => _deviceViewModel;
            private set => SetProperty(ref _deviceViewModel, value);
        }

        /// <summary>
        /// Command to navigate to the next step.
        /// </summary>
        public ICommand GoToNextStepCommand { get; private set; }

        /// <summary>
        /// Command to navigate to the previous step.
        /// </summary>
        public ICommand GoToPreviousStepCommand { get; private set; }

        /// <summary>
        /// Command to exit to dashboard.
        /// </summary>
        public ICommand ExitToDashboardCommand { get; private set; }

        /// <summary>
        /// Command to navigate directly to a specific workflow step.
        /// </summary>
        public ICommand NavigateToStepCommand { get; private set; }

        /// <summary>
        /// Command to enable editing mode.
        /// </summary>
        public ICommand EnableEditCommand { get; private set; }

        private void InitializeWorkflowSteps()
        {
            int stepNumber = 1;

            if (StartType == HardwareLayer.Equipment)
            {
                WorkflowSteps.Add(new WorkflowStepViewModel(stepNumber++, "Equipment"));
            }

            if (StartType <= HardwareLayer.System)
            {
                WorkflowSteps.Add(new WorkflowStepViewModel(stepNumber++, "System"));
            }

            if (StartType <= HardwareLayer.Unit)
            {
                WorkflowSteps.Add(new WorkflowStepViewModel(stepNumber++, "Unit"));
            }

            WorkflowSteps.Add(new WorkflowStepViewModel(stepNumber, "Device"));
        }

        private void InitializeViewModels()
        {
            // Initialize all ViewModels - they will be used based on the current step
            EquipmentViewModel = new EquipmentDefineViewModel();
            SystemViewModel = new SystemDefineViewModel();
            UnitViewModel = new UnitDefineViewModel();
            DeviceViewModel = new DeviceDefineViewModel();

            // Set the callback for DeviceViewModel to check if all steps are completed
            DeviceViewModel.SetAllStepsRequiredFieldsFilledCheck(() => AllStepsRequiredFieldsFilled);

            // Subscribe to workflow completion request
            DeviceViewModel.WorkflowCompletedRequest += async (s, e) => await CompleteWorkflowAsync();

            // Subscribe to validation and field count changes
            EquipmentViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(EquipmentDefineViewModel.CanProceedToNext))
                {
                    OnPropertyChanged(nameof(AllStepsRequiredFieldsFilled));
                    UpdateStepCompletionStatus("Equipment");
                    DeviceViewModel.RaiseCanCompleteWorkflowChanged();
                }
                if (e.PropertyName == nameof(EquipmentDefineViewModel.FilledFieldCount))
                {
                    UpdateStepFieldCounts("Equipment");
                }
            };

            SystemViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(SystemDefineViewModel.CanProceedToNext))
                {
                    OnPropertyChanged(nameof(AllStepsRequiredFieldsFilled));
                    UpdateStepCompletionStatus("System");
                    DeviceViewModel.RaiseCanCompleteWorkflowChanged();
                }
                if (e.PropertyName == nameof(SystemDefineViewModel.FilledFieldCount))
                {
                    UpdateStepFieldCounts("System");
                }
            };

            UnitViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(UnitDefineViewModel.CanProceedToNext))
                {
                    OnPropertyChanged(nameof(AllStepsRequiredFieldsFilled));
                    UpdateStepCompletionStatus("Unit");
                    DeviceViewModel.RaiseCanCompleteWorkflowChanged();
                }
                if (e.PropertyName == nameof(UnitDefineViewModel.FilledFieldCount))
                {
                    UpdateStepFieldCounts("Unit");
                }
            };

            DeviceViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(DeviceDefineViewModel.CanProceedToNext))
                {
                    OnPropertyChanged(nameof(AllStepsRequiredFieldsFilled));
                    UpdateStepCompletionStatus("Device");
                    DeviceViewModel.RaiseCanCompleteWorkflowChanged();
                }
                if (e.PropertyName == nameof(DeviceDefineViewModel.FilledFieldCount))
                {
                    UpdateStepFieldCounts("Device");
                }
            };

            // Initialize field counts for all steps
            InitializeStepFieldCounts();

            // Set initial editability state (new workflow = editable, loaded for view = read-only)
            UpdateChildViewModelsEditability(!IsReadOnly);
        }

        private void InitializeCommands()
        {
            GoToNextStepCommand = new RelayCommand(ExecuteGoToNextStep, CanExecuteGoToNextStep);
            GoToPreviousStepCommand = new RelayCommand(ExecuteGoToPreviousStep, CanExecuteGoToPreviousStep);
            ExitToDashboardCommand = new RelayCommand(ExecuteExitToDashboard);
            NavigateToStepCommand = new RelayCommand<WorkflowStepViewModel>(ExecuteNavigateToStep, CanExecuteNavigateToStep);
            EnableEditCommand = new RelayCommand(ExecuteEnableEdit);
        }

        private void ExecuteEnableEdit()
        {
            IsReadOnly = false;
            
            // If a component was loaded for viewing, change its state from Uploaded/Validated to Defined
            if (!string.IsNullOrEmpty(LoadedComponentId) && LoadedHardwareLayer.HasValue)
            {
                ChangeComponentStateToDefinedAsync();
            }
        }

        /// <summary>
        /// Changes the loaded component's state from Uploaded/Validated to Defined in the repository.
        /// </summary>
        private async void ChangeComponentStateToDefinedAsync()
        {
            var repository = ServiceLocator.GetService<IDataRepository>();
            var dataStore = await repository.LoadAsync();

            switch (LoadedHardwareLayer.Value)
            {
                case HardwareLayer.Equipment:
                    var equipment = dataStore.Equipments?.FirstOrDefault(e => e.Id == LoadedComponentId);
                    if (equipment != null && (equipment.State == ComponentState.Uploaded || equipment.State == ComponentState.Validated))
                    {
                        equipment.State = ComponentState.Defined;
                        equipment.UpdatedAt = DateTime.Now;
                    }
                    break;
                case HardwareLayer.System:
                    var system = dataStore.Systems?.FirstOrDefault(s => s.Id == LoadedComponentId);
                    if (system != null && (system.State == ComponentState.Uploaded || system.State == ComponentState.Validated))
                    {
                        system.State = ComponentState.Defined;
                        system.UpdatedAt = DateTime.Now;
                    }
                    break;
                case HardwareLayer.Unit:
                    var unit = dataStore.Units?.FirstOrDefault(u => u.Id == LoadedComponentId);
                    if (unit != null && (unit.State == ComponentState.Uploaded || unit.State == ComponentState.Validated))
                    {
                        unit.State = ComponentState.Defined;
                        unit.UpdatedAt = DateTime.Now;
                    }
                    break;
                case HardwareLayer.Device:
                    var device = dataStore.Devices?.FirstOrDefault(d => d.Id == LoadedComponentId);
                    if (device != null && (device.State == ComponentState.Uploaded || device.State == ComponentState.Validated))
                    {
                        device.State = ComponentState.Defined;
                        device.UpdatedAt = DateTime.Now;
                    }
                    break;
            }

            await repository.SaveAsync(dataStore);
        }

        /// <summary>
        /// Loads an existing component for viewing in read-only mode.
        /// </summary>
        public async Task LoadComponentForViewAsync(string componentId, HardwareLayer hardwareLayer)
        {
            var repository = ServiceLocator.GetService<IDataRepository>();
            var dataStore = await repository.LoadAsync();

            LoadedComponentId = componentId;
            LoadedHardwareLayer = hardwareLayer;

            switch (hardwareLayer)
            {
                case HardwareLayer.Equipment:
                    var equipment = dataStore.Equipments?.FirstOrDefault(e => e.Id == componentId);
                    if (equipment != null)
                    {
                        EquipmentViewModel = EquipmentDefineViewModel.FromDto(equipment);
                    }
                    break;
                case HardwareLayer.System:
                    var system = dataStore.Systems?.FirstOrDefault(s => s.Id == componentId);
                    if (system != null)
                    {
                        SystemViewModel = SystemDefineViewModel.FromDto(system);
                    }
                    break;
                case HardwareLayer.Unit:
                    var unit = dataStore.Units?.FirstOrDefault(u => u.Id == componentId);
                    if (unit != null)
                    {
                        UnitViewModel = UnitDefineViewModel.FromDto(unit);
                    }
                    break;
                case HardwareLayer.Device:
                    var device = dataStore.Devices?.FirstOrDefault(d => d.Id == componentId);
                    if (device != null)
                    {
                        DeviceViewModel = DeviceDefineViewModel.FromDto(device);
                    }
                    break;
            }

            IsReadOnly = true;
        }

        private void ExecuteGoToNextStep()
        {
            if (CurrentStepIndex < WorkflowSteps.Count - 1)
            {
                CurrentStepIndex++;
            }
        }

        private bool CanExecuteGoToNextStep()
        {
            return CanGoToNext;
        }

        private void ExecuteGoToPreviousStep()
        {
            if (CurrentStepIndex > 0)
            {
                CurrentStepIndex--;
            }
        }

        private bool CanExecuteGoToPreviousStep()
        {
            return !IsFirstStep;
        }

        private async void ExecuteExitToDashboard()
        {
            await SaveWorkflowStateAsync();
            NavigationService.Instance.NavigateToDashboard();
        }

        /// <summary>
        /// Saves the current workflow state to repository.
        /// </summary>
        private async Task SaveWorkflowStateAsync()
        {
            var repository = ServiceLocator.GetService<IDataRepository>();
            var dataStore = await repository.LoadAsync();

            // Ensure collections are initialized
            dataStore.WorkflowSessions ??= new System.Collections.Generic.List<WorkflowSessionDto>();
            dataStore.SessionContext ??= new WorkSessionContext();
            dataStore.SessionContext.IncompleteWorkflows ??= new System.Collections.Generic.List<IncompleteWorkflowInfo>();

            // Create or update workflow session
            var sessionDto = ToWorkflowSessionDto();
            var existingIndex = dataStore.WorkflowSessions.FindIndex(s => s.WorkflowId == WorkflowId);

            if (existingIndex >= 0)
                dataStore.WorkflowSessions[existingIndex] = sessionDto;
            else
                dataStore.WorkflowSessions.Add(sessionDto);

            // Update incomplete workflow info for dashboard display
            var incompleteInfo = new IncompleteWorkflowInfo
            {
                WorkflowId = WorkflowId,
                StartType = StartType,
                CurrentStepName = sessionDto.GetCurrentStepName(),
                ComponentId = WorkflowId,
                HardwareLayer = StartType switch
                {
                    HardwareLayer.Equipment => HardwareLayer.Equipment,
                    HardwareLayer.System => HardwareLayer.System,
                    HardwareLayer.Unit => HardwareLayer.Unit,
                    HardwareLayer.Device => HardwareLayer.Device,
                    _ => HardwareLayer.Equipment
                },
                State = AllStepsRequiredFieldsFilled ? ComponentState.Defined : ComponentState.Undefined,
                LastModifiedAt = DateTime.Now,
                CompletedFields = WorkflowSteps.Sum(s => s.FilledFieldCount),
                TotalFields = WorkflowSteps.Sum(s => s.TotalFieldCount)
            };

            var existingInfoIndex = dataStore.SessionContext.IncompleteWorkflows
                .FindIndex(i => i.WorkflowId == WorkflowId);

            if (existingInfoIndex >= 0)
                dataStore.SessionContext.IncompleteWorkflows[existingInfoIndex] = incompleteInfo;
            else
                dataStore.SessionContext.IncompleteWorkflows.Add(incompleteInfo);

            await repository.SaveAsync(dataStore);
        }

        /// <summary>
        /// Converts this ViewModel to a WorkflowSessionDto for persistence.
        /// </summary>
        public WorkflowSessionDto ToWorkflowSessionDto()
        {
            return new WorkflowSessionDto
            {
                WorkflowId = WorkflowId,
                StartType = StartType,
                CurrentStepIndex = CurrentStepIndex,
                LastModifiedAt = DateTime.Now,
                EquipmentData = EquipmentViewModel?.ToDto(),
                SystemData = SystemViewModel?.ToDto(),
                UnitData = UnitViewModel?.ToDto(),
                DeviceData = DeviceViewModel?.ToDto()
            };
        }

        /// <summary>
        /// Creates a HardwareDefineWorkflowViewModel from a saved WorkflowSessionDto.
        /// </summary>
        public static HardwareDefineWorkflowViewModel FromWorkflowSessionDto(WorkflowSessionDto dto)
        {
            var viewModel = new HardwareDefineWorkflowViewModel(dto.StartType, dto.WorkflowId);

            // Load component data from DTOs
            if (dto.EquipmentData != null)
                viewModel.EquipmentViewModel = EquipmentDefineViewModel.FromDto(dto.EquipmentData);

            if (dto.SystemData != null)
                viewModel.SystemViewModel = SystemDefineViewModel.FromDto(dto.SystemData);

            if (dto.UnitData != null)
                viewModel.UnitViewModel = UnitDefineViewModel.FromDto(dto.UnitData);

            if (dto.DeviceData != null)
                viewModel.DeviceViewModel = DeviceDefineViewModel.FromDto(dto.DeviceData);

            // Restore current step index
            viewModel.SetCurrentStepIndex(dto.CurrentStepIndex);

            // Re-initialize step states and callbacks
            viewModel.DeviceViewModel.SetAllStepsRequiredFieldsFilledCheck(() => viewModel.AllStepsRequiredFieldsFilled);
            viewModel.InitializeStepFieldCounts();
            viewModel.UpdateStepStates();

            return viewModel;
        }

        /// <summary>
        /// Sets the current step index (for resume scenarios).
        /// </summary>
        internal void SetCurrentStepIndex(int index)
        {
            if (index >= 0 && index < WorkflowSteps.Count)
            {
                _currentStepIndex = index;
                OnPropertyChanged(nameof(CurrentStepIndex));
                OnPropertyChanged(nameof(CurrentStep));
                OnPropertyChanged(nameof(IsFirstStep));
                OnPropertyChanged(nameof(IsLastStep));
                OnPropertyChanged(nameof(CanGoToNext));
            }
        }

        private void ExecuteNavigateToStep(WorkflowStepViewModel step)
        {
            if (step == null) return;

            var targetIndex = WorkflowSteps.IndexOf(step);
            if (targetIndex < 0) return; // Step not in workflow

            // Only navigate if the step is reachable (active or completed)
            if (!step.CanNavigateTo) return;

            // Don't navigate if already at this step
            if (targetIndex == CurrentStepIndex) return;

            CurrentStepIndex = targetIndex;
        }

        private bool CanExecuteNavigateToStep(WorkflowStepViewModel step)
        {
            if (step == null) return false;
            return step.CanNavigateTo;
        }

        private void UpdateStepStates()
        {
            for (int i = 0; i < WorkflowSteps.Count; i++)
            {
                var step = WorkflowSteps[i];
                step.IsActive = i == CurrentStepIndex;

                // IsCompleted is based on whether required fields are filled
                var vm = GetViewModelForStep(step.StepName);
                step.IsCompleted = vm?.CanProceedToNext == true;
            }
        }

        private void InitializeStepFieldCounts()
        {
            foreach (var step in WorkflowSteps)
            {
                UpdateStepFieldCounts(step.StepName);
                UpdateStepCompletionStatus(step.StepName);
            }
        }

        private void UpdateStepFieldCounts(string stepName)
        {
            var step = WorkflowSteps.FirstOrDefault(s => s.StepName == stepName);
            if (step == null) return;

            var vm = GetViewModelForStep(stepName);
            if (vm != null)
            {
                step.FilledFieldCount = vm.FilledFieldCount;
                step.TotalFieldCount = vm.TotalFieldCount;
            }
        }

        private void UpdateStepCompletionStatus(string stepName)
        {
            var step = WorkflowSteps.FirstOrDefault(s => s.StepName == stepName);
            if (step == null) return;

            var vm = GetViewModelForStep(stepName);
            step.IsCompleted = vm?.CanProceedToNext == true;
        }

        private dynamic GetViewModelForStep(string stepName)
        {
            return stepName switch
            {
                "Equipment" => EquipmentViewModel,
                "System" => SystemViewModel,
                "Unit" => UnitViewModel,
                "Device" => DeviceViewModel,
                _ => null
            };
        }

        private dynamic GetCurrentStepViewModel()
        {
            return GetViewModelForStep(CurrentStep.StepName);
        }

        /// <summary>
        /// Completes the workflow by saving all components to SharedMemory and navigating to Dashboard.
        /// Components saved depend on StartType:
        /// - Equipment: Equipment, System, Unit, Device (4 components)
        /// - System: System, Unit, Device (3 components)
        /// - Unit: Unit, Device (2 components)
        /// - Device: Device only (1 component)
        /// </summary>
        private async Task CompleteWorkflowAsync()
        {
            var repository = ServiceLocator.GetService<IDataRepository>();
            var dataStore = await repository.LoadAsync();

            // Ensure collections are initialized
            dataStore.Equipments ??= new System.Collections.Generic.List<EquipmentDto>();
            dataStore.Systems ??= new System.Collections.Generic.List<SystemDto>();
            dataStore.Units ??= new System.Collections.Generic.List<UnitDto>();
            dataStore.Devices ??= new System.Collections.Generic.List<DeviceDto>();
            dataStore.WorkflowSessions ??= new System.Collections.Generic.List<WorkflowSessionDto>();
            dataStore.SessionContext ??= new WorkSessionContext();
            dataStore.SessionContext.IncompleteWorkflows ??= new System.Collections.Generic.List<IncompleteWorkflowInfo>();

            var now = DateTime.Now;
            string equipmentId = null;
            string systemId = null;
            string unitId = null;
            string deviceId = null;

            // Save Equipment if StartType includes it
            if (StartType == HardwareLayer.Equipment)
            {
                var equipmentDto = EquipmentViewModel.ToDto();
                equipmentId = Guid.NewGuid().ToString();
                equipmentDto.Id = equipmentId;
                equipmentDto.State = ComponentState.Defined;
                equipmentDto.CreatedAt = now;
                equipmentDto.UpdatedAt = now;
                dataStore.Equipments.Add(equipmentDto);
            }

            // Save System if StartType includes it
            if (StartType <= HardwareLayer.System)
            {
                var systemDto = SystemViewModel.ToDto();
                systemId = Guid.NewGuid().ToString();
                systemDto.Id = systemId;
                systemDto.EquipmentId = equipmentId; // null if StartType is System
                systemDto.State = ComponentState.Defined;
                systemDto.CreatedAt = now;
                systemDto.UpdatedAt = now;
                dataStore.Systems.Add(systemDto);
            }

            // Save Unit if StartType includes it
            if (StartType <= HardwareLayer.Unit)
            {
                var unitDto = UnitViewModel.ToDto();
                unitId = Guid.NewGuid().ToString();
                unitDto.Id = unitId;
                unitDto.SystemId = systemId; // null if StartType is Unit
                unitDto.State = ComponentState.Defined;
                unitDto.CreatedAt = now;
                unitDto.UpdatedAt = now;
                dataStore.Units.Add(unitDto);
            }

            // Always save Device (Device is always part of the workflow)
            var deviceDto = DeviceViewModel.ToDto();
            deviceId = Guid.NewGuid().ToString();
            deviceDto.Id = deviceId;
            deviceDto.UnitId = unitId; // null if StartType is Device
            deviceDto.State = ComponentState.Defined;
            deviceDto.CreatedAt = now;
            deviceDto.UpdatedAt = now;
            dataStore.Devices.Add(deviceDto);

            // Remove from WorkflowSessions
            var sessionToRemove = dataStore.WorkflowSessions.FirstOrDefault(s => s.WorkflowId == WorkflowId);
            if (sessionToRemove != null)
            {
                dataStore.WorkflowSessions.Remove(sessionToRemove);
            }

            // Remove from IncompleteWorkflows
            var infoToRemove = dataStore.SessionContext.IncompleteWorkflows.FirstOrDefault(i => i.WorkflowId == WorkflowId);
            if (infoToRemove != null)
            {
                dataStore.SessionContext.IncompleteWorkflows.Remove(infoToRemove);
            }

            // Save the data store
            await repository.SaveAsync(dataStore);

            // Navigate to Dashboard
            NavigationService.Instance.NavigateToDashboard();
        }

        /// <summary>
        /// Updates the IsEditable property on all child ViewModels.
        /// </summary>
        private void UpdateChildViewModelsEditability(bool isEditable)
        {
            if (EquipmentViewModel != null) EquipmentViewModel.IsEditable = isEditable;
            if (SystemViewModel != null) SystemViewModel.IsEditable = isEditable;
            if (UnitViewModel != null) UnitViewModel.IsEditable = isEditable;
            if (DeviceViewModel != null) DeviceViewModel.IsEditable = isEditable;
        }
    }
}