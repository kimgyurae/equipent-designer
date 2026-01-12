using System.Windows.Input;
using EquipmentDesigner.Controls;
using EquipmentDesigner.Models;
using EquipmentDesigner.Services;

namespace EquipmentDesigner.ViewModels
{
    public class CreateNewComponentViewModel
    {
        public CreateNewComponentViewModel()
        {
            CreateEquipmentCommand = new RelayCommand(_ => NavigationService.Instance.NavigateToHardwareDefineWorkflow(HardwareType.Equipment));
            CreateSystemCommand = new RelayCommand(_ => NavigationService.Instance.NavigateToHardwareDefineWorkflow(HardwareType.System));
            CreateUnitCommand = new RelayCommand(_ => NavigationService.Instance.NavigateToHardwareDefineWorkflow(HardwareType.Unit));
            CreateDeviceCommand = new RelayCommand(_ => NavigationService.Instance.NavigateToHardwareDefineWorkflow(HardwareType.Device));
            NavigateBackCommand = new RelayCommand(_ => NavigationService.Instance.NavigateToDashboard());
        }

        public ICommand CreateEquipmentCommand { get; }
        public ICommand CreateSystemCommand { get; }
        public ICommand CreateUnitCommand { get; }
        public ICommand CreateDeviceCommand { get; }
        public ICommand NavigateBackCommand { get; }
    }
}