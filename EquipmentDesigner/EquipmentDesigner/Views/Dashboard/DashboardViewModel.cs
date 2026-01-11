using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.IO;
using EquipmentDesigner.Models;
using EquipmentDesigner.Services;
using EquipmentDesigner.Controls;
using EquipmentDesigner.Resources;

namespace EquipmentDesigner.ViewModels
{
    /// <summary>
    /// ViewModel for the Dashboard view.
    /// </summary>
    public class DashboardViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool _isAdminModeVisible;

        /// <summary>
        /// Gets or sets whether Admin mode panel is visible.
        /// </summary>
        public bool IsAdminModeVisible
        {
            get => _isAdminModeVisible;
            set
            {
                if (_isAdminModeVisible != value)
                {
                    _isAdminModeVisible = value;
                    OnPropertyChanged();
                }
            }
        }

        public DashboardViewModel()
        {
            // Initialize navigation commands
            CreateEquipmentCommand = new RelayCommand(_ => NavigationService.Instance.NavigateToHardwareDefineWorkflow(HardwareLayer.Equipment));
            CreateSystemCommand = new RelayCommand(_ => NavigationService.Instance.NavigateToHardwareDefineWorkflow(HardwareLayer.System));
            CreateUnitCommand = new RelayCommand(_ => NavigationService.Instance.NavigateToHardwareDefineWorkflow(HardwareLayer.Unit));
            CreateDeviceCommand = new RelayCommand(_ => NavigationService.Instance.NavigateToHardwareDefineWorkflow(HardwareLayer.Device));

            // Navigation commands for new views
            NavigateToCreateNewComponentCommand = new RelayCommand(_ => NavigationService.Instance.NavigateToCreateNewComponent());
            NavigateToComponentListCommand = new RelayCommand(_ => NavigationService.Instance.NavigateToComponentList());

            // Resume workflow command
            ResumeWorkflowCommand = new RelayCommand<WorkflowItem>(ExecuteResumeWorkflow);

            // Delete workflow command
            DeleteWorkflowCommand = new RelayCommand<WorkflowItem>(ExecuteDeleteWorkflow);

            // View component command (opens in read-only mode)
            ViewComponentCommand = new RelayCommand<ComponentItem>(ExecuteViewComponent);

            // Admin mode commands
            DeleteAllIncompleteWorkflowsCommand = new RelayCommand(_ => ExecuteDeleteAllIncompleteWorkflows(), _ => HasIncompleteWorkflows);
            ResetAllDataCommand = new RelayCommand(_ => ExecuteResetAllData());

            // Load data from repository
            LoadIncompleteWorkflowsAsync();
            LoadComponentsAsync();
        }

        #region Navigation Commands

        public ICommand CreateEquipmentCommand { get; }
        public ICommand CreateSystemCommand { get; }
        public ICommand CreateUnitCommand { get; }
        public ICommand CreateDeviceCommand { get; }

        /// <summary>
        /// Command to navigate to the Create New Component view.
        /// </summary>
        public ICommand NavigateToCreateNewComponentCommand { get; }

        /// <summary>
        /// Command to navigate to the Component List view.
        /// </summary>
        public ICommand NavigateToComponentListCommand { get; }

        /// <summary>
        /// Command to resume an incomplete workflow.
        /// </summary>
        public ICommand ResumeWorkflowCommand { get; }

        /// <summary>
        /// Command to delete an incomplete workflow.
        /// </summary>
        public ICommand DeleteWorkflowCommand { get; }

        /// <summary>
        /// Command to view a component in read-only mode.
        /// </summary>
        public ICommand ViewComponentCommand { get; }

        /// <summary>
        /// Command to delete all incomplete workflows (Admin mode).
        /// </summary>
        public ICommand DeleteAllIncompleteWorkflowsCommand { get; }

        /// <summary>
        /// Command to reset all data including workflows and uploaded hardwares (Admin mode).
        /// </summary>
        public ICommand ResetAllDataCommand { get; }

        #endregion

        #region Collections

        public ObservableCollection<WorkflowItem> IncompleteWorkflows { get; } = new ObservableCollection<WorkflowItem>();
        public ObservableCollection<ComponentItem> Equipments { get; } = new ObservableCollection<ComponentItem>();
        public ObservableCollection<ComponentItem> Systems { get; } = new ObservableCollection<ComponentItem>();
        public ObservableCollection<ComponentItem> Units { get; } = new ObservableCollection<ComponentItem>();
        public ObservableCollection<ComponentItem> Devices { get; } = new ObservableCollection<ComponentItem>();

        #endregion

        #region Counts

        public int EquipmentsCount => Equipments.Count;
        public int SystemsCount => Systems.Count;
        public int UnitsCount => Units.Count;
        public int DevicesCount => Devices.Count;

        #endregion

        #region Empty State Visibility

        public bool HasNoEquipments => Equipments.Count == 0;
        public bool HasNoSystems => Systems.Count == 0;
        public bool HasNoUnits => Units.Count == 0;
        public bool HasNoDevices => Devices.Count == 0;

        /// <summary>
        /// Returns true if there are incomplete workflows to display.
        /// </summary>
        public bool HasIncompleteWorkflows => IncompleteWorkflows.Count > 0;

        /// <summary>
        /// Returns the count of incomplete workflows.
        /// </summary>
        public int IncompleteWorkflowsCount => IncompleteWorkflows.Count;

        /// <summary>
        /// Returns the message describing incomplete workflows count.
        /// </summary>
        public string IncompleteWorkflowsMessage => 
            $"You have {IncompleteWorkflowsCount} incomplete workflow{(IncompleteWorkflowsCount == 1 ? "" : "s")}. Click to continue where you left off.";

        #endregion

        /// <summary>
        /// Toggles Admin mode visibility.
        /// </summary>
        public void ToggleAdminMode()
        {
            IsAdminModeVisible = !IsAdminModeVisible;
        }

        /// <summary>
        /// Loads incomplete workflows from the repository.
        /// </summary>
        private async void LoadIncompleteWorkflowsAsync()
        {
            try
            {
                var repository = ServiceLocator.GetService<IWorkflowRepository>();
                var dataStore = await repository.LoadAsync();

                IncompleteWorkflows.Clear();

                if (dataStore?.WorkflowSessions != null)
                {
                    foreach (var session in dataStore.WorkflowSessions)
                    {
                        IncompleteWorkflows.Add(new WorkflowItem
                        {
                            WorkflowId = session.Id,
                            StartedFrom = session.HardwareType.ToString(),
                            ComponentState = session.State,
                            Date = session.LastModifiedAt.ToString("yyyy. M. d.")
                        });
                    }
                }

                OnPropertyChanged(nameof(HasIncompleteWorkflows));
                OnPropertyChanged(nameof(IncompleteWorkflowsCount));
                OnPropertyChanged(nameof(IncompleteWorkflowsMessage));
            }
            catch
            {
                // Silently fail - dashboard will show empty state
            }
        }

        /// <summary>
        /// Loads components from the unified repository.
        /// Filters by Uploaded or Validated state and routes to cards based on StartType.
        /// </summary>
        private async void LoadComponentsAsync()
        {
            try
            {
                var apiService = ServiceLocator.GetService<IHardwareApiService>();
                var response = await apiService.GetSessionsByStateAsync(
                    ComponentState.Ready, ComponentState.Uploaded, ComponentState.Validated);

                // Clear existing collections
                Equipments.Clear();
                Systems.Clear();
                Units.Clear();
                Devices.Clear();

                if (!response.Success || response.Data == null) return;

                // Filter and load from WorkflowSessions based on StartType
                foreach (var session in response.Data)
                {
                    // Extract component info from root tree node
                    var rootNode = session.TreeNodes?.FirstOrDefault();
                    if (rootNode == null) continue;

                    var (name, description, version, equipmentType, hardwareKey) = ExtractComponentInfo(rootNode);
                    var componentItem = CreateComponentItem(
                        session.Id,  // Use WorkflowId as ID for navigation
                        name ?? "Unnamed",
                        description ?? "",
                        version,
                        session.State,
                        session.HardwareType,
                        equipmentType,
                        hardwareKey);

                    // Add to correct collection based on StartType
                    switch (session.HardwareType)
                    {
                        case HardwareLayer.Equipment:
                            Equipments.Add(componentItem);
                            break;
                        case HardwareLayer.System:
                            Systems.Add(componentItem);
                            break;
                        case HardwareLayer.Unit:
                            Units.Add(componentItem);
                            break;
                        case HardwareLayer.Device:
                            Devices.Add(componentItem);
                            break;
                    }
                }

                // Raise property changed for counts and empty state
                OnPropertyChanged(nameof(EquipmentsCount));
                OnPropertyChanged(nameof(SystemsCount));
                OnPropertyChanged(nameof(UnitsCount));
                OnPropertyChanged(nameof(DevicesCount));
                OnPropertyChanged(nameof(HasNoEquipments));
                OnPropertyChanged(nameof(HasNoSystems));
                OnPropertyChanged(nameof(HasNoUnits));
                OnPropertyChanged(nameof(HasNoDevices));
            }
            catch
            {
                // Silently fail - dashboard will show empty state
            }
        }

        /// <summary>
        /// Extracts name and description from a tree node based on its hardware layer.
        /// </summary>
        private (string name, string description, string version, string equipmentType, string hardwareKey) ExtractComponentInfo(TreeNodeDataDto node)
        {
            return node.HardwareLayer switch
            {
                HardwareLayer.Equipment => (node.EquipmentData?.Name, node.EquipmentData?.Description, node.EquipmentData?.Version, node.EquipmentData?.EquipmentType, node.EquipmentData?.HardwareKey ?? node.EquipmentData?.Name),
                HardwareLayer.System => (node.SystemData?.Name, node.SystemData?.Description, node.SystemData?.Version, null, node.SystemData?.HardwareKey ?? node.SystemData?.Name),
                HardwareLayer.Unit => (node.UnitData?.Name, node.UnitData?.Description, node.UnitData?.Version, null, node.UnitData?.HardwareKey ?? node.UnitData?.Name),
                HardwareLayer.Device => (node.DeviceData?.Name, node.DeviceData?.Description, node.DeviceData?.Version, null, node.DeviceData?.HardwareKey ?? node.DeviceData?.Name),
                _ => (null, null, null, null, null)
            };
        }

        /// <summary>
        /// Creates a ComponentItem with the given state.
        /// </summary>
        private ComponentItem CreateComponentItem(string id, string name, string description, string version, ComponentState state, HardwareLayer hardwareLayer, string equipmentType, string hardwareKey)
        {
            return new ComponentItem
            {
                Id = id,
                HardwareLayer = hardwareLayer,
                Name = name,
                Description = description,
                Version = version ?? "v1.0.0",
                Status = state.ToString(),
                ComponentState = state,
                EquipmentType = equipmentType ?? string.Empty,
                HardwareKey = hardwareKey
            };
        }

        /// <summary>
        /// Executes the resume workflow command.
        /// </summary>
        private void ExecuteResumeWorkflow(WorkflowItem item)
        {
            if (!string.IsNullOrEmpty(item?.WorkflowId))
            {
                NavigationService.Instance.ResumeWorkflow(item.WorkflowId);
            }
        }

        /// <summary>
        /// Executes the delete workflow command.
        /// Shows confirmation dialog and deletes if confirmed.
        /// </summary>
        private void ExecuteDeleteWorkflow(WorkflowItem item)
        {
            if (item == null || string.IsNullOrEmpty(item.WorkflowId))
                return;

            // Get MainWindow for backdrop control
            var mainWindow = Application.Current.MainWindow as MainWindow;

            // Show backdrop
            mainWindow?.ShowBackdrop();

            try
            {
                // Show delete confirmation dialog
                var dialog = new ConfirmDialog(
                    Strings.DeleteWorkflow_Title,
                    Strings.DeleteWorkflow_Description)
                {
                    Owner = mainWindow,
                    ConfirmText = Strings.DeleteWorkflow_DeleteButton,
                    CancelText = Strings.Common_Cancel
                };

                var result = dialog.ShowDialog();

                if (result == true && dialog.IsConfirmed)
                {
                    // Delete the workflow
                    DeleteWorkflowAsync(item.WorkflowId);
                }
            }
            finally
            {
                // Hide backdrop
                mainWindow?.HideBackdrop();
            }
        }

        /// <summary>
        /// Executes the delete all incomplete workflows command.
        /// Shows confirmation dialog and deletes all if confirmed.
        /// </summary>
        private void ExecuteDeleteAllIncompleteWorkflows()
        {
            if (!HasIncompleteWorkflows)
                return;

            // Get MainWindow for backdrop control
            var mainWindow = Application.Current.MainWindow as MainWindow;

            // Show backdrop
            mainWindow?.ShowBackdrop();

            try
            {
                // Show delete confirmation dialog
                var dialog = new ConfirmDialog(
                    Strings.DeleteWorkflow_Title,
                    Strings.DeleteWorkflow_Description)
                {
                    Owner = mainWindow,
                    ConfirmText = Strings.DeleteWorkflow_DeleteButton,
                    CancelText = Strings.Common_Cancel
                };

                var result = dialog.ShowDialog();

                if (result == true && dialog.IsConfirmed)
                {
                    // Delete all incomplete workflows
                    DeleteAllIncompleteWorkflowsAsync();
                }
            }
            finally
            {
                // Hide backdrop
                mainWindow?.HideBackdrop();
            }
        }

        /// <summary>
        /// Deletes all incomplete workflows from the repository.
        /// </summary>
        private async void DeleteAllIncompleteWorkflowsAsync()
        {
            try
            {
                var repository = ServiceLocator.GetService<IWorkflowRepository>();
                var dataStore = await repository.LoadAsync();

                if (dataStore?.WorkflowSessions != null)
                {
                    // Clear all workflow sessions
                    dataStore.WorkflowSessions.Clear();
                }

                // Save changes
                await repository.SaveAsync(dataStore);

                // Refresh the list
                RefreshAsync();
            }
            catch
            {
                // Silently fail
            }
        }

        /// <summary>
        /// Executes the reset all data command.
        /// Shows confirmation dialog and deletes all stored data if confirmed.
        /// </summary>
        private void ExecuteResetAllData()
        {
            // Get MainWindow for backdrop control
            var mainWindow = Application.Current.MainWindow as MainWindow;

            // Show backdrop
            mainWindow?.ShowBackdrop();

            try
            {
                // Show delete confirmation dialog
                var dialog = new ConfirmDialog(
                    Strings.DeleteWorkflow_Title,
                    Strings.DeleteWorkflow_Description)
                {
                    Owner = mainWindow,
                    ConfirmText = Strings.DeleteWorkflow_DeleteButton,
                    CancelText = Strings.Common_Cancel
                };

                var result = dialog.ShowDialog();

                if (result == true && dialog.IsConfirmed)
                {
                    // Delete all data files
                    DeleteAllDataFiles();
                }
            }
            finally
            {
                // Hide backdrop
                mainWindow?.HideBackdrop();
            }
        }

        /// <summary>
        /// Deletes all data files (workflows.json and uploaded-hardwares.json).
        /// </summary>
        private void DeleteAllDataFiles()
        {
            try
            {
                var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var appFolder = Path.Combine(appData, "EquipmentDesigner");

                // Delete workflows.json
                var workflowsPath = Path.Combine(appFolder, "local", "workflows.json");
                if (File.Exists(workflowsPath))
                {
                    File.Delete(workflowsPath);
                }

                // Delete uploaded-hardwares.json
                var uploadedHardwaresPath = Path.Combine(appFolder, "remote", "uploaded-hardwares.json");
                if (File.Exists(uploadedHardwaresPath))
                {
                    File.Delete(uploadedHardwaresPath);
                }

                // Refresh the dashboard to reflect changes
                RefreshAsync();
            }
            catch
            {
                // Silently fail
            }
        }

        /// <summary>
        /// Executes the view component command.
        /// Opens the component in read-only mode.
        /// </summary>
        private void ExecuteViewComponent(ComponentItem item)
        {
            if (item == null || string.IsNullOrEmpty(item.Id))
                return;

            NavigationService.Instance.ViewComponent(item.Id, item.HardwareLayer);
        }

        /// <summary>
        /// Deletes a workflow from the repository.
        /// </summary>
        private async void DeleteWorkflowAsync(string workflowId)
        {
            try
            {
                var repository = ServiceLocator.GetService<IWorkflowRepository>();
                var dataStore = await repository.LoadAsync();

                // Remove from WorkflowSessions
                if (dataStore?.WorkflowSessions != null)
                {
                    var sessionToRemove = dataStore.WorkflowSessions.FirstOrDefault(s => s.Id == workflowId);
                    if (sessionToRemove != null)
                    {
                        dataStore.WorkflowSessions.Remove(sessionToRemove);
                    }
                }

                // Save changes
                await repository.SaveAsync(dataStore);

                // Refresh the list
                RefreshAsync();
            }
            catch
            {
                // Silently fail
            }
        }

        /// <summary>
        /// Refreshes the dashboard data from repository.
        /// </summary>
        public void RefreshAsync()
        {
            LoadIncompleteWorkflowsAsync();
            LoadComponentsAsync();
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// Model for incomplete workflow items.
    /// </summary>
    public class WorkflowItem
    {
        /// <summary>
        /// Unique identifier for the workflow session.
        /// </summary>
        public string WorkflowId { get; set; }

        public string StartedFrom { get; set; }

        /// <summary>
        /// The component state as enum for proper converter binding.
        /// </summary>
        public ComponentState ComponentState { get; set; }

        /// <summary>
        /// Display text for the component state (for XAML Text binding).
        /// </summary>
        public string ComponentStateDisplayText => ComponentState.ToString();

        public string Date { get; set; }
    }

    /// <summary>
    /// Model for component items (Equipment, System, Unit, Device).
    /// </summary>
    public class ComponentItem
    {
        public string Id { get; set; }
        public HardwareLayer HardwareLayer { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Version { get; set; }
        public string Status { get; set; }
        public ComponentState ComponentState { get; set; }
        public string EquipmentType { get; set; } = string.Empty;

        /// <summary>
        /// 하드웨어 고유 식별 키 - 버전 선택 다이얼로그 호출 시 사용
        /// </summary>
        public string HardwareKey { get; set; }
    }
}