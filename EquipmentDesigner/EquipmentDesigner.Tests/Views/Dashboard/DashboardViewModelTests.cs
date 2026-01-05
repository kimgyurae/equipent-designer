using EquipmentDesigner.Models;
using EquipmentDesigner.Services;
using EquipmentDesigner.Views.Dashboard;
using EquipmentDesigner.Views.HardwareDefineWorkflow;
using FluentAssertions;
using Xunit;

namespace EquipmentDesigner.Tests.Views.Dashboard
{
    public class DashboardViewModelTests
    {
        #region Command Initialization Tests

        [Fact]
        public void Constructor_Default_CreateEquipmentCommandIsNotNull()
        {
            var viewModel = new DashboardViewModel();
            viewModel.CreateEquipmentCommand.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_Default_CreateSystemCommandIsNotNull()
        {
            var viewModel = new DashboardViewModel();
            viewModel.CreateSystemCommand.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_Default_CreateUnitCommandIsNotNull()
        {
            var viewModel = new DashboardViewModel();
            viewModel.CreateUnitCommand.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_Default_CreateDeviceCommandIsNotNull()
        {
            var viewModel = new DashboardViewModel();
            viewModel.CreateDeviceCommand.Should().NotBeNull();
        }

        #endregion

        #region Command Executability Tests

        [Fact]
        public void CreateEquipmentCommand_CanExecute_ReturnsTrue()
        {
            var viewModel = new DashboardViewModel();
            viewModel.CreateEquipmentCommand.CanExecute(null).Should().BeTrue();
        }

        [Fact]
        public void CreateSystemCommand_CanExecute_ReturnsTrue()
        {
            var viewModel = new DashboardViewModel();
            viewModel.CreateSystemCommand.CanExecute(null).Should().BeTrue();
        }

        [Fact]
        public void CreateUnitCommand_CanExecute_ReturnsTrue()
        {
            var viewModel = new DashboardViewModel();
            viewModel.CreateUnitCommand.CanExecute(null).Should().BeTrue();
        }

        [Fact]
        public void CreateDeviceCommand_CanExecute_ReturnsTrue()
        {
            var viewModel = new DashboardViewModel();
            viewModel.CreateDeviceCommand.CanExecute(null).Should().BeTrue();
        }

        #endregion

        #region Navigation Behavior Tests

        [Fact]
        public void CreateEquipmentCommand_Execute_TriggersNavigationWithEquipmentStartType()
        {
            var viewModel = new DashboardViewModel();
            NavigationTarget capturedTarget = null;
            NavigationService.Instance.NavigationRequested += target => capturedTarget = target;

            viewModel.CreateEquipmentCommand.Execute(null);

            capturedTarget.Should().NotBeNull();
            capturedTarget.StartType.Should().Be(HardwareLayer.Equipment);
        }

        [Fact]
        public void CreateSystemCommand_Execute_TriggersNavigationWithSystemStartType()
        {
            var viewModel = new DashboardViewModel();
            NavigationTarget capturedTarget = null;
            NavigationService.Instance.NavigationRequested += target => capturedTarget = target;

            viewModel.CreateSystemCommand.Execute(null);

            capturedTarget.Should().NotBeNull();
            capturedTarget.StartType.Should().Be(HardwareLayer.System);
        }

        [Fact]
        public void CreateUnitCommand_Execute_TriggersNavigationWithUnitStartType()
        {
            var viewModel = new DashboardViewModel();
            NavigationTarget capturedTarget = null;
            NavigationService.Instance.NavigationRequested += target => capturedTarget = target;

            viewModel.CreateUnitCommand.Execute(null);

            capturedTarget.Should().NotBeNull();
            capturedTarget.StartType.Should().Be(HardwareLayer.Unit);
        }

        [Fact]
        public void CreateDeviceCommand_Execute_TriggersNavigationWithDeviceStartType()
        {
            var viewModel = new DashboardViewModel();
            NavigationTarget capturedTarget = null;
            NavigationService.Instance.NavigationRequested += target => capturedTarget = target;

            viewModel.CreateDeviceCommand.Execute(null);

            capturedTarget.Should().NotBeNull();
            capturedTarget.StartType.Should().Be(HardwareLayer.Device);
        }

        #endregion
    }
}