using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;
using EquipmentDesigner.Services;
using EquipmentDesigner.Views.HardwareDefineWorkflow;

namespace EquipmentDesigner.Views.Dashboard
{
    /// <summary>
    /// ViewModel for the Dashboard view with placeholder data.
    /// </summary>
    public class DashboardViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public DashboardViewModel()
        {
            // Initialize navigation commands
            CreateEquipmentCommand = new RelayCommand(_ => NavigationService.Instance.NavigateToHardwareDefineWorkflow(WorkflowStartType.Equipment));
            CreateSystemCommand = new RelayCommand(_ => NavigationService.Instance.NavigateToHardwareDefineWorkflow(WorkflowStartType.System));
            CreateUnitCommand = new RelayCommand(_ => NavigationService.Instance.NavigateToHardwareDefineWorkflow(WorkflowStartType.Unit));
            CreateDeviceCommand = new RelayCommand(_ => NavigationService.Instance.NavigateToHardwareDefineWorkflow(WorkflowStartType.Device));
            
            InitializePlaceholderData();
        }

        #region Navigation Commands

        public ICommand CreateEquipmentCommand { get; }
        public ICommand CreateSystemCommand { get; }
        public ICommand CreateUnitCommand { get; }
        public ICommand CreateDeviceCommand { get; }

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

        #endregion

        private void InitializePlaceholderData()
        {
            // Placeholder incomplete workflows matching Figma design
            var today = DateTime.Now.ToString("yyyy. M. d.");

            IncompleteWorkflows.Add(new WorkflowItem { StartedFrom = "Equipment", CurrentStep = "Equipment", Date = today });
            IncompleteWorkflows.Add(new WorkflowItem { StartedFrom = "System", CurrentStep = "System", Date = today });
            IncompleteWorkflows.Add(new WorkflowItem { StartedFrom = "Unit", CurrentStep = "Unit", Date = today });
            IncompleteWorkflows.Add(new WorkflowItem { StartedFrom = "Unit", CurrentStep = "Unit", Date = today });
            IncompleteWorkflows.Add(new WorkflowItem { StartedFrom = "Device", CurrentStep = "Device", Date = today });
            IncompleteWorkflows.Add(new WorkflowItem { StartedFrom = "Device", CurrentStep = "Device", Date = today });
            IncompleteWorkflows.Add(new WorkflowItem { StartedFrom = "Equipment", CurrentStep = "Equipment", Date = today });
            IncompleteWorkflows.Add(new WorkflowItem { StartedFrom = "Equipment", CurrentStep = "Unit", Date = today });
            IncompleteWorkflows.Add(new WorkflowItem { StartedFrom = "Equipment", CurrentStep = "Equipment", Date = today });
            IncompleteWorkflows.Add(new WorkflowItem { StartedFrom = "Equipment", CurrentStep = "Unit", Date = today });

            // Placeholder device matching Figma design (showing 1 device)
            Devices.Add(new ComponentItem
            {
                Name = "Gripper",
                Description = "0 commands, 0 IO",
                Status = "defined",
                StatusBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF7ED")),
                StatusBorder = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFEDD5")),
                StatusForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F59E0B"))
            });
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
        public string StartedFrom { get; set; }
        public string CurrentStep { get; set; }
        public string Date { get; set; }
    }

    /// <summary>
    /// Model for component items (Equipment, System, Unit, Device).
    /// </summary>
    public class ComponentItem
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public Brush StatusBackground { get; set; }
        public Brush StatusBorder { get; set; }
        public Brush StatusForeground { get; set; }
    }
}