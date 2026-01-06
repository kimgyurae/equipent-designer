using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using EquipmentDesigner.Models;
using EquipmentDesigner.Models.Dtos;
using EquipmentDesigner.Models.Storage;
using EquipmentDesigner.Services;
using EquipmentDesigner.Services.Storage;
using EquipmentDesigner.Views.HardwareDefineWorkflow;

namespace EquipmentDesigner.Views.Dashboard
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

            // Resume workflow command
            ResumeWorkflowCommand = new RelayCommand<WorkflowItem>(ExecuteResumeWorkflow);

            // Delete workflow command
            DeleteWorkflowCommand = new RelayCommand<WorkflowItem>(ExecuteDeleteWorkflow);

            // View component command (opens in read-only mode)
            ViewComponentCommand = new RelayCommand<ComponentItem>(ExecuteViewComponent);

            // Admin mode commands
            DeleteAllIncompleteWorkflowsCommand = new RelayCommand(_ => ExecuteDeleteAllIncompleteWorkflows(), _ => HasIncompleteWorkflows);

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
                var repository = ServiceLocator.GetService<IDataRepository>();
                var dataStore = await repository.LoadAsync();

                IncompleteWorkflows.Clear();

                if (dataStore?.SessionContext?.IncompleteWorkflows != null)
                {
                    foreach (var info in dataStore.SessionContext.IncompleteWorkflows)
                    {
                        IncompleteWorkflows.Add(new WorkflowItem
                        {
                            WorkflowId = info.WorkflowId,
                            StartedFrom = info.StartType.ToString(),
                            ComponentState = info.State.ToString(),
                            Date = info.LastModifiedAt.ToString("yyyy. M. d.")
                        });;
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
        /// Loads components from the repository and filters by Uploaded or Validated state.
        /// </summary>
        private async void LoadComponentsAsync()
        {
            try
            {
                var repository = ServiceLocator.GetService<IDataRepository>();
                var dataStore = await repository.LoadAsync();

                // Clear existing collections
                Equipments.Clear();
                Systems.Clear();
                Units.Clear();
                Devices.Clear();

                if (dataStore == null) return;

                // Filter and load Equipments (Defined, Uploaded or Validated only)
                if (dataStore.Equipments != null)
                {
                    foreach (var dto in dataStore.Equipments.Where(e =>
                        e.State == ComponentState.Uploaded || e.State == ComponentState.Validated))
                    {
                        Equipments.Add(CreateComponentItem(dto.Id, dto.Name, dto.Description, dto.State, HardwareLayer.Equipment));
                    }
                }

                // Filter and load Systems (Defined, Uploaded or Validated only)
                if (dataStore.Systems != null)
                {
                    foreach (var dto in dataStore.Systems.Where(s =>
                        s.State == ComponentState.Uploaded || s.State == ComponentState.Validated))
                    {
                        Systems.Add(CreateComponentItem(dto.Id, dto.Name, dto.Description, dto.State, HardwareLayer.System));
                    }
                }

                // Filter and load Units (Defined, Uploaded or Validated only)
                if (dataStore.Units != null)
                {
                    foreach (var dto in dataStore.Units.Where(u =>
                        u.State == ComponentState.Uploaded || u.State == ComponentState.Validated))
                    {
                        Units.Add(CreateComponentItem(dto.Id, dto.Name, dto.Description, dto.State, HardwareLayer.Unit));
                    }
                }

                // Filter and load Devices (Defined, Uploaded or Validated only)
                if (dataStore.Devices != null)
                {
                    foreach (var dto in dataStore.Devices.Where(d =>
                        d.State == ComponentState.Uploaded || d.State == ComponentState.Validated))
                    {
                        Devices.Add(CreateComponentItem(dto.Id, dto.Name, dto.Description, dto.State, HardwareLayer.Device));
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
        /// Creates a ComponentItem with appropriate styling based on state.
        /// </summary>
        private ComponentItem CreateComponentItem(string id, string name, string description, ComponentState state, HardwareLayer hardwareLayer)
        {
            Brush statusBackground;
            Brush statusBorder;
            Brush statusForeground;
            string statusText;

            if (state == ComponentState.Validated)
            {
                // Green theme for Validated
                statusText = "Validated";
                statusBackground = new SolidColorBrush(Color.FromRgb(220, 252, 231)); // Light green
                statusBorder = new SolidColorBrush(Color.FromRgb(34, 197, 94));       // Green
                statusForeground = new SolidColorBrush(Color.FromRgb(22, 101, 52));   // Dark green
            }
            else if (state == ComponentState.Defined)
            {
                // Yellow/Amber theme for Defined
                statusText = "Defined";
                statusBackground = new SolidColorBrush(Color.FromRgb(254, 249, 195)); // Light yellow
                statusBorder = new SolidColorBrush(Color.FromRgb(234, 179, 8));       // Amber
                statusForeground = new SolidColorBrush(Color.FromRgb(133, 77, 14));   // Dark amber
            }
            else
            {
                // Blue theme for Uploaded
                statusText = "Uploaded";
                statusBackground = new SolidColorBrush(Color.FromRgb(219, 234, 254)); // Light blue
                statusBorder = new SolidColorBrush(Color.FromRgb(59, 130, 246));      // Blue
                statusForeground = new SolidColorBrush(Color.FromRgb(30, 64, 175));   // Dark blue
            }

            return new ComponentItem
            {
                Id = id,
                HardwareLayer = hardwareLayer,
                Name = name,
                Description = description,
                Status = statusText,
                StatusBackground = statusBackground,
                StatusBorder = statusBorder,
                StatusForeground = statusForeground
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
                var dialog = new DeleteWorkflowDialog
                {
                    Owner = mainWindow
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
                var dialog = new DeleteWorkflowDialog
                {
                    Owner = mainWindow
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
                var repository = ServiceLocator.GetService<IDataRepository>();
                var dataStore = await repository.LoadAsync();

                if (dataStore?.SessionContext?.IncompleteWorkflows != null)
                {
                    // Get all workflow IDs to delete
                    var workflowIds = dataStore.SessionContext.IncompleteWorkflows.Select(i => i.WorkflowId).ToList();

                    // Remove from WorkflowSessions
                    if (dataStore.WorkflowSessions != null)
                    {
                        foreach (var workflowId in workflowIds)
                        {
                            var sessionToRemove = dataStore.WorkflowSessions.FirstOrDefault(s => s.WorkflowId == workflowId);
                            if (sessionToRemove != null)
                            {
                                dataStore.WorkflowSessions.Remove(sessionToRemove);
                            }
                        }
                    }

                    // Clear all incomplete workflows
                    dataStore.SessionContext.IncompleteWorkflows.Clear();
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
                var repository = ServiceLocator.GetService<IDataRepository>();
                var dataStore = await repository.LoadAsync();

                // Remove from WorkflowSessions
                if (dataStore?.WorkflowSessions != null)
                {
                    var sessionToRemove = dataStore.WorkflowSessions.FirstOrDefault(s => s.WorkflowId == workflowId);
                    if (sessionToRemove != null)
                    {
                        dataStore.WorkflowSessions.Remove(sessionToRemove);
                    }
                }

                // Remove from IncompleteWorkflows
                if (dataStore?.SessionContext?.IncompleteWorkflows != null)
                {
                    var infoToRemove = dataStore.SessionContext.IncompleteWorkflows.FirstOrDefault(i => i.WorkflowId == workflowId);
                    if (infoToRemove != null)
                    {
                        dataStore.SessionContext.IncompleteWorkflows.Remove(infoToRemove);
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

        public string ComponentState { get; set; }
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
        public string Status { get; set; }
        public Brush StatusBackground { get; set; }
        public Brush StatusBorder { get; set; }
        public Brush StatusForeground { get; set; }
    }
}