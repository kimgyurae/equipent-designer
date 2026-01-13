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
    public class HardwareDefineWorkflowViewModel : ViewModelBase
    {
        private int _currentStepIndex;
        private bool _isReadOnly;
        private string _loadedComponentId;
        private HardwareType? _loadedHardwareType;
        private HardwareTreeNodeViewModel _selectedTreeNode;
        private bool _isWorkflowCompleted;
        private bool _hasDataChangedSinceCompletion;

        // Autosave fields
        private DispatcherTimer _autosaveTimer;
        private CancellationTokenSource _debounceCts;
        private bool _isDirty;
        private bool _isAutosaveEnabled;
        private static readonly TimeSpan DefaultAutosaveInterval = TimeSpan.FromSeconds(15);
        private static readonly TimeSpan DefaultDebounceDelay = TimeSpan.FromSeconds(2);
        private static readonly TimeSpan MaxWaitTime = TimeSpan.FromSeconds(10);
        private bool _isAutosaveIndicatorVisible;
        private DispatcherTimer _autosaveIndicatorTimer;
        private DateTime? _firstDirtyTime;

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
        /// Gets or sets whether the autosave indicator is visible.
        /// </summary>
        public bool IsAutosaveIndicatorVisible
        {
            get => _isAutosaveIndicatorVisible;
            private set => SetProperty(ref _isAutosaveIndicatorVisible, value);
        }

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

        private void ExecuteEnableEdit()
        {
            // Get MainWindow for backdrop control (null-safe for test environment)
            var mainWindow = System.Windows.Application.Current?.MainWindow as MainWindow;

            // If no MainWindow (test environment), directly enable edit mode
            if (mainWindow == null)
            {
                EnableEditModeDirectly();
                return;
            }

            // Show backdrop
            mainWindow.ShowBackdrop();

            try
            {
                // Show edit mode selection dialog
                var dialog = new EditModeSelectionDialog
                {
                    Owner = mainWindow,
                    CurrentVersion = TopLevelComponentVersion
                };

                if (dialog.ShowDialog() == true && dialog.IsConfirmed)
                {
                    // Store the selected mode for future use
                    var selectedMode = dialog.SelectedMode;

                    // Execute edit mode based on selection
                    switch (selectedMode)
                    {
                        case EditModeSelection.DirectEdit:
                            EnableEditModeDirectly();
                            break;

                        case EditModeSelection.CreateNewVersion:
                            EnableEditModeWithNewVersionAsync(dialog.NewVersion);
                            break;

                        case EditModeSelection.CreateNewHardware:
                            EnableEditModeWithNewHardwareAsync();
                            break;
                    }
                }
            }
            finally
            {
                // Hide backdrop
                mainWindow?.HideBackdrop();
            }
        }

        /// <summary>
        /// Enables edit mode directly on the current data.
        /// </summary>
        private void EnableEditModeDirectly()
        {
            IsReadOnly = false;
            
            // If a component was loaded for viewing, change its state from Uploaded/Validated to Defined
            if (!string.IsNullOrEmpty(LoadedComponentId) && LoadedHardwareType.HasValue)
            {
                ChangeComponentStateToDefinedAsync();
            }
        }

        /// <summary>
        /// Changes the loaded component's state from Uploaded/Validated to Defined in the repository.
        /// </summary>
        private async void ChangeComponentStateToDefinedAsync()
        {
            try
            {
                var apiService = ServiceLocator.GetService<IHardwareApiService>();
                var response = await apiService.UpdateSessionStateAsync(LoadedComponentId, ComponentState.Draft);

                if (!response.Success)
                {
                    // Silently fail - non-critical operation
                }
            }
            catch
            {
                // Silently fail - non-critical operation
            }
        }

        /// <summary>
        /// Creates a new version of the current workflow with updated version number.
        /// Original data remains unchanged. HardwareKey is preserved.
        /// </summary>
        /// <param name="newVersion">The new version string to apply.</param>
        private async void EnableEditModeWithNewVersionAsync(string newVersion)
        {
            try
            {
                // 1. Create HardwareDefinition from current state
                var sessionDto = ToHardwareDefinition();

                // 2. Create a copy with new session ID but same HardwareKey
                var copiedSession = CreateNewVersionSession(sessionDto, newVersion);

                // 3. Save to WorkflowRepository
                var workflowRepo = ServiceLocator.GetService<IWorkflowRepository>();
                var sessions = await workflowRepo.LoadAsync();
                sessions.Add(copiedSession);
                await workflowRepo.SaveAsync(sessions);

                // 4. Navigate to the copied workflow
                NavigationService.Instance.ResumeWorkflow(copiedSession.Id);
            }
            catch (Exception)
            {
                // Show error toast
                ToastService.Instance.ShowError(
                    Strings.Toast_CopyWorkflowFailed_Title,
                    Strings.Toast_CopyWorkflowFailed_Description);
            }
        }

        /// <summary>
        /// Creates a completely new hardware with new GUID and ID from the current workflow.
        /// Original data remains unchanged. No version history is shared.
        /// </summary>
        private async void EnableEditModeWithNewHardwareAsync()
        {
            try
            {
                // 1. Create HardwareDefinition from current state
                var sessionDto = ToHardwareDefinition();

                // 2. Create a copy with new HardwareKey and regenerated IDs
                var copiedSession = CreateNewHardwareSession(sessionDto);

                // 3. Save to WorkflowRepository
                var workflowRepo = ServiceLocator.GetService<IWorkflowRepository>();
                var sessions = await workflowRepo.LoadAsync();
                sessions.Add(copiedSession);
                await workflowRepo.SaveAsync(sessions);

                // 4. Navigate to the copied workflow
                NavigationService.Instance.ResumeWorkflow(copiedSession.Id);
            }
            catch (Exception)
            {
                // Show error toast
                ToastService.Instance.ShowError(
                    Strings.Toast_CopyWorkflowFailed_Title,
                    Strings.Toast_CopyWorkflowFailed_Description);
            }
        }

        /// <summary>
        /// Recursively regenerates all node IDs in a HardwareDefinition tree.
        /// </summary>
        /// <param name="node">The root node to regenerate IDs for.</param>
        public static void RegenerateNodeIds(HardwareDefinition node)
        {
            node.Id = Guid.NewGuid().ToString();
            foreach (var child in node.Children ?? new List<HardwareDefinition>())
            {
                RegenerateNodeIds(child);
            }
        }

        /// <summary>
        /// Applies copy suffix to the node's name.
        /// Uses the existing GenerateCopyName logic from HardwareTreeNodeViewModel.
        /// </summary>
        /// <param name="node">The node to apply copy suffix to.</param>
        public static void ApplyCopySuffixToNode(HardwareDefinition node)
        {
            node.Name = HardwareTreeNodeViewModel.GenerateCopyName(node.Name);
        }

        /// <summary>
        /// Creates a copy of a workflow session with new IDs and copy suffix applied.
        /// The copied session is set to Draft state.
        /// </summary>
        /// <param name="originalSession">The original session to copy.</param>
        /// <returns>A new HardwareDefinition with regenerated IDs and copy suffix.</returns>
        public static HardwareDefinition CreateCopySession(HardwareDefinition originalSession)
        {
            // Create a deep copy
            var copiedSession = DeepCopyHardwareDefinition(originalSession);
            copiedSession.Id = Guid.NewGuid().ToString();
            copiedSession.State = ComponentState.Draft;
            copiedSession.LastModifiedAt = DateTime.Now;

            // Regenerate all node IDs
            RegenerateNodeIds(copiedSession);

            // Apply copy suffix to root node name
            ApplyCopySuffixToNode(copiedSession);

            return copiedSession;
        }

        /// <summary>
        /// Creates a new version of a workflow session with updated version number.
        /// The copied session has a new session ID but preserves the HardwareKey.
        /// </summary>
        /// <param name="originalSession">The original session to copy.</param>
        /// <param name="newVersion">The new version string to apply.</param>
        /// <returns>A new HardwareDefinition with updated version.</returns>
        public static HardwareDefinition CreateNewVersionSession(HardwareDefinition originalSession, string newVersion)
        {
            // Create a deep copy
            var copiedSession = DeepCopyHardwareDefinition(originalSession);
            copiedSession.Id = Guid.NewGuid().ToString();
            copiedSession.State = ComponentState.Draft;
            copiedSession.LastModifiedAt = DateTime.Now;
            copiedSession.Version = newVersion;

            // Regenerate all node IDs but preserve HardwareKey
            RegenerateNodeIds(copiedSession);

            return copiedSession;
        }

        /// <summary>
        /// Creates a completely new hardware session with new GUID and ID.
        /// No version history is shared with the original.
        /// </summary>
        /// <param name="originalSession">The original session to copy.</param>
        /// <returns>A new HardwareDefinition with new HardwareKey and IDs.</returns>
        public static HardwareDefinition CreateNewHardwareSession(HardwareDefinition originalSession)
        {
            // Create a deep copy
            var copiedSession = DeepCopyHardwareDefinition(originalSession);
            copiedSession.Id = Guid.NewGuid().ToString();
            // TODO: Deepcopy Process with new Process ID
            copiedSession.State = ComponentState.Draft;
            copiedSession.LastModifiedAt = DateTime.Now;
            copiedSession.HardwareKey = Guid.NewGuid().ToString();

            // Regenerate all node IDs
            RegenerateNodeIds(copiedSession);

            // Apply copy suffix to root node name
            ApplyCopySuffixToNode(copiedSession);

            return copiedSession;
        }

        /// <summary>
        /// Creates a deep copy of a HardwareDefinition including all children.
        /// </summary>
        private static HardwareDefinition DeepCopyHardwareDefinition(HardwareDefinition original)
        {
            return new HardwareDefinition
            {
                Id = original.Id,
                HardwareKey = original.HardwareKey,
                HardwareType = original.HardwareType,
                Version = original.Version,
                State = original.State,
                Name = original.Name,
                DisplayName = original.DisplayName,
                Description = original.Description,
                Customer = original.Customer,
                ProcessId = original.ProcessId,
                ProcessInfo = original.ProcessInfo,
                EquipmentType = original.EquipmentType,
                DeviceType = original.DeviceType,
                ImplementationInstructions = original.ImplementationInstructions?.ToList() ?? new List<string>(),
                Commands = original.Commands?.Select(CopyCommandDto).ToList() ?? new List<CommandDto>(),
                IoInfo = original.IoInfo?.Select(CopyIoInfoDto).ToList() ?? new List<IoInfoDto>(),
                AttachedDocumentsIds = original.AttachedDocumentsIds?.ToList() ?? new List<string>(),
                ProgramRoot = original.ProgramRoot,
                CreatedAt = original.CreatedAt,
                UpdatedAt = original.UpdatedAt,
                LastModifiedAt = original.LastModifiedAt,
                Children = original.Children?.Select(DeepCopyHardwareDefinition).ToList() ?? new List<HardwareDefinition>()
            };
        }

        private static CommandDto CopyCommandDto(CommandDto original)
        {
            return new CommandDto
            {
                Name = original.Name,
                Description = original.Description,
                Parameters = original.Parameters?.Select(p => new ParameterDto
                {
                    Name = p.Name,
                    Type = p.Type,
                    Description = p.Description
                }).ToList() ?? new List<ParameterDto>()
            };
        }

        private static IoInfoDto CopyIoInfoDto(IoInfoDto original)
        {
            return new IoInfoDto
            {
                Name = original.Name,
                IoType = original.IoType,
                Address = original.Address,
                Description = original.Description
            };
        }

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
        /// Called when any data in any tree node changes.
        /// Resets the workflow completion state if data is modified after completion.
        /// Marks data as dirty and starts debounce timer for autosave.
        /// </summary>
        public void OnDataChanged()
        {
            if (IsWorkflowCompleted)
            {
                HasDataChangedSinceCompletion = true;
            }
            OnPropertyChanged(nameof(CanCompleteWorkflow));
            OnPropertyChanged(nameof(AllStepsRequiredFieldsFilled));

            // Mark dirty and start debounce for autosave
            MarkDirty();
            RestartDebounceTimer();
        }

        /// <summary>
        /// Saves the current workflow state to repository.
        /// Uses the new WorkflowRepository (IWorkflowRepository)
        /// structure to persist in-progress workflow sessions.
        /// </summary>
        /// <param name="showAutosaveIndicator">Whether to show the autosave indicator after saving. Default is true for autosave, false for explicit saves.</param>
        private async Task SaveWorkflowStateAsync(bool showAutosaveIndicator = true)
        {
            var workflowRepo = ServiceLocator.GetService<IWorkflowRepository>();
            var sessions = await workflowRepo.LoadAsync();

            // Create or update HardwareDefinition
            var sessionDto = ToHardwareDefinition();
            var existingIndex = sessions.FindIndex(s => s.Id == HardwareId);

            if (existingIndex >= 0)
                sessions[existingIndex] = sessionDto;
            else
                sessions.Add(sessionDto);

            await workflowRepo.SaveAsync(sessions);
            _isDirty = false;
            _firstDirtyTime = null; // Reset first dirty time after successful save

            // Show autosave indicator only for autosave
            if (showAutosaveIndicator)
            {
                ShowAutosaveIndicator();
            }
        }

        /// <summary>
        /// Shows the autosave indicator for 2 seconds.
        /// </summary>
        private void ShowAutosaveIndicator()
        {
            // Ensure we're on the UI thread
            var dispatcher = System.Windows.Application.Current?.Dispatcher;
            if (dispatcher == null)
                return;

            if (!dispatcher.CheckAccess())
            {
                dispatcher.BeginInvoke(new Action(ShowAutosaveIndicator));
                return;
            }

            // Stop existing timer if running
            if (_autosaveIndicatorTimer != null)
            {
                _autosaveIndicatorTimer.Stop();
                _autosaveIndicatorTimer.Tick -= OnAutosaveIndicatorTimerTick;
                _autosaveIndicatorTimer = null;
            }

            IsAutosaveIndicatorVisible = true;

            // Start 2-second timer to hide indicator
            _autosaveIndicatorTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2)
            };
            _autosaveIndicatorTimer.Tick += OnAutosaveIndicatorTimerTick;
            _autosaveIndicatorTimer.Start();
        }

        /// <summary>
        /// Handler for autosave indicator timer tick.
        /// Hides the indicator after 2 seconds.
        /// </summary>
        private void OnAutosaveIndicatorTimerTick(object sender, EventArgs e)
        {
            if (_autosaveIndicatorTimer != null)
            {
                _autosaveIndicatorTimer.Stop();
                _autosaveIndicatorTimer.Tick -= OnAutosaveIndicatorTimerTick;
                _autosaveIndicatorTimer = null;
            }

            IsAutosaveIndicatorVisible = false;
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
        /// Disables autosave and stops both autosave and debounce timers.
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

            if (_debounceCts != null)
            {
                _debounceCts.Cancel();
                _debounceCts = null;
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
            
            // Track when the first dirty change occurred
            if (_firstDirtyTime == null)
            {
                _firstDirtyTime = DateTime.Now;
            }
        }

        /// <summary>
        /// Restarts the debounce timer. Saves data 2 seconds after the last change.
        /// If max wait time has been exceeded since first change, saves immediately.
        /// </summary>
        private void RestartDebounceTimer()
        {
            if (IsReadOnly || !_isAutosaveEnabled)
                return;

            // Check if max wait time has been exceeded since first dirty change
            if (_firstDirtyTime.HasValue && 
                DateTime.Now - _firstDirtyTime.Value >= MaxWaitTime)
            {
                // Cancel any pending debounce and save immediately
                if (_debounceCts != null)
                {
                    _debounceCts.Cancel();
                    _debounceCts = null;
                }

                // Save immediately on a background thread
                Task.Run(async () =>
                {
                    if (_isDirty && !IsReadOnly)
                    {
                        await SaveWorkflowStateAsync();
                    }
                });
                return;
            }

            // Normal debounce behavior
            if (_debounceCts != null)
            {
                _debounceCts.Cancel();
                _debounceCts = null;
            }

            _debounceCts = new CancellationTokenSource();
            var token = _debounceCts.Token;

            Task.Delay(DefaultDebounceDelay, token).ContinueWith(async t =>
            {
                if (t.IsCanceled)
                    return;

                if (_isDirty && !IsReadOnly)
                {
                    await SaveWorkflowStateAsync();
                }
            }, token);
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
        /// Converts this ViewModel to a HardwareDefinition for persistence.
        /// Serializes the entire tree structure for multi-instance support.
        /// </summary>
        public HardwareDefinition ToHardwareDefinition()
        {
            // For single root node (most common case)
            var rootNode = TreeRootNodes.FirstOrDefault();
            if (rootNode == null)
            {
                return new HardwareDefinition
                {
                    Id = HardwareId,
                    HardwareType = StartType,
                    HardwareKey = HardwareKey,
                    Version = Version,
                    State = ComponentState.Draft,
                    LastModifiedAt = DateTime.Now,
                    CreatedAt = DateTime.Now
                };
            }

            var hw = SerializeToHardwareDefinition(rootNode);
            hw.Id = HardwareId;
            hw.HardwareKey = HardwareKey;
            hw.Version = Version;
            hw.State = ComponentState.Draft;
            hw.LastModifiedAt = DateTime.Now;
            return hw;
        }

        /// <summary>
        /// Serializes a tree node and its children to HardwareDefinition.
        /// Uses ViewModel's ToHardwareDefinition() method for property mapping.
        /// </summary>
        private HardwareDefinition SerializeToHardwareDefinition(HardwareTreeNodeViewModel node)
        {
            HardwareDefinition hw;

            // Use ViewModel's ToHardwareDefinition() method for property mapping
            if (node.DataViewModel != null)
            {
                hw = node.DataViewModel switch
                {
                    EquipmentDefineViewModel eqVm => eqVm.ToHardwareDefinition(),
                    SystemDefineViewModel sysVm => sysVm.ToHardwareDefinition(),
                    UnitDefineViewModel unitVm => unitVm.ToHardwareDefinition(),
                    DeviceDefineViewModel devVm => devVm.ToHardwareDefinition(),
                    _ => new HardwareDefinition { HardwareType = node.HardwareType }
                };
            }
            else
            {
                hw = new HardwareDefinition { HardwareType = node.HardwareType };
            }

            // Add tree-specific properties
            hw.Id = node.NodeId;
            hw.CreatedAt = DateTime.Now;
            hw.LastModifiedAt = DateTime.Now;
            hw.Children = node.Children.Select(SerializeToHardwareDefinition).ToList();

            return hw;
        }

        /// <summary>
        /// Creates a HardwareDefineWorkflowViewModel from a saved HardwareDefinition.
        /// Uses tree-based format exclusively.
        /// </summary>
        public static HardwareDefineWorkflowViewModel FromHardwareDefinition(HardwareDefinition dto)
        {
            var viewModel = new HardwareDefineWorkflowViewModel(dto.HardwareType, dto.Id, dto.ProcessId, dto.HardwareKey, dto.Version ?? "1.0.0");

            // Rebuild tree from the HardwareDefinition itself (it is the root node)
            viewModel.TreeRootNodes.Clear();
            var rootNode = DeserializeFromHardwareDefinition(dto, null);
            viewModel.TreeRootNodes.Add(rootNode);

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
        /// Deserializes a HardwareDefinition to a HardwareTreeNodeViewModel.
        /// </summary>
        private static HardwareTreeNodeViewModel DeserializeFromHardwareDefinition(HardwareDefinition dto, HardwareTreeNodeViewModel parent)
        {
            IHardwareDefineViewModel dataViewModel = CreateViewModelFromHardwareDefinition(dto);

            var node = new HardwareTreeNodeViewModel(dto.HardwareType, parent, dataViewModel);

            foreach (var childDto in dto.Children ?? new List<HardwareDefinition>())
            {
                var childNode = DeserializeFromHardwareDefinition(childDto, node);
                node.Children.Add(childNode);
            }

            return node;
        }

        /// <summary>
        /// Creates an appropriate IHardwareDefineViewModel from HardwareDefinition data.
        /// Uses ViewModel's FromHardwareDefinition() static factory methods.
        /// </summary>
        private static IHardwareDefineViewModel CreateViewModelFromHardwareDefinition(HardwareDefinition dto)
        {
            return dto.HardwareType switch
            {
                HardwareType.Equipment => EquipmentDefineViewModel.FromHardwareDefinition(dto),
                HardwareType.System => SystemDefineViewModel.FromHardwareDefinition(dto),
                HardwareType.Unit => UnitDefineViewModel.FromHardwareDefinition(dto),
                HardwareType.Device => DeviceDefineViewModel.FromHardwareDefinition(dto),
                _ => null
            };
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
            var workflowRepo = ServiceLocator.GetService<IWorkflowRepository>();
            var sessions = await workflowRepo.LoadAsync();

            // Create or update HardwareDefinition with Defined state
            var sessionDto = ToHardwareDefinition();
            sessionDto.State = ComponentState.Ready;

            var existingIndex = sessions.FindIndex(s => s.Id == HardwareId);

            if (existingIndex >= 0)
                sessions[existingIndex] = sessionDto;
            else
                sessions.Add(sessionDto);

            await workflowRepo.SaveAsync(sessions);
        }

        /// <summary>
        /// Sets up workflow completion callbacks for the currently selected DeviceViewModel.
        /// </summary>
        private void SetupCurrentDeviceViewModelCallbacks()
        {
            if (SelectedTreeNode?.HardwareType == HardwareType.Device &&
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

                if (node.HardwareType == HardwareType.Device &&
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
            if (node.HardwareType == HardwareType.Device &&
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

        #region Delete and Copy Commands

        /// <summary>
        /// Determines if a node can be deleted.
        /// Returns true if node is valid and not in read-only mode.
        /// Actual constraint validation happens in ExecuteDeleteNode.
        /// </summary>
        private bool CanDeleteNode(HardwareTreeNodeViewModel node)
        {
            if (node == null) return false;
            if (IsReadOnly) return false;
            return true;
        }

        /// <summary>
        /// Executes node deletion with minimum child constraint validation.
        /// Shows Toast warning if deletion would violate the constraint.
        /// Shows ConfirmDialog if deletion is allowed.
        /// </summary>
        private void ExecuteDeleteNode(HardwareTreeNodeViewModel node)
        {
            if (node == null) return;

            // Check minimum child constraint
            bool isRootNode = node.Parent == null;
            
            if (isRootNode)
            {
                // Root node: check if it's the only root
                if (TreeRootNodes.Count <= 1)
                {
                    ToastService.Instance.ShowError(
                        Strings.DeleteHardware_CannotDelete_Title,
                        Strings.DeleteHardware_CannotDelete_MinChild);
                    return;
                }
            }
            else
            {
                // Non-root node: check if parent has only one child
                if (node.Parent.Children.Count <= 1)
                {
                    ToastService.Instance.ShowError(
                        Strings.DeleteHardware_CannotDelete_Title,
                        Strings.DeleteHardware_CannotDelete_MinChild);
                    return;
                }
            }

            // Get all descendants for the dialog
            var descendants = node.GetAllDescendants();
            var descendantNames = descendants.Select(d => $"{d.HardwareType}: {d.DisplayName}").ToList();

            // Build description with descendants list
            var description = Strings.DeleteHardware_Description;
            if (descendantNames.Count > 0)
            {
                description += "\n\n" + Strings.DeleteHardware_DescendantsLabel + "\n• " + string.Join("\n• ", descendantNames);
            }

            // Get MainWindow for backdrop control
            var mainWindow = System.Windows.Application.Current?.MainWindow as MainWindow;

            // If no MainWindow (test environment), perform deletion directly
            if (mainWindow == null)
            {
                PerformNodeDeletion(node, isRootNode);
                return;
            }

            // Show backdrop
            mainWindow.ShowBackdrop();

            try
            {
                // Show confirmation dialog
                var dialog = new ConfirmDialog(
                    Strings.DeleteHardware_Title,
                    description,
                    "delete",
                    $"{node.HardwareType}: {node.DisplayName}")
                {
                    Owner = mainWindow,
                    ConfirmText = Strings.DeleteHardware_DeleteButton,
                    CancelText = Strings.Common_Cancel
                };

                if (dialog.ShowDialog() == true && dialog.IsConfirmed)
                {
                    PerformNodeDeletion(node, isRootNode);
                }
            }
            finally
            {
                mainWindow?.HideBackdrop();
            }
        }

        /// <summary>
        /// Performs the actual node deletion from the tree.
        /// </summary>
        private void PerformNodeDeletion(HardwareTreeNodeViewModel node, bool isRootNode)
        {
            // Remove from parent or root
            if (isRootNode)
            {
                TreeRootNodes.Remove(node);
            }
            else
            {
                node.Parent.Children.Remove(node);
            }

            // If deleted node was selected, select another node
            if (SelectedTreeNode == node)
            {
                // Try to select sibling or parent
                HardwareTreeNodeViewModel newSelection = null;
                
                if (isRootNode && TreeRootNodes.Count > 0)
                {
                    newSelection = TreeRootNodes[0];
                }
                else if (!isRootNode && node.Parent != null)
                {
                    newSelection = node.Parent.Children.FirstOrDefault() ?? node.Parent;
                }

                if (newSelection != null)
                {
                    ExecuteSelectTreeNode(newSelection);
                }
                else
                {
                    SelectedTreeNode = null;
                }
            }

            // Mark dirty for autosave
            MarkDirty();
        }

        /// <summary>
        /// Determines if a node can be copied.
        /// </summary>
        private bool CanCopyNode(HardwareTreeNodeViewModel node)
        {
            if (node == null) return false;
            if (IsReadOnly) return false;
            return true;
        }

        /// <summary>
        /// Executes node copy operation.
        /// Creates a deep copy of the node and adds it to the parent's children.
        /// Root nodes cannot be copied as only one top-level component is allowed per session.
        /// </summary>
        private void ExecuteCopyNode(HardwareTreeNodeViewModel node)
        {
            if (node == null) return;

            bool isRootNode = node.Parent == null;

            // Root nodes cannot be copied - only one top-level component allowed per session
            if (isRootNode)
            {
                ToastService.Instance.ShowWarning(
                    Strings.CopyHardware_CannotCopy_Title,
                    Strings.CopyHardware_CannotCopy_RootNode);
                return;
            }

            // Create deep copy with the same parent
            var copiedNode = node.DeepCopy(node.Parent);

            // Add to parent's children
            node.Parent.Children.Add(copiedNode);

            // Setup callbacks for the copied node hierarchy
            SetupDeviceCallbacksForNode(copiedNode);

            // Set editability for the copied node hierarchy
            SetEditabilityForNode(copiedNode, !IsReadOnly);

            // Subscribe to property changes for autosave
            SubscribeToNodePropertyChanges(copiedNode);

            // Select the copied node
            ExecuteSelectTreeNode(copiedNode);

            // Mark dirty for autosave
            MarkDirty();
        }

        /// <summary>
        /// Subscribes to PropertyChanged events for a node and its descendants.
        /// </summary>
        private void SubscribeToNodePropertyChanges(HardwareTreeNodeViewModel node)
        {
            if (node.DataViewModel != null)
            {
                node.DataViewModel.PropertyChanged -= OnNodeDataPropertyChanged;
                node.DataViewModel.PropertyChanged += OnNodeDataPropertyChanged;
            }

            foreach (var child in node.Children)
            {
                SubscribeToNodePropertyChanges(child);
            }
        }

        #endregion
    }
}