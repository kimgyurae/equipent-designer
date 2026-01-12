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

                    var (name, description) = ExtractComponentInfo(rootNode);
                    var componentItem = CreateComponentItem(
                        session.Id,
                        name ?? $"Unnamed {session.HardwareType}",
                        description ?? "",
                        session.Version,
                        session.State,
                        session.HardwareType,
                        session.HardwareKey);

                    switch (session.HardwareType)
                    {
                        case HardwareType.Equipment:
                            Equipments.Add(componentItem);
                            break;
                        case HardwareType.System:
                            Systems.Add(componentItem);
                            break;
                        case HardwareType.Unit:
                            Units.Add(componentItem);
                            break;
                        case HardwareType.Device:
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

        private (string name, string description) ExtractComponentInfo(TreeNodeDataDto node)
        {
            return node.HardwareType switch
            {
                HardwareType.Equipment => (node.EquipmentData?.Name, node.EquipmentData?.Description),
                HardwareType.System => (node.SystemData?.Name, node.SystemData?.Description),
                HardwareType.Unit => (node.UnitData?.Name, node.UnitData?.Description),
                HardwareType.Device => (node.DeviceData?.Name, node.DeviceData?.Description),
                _ => (null, null)
            };
        }

        private ComponentItem CreateComponentItem(string id, string name, string description, string version, ComponentState state, HardwareType hardwareType, string hardwareKey)
        {
            return new ComponentItem
            {
                Id = id,
                HardwareType = hardwareType,
                Name = name,
                Description = description,
                Version = version ?? "undefined",
                Status = state.ToString(),
                ComponentState = state,
                HardwareKey = hardwareKey
            };
        }

        private void ExecuteViewComponent(ComponentItem item)
        {
            if (item == null || string.IsNullOrEmpty(item.Id))
                return;

            NavigationService.Instance.ViewComponent(item.Id, item.HardwareType);
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