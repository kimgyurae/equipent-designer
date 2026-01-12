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
        public void CreateEquipmentCommand_Execute_TriggersNavigationWithEquipmentHardwareType()
        {
            var viewModel = new CreateNewComponentViewModel();
            NavigationTarget capturedTarget = null;
            NavigationService.Instance.NavigationRequested += target => capturedTarget = target;

            viewModel.CreateEquipmentCommand.Execute(null);

            capturedTarget.Should().NotBeNull();
            capturedTarget.StartType.Should().Be(HardwareType.Equipment);
        }

        [Fact]
        public void CreateSystemCommand_Execute_TriggersNavigationWithSystemHardwareType()
        {
            var viewModel = new CreateNewComponentViewModel();
            NavigationTarget capturedTarget = null;
            NavigationService.Instance.NavigationRequested += target => capturedTarget = target;

            viewModel.CreateSystemCommand.Execute(null);

            capturedTarget.Should().NotBeNull();
            capturedTarget.StartType.Should().Be(HardwareType.System);
        }

        [Fact]
        public void CreateUnitCommand_Execute_TriggersNavigationWithUnitHardwareType()
        {
            var viewModel = new CreateNewComponentViewModel();
            NavigationTarget capturedTarget = null;
            NavigationService.Instance.NavigationRequested += target => capturedTarget = target;

            viewModel.CreateUnitCommand.Execute(null);

            capturedTarget.Should().NotBeNull();
            capturedTarget.StartType.Should().Be(HardwareType.Unit);
        }

        [Fact]
        public void CreateDeviceCommand_Execute_TriggersNavigationWithDeviceHardwareType()
        {
            var viewModel = new CreateNewComponentViewModel();
            NavigationTarget capturedTarget = null;
            NavigationService.Instance.NavigationRequested += target => capturedTarget = target;

            viewModel.CreateDeviceCommand.Execute(null);

            capturedTarget.Should().NotBeNull();
            capturedTarget.StartType.Should().Be(HardwareType.Device);
        }

        #endregion
    }
}
