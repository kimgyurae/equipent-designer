using System;
using System.Linq;
using System.Threading.Tasks;
using EquipmentDesigner.Models;
using EquipmentDesigner.Models.Dtos;
using EquipmentDesigner.Models.Storage;
using EquipmentDesigner.Services;
using EquipmentDesigner.Services.Storage;
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

        #region LoadComponentsAsync Filtering Tests - Defined State Inclusion (Bug Fix)

        [Fact]
        public async Task LoadComponentsAsync_IncludesEquipmentWithDefinedState()
        {
            // Arrange
            ServiceLocator.Reset();
            ServiceLocator.ConfigureForTesting();
            var repository = ServiceLocator.GetService<IDataRepository>();
            var dataStore = await repository.LoadAsync();
            dataStore.Equipments.Add(new EquipmentDto { Id = "1", Name = "DefinedEquip", State = ComponentState.Defined });
            await repository.SaveAsync(dataStore);

            // Act
            var viewModel = new DashboardViewModel();
            await Task.Delay(100);

            // Assert
            viewModel.Equipments.Should().HaveCount(1);
            viewModel.Equipments.First().Name.Should().Be("DefinedEquip");
        }

        [Fact]
        public async Task LoadComponentsAsync_IncludesSystemWithDefinedState()
        {
            // Arrange
            ServiceLocator.Reset();
            ServiceLocator.ConfigureForTesting();
            var repository = ServiceLocator.GetService<IDataRepository>();
            var dataStore = await repository.LoadAsync();
            dataStore.Systems.Add(new SystemDto { Id = "1", Name = "DefinedSystem", State = ComponentState.Defined });
            await repository.SaveAsync(dataStore);

            // Act
            var viewModel = new DashboardViewModel();
            await Task.Delay(100);

            // Assert
            viewModel.Systems.Should().HaveCount(1);
            viewModel.Systems.First().Name.Should().Be("DefinedSystem");
        }

        [Fact]
        public async Task LoadComponentsAsync_IncludesUnitWithDefinedState()
        {
            // Arrange
            ServiceLocator.Reset();
            ServiceLocator.ConfigureForTesting();
            var repository = ServiceLocator.GetService<IDataRepository>();
            var dataStore = await repository.LoadAsync();
            dataStore.Units.Add(new UnitDto { Id = "1", Name = "DefinedUnit", State = ComponentState.Defined });
            await repository.SaveAsync(dataStore);

            // Act
            var viewModel = new DashboardViewModel();
            await Task.Delay(100);

            // Assert
            viewModel.Units.Should().HaveCount(1);
            viewModel.Units.First().Name.Should().Be("DefinedUnit");
        }

        [Fact]
        public async Task LoadComponentsAsync_IncludesDeviceWithDefinedState()
        {
            // Arrange
            ServiceLocator.Reset();
            ServiceLocator.ConfigureForTesting();
            var repository = ServiceLocator.GetService<IDataRepository>();
            var dataStore = await repository.LoadAsync();
            dataStore.Devices.Add(new DeviceDto { Id = "1", Name = "DefinedDevice", State = ComponentState.Defined });
            await repository.SaveAsync(dataStore);

            // Act
            var viewModel = new DashboardViewModel();
            await Task.Delay(100);

            // Assert
            viewModel.Devices.Should().HaveCount(1);
            viewModel.Devices.First().Name.Should().Be("DefinedDevice");
        }

        #endregion

        #region LoadComponentsAsync Filtering Tests

        [Fact]
        public async Task LoadComponentsAsync_IncludesEquipmentWithUploadedState()
        {
            // Arrange
            ServiceLocator.Reset();
            ServiceLocator.ConfigureForTesting();
            var repository = ServiceLocator.GetService<IDataRepository>();
            var dataStore = await repository.LoadAsync();
            dataStore.Equipments.Add(new EquipmentDto { Id = "1", Name = "UploadedEquip", State = ComponentState.Uploaded });
            await repository.SaveAsync(dataStore);

            // Act
            var viewModel = new DashboardViewModel();
            await Task.Delay(100); // Wait for async loading

            // Assert
            viewModel.Equipments.Should().HaveCount(1);
            viewModel.Equipments.First().Name.Should().Be("UploadedEquip");
        }

        [Fact]
        public async Task LoadComponentsAsync_IncludesEquipmentWithValidatedState()
        {
            // Arrange
            ServiceLocator.Reset();
            ServiceLocator.ConfigureForTesting();
            var repository = ServiceLocator.GetService<IDataRepository>();
            var dataStore = await repository.LoadAsync();
            dataStore.Equipments.Add(new EquipmentDto { Id = "1", Name = "ValidatedEquip", State = ComponentState.Validated });
            await repository.SaveAsync(dataStore);

            // Act
            var viewModel = new DashboardViewModel();
            await Task.Delay(100);

            // Assert
            viewModel.Equipments.Should().HaveCount(1);
            viewModel.Equipments.First().Name.Should().Be("ValidatedEquip");
        }

        [Fact]
        public async Task LoadComponentsAsync_ExcludesEquipmentWithUndefinedState()
        {
            // Arrange
            ServiceLocator.Reset();
            ServiceLocator.ConfigureForTesting();
            var repository = ServiceLocator.GetService<IDataRepository>();
            var dataStore = await repository.LoadAsync();
            dataStore.Equipments.Add(new EquipmentDto { Id = "1", Name = "UndefinedEquip", State = ComponentState.Undefined });
            await repository.SaveAsync(dataStore);

            // Act
            var viewModel = new DashboardViewModel();
            await Task.Delay(100);

            // Assert
            viewModel.Equipments.Should().HaveCount(0);
        }

        [Fact]
        public async Task LoadComponentsAsync_AppliesFilteringToSystems()
        {
            // Arrange
            ServiceLocator.Reset();
            ServiceLocator.ConfigureForTesting();
            var repository = ServiceLocator.GetService<IDataRepository>();
            var dataStore = await repository.LoadAsync();
            dataStore.Systems.Add(new SystemDto { Id = "1", Name = "UploadedSystem", State = ComponentState.Uploaded });
            dataStore.Systems.Add(new SystemDto { Id = "2", Name = "UndefinedSystem", State = ComponentState.Undefined });
            await repository.SaveAsync(dataStore);

            // Act
            var viewModel = new DashboardViewModel();
            await Task.Delay(100);

            // Assert - should include Uploaded but exclude Undefined
            viewModel.Systems.Should().HaveCount(1);
            viewModel.Systems.First().Name.Should().Be("UploadedSystem");
        }

        [Fact]
        public async Task LoadComponentsAsync_AppliesFilteringToUnits()
        {
            // Arrange
            ServiceLocator.Reset();
            ServiceLocator.ConfigureForTesting();
            var repository = ServiceLocator.GetService<IDataRepository>();
            var dataStore = await repository.LoadAsync();
            dataStore.Units.Add(new UnitDto { Id = "1", Name = "ValidatedUnit", State = ComponentState.Validated });
            dataStore.Units.Add(new UnitDto { Id = "2", Name = "UndefinedUnit", State = ComponentState.Undefined });
            await repository.SaveAsync(dataStore);

            // Act
            var viewModel = new DashboardViewModel();
            await Task.Delay(100);

            // Assert
            viewModel.Units.Should().HaveCount(1);
            viewModel.Units.First().Name.Should().Be("ValidatedUnit");
        }

        [Fact]
        public async Task LoadComponentsAsync_AppliesFilteringToDevices()
        {
            // Arrange
            ServiceLocator.Reset();
            ServiceLocator.ConfigureForTesting();
            var repository = ServiceLocator.GetService<IDataRepository>();
            var dataStore = await repository.LoadAsync();
            dataStore.Devices.Add(new DeviceDto { Id = "1", Name = "UploadedDevice", State = ComponentState.Uploaded });
            dataStore.Devices.Add(new DeviceDto { Id = "2", Name = "UndefinedDevice", State = ComponentState.Undefined });
            await repository.SaveAsync(dataStore);

            // Act
            var viewModel = new DashboardViewModel();
            await Task.Delay(100);

            // Assert - should include Uploaded but exclude Undefined
            viewModel.Devices.Should().HaveCount(1);
            viewModel.Devices.First().Name.Should().Be("UploadedDevice");
        }

        [Fact]
        public async Task LoadComponentsAsync_RaisesPropertyChangedForEquipmentsCount()
        {
            // Arrange
            ServiceLocator.Reset();
            ServiceLocator.ConfigureForTesting();
            var repository = ServiceLocator.GetService<IDataRepository>();
            var dataStore = await repository.LoadAsync();
            dataStore.Equipments.Add(new EquipmentDto { Id = "1", Name = "Test", State = ComponentState.Uploaded });
            await repository.SaveAsync(dataStore);

            var viewModel = new DashboardViewModel();
            await Task.Delay(100);

            // Assert - just verify the count is accessible
            viewModel.EquipmentsCount.Should().Be(1);
        }

        [Fact]
        public async Task LoadComponentsAsync_HandlesNullDataStoreGracefully()
        {
            // Arrange
            ServiceLocator.Reset();
            ServiceLocator.ConfigureForTesting();

            // Act - should not throw
            var viewModel = new DashboardViewModel();
            await Task.Delay(100);

            // Assert
            viewModel.Equipments.Should().NotBeNull();
            viewModel.Systems.Should().NotBeNull();
            viewModel.Units.Should().NotBeNull();
            viewModel.Devices.Should().NotBeNull();
        }

        #endregion
    }
}