using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using EquipmentDesigner.Controls;
using EquipmentDesigner.Models;
using EquipmentDesigner.Services;

namespace EquipmentDesigner.ViewModels
{
    public class ComponentListViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ComponentListViewModel()
        {
            ViewComponentCommand = new RelayCommand<ComponentItem>(ExecuteViewComponent);
            NavigateBackCommand = new RelayCommand(_ => NavigationService.Instance.NavigateToDashboard());
            LoadComponentsAsync();
        }

        #region Commands

        public ICommand ViewComponentCommand { get; }
        public ICommand NavigateBackCommand { get; }

        #endregion

        #region Collections

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

        private async void LoadComponentsAsync()
        {
            try
            {
                var apiService = ServiceLocator.GetService<IHardwareApiService>();
                var response = await apiService.GetSessionsByStateAsync(
                    ComponentState.Ready, ComponentState.Uploaded, ComponentState.Validated);

                Equipments.Clear();
                Systems.Clear();
                Units.Clear();
                Devices.Clear();

                if (!response.Success || response.Data == null) return;

                foreach (var session in response.Data)
                {
                    var rootNode = session.TreeNodes?.FirstOrDefault();
                    if (rootNode == null) continue;

                    var (name, description, version, equipmentType, hardwareKey) = ExtractComponentInfo(rootNode);
                    var componentItem = CreateComponentItem(
                        session.Id,
                        name ?? "Unnamed",
                        description ?? "",
                        version,
                        session.State,
                        session.HardwareType,
                        equipmentType,
                        hardwareKey);

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
                // Silently fail - view will show empty state
            }
        }

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

        private void ExecuteViewComponent(ComponentItem item)
        {
            if (item == null || string.IsNullOrEmpty(item.Id))
                return;

            NavigationService.Instance.ViewComponent(item.Id, item.HardwareLayer);
        }

        public void RefreshAsync()
        {
            LoadComponentsAsync();
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}