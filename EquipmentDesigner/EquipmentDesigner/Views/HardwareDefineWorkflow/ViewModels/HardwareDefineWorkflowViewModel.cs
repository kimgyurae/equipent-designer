using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using EquipmentDesigner.Models;
using EquipmentDesigner.Services;
using EquipmentDesigner.Controls;
using EquipmentDesigner.Resources;
using EquipmentDesigner.Views;

using MainWindow = EquipmentDesigner.MainWindow;

namespace EquipmentDesigner.ViewModels
{
    /// <summary>
    /// Main ViewModel for the Hardware Define Workflow.
    /// Orchestrates navigation between Equipment, System, Unit, and Device definition steps.
    /// </summary>
    public partial class HardwareDefineWorkflowViewModel : ViewModelBase
    {
        #region Fields

        private int _currentStepIndex;
        private bool _isReadOnly;
        private string _loadedComponentId;
        private HardwareType? _loadedHardwareType;
        private HardwareTreeNodeViewModel _selectedTreeNode;
        private bool _isWorkflowCompleted;
        private bool _hasDataChangedSinceCompletion;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new workflow with a unique ID.
        /// </summary>
        public HardwareDefineWorkflowViewModel(HardwareType startType)
            : this(startType, Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), "1.0.0")
        {
        }

        /// <summary>
        /// Creates a workflow with a specific ID (for resume scenarios).
        /// </summary>
        private HardwareDefineWorkflowViewModel(HardwareType startType, string hardwareId, string processId, string hardwareKey, string version)
        {
            HardwareId = hardwareId;
            ProcessId = processId;
            StartType = startType;
            HardwareKey = hardwareKey;
            Version = version;
            WorkflowSteps = new ObservableCollection<WorkflowStepViewModel>();

            InitializeWorkflowSteps();
            InitializeViewModels();
            InitializeCommands();

            UpdateStepStates();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Unique identifier for this workflow session.
        /// </summary>
        public string HardwareId { get; }

        /// <summary>
        /// Unique identifier for process for this hardwdare
        /// </summary>
        public string ProcessId { get; }

        /// <summary>
        /// The starting type of the workflow.
        /// </summary>
        public HardwareType StartType { get; }

        /// <summary>
        /// Hardware unique identification key - all versions of the same hardware share this key.
        /// If null, the root node's Name is used as default (backward compatibility).
        /// </summary>
        public string HardwareKey { get; private set; }

        /// <summary>
        /// Version information for the top-level hardware component (e.g., 1.0.0).
        /// </summary>
        public string Version { get; private set; }

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
        public HardwareType? LoadedHardwareType
        {
            get => _loadedHardwareType;
            private set => SetProperty(ref _loadedHardwareType, value);
        }

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
                    OnPropertyChanged(nameof(CurrentHardwareType));
                    UpdateCurrentStepFromTreeNode();
                    SetupCurrentDeviceViewModelCallbacks();
                }
            }
        }

        /// <summary>
        /// The hardware layer of the currently selected tree node.
        /// </summary>
        public HardwareType? CurrentHardwareType => SelectedTreeNode?.HardwareType;

        #endregion

        #region Commands

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
        /// Command to delete a tree node.
        /// </summary>
        public ICommand DeleteNodeCommand { get; private set; }

        /// <summary>
        /// Command to copy a tree node.
        /// </summary>
        public ICommand CopyNodeCommand { get; private set; }

        #endregion

        #region Convenience Properties for Tree Node ViewModels

        /// <summary>
        /// Gets the first EquipmentDefineViewModel from the tree.
        /// Convenience property for backward compatibility.
        /// </summary>
        public EquipmentDefineViewModel EquipmentViewModel =>
            GetAllNodes().FirstOrDefault(n => n.HardwareType == HardwareType.Equipment)?.DataViewModel as EquipmentDefineViewModel;

        /// <summary>
        /// Gets the first SystemDefineViewModel from the tree.
        /// Convenience property for backward compatibility.
        /// </summary>
        public SystemDefineViewModel SystemViewModel =>
            GetAllNodes().FirstOrDefault(n => n.HardwareType == HardwareType.System)?.DataViewModel as SystemDefineViewModel;

        /// <summary>
        /// Gets the first UnitDefineViewModel from the tree.
        /// Convenience property for backward compatibility.
        /// </summary>
        public UnitDefineViewModel UnitViewModel =>
            GetAllNodes().FirstOrDefault(n => n.HardwareType == HardwareType.Unit)?.DataViewModel as UnitDefineViewModel;

        /// <summary>
        /// Gets the first DeviceDefineViewModel from the tree.
        /// Convenience property for backward compatibility.
        /// </summary>
        public DeviceDefineViewModel DeviceViewModel =>
            GetAllNodes().FirstOrDefault(n => n.HardwareType == HardwareType.Device)?.DataViewModel as DeviceDefineViewModel;

        /// <summary>
        /// Gets the version of the top-level component (root node).
        /// Returns the Version from the first root node's data ViewModel.
        /// </summary>
        public string TopLevelComponentVersion => TreeRootNodes?.FirstOrDefault()?.DataViewModel?.Version ?? "undefined";

        #endregion

        #region Initialization

        private void InitializeWorkflowSteps()
        {
            int stepNumber = 1;

            if (StartType == HardwareType.Equipment)
            {
                WorkflowSteps.Add(new WorkflowStepViewModel(stepNumber++, "Equipment"));
            }

            if (StartType <= HardwareType.System)
            {
                WorkflowSteps.Add(new WorkflowStepViewModel(stepNumber++, "System"));
            }

            if (StartType <= HardwareType.Unit)
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
            if (StartType == HardwareType.Equipment)
            {
                rootNode = new HardwareTreeNodeViewModel(HardwareType.Equipment);
                TreeRootNodes.Add(rootNode);
                currentParent = rootNode;

                // Add initial System node under Equipment
                var systemNode = currentParent.AddChild();
                currentParent = systemNode;
            }
            else if (StartType == HardwareType.System)
            {
                rootNode = new HardwareTreeNodeViewModel(HardwareType.System);
                TreeRootNodes.Add(rootNode);
                currentParent = rootNode;
            }

            if (StartType <= HardwareType.System && currentParent != null)
            {
                // Add initial Unit node under System
                var unitNode = currentParent.AddChild();
                currentParent = unitNode;
            }
            else if (StartType == HardwareType.Unit)
            {
                rootNode = new HardwareTreeNodeViewModel(HardwareType.Unit);
                TreeRootNodes.Add(rootNode);
                currentParent = rootNode;
            }

            if (StartType <= HardwareType.Unit && currentParent != null)
            {
                // Add initial Device node under Unit
                currentParent.AddChild();
            }
            else if (StartType == HardwareType.Device)
            {
                rootNode = new HardwareTreeNodeViewModel(HardwareType.Device);
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
            DeleteNodeCommand = new RelayCommand<HardwareTreeNodeViewModel>(ExecuteDeleteNode, CanDeleteNode);
            CopyNodeCommand = new RelayCommand<HardwareTreeNodeViewModel>(ExecuteCopyNode, CanCopyNode);
        }

        #endregion

        #region Navigation Commands

        private void ExecuteGoToNextStep()
        {
            if (CurrentStepIndex < WorkflowSteps.Count - 1)
            {
                CurrentStepIndex++;
                SelectTreeNodeForCurrentStep();
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
                SelectTreeNodeForCurrentStep();
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
            // Save workflow state before navigating (explicit save, no indicator)
            await SaveWorkflowStateAsync(showAutosaveIndicator: false);

            // Create session DTO with Ready state for the complete view
            var sessionDto = ToHardwareDefinition();
            sessionDto.State = ComponentState.Ready;

            NavigationService.Instance.NavigateToWorkflowComplete(sessionDto);
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
        /// 1. Creating HardwareDefinition with Uploaded state
        /// 2. Saving to UploadedWorkflowRepository (uploaded-hardwares.json)
        /// 3. Removing from WorkflowRepository (workflows.json)
        /// </summary>
        private async void UploadToServerAsync()
        {
            // Create session DTO with Uploaded state
            var sessionDto = ToHardwareDefinition();
            sessionDto.State = ComponentState.Uploaded;

            // Save to server via API service
            var apiService = ServiceLocator.GetService<IHardwareApiService>();
            var response = await apiService.SaveSessionAsync(sessionDto);

            if (!response.Success)
            {
                ToastService.Instance.ShowError(
                    Strings.Toast_UploadFailed_Title,
                    response.ErrorMessage ?? Strings.Toast_UploadFailed_Description);
                return;
            }

            // Remove workflow from WorkflowRepository after successful upload
            var workflowRepo = ServiceLocator.GetService<IWorkflowRepository>();
            var sessions = await workflowRepo.LoadAsync();

            var session = sessions.FirstOrDefault(s => s.Id == HardwareId);
            if (session != null)
            {
                sessions.Remove(session);
                await workflowRepo.SaveAsync(sessions);
            }

            // Show success toast
            ToastService.Instance.ShowSuccess(
                Strings.Toast_UploadComplete_Title,
                Strings.Toast_UploadComplete_Description);
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

            var stepName = SelectedTreeNode.HardwareType.ToString();
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

        /// <summary>
        /// Selects the tree node matching the current step's hardware layer.
        /// Called after step navigation to sync tree selection.
        /// </summary>
        private void SelectTreeNodeForCurrentStep()
        {
            if (CurrentStepIndex < 0 || CurrentStepIndex >= WorkflowSteps.Count)
                return;

            var targetStepName = CurrentStep.StepName;

            // Parse step name to HardwareType
            if (!Enum.TryParse<HardwareType>(targetStepName, out var targetLayer))
                return;

            // Find first tree node matching the target layer
            var targetNode = GetAllNodes().FirstOrDefault(n => n.HardwareType == targetLayer);

            if (targetNode != null && targetNode != SelectedTreeNode)
            {
                ExecuteSelectTreeNode(targetNode);
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

        #endregion
    }
}
