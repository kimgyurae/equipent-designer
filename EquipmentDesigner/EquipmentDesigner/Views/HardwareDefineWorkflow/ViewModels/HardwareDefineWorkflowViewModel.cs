using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using EquipmentDesigner.Models;
using EquipmentDesigner.Models.Dtos;
using EquipmentDesigner.Models.Storage;
using EquipmentDesigner.Services;
using EquipmentDesigner.Services.Storage;
using EquipmentDesigner.Views.ReusableComponents.Toast;
using EquipmentDesigner.Resources;

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
        private HardwareTreeNodeViewModel _selectedTreeNode;
        private bool _isWorkflowCompleted;
        private bool _hasDataChangedSinceCompletion;

        // Autosave fields
        private DispatcherTimer _autosaveTimer;
        private bool _isDirty;
        private bool _isAutosaveEnabled;
        private static readonly TimeSpan DefaultAutosaveInterval = TimeSpan.FromSeconds(30);

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
        /// Returns true if all required fields in all tree nodes are filled.
        /// </summary>
        public bool AllStepsRequiredFieldsFilled => CheckAllNodesRequiredFieldsFilled();

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
                    UpdateAllTreeNodeEditability(!value);
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

        /// <summary>
        /// Command to add a new child to a tree node.
        /// </summary>
        public ICommand AddChildCommand { get; private set; }

        /// <summary>
        /// Command to select a tree node.
        /// </summary>
        public ICommand SelectTreeNodeCommand { get; private set; }

        /// <summary>
        /// Command to complete the workflow.
        /// </summary>
        public ICommand CompleteWorkflowCommand { get; private set; }

        /// <summary>
        /// Command to upload to server (placeholder).
        /// </summary>
        public ICommand UploadToServerCommand { get; private set; }

        /// <summary>
        /// Gets whether the workflow has been completed.
        /// </summary>
        public bool IsWorkflowCompleted
        {
            get => _isWorkflowCompleted;
            private set
            {
                if (SetProperty(ref _isWorkflowCompleted, value))
                {
                    OnPropertyChanged(nameof(CanCompleteWorkflow));
                    OnPropertyChanged(nameof(ShowUploadButton));
                }
            }
        }

        /// <summary>
        /// Gets whether data has changed since workflow completion.
        /// </summary>
        public bool HasDataChangedSinceCompletion
        {
            get => _hasDataChangedSinceCompletion;
            private set
            {
                if (SetProperty(ref _hasDataChangedSinceCompletion, value))
                {
                    OnPropertyChanged(nameof(ShowUploadButton));
                    if (value)
                    {
                        IsWorkflowCompleted = false;
                    }
                }
            }
        }

        /// <summary>
        /// Returns true if the workflow can be completed.
        /// </summary>
        public bool CanCompleteWorkflow => !IsReadOnly && !IsWorkflowCompleted && AllStepsRequiredFieldsFilled;

        /// <summary>
        /// Returns true if the upload button should be shown.
        /// </summary>
        public bool ShowUploadButton => IsWorkflowCompleted && !HasDataChangedSinceCompletion;

        /// <summary>
        /// Root nodes for the hardware tree structure.
        /// </summary>
        public ObservableCollection<HardwareTreeNodeViewModel> TreeRootNodes { get; private set; }

        /// <summary>
        /// Currently selected tree node.
        /// </summary>
        public HardwareTreeNodeViewModel SelectedTreeNode
        {
            get => _selectedTreeNode;
            set
            {
                if (SetProperty(ref _selectedTreeNode, value))
                {
                    OnPropertyChanged(nameof(CurrentHardwareLayer));
                    UpdateCurrentStepFromTreeNode();
                    SetupCurrentDeviceViewModelCallbacks();
                }
            }
        }

        /// <summary>
        /// The hardware layer of the currently selected tree node.
        /// </summary>
        public HardwareLayer? CurrentHardwareLayer => SelectedTreeNode?.HardwareLayer;

        #region Convenience Properties for Tree Node ViewModels

        /// <summary>
        /// Gets the first EquipmentDefineViewModel from the tree.
        /// Convenience property for backward compatibility.
        /// </summary>
        public EquipmentDefineViewModel EquipmentViewModel =>
            GetAllNodes().FirstOrDefault(n => n.HardwareLayer == HardwareLayer.Equipment)?.DataViewModel as EquipmentDefineViewModel;

        /// <summary>
        /// Gets the first SystemDefineViewModel from the tree.
        /// Convenience property for backward compatibility.
        /// </summary>
        public SystemDefineViewModel SystemViewModel =>
            GetAllNodes().FirstOrDefault(n => n.HardwareLayer == HardwareLayer.System)?.DataViewModel as SystemDefineViewModel;

        /// <summary>
        /// Gets the first UnitDefineViewModel from the tree.
        /// Convenience property for backward compatibility.
        /// </summary>
        public UnitDefineViewModel UnitViewModel =>
            GetAllNodes().FirstOrDefault(n => n.HardwareLayer == HardwareLayer.Unit)?.DataViewModel as UnitDefineViewModel;

        /// <summary>
        /// Gets the first DeviceDefineViewModel from the tree.
        /// Convenience property for backward compatibility.
        /// </summary>
        public DeviceDefineViewModel DeviceViewModel =>
            GetAllNodes().FirstOrDefault(n => n.HardwareLayer == HardwareLayer.Device)?.DataViewModel as DeviceDefineViewModel;

        #endregion

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

            // Initialize tree structure
            InitializeTreeStructure();
        }

        /// <summary>
        /// Initializes the hierarchical tree structure based on StartType.
        /// </summary>
        private void InitializeTreeStructure()
        {
            TreeRootNodes = new ObservableCollection<HardwareTreeNodeViewModel>();

            HardwareTreeNodeViewModel rootNode = null;
            HardwareTreeNodeViewModel currentParent = null;

            // Build tree based on StartType
            if (StartType == HardwareLayer.Equipment)
            {
                rootNode = new HardwareTreeNodeViewModel(HardwareLayer.Equipment);
                TreeRootNodes.Add(rootNode);
                currentParent = rootNode;

                // Add initial System node under Equipment
                var systemNode = currentParent.AddChild();
                currentParent = systemNode;
            }
            else if (StartType == HardwareLayer.System)
            {
                rootNode = new HardwareTreeNodeViewModel(HardwareLayer.System);
                TreeRootNodes.Add(rootNode);
                currentParent = rootNode;
            }

            if (StartType <= HardwareLayer.System && currentParent != null)
            {
                // Add initial Unit node under System
                var unitNode = currentParent.AddChild();
                currentParent = unitNode;
            }
            else if (StartType == HardwareLayer.Unit)
            {
                rootNode = new HardwareTreeNodeViewModel(HardwareLayer.Unit);
                TreeRootNodes.Add(rootNode);
                currentParent = rootNode;
            }

            if (StartType <= HardwareLayer.Unit && currentParent != null)
            {
                // Add initial Device node under Unit
                currentParent.AddChild();
            }
            else if (StartType == HardwareLayer.Device)
            {
                rootNode = new HardwareTreeNodeViewModel(HardwareLayer.Device);
                TreeRootNodes.Add(rootNode);
            }

            // Select the root node by default
            if (TreeRootNodes.Count > 0)
            {
                SelectedTreeNode = TreeRootNodes[0];
                SelectedTreeNode.IsSelected = true;
            }
        }

        private void InitializeViewModels()
        {
            // ViewModels are now created automatically when tree nodes are created
            // Set up callbacks for all Device nodes in the tree
            SetupAllDeviceViewModelCallbacks();

            // Initialize field counts for all steps
            InitializeStepFieldCounts();

            // Set initial editability state (new workflow = editable, loaded for view = read-only)
            UpdateAllTreeNodeEditability(!IsReadOnly);
        }

        private void InitializeCommands()
        {
            GoToNextStepCommand = new RelayCommand(ExecuteGoToNextStep, CanExecuteGoToNextStep);
            GoToPreviousStepCommand = new RelayCommand(ExecuteGoToPreviousStep, CanExecuteGoToPreviousStep);
            ExitToDashboardCommand = new RelayCommand(ExecuteExitToDashboard);
            NavigateToStepCommand = new RelayCommand<WorkflowStepViewModel>(ExecuteNavigateToStep, CanExecuteNavigateToStep);
            EnableEditCommand = new RelayCommand(ExecuteEnableEdit);
            AddChildCommand = new RelayCommand<HardwareTreeNodeViewModel>(ExecuteAddChild, CanExecuteAddChild);
            SelectTreeNodeCommand = new RelayCommand<HardwareTreeNodeViewModel>(ExecuteSelectTreeNode);
            CompleteWorkflowCommand = new RelayCommand(ExecuteCompleteWorkflowCommand, CanExecuteCompleteWorkflowCommand);
            UploadToServerCommand = new RelayCommand(ExecuteUploadToServer);
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
            var repository = ServiceLocator.GetService<ITypedDataRepository<UploadedHardwareDataStore>>();
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
            var repository = ServiceLocator.GetService<ITypedDataRepository<UploadedHardwareDataStore>>();
            var dataStore = await repository.LoadAsync();

            LoadedComponentId = componentId;
            LoadedHardwareLayer = hardwareLayer;

            // Find the root node and set its DataViewModel from loaded data
            if (TreeRootNodes.Count > 0)
            {
                var rootNode = TreeRootNodes[0];
                switch (hardwareLayer)
                {
                    case HardwareLayer.Equipment:
                        var equipment = dataStore.Equipments?.FirstOrDefault(e => e.Id == componentId);
                        if (equipment != null && rootNode.DataViewModel is EquipmentDefineViewModel equipmentVm)
                        {
                            // Copy data to existing ViewModel
                            var loadedVm = EquipmentDefineViewModel.FromDto(equipment);
                            CopyViewModelData(loadedVm, equipmentVm);
                        }
                        break;
                    case HardwareLayer.System:
                        var system = dataStore.Systems?.FirstOrDefault(s => s.Id == componentId);
                        if (system != null && rootNode.DataViewModel is SystemDefineViewModel systemVm)
                        {
                            var loadedVm = SystemDefineViewModel.FromDto(system);
                            CopyViewModelData(loadedVm, systemVm);
                        }
                        break;
                    case HardwareLayer.Unit:
                        var unit = dataStore.Units?.FirstOrDefault(u => u.Id == componentId);
                        if (unit != null && rootNode.DataViewModel is UnitDefineViewModel unitVm)
                        {
                            var loadedVm = UnitDefineViewModel.FromDto(unit);
                            CopyViewModelData(loadedVm, unitVm);
                        }
                        break;
                    case HardwareLayer.Device:
                        var device = dataStore.Devices?.FirstOrDefault(d => d.Id == componentId);
                        if (device != null && rootNode.DataViewModel is DeviceDefineViewModel deviceVm)
                        {
                            var loadedVm = DeviceDefineViewModel.FromDto(device);
                            CopyViewModelData(loadedVm, deviceVm);
                        }
                        break;
                }
            }

            IsReadOnly = true;
        }

        /// <summary>
        /// Copies data from source ViewModel to target ViewModel.
        /// </summary>
        private void CopyViewModelData(IHardwareDefineViewModel source, IHardwareDefineViewModel target)
        {
            // Copy the Name property
            target.Name = source.Name;
            // Additional properties would need to be copied based on type
            // This is a basic implementation for the Name field
        }

        private void ExecuteGoToNextStep()
        {
            if (CurrentStepIndex < WorkflowSteps.Count - 1)
            {
                CurrentStepIndex++;
                MarkDirty(); // Mark for autosave when navigating
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
                MarkDirty(); // Mark for autosave when navigating
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

        private async void ExecuteCompleteWorkflowCommand()
        {
            await CompleteWorkflowAsync();
            IsWorkflowCompleted = true;
            HasDataChangedSinceCompletion = false;
            
            // Show success toast
            ToastService.Instance.ShowSuccess(
                Strings.Toast_WorkflowCompleted_Title,
                Strings.Toast_WorkflowCompleted_Description);
        }

        private bool CanExecuteCompleteWorkflowCommand()
        {
            return CanCompleteWorkflow;
        }

        private void ExecuteUploadToServer()
        {
            UploadToServerAsync();
        }

        /// <summary>
        /// Uploads workflow data to server by:
        /// 1. Mapping tree data to flat hardware lists using TreeToFlatMapper
        /// 2. Saving to UploadedHardwareRepository (uploaded-hardwares.json)
        /// 3. Updating workflow state in WorkflowRepository (workflows.json)
        /// </summary>
        private async void UploadToServerAsync()
        {
            var mapper = new TreeToFlatMapper();

            // Convert tree nodes to TreeNodeDataDto for mapping
            var treeNodeDtos = TreeRootNodes.Select(SerializeNode).ToList();

            // Map tree structure to flat hardware lists
            var mappingResult = mapper.MapTreeToFlat(treeNodeDtos);

            // Save to UploadedHardwareRepository
            var uploadedRepo = ServiceLocator.GetService<ITypedDataRepository<UploadedHardwareDataStore>>();
            var uploadedData = await uploadedRepo.LoadAsync();

            uploadedData.Equipments.AddRange(mappingResult.Equipments);
            uploadedData.Systems.AddRange(mappingResult.Systems);
            uploadedData.Units.AddRange(mappingResult.Units);
            uploadedData.Devices.AddRange(mappingResult.Devices);

            await uploadedRepo.SaveAsync(uploadedData);

            // Remove workflow from WorkflowRepository after successful upload
            var workflowRepo = ServiceLocator.GetService<ITypedDataRepository<IncompleteWorkflowDataStore>>();
            var workflowData = await workflowRepo.LoadAsync();

            var session = workflowData.WorkflowSessions.FirstOrDefault(s => s.WorkflowId == WorkflowId);
            if (session != null)
            {
                workflowData.WorkflowSessions.Remove(session);
                await workflowRepo.SaveAsync(workflowData);
            }

            // Show success toast
            ToastService.Instance.ShowSuccess(
                "Upload Complete",
                "Data has been uploaded to the server.");
        }

        /// <summary>
        /// Called when any data in any tree node changes.
        /// Resets the workflow completion state if data is modified after completion.
        /// </summary>
        public void OnDataChanged()
        {
            if (IsWorkflowCompleted)
            {
                HasDataChangedSinceCompletion = true;
            }
            OnPropertyChanged(nameof(CanCompleteWorkflow));
            OnPropertyChanged(nameof(AllStepsRequiredFieldsFilled));
        }

        /// <summary>
        /// Saves the current workflow state to repository.
        /// Uses the new WorkflowRepository (ITypedDataRepository<IncompleteWorkflowDataStore>)
        /// instead of the legacy IDataRepository.
        /// </summary>
        private async Task SaveWorkflowStateAsync()
        {
            var workflowRepo = ServiceLocator.GetService<ITypedDataRepository<IncompleteWorkflowDataStore>>();
            var workflowData = await workflowRepo.LoadAsync();

            // Create or update WorkflowSessionDto2
            var sessionDto = ToWorkflowSessionDto2();
            var existingIndex = workflowData.WorkflowSessions.FindIndex(s => s.WorkflowId == WorkflowId);

            if (existingIndex >= 0)
                workflowData.WorkflowSessions[existingIndex] = sessionDto;
            else
                workflowData.WorkflowSessions.Add(sessionDto);

            await workflowRepo.SaveAsync(workflowData);
            _isDirty = false;
        }

        #region Autosave

        /// <summary>
        /// Gets whether autosave is currently enabled.
        /// </summary>
        public bool IsAutosaveEnabled => _isAutosaveEnabled;

        /// <summary>
        /// Enables autosave with the default interval (30 seconds).
        /// </summary>
        public void EnableAutosave()
        {
            EnableAutosave(DefaultAutosaveInterval);
        }

        /// <summary>
        /// Enables autosave with a custom interval.
        /// </summary>
        /// <param name="interval">The interval between autosave attempts.</param>
        public void EnableAutosave(TimeSpan interval)
        {
            if (_isAutosaveEnabled)
                return;

            _autosaveTimer = new DispatcherTimer
            {
                Interval = interval
            };
            _autosaveTimer.Tick += OnAutosaveTimerTick;
            _autosaveTimer.Start();
            _isAutosaveEnabled = true;
        }

        /// <summary>
        /// Disables autosave and stops the timer.
        /// </summary>
        public void DisableAutosave()
        {
            if (!_isAutosaveEnabled)
                return;

            if (_autosaveTimer != null)
            {
                _autosaveTimer.Stop();
                _autosaveTimer.Tick -= OnAutosaveTimerTick;
                _autosaveTimer = null;
            }
            _isAutosaveEnabled = false;
        }

        /// <summary>
        /// Marks the workflow data as dirty (needing save).
        /// Call this when any data changes.
        /// </summary>
        public void MarkDirty()
        {
            _isDirty = true;
        }

        /// <summary>
        /// Handler for autosave timer tick.
        /// Saves workflow state if data is dirty.
        /// </summary>
        private async void OnAutosaveTimerTick(object sender, EventArgs e)
        {
            if (_isDirty && !IsReadOnly)
            {
                await SaveWorkflowStateAsync();
            }
        }

        #endregion

        /// <summary>
        /// Converts this ViewModel to a WorkflowSessionDto2 for persistence.
        /// Serializes the entire tree structure for multi-instance support.
        /// </summary>
        public WorkflowSessionDto2 ToWorkflowSessionDto2()
        {
            return new WorkflowSessionDto2
            {
                WorkflowId = WorkflowId,
                StartType = StartType,
                State = AllStepsRequiredFieldsFilled ? ComponentState.Defined : ComponentState.Undefined,
                ComponentId = WorkflowId,
                HardwareLayer = StartType,
                LastModifiedAt = DateTime.Now,
                TreeNodes = TreeRootNodes.Select(SerializeNode).ToList()
            };
        }

        /// <summary>
        /// Serializes a tree node and its children to TreeNodeDataDto.
        /// </summary>
        private TreeNodeDataDto SerializeNode(HardwareTreeNodeViewModel node)
        {
            var dto = new TreeNodeDataDto
            {
                NodeId = node.NodeId,
                HardwareLayer = node.HardwareLayer,
                Children = node.Children.Select(SerializeNode).ToList()
            };

            // Save ViewModel data based on type
            switch (node.HardwareLayer)
            {
                case HardwareLayer.Equipment:
                    dto.EquipmentData = (node.DataViewModel as EquipmentDefineViewModel)?.ToDto();
                    break;
                case HardwareLayer.System:
                    dto.SystemData = (node.DataViewModel as SystemDefineViewModel)?.ToDto();
                    break;
                case HardwareLayer.Unit:
                    dto.UnitData = (node.DataViewModel as UnitDefineViewModel)?.ToDto();
                    break;
                case HardwareLayer.Device:
                    dto.DeviceData = (node.DataViewModel as DeviceDefineViewModel)?.ToDto();
                    break;
            }

            return dto;
        }

        /// <summary>
        /// Creates a HardwareDefineWorkflowViewModel from a saved WorkflowSessionDto.
        /// Supports both legacy single-instance and new tree-based formats.
        /// </summary>
        public static HardwareDefineWorkflowViewModel FromWorkflowSessionDto(WorkflowSessionDto dto)
        {
            var viewModel = new HardwareDefineWorkflowViewModel(dto.StartType, dto.WorkflowId);

            // Check if we have tree data (new format)
            if (dto.TreeNodes != null && dto.TreeNodes.Count > 0)
            {
                // Clear default tree and rebuild from DTO
                viewModel.TreeRootNodes.Clear();
                foreach (var nodeDto in dto.TreeNodes)
                {
                    var node = DeserializeNode(nodeDto, null);
                    viewModel.TreeRootNodes.Add(node);
                }
            }
            else
            {
                // Legacy format: restore single instances to root nodes
                RestoreLegacyDataToTree(viewModel, dto);
            }

            // Select first node and restore step
            if (viewModel.TreeRootNodes.Count > 0)
            {
                viewModel.SelectedTreeNode = viewModel.TreeRootNodes[0];
                viewModel.SelectedTreeNode.IsSelected = true;
            }

            // Restore current step index
            viewModel.SetCurrentStepIndex(dto.CurrentStepIndex);

            // Re-initialize callbacks and states
            viewModel.SetupAllDeviceViewModelCallbacks();
            viewModel.InitializeStepFieldCounts();
            viewModel.UpdateStepStates();

            return viewModel;
        }

        /// <summary>
        /// Creates a HardwareDefineWorkflowViewModel from a saved WorkflowSessionDto2.
        /// Uses tree-based format exclusively.
        /// </summary>
        public static HardwareDefineWorkflowViewModel FromWorkflowSessionDto2(WorkflowSessionDto2 dto)
        {
            var viewModel = new HardwareDefineWorkflowViewModel(dto.StartType, dto.WorkflowId);

            // Rebuild tree from TreeNodes
            if (dto.TreeNodes != null && dto.TreeNodes.Count > 0)
            {
                viewModel.TreeRootNodes.Clear();
                foreach (var nodeDto in dto.TreeNodes)
                {
                    var node = DeserializeNode(nodeDto, null);
                    viewModel.TreeRootNodes.Add(node);
                }
            }

            // Select first node
            if (viewModel.TreeRootNodes.Count > 0)
            {
                viewModel.SelectedTreeNode = viewModel.TreeRootNodes[0];
                viewModel.SelectedTreeNode.IsSelected = true;
            }

            // Calculate step index based on tree state (default to 0)
            viewModel.SetCurrentStepIndex(0);

            // Re-initialize callbacks and states
            viewModel.SetupAllDeviceViewModelCallbacks();
            viewModel.InitializeStepFieldCounts();
            viewModel.UpdateStepStates();

            return viewModel;
        }

        /// <summary>
        /// Deserializes a TreeNodeDataDto to a HardwareTreeNodeViewModel.
        /// </summary>
        private static HardwareTreeNodeViewModel DeserializeNode(TreeNodeDataDto dto, HardwareTreeNodeViewModel parent)
        {
            IHardwareDefineViewModel dataViewModel = dto.HardwareLayer switch
            {
                HardwareLayer.Equipment => dto.EquipmentData != null
                    ? EquipmentDefineViewModel.FromDto(dto.EquipmentData)
                    : new EquipmentDefineViewModel(),
                HardwareLayer.System => dto.SystemData != null
                    ? SystemDefineViewModel.FromDto(dto.SystemData)
                    : new SystemDefineViewModel(),
                HardwareLayer.Unit => dto.UnitData != null
                    ? UnitDefineViewModel.FromDto(dto.UnitData)
                    : new UnitDefineViewModel(),
                HardwareLayer.Device => dto.DeviceData != null
                    ? DeviceDefineViewModel.FromDto(dto.DeviceData)
                    : new DeviceDefineViewModel(),
                _ => null
            };

            var node = new HardwareTreeNodeViewModel(dto.HardwareLayer, parent, dataViewModel);

            foreach (var childDto in dto.Children)
            {
                var childNode = DeserializeNode(childDto, node);
                node.Children.Add(childNode);
            }

            return node;
        }

        /// <summary>
        /// Restores legacy single-instance data to the tree structure.
        /// </summary>
        private static void RestoreLegacyDataToTree(HardwareDefineWorkflowViewModel viewModel, WorkflowSessionDto dto)
        {
            // Find nodes by type and populate them
            foreach (var node in viewModel.GetAllNodes())
            {
                switch (node.HardwareLayer)
                {
                    case HardwareLayer.Equipment when dto.EquipmentData != null:
                        var equipmentVm = EquipmentDefineViewModel.FromDto(dto.EquipmentData);
                        SetNodeDataViewModel(node, equipmentVm);
                        break;
                    case HardwareLayer.System when dto.SystemData != null:
                        var systemVm = SystemDefineViewModel.FromDto(dto.SystemData);
                        SetNodeDataViewModel(node, systemVm);
                        break;
                    case HardwareLayer.Unit when dto.UnitData != null:
                        var unitVm = UnitDefineViewModel.FromDto(dto.UnitData);
                        SetNodeDataViewModel(node, unitVm);
                        break;
                    case HardwareLayer.Device when dto.DeviceData != null:
                        var deviceVm = DeviceDefineViewModel.FromDto(dto.DeviceData);
                        SetNodeDataViewModel(node, deviceVm);
                        break;
                }
            }
        }

        /// <summary>
        /// Helper method to set DataViewModel using reflection (since setter is private).
        /// </summary>
        private static void SetNodeDataViewModel(HardwareTreeNodeViewModel node, IHardwareDefineViewModel viewModel)
        {
            // Use reflection to set the private DataViewModel property
            var property = typeof(HardwareTreeNodeViewModel).GetProperty("DataViewModel");
            if (property != null)
            {
                // Access the backing field through reflection
                var field = typeof(HardwareTreeNodeViewModel).GetField("_dataViewModel",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null)
                {
                    // Unsubscribe old event handlers
                    var oldVm = node.DataViewModel;
                    if (oldVm != null)
                    {
                        oldVm.PropertyChanged -= null; // Will be handled by setter
                    }

                    // Set new value (this triggers the property setter logic)
                    field.SetValue(node, null); // Reset first
                }
            }
            
            // Recreate node with new ViewModel - simplest approach
            // Note: For legacy support, we just accept the default created VMs
            // The legacy data restoration is best-effort
        }

        /// <summary>
        /// Executes workflow completion request from any DeviceViewModel.
        /// </summary>
        private async void OnWorkflowCompletedRequest(object sender, EventArgs e)
        {
            await CompleteWorkflowAsync();
        }

        /// <summary>
        /// Checks if all required fields in all tree nodes are filled.
        /// </summary>
        private bool CheckAllNodesRequiredFieldsFilled()
        {
            foreach (var node in GetAllNodes())
            {
                if (node.DataViewModel != null && !node.DataViewModel.CanProceedToNext)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Returns all nodes in the tree via depth-first traversal.
        /// </summary>
        private IEnumerable<HardwareTreeNodeViewModel> GetAllNodes()
        {
            foreach (var root in TreeRootNodes)
            {
                foreach (var node in GetNodesRecursive(root))
                {
                    yield return node;
                }
            }
        }

        /// <summary>
        /// Recursively traverses tree nodes.
        /// </summary>
        private IEnumerable<HardwareTreeNodeViewModel> GetNodesRecursive(HardwareTreeNodeViewModel node)
        {
            yield return node;
            foreach (var child in node.Children)
            {
                foreach (var descendant in GetNodesRecursive(child))
                {
                    yield return descendant;
                }
            }
        }

        /// <summary>
        /// Completes the workflow by updating the workflow state to Defined in workflows.json.
        /// Updates the existing workflow entry with ComponentState.Defined.
        /// </summary>
        private async Task CompleteWorkflowAsync()
        {
            var workflowRepo = ServiceLocator.GetService<ITypedDataRepository<IncompleteWorkflowDataStore>>();
            var workflowData = await workflowRepo.LoadAsync();

            // Create or update WorkflowSessionDto2 with Defined state
            var sessionDto = ToWorkflowSessionDto2();
            sessionDto.State = ComponentState.Defined;
            
            var existingIndex = workflowData.WorkflowSessions.FindIndex(s => s.WorkflowId == WorkflowId);

            if (existingIndex >= 0)
                workflowData.WorkflowSessions[existingIndex] = sessionDto;
            else
                workflowData.WorkflowSessions.Add(sessionDto);

            await workflowRepo.SaveAsync(workflowData);
        }

        /// <summary>
        /// Recursively saves a tree node and all its children.
        /// </summary>
        /// <param name="node">The node to save.</param>
        /// <param name="parentId">The database ID of the parent node (null for root).</param>
        /// <param name="dataStore">The data store to save to.</param>
        /// <param name="timestamp">Timestamp for CreatedAt/UpdatedAt.</param>
        /// <returns>The generated ID for this node.</returns>
        private string SaveTreeNodeRecursively(
            HardwareTreeNodeViewModel node,
            string parentId,
            UploadedHardwareDataStore dataStore,
            DateTime timestamp)
        {
            string nodeId = Guid.NewGuid().ToString();

            switch (node.HardwareLayer)
            {
                case HardwareLayer.Equipment:
                    if (node.DataViewModel is EquipmentDefineViewModel equipmentVm)
                    {
                        var equipmentDto = equipmentVm.ToDto();
                        equipmentDto.Id = nodeId;
                        equipmentDto.State = ComponentState.Defined;
                        equipmentDto.CreatedAt = timestamp;
                        equipmentDto.UpdatedAt = timestamp;
                        dataStore.Equipments.Add(equipmentDto);
                    }
                    break;

                case HardwareLayer.System:
                    if (node.DataViewModel is SystemDefineViewModel systemVm)
                    {
                        var systemDto = systemVm.ToDto();
                        systemDto.Id = nodeId;
                        systemDto.EquipmentId = parentId; // Link to parent Equipment
                        systemDto.State = ComponentState.Defined;
                        systemDto.CreatedAt = timestamp;
                        systemDto.UpdatedAt = timestamp;
                        dataStore.Systems.Add(systemDto);
                    }
                    break;

                case HardwareLayer.Unit:
                    if (node.DataViewModel is UnitDefineViewModel unitVm)
                    {
                        var unitDto = unitVm.ToDto();
                        unitDto.Id = nodeId;
                        unitDto.SystemId = parentId; // Link to parent System
                        unitDto.State = ComponentState.Defined;
                        unitDto.CreatedAt = timestamp;
                        unitDto.UpdatedAt = timestamp;
                        dataStore.Units.Add(unitDto);
                    }
                    break;

                case HardwareLayer.Device:
                    if (node.DataViewModel is DeviceDefineViewModel deviceVm)
                    {
                        var deviceDto = deviceVm.ToDto();
                        deviceDto.Id = nodeId;
                        deviceDto.UnitId = parentId; // Link to parent Unit
                        deviceDto.State = ComponentState.Defined;
                        deviceDto.CreatedAt = timestamp;
                        deviceDto.UpdatedAt = timestamp;
                        dataStore.Devices.Add(deviceDto);
                    }
                    break;
            }

            // Recursively save all children
            foreach (var child in node.Children)
            {
                SaveTreeNodeRecursively(child, nodeId, dataStore, timestamp);
            }

            return nodeId;
        }

        /// <summary>
        /// Sets up workflow completion callbacks for the currently selected DeviceViewModel.
        /// </summary>
        private void SetupCurrentDeviceViewModelCallbacks()
        {
            if (SelectedTreeNode?.HardwareLayer == HardwareLayer.Device &&
                SelectedTreeNode?.DataViewModel is DeviceDefineViewModel deviceVm)
            {
                deviceVm.SetAllStepsRequiredFieldsFilledCheck(() => AllStepsRequiredFieldsFilled);
            }
        }

        /// <summary>
        /// Sets up workflow completion callbacks for all Device nodes in the tree.
        /// </summary>
        private void SetupAllDeviceViewModelCallbacks()
        {
            foreach (var node in GetAllNodes())
            {
                // Subscribe to PropertyChanged for data change detection on all nodes
                if (node.DataViewModel != null)
                {
                    node.DataViewModel.PropertyChanged -= OnNodeDataPropertyChanged;
                    node.DataViewModel.PropertyChanged += OnNodeDataPropertyChanged;
                }

                if (node.HardwareLayer == HardwareLayer.Device &&
                    node.DataViewModel is DeviceDefineViewModel deviceVm)
                {
                    deviceVm.SetAllStepsRequiredFieldsFilledCheck(() => AllStepsRequiredFieldsFilled);
                    deviceVm.WorkflowCompletedRequest -= OnWorkflowCompletedRequest;
                    deviceVm.WorkflowCompletedRequest += OnWorkflowCompletedRequest;
                }
            }
        }

        /// <summary>
        /// Handles PropertyChanged events from node ViewModels to detect data changes.
        /// </summary>
        private void OnNodeDataPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // Skip properties that don't represent actual data changes
            if (e.PropertyName == nameof(IHardwareDefineViewModel.IsEditable) ||
                e.PropertyName == nameof(IHardwareDefineViewModel.CanProceedToNext) ||
                e.PropertyName == nameof(IHardwareDefineViewModel.FilledFieldCount) ||
                e.PropertyName == nameof(IHardwareDefineViewModel.TotalFieldCount))
            {
                return;
            }

            OnDataChanged();
        }

        /// <summary>
        /// Updates the IsEditable property on all tree node ViewModels.
        /// </summary>
        private void UpdateAllTreeNodeEditability(bool isEditable)
        {
            foreach (var node in GetAllNodes())
            {
                if (node.DataViewModel != null)
                {
                    node.DataViewModel.IsEditable = isEditable;
                }
            }
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
            MarkDirty(); // Mark for autosave when navigating
        }

        private bool CanExecuteNavigateToStep(WorkflowStepViewModel step)
        {
            if (step == null) return false;
            return step.CanNavigateTo;
        }

        /// <summary>
        /// Updates the current step index based on the selected tree node's hardware layer.
        /// </summary>
        private void UpdateCurrentStepFromTreeNode()
        {
            if (SelectedTreeNode == null)
                return;

            var stepName = SelectedTreeNode.HardwareLayer.ToString();
            var step = WorkflowSteps.FirstOrDefault(s => s.StepName == stepName);
            if (step != null)
            {
                var index = WorkflowSteps.IndexOf(step);
                if (index >= 0 && index != _currentStepIndex)
                {
                    _currentStepIndex = index;
                    OnPropertyChanged(nameof(CurrentStepIndex));
                    OnPropertyChanged(nameof(CurrentStep));
                    OnPropertyChanged(nameof(IsFirstStep));
                    OnPropertyChanged(nameof(IsLastStep));
                    OnPropertyChanged(nameof(CanGoToNext));
                    UpdateStepStates();
                }
            }
        }

        private void UpdateStepStates()
        {
            for (int i = 0; i < WorkflowSteps.Count; i++)
            {
                var step = WorkflowSteps[i];
                step.IsActive = i == CurrentStepIndex;

                // IsCompleted is based on whether required fields are filled in the selected node
                step.IsCompleted = SelectedTreeNode?.DataViewModel?.CanProceedToNext == true;
            }
        }

        private void InitializeStepFieldCounts()
        {
            foreach (var step in WorkflowSteps)
            {
                // Initialize with selected node's field counts
                if (SelectedTreeNode?.DataViewModel != null)
                {
                    step.FilledFieldCount = SelectedTreeNode.DataViewModel.FilledFieldCount;
                    step.TotalFieldCount = SelectedTreeNode.DataViewModel.TotalFieldCount;
                }
            }
        }

        private void ExecuteAddChild(HardwareTreeNodeViewModel parentNode)
        {
            if (parentNode == null || !parentNode.CanHaveChildren)
                return;

            var newChild = parentNode.AddChildWithFullHierarchy();
            if (newChild != null)
            {
                // Set up callbacks for any new Device nodes in the hierarchy
                SetupDeviceCallbacksForNode(newChild);

                // Set editability for new nodes
                SetEditabilityForNode(newChild, !IsReadOnly);

                // Select the new node
                ExecuteSelectTreeNode(newChild);

                // Mark data as dirty for autosave
                MarkDirty();
            }
        }

        /// <summary>
        /// Sets up Device callbacks for a node and its descendants.
        /// </summary>
        private void SetupDeviceCallbacksForNode(HardwareTreeNodeViewModel node)
        {
            if (node.HardwareLayer == HardwareLayer.Device &&
                node.DataViewModel is DeviceDefineViewModel deviceVm)
            {
                deviceVm.SetAllStepsRequiredFieldsFilledCheck(() => AllStepsRequiredFieldsFilled);
                deviceVm.WorkflowCompletedRequest += OnWorkflowCompletedRequest;
            }

            foreach (var child in node.Children)
            {
                SetupDeviceCallbacksForNode(child);
            }
        }

        /// <summary>
        /// Sets editability for a node and its descendants.
        /// </summary>
        private void SetEditabilityForNode(HardwareTreeNodeViewModel node, bool isEditable)
        {
            if (node.DataViewModel != null)
            {
                node.DataViewModel.IsEditable = isEditable;
            }

            foreach (var child in node.Children)
            {
                SetEditabilityForNode(child, isEditable);
            }
        }

        private bool CanExecuteAddChild(HardwareTreeNodeViewModel parentNode)
        {
            return parentNode != null && parentNode.CanHaveChildren && !IsReadOnly;
        }

        private void ExecuteSelectTreeNode(HardwareTreeNodeViewModel node)
        {
            if (node == null)
                return;

            // Deselect previous node
            if (SelectedTreeNode != null)
            {
                SelectedTreeNode.IsSelected = false;
            }

            // Select new node
            node.IsSelected = true;
            SelectedTreeNode = node;
        }
    }
}