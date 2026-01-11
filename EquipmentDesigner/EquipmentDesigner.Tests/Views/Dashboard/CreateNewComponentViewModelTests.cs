using EquipmentDesigner.Models;
using EquipmentDesigner.Services;
using EquipmentDesigner.ViewModels;
using FluentAssertions;
using Xunit;

namespace EquipmentDesigner.Tests.Views.Dashboard
{
    public class CreateNewComponentViewModelTests
    {
        #region Command Initialization Tests

        [Fact]
        public void Constructor_Default_CreateEquipmentCommandIsNotNull()
        {
            var viewModel = new CreateNewComponentViewModel();
            viewModel.CreateEquipmentCommand.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_Default_CreateSystemCommandIsNotNull()
        {
            var viewModel = new CreateNewComponentViewModel();
            viewModel.CreateSystemCommand.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_Default_CreateUnitCommandIsNotNull()
        {
            var viewModel = new CreateNewComponentViewModel();
            viewModel.CreateUnitCommand.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_Default_CreateDeviceCommandIsNotNull()
        {
            var viewModel = new CreateNewComponentViewModel();
            viewModel.CreateDeviceCommand.Should().NotBeNull();
        }

        #endregion

        #region Command Executability Tests

        [Fact]
        public void CreateEquipmentCommand_CanExecute_ReturnsTrue()
        {
            var viewModel = new CreateNewComponentViewModel();
            viewModel.CreateEquipmentCommand.CanExecute(null).Should().BeTrue();
        }

        [Fact]
        public void CreateSystemCommand_CanExecute_ReturnsTrue()
        {
            var viewModel = new CreateNewComponentViewModel();
            viewModel.CreateSystemCommand.CanExecute(null).Should().BeTrue();
        }

        [Fact]
        public void CreateUnitCommand_CanExecute_ReturnsTrue()
        {
            var viewModel = new CreateNewComponentViewModel();
            viewModel.CreateUnitCommand.CanExecute(null).Should().BeTrue();
        }

        [Fact]
        public void CreateDeviceCommand_CanExecute_ReturnsTrue()
        {
            var viewModel = new CreateNewComponentViewModel();
            viewModel.CreateDeviceCommand.CanExecute(null).Should().BeTrue();
        }

        #endregion

        #region Navigation Behavior Tests

        [Fact]
        public void CreateEquipmentCommand_Execute_TriggersNavigationWithEquipmentHardwareLayer()
        {
            var viewModel = new CreateNewComponentViewModel();
            NavigationTarget capturedTarget = null;
            NavigationService.Instance.NavigationRequested += target => capturedTarget = target;

            viewModel.CreateEquipmentCommand.Execute(null);

            capturedTarget.Should().NotBeNull();
            capturedTarget.StartType.Should().Be(HardwareLayer.Equipment);
        }

        [Fact]
        public void CreateSystemCommand_Execute_TriggersNavigationWithSystemHardwareLayer()
        {
            var viewModel = new CreateNewComponentViewModel();
            NavigationTarget capturedTarget = null;
            NavigationService.Instance.NavigationRequested += target => capturedTarget = target;

            viewModel.CreateSystemCommand.Execute(null);

            capturedTarget.Should().NotBeNull();
            capturedTarget.StartType.Should().Be(HardwareLayer.System);
        }

        [Fact]
        public void CreateUnitCommand_Execute_TriggersNavigationWithUnitHardwareLayer()
        {
            var viewModel = new CreateNewComponentViewModel();
            NavigationTarget capturedTarget = null;
            NavigationService.Instance.NavigationRequested += target => capturedTarget = target;

            viewModel.CreateUnitCommand.Execute(null);

            capturedTarget.Should().NotBeNull();
            capturedTarget.StartType.Should().Be(HardwareLayer.Unit);
        }

        [Fact]
        public void CreateDeviceCommand_Execute_TriggersNavigationWithDeviceHardwareLayer()
        {
            var viewModel = new CreateNewComponentViewModel();
            NavigationTarget capturedTarget = null;
            NavigationService.Instance.NavigationRequested += target => capturedTarget = target;

            viewModel.CreateDeviceCommand.Execute(null);

            capturedTarget.Should().NotBeNull();
            capturedTarget.StartType.Should().Be(HardwareLayer.Device);
        }

        #endregion
    }
}
