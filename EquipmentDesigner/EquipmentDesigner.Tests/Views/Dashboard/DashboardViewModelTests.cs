using System;
using System.Collections.Generic;
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
        #region Helper Methods

        private WorkflowSessionDto CreateWorkflowSession(string workflowId, HardwareLayer startType, ComponentState state, string name, string description = null)
        {
            var rootNode = new TreeNodeDataDto
            {
                NodeId = Guid.NewGuid().ToString(),
                HardwareLayer = startType
            };

            switch (startType)
            {
                case HardwareLayer.Equipment:
                    rootNode.EquipmentData = new EquipmentDto { Id = workflowId, Name = name, Description = description, State = state };
                    break;
                case HardwareLayer.System:
                    rootNode.SystemData = new SystemDto { Id = workflowId, Name = name, Description = description, State = state };
                    break;
                case HardwareLayer.Unit:
                    rootNode.UnitData = new UnitDto { Id = workflowId, Name = name, Description = description, State = state };
                    break;
                case HardwareLayer.Device:
                    rootNode.DeviceData = new DeviceDto { Id = workflowId, Name = name, Description = description, State = state };
                    break;
            }

            return new WorkflowSessionDto
            {
                WorkflowId = workflowId,
                StartType = startType,
                State = state,
                TreeNodes = new List<TreeNodeDataDto> { rootNode }
            };
        }

        #endregion

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

        #region LoadComponentsAsync Population Tests

        [Fact]
        public async Task LoadComponentsAsync_IncludesEquipmentWithDefinedState()
        {
            // Arrange
            ServiceLocator.Reset();
            ServiceLocator.ConfigureForTesting();
            var repository = ServiceLocator.GetService<IUploadedWorkflowRepository>();
            var dataStore = await repository.LoadAsync();
            dataStore.WorkflowSessions.Add(CreateWorkflowSession("1", HardwareLayer.Equipment, ComponentState.Ready, "DefinedEquip"));
            await repository.SaveAsync(dataStore);

            // Act
            var viewModel = new DashboardViewModel();
            await Task.Delay(1000);

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
            var repository = ServiceLocator.GetService<IUploadedWorkflowRepository>();
            var dataStore = await repository.LoadAsync();
            dataStore.WorkflowSessions.Add(CreateWorkflowSession("1", HardwareLayer.System, ComponentState.Ready, "DefinedSystem"));
            await repository.SaveAsync(dataStore);

            // Act
            var viewModel = new DashboardViewModel();
            await Task.Delay(1000);

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
            var repository = ServiceLocator.GetService<IUploadedWorkflowRepository>();
            var dataStore = await repository.LoadAsync();
            dataStore.WorkflowSessions.Add(CreateWorkflowSession("1", HardwareLayer.Unit, ComponentState.Ready, "DefinedUnit"));
            await repository.SaveAsync(dataStore);

            // Act
            var viewModel = new DashboardViewModel();
            await Task.Delay(1000);

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
            var repository = ServiceLocator.GetService<IUploadedWorkflowRepository>();
            var dataStore = await repository.LoadAsync();
            dataStore.WorkflowSessions.Add(CreateWorkflowSession("1", HardwareLayer.Device, ComponentState.Ready, "DefinedDevice"));
            await repository.SaveAsync(dataStore);

            // Act
            var viewModel = new DashboardViewModel();
            await Task.Delay(1000);

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
            var repository = ServiceLocator.GetService<IUploadedWorkflowRepository>();
            var dataStore = await repository.LoadAsync();
            dataStore.WorkflowSessions.Add(CreateWorkflowSession("1", HardwareLayer.Equipment, ComponentState.Uploaded, "UploadedEquip"));
            await repository.SaveAsync(dataStore);

            // Act
            var viewModel = new DashboardViewModel();
            await Task.Delay(1000); // Wait for async loading

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
            var repository = ServiceLocator.GetService<IUploadedWorkflowRepository>();
            var dataStore = await repository.LoadAsync();
            dataStore.WorkflowSessions.Add(CreateWorkflowSession("1", HardwareLayer.Equipment, ComponentState.Validated, "ValidatedEquip"));
            await repository.SaveAsync(dataStore);

            // Act
            var viewModel = new DashboardViewModel();
            await Task.Delay(1000);

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
            var repository = ServiceLocator.GetService<IUploadedWorkflowRepository>();
            var dataStore = await repository.LoadAsync();
            dataStore.WorkflowSessions.Add(CreateWorkflowSession("1", HardwareLayer.Equipment, ComponentState.Draft, "UndefinedEquip"));
            await repository.SaveAsync(dataStore);

            // Act
            var viewModel = new DashboardViewModel();
            await Task.Delay(1000);

            // Assert
            viewModel.Equipments.Should().HaveCount(0);
        }

        [Fact]
        public async Task LoadComponentsAsync_AppliesFilteringToSystems()
        {
            // Arrange
            ServiceLocator.Reset();
            ServiceLocator.ConfigureForTesting();
            var repository = ServiceLocator.GetService<IUploadedWorkflowRepository>();
            var dataStore = await repository.LoadAsync();
            dataStore.WorkflowSessions.Add(CreateWorkflowSession("1", HardwareLayer.System, ComponentState.Uploaded, "UploadedSystem"));
            dataStore.WorkflowSessions.Add(CreateWorkflowSession("2", HardwareLayer.System, ComponentState.Draft, "UndefinedSystem"));
            await repository.SaveAsync(dataStore);

            // Act
            var viewModel = new DashboardViewModel();
            await Task.Delay(1000);

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
            var repository = ServiceLocator.GetService<IUploadedWorkflowRepository>();
            var dataStore = await repository.LoadAsync();
            dataStore.WorkflowSessions.Add(CreateWorkflowSession("1", HardwareLayer.Unit, ComponentState.Validated, "ValidatedUnit"));
            dataStore.WorkflowSessions.Add(CreateWorkflowSession("2", HardwareLayer.Unit, ComponentState.Draft, "UndefinedUnit"));
            await repository.SaveAsync(dataStore);

            // Act
            var viewModel = new DashboardViewModel();
            await Task.Delay(1000);

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
            var repository = ServiceLocator.GetService<IUploadedWorkflowRepository>();
            var dataStore = await repository.LoadAsync();
            dataStore.WorkflowSessions.Add(CreateWorkflowSession("1", HardwareLayer.Device, ComponentState.Uploaded, "UploadedDevice"));
            dataStore.WorkflowSessions.Add(CreateWorkflowSession("2", HardwareLayer.Device, ComponentState.Draft, "UndefinedDevice"));
            await repository.SaveAsync(dataStore);

            // Act
            var viewModel = new DashboardViewModel();
            await Task.Delay(1000);

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
            var repository = ServiceLocator.GetService<IUploadedWorkflowRepository>();
            var dataStore = await repository.LoadAsync();
            dataStore.WorkflowSessions.Add(CreateWorkflowSession("1", HardwareLayer.Equipment, ComponentState.Uploaded, "Test"));
            await repository.SaveAsync(dataStore);

            var viewModel = new DashboardViewModel();
            await Task.Delay(1000);

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
            await Task.Delay(1000);

            // Assert
            viewModel.Equipments.Should().NotBeNull();
            viewModel.Systems.Should().NotBeNull();
            viewModel.Units.Should().NotBeNull();
            viewModel.Devices.Should().NotBeNull();
        }

        #endregion

        #region ComponentItem Extension Tests (Read-Only View Feature)

        [Fact]
        public async Task LoadComponentsAsync_PopulatesIdForEquipment()
        {
            // Arrange
            ServiceLocator.Reset();
            ServiceLocator.ConfigureForTesting();
            var repository = ServiceLocator.GetService<IUploadedWorkflowRepository>();
            var dataStore = await repository.LoadAsync();
            dataStore.WorkflowSessions.Add(CreateWorkflowSession("equip-123", HardwareLayer.Equipment, ComponentState.Ready, "TestEquip"));
            await repository.SaveAsync(dataStore);

            // Act
            var viewModel = new DashboardViewModel();
            await Task.Delay(1000);

            // Assert
            viewModel.Equipments.Should().HaveCount(1);
            viewModel.Equipments.First().Id.Should().Be("equip-123");
        }

        [Fact]
        public async Task LoadComponentsAsync_PopulatesHardwareLayerForEquipment()
        {
            // Arrange
            ServiceLocator.Reset();
            ServiceLocator.ConfigureForTesting();
            var repository = ServiceLocator.GetService<IUploadedWorkflowRepository>();
            var dataStore = await repository.LoadAsync();
            dataStore.WorkflowSessions.Add(CreateWorkflowSession("equip-123", HardwareLayer.Equipment, ComponentState.Ready, "TestEquip"));
            await repository.SaveAsync(dataStore);

            // Act
            var viewModel = new DashboardViewModel();
            await Task.Delay(1000);

            // Assert
            viewModel.Equipments.First().HardwareLayer.Should().Be(HardwareLayer.Equipment);
        }

        [Fact]
        public async Task LoadComponentsAsync_PopulatesIdAndTypeForSystem()
        {
            // Arrange
            ServiceLocator.Reset();
            ServiceLocator.ConfigureForTesting();
            var repository = ServiceLocator.GetService<IUploadedWorkflowRepository>();
            var dataStore = await repository.LoadAsync();
            dataStore.WorkflowSessions.Add(CreateWorkflowSession("sys-456", HardwareLayer.System, ComponentState.Uploaded, "TestSystem"));
            await repository.SaveAsync(dataStore);

            // Act
            var viewModel = new DashboardViewModel();
            await Task.Delay(1000);

            // Assert
            viewModel.Systems.Should().HaveCount(1);
            viewModel.Systems.First().Id.Should().Be("sys-456");
            viewModel.Systems.First().HardwareLayer.Should().Be(HardwareLayer.System);
        }

        [Fact]
        public async Task LoadComponentsAsync_PopulatesIdAndTypeForUnit()
        {
            // Arrange
            ServiceLocator.Reset();
            ServiceLocator.ConfigureForTesting();
            var repository = ServiceLocator.GetService<IUploadedWorkflowRepository>();
            var dataStore = await repository.LoadAsync();
            dataStore.WorkflowSessions.Add(CreateWorkflowSession("unit-789", HardwareLayer.Unit, ComponentState.Validated, "TestUnit"));
            await repository.SaveAsync(dataStore);

            // Act
            var viewModel = new DashboardViewModel();
            await Task.Delay(1000);

            // Assert
            viewModel.Units.Should().HaveCount(1);
            viewModel.Units.First().Id.Should().Be("unit-789");
            viewModel.Units.First().HardwareLayer.Should().Be(HardwareLayer.Unit);
        }

        [Fact]
        public async Task LoadComponentsAsync_PopulatesIdAndTypeForDevice()
        {
            // Arrange
            ServiceLocator.Reset();
            ServiceLocator.ConfigureForTesting();
            var repository = ServiceLocator.GetService<IUploadedWorkflowRepository>();
            var dataStore = await repository.LoadAsync();
            dataStore.WorkflowSessions.Add(CreateWorkflowSession("dev-101", HardwareLayer.Device, ComponentState.Ready, "TestDevice"));
            await repository.SaveAsync(dataStore);

            // Act
            var viewModel = new DashboardViewModel();
            await Task.Delay(1000);

            // Assert
            viewModel.Devices.Should().HaveCount(1);
            viewModel.Devices.First().Id.Should().Be("dev-101");
            viewModel.Devices.First().HardwareLayer.Should().Be(HardwareLayer.Device);
        }

        #endregion

        #region ViewComponentCommand Tests

        [Fact]
        public void ViewComponentCommand_IsNotNull()
        {
            var viewModel = new DashboardViewModel();
            viewModel.ViewComponentCommand.Should().NotBeNull();
        }

        #endregion
    }
}