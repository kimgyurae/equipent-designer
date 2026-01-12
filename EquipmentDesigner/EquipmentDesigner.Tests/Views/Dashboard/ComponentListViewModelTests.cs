using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EquipmentDesigner.Models;
using EquipmentDesigner.Services;
using EquipmentDesigner.ViewModels;
using FluentAssertions;
using Xunit;

namespace EquipmentDesigner.Tests.Views.Dashboard
{
    public class ComponentListViewModelTests
    {
        #region Helper Methods

        private HardwareDefinition CreateWorkflowSession(string workflowId, HardwareType hardwareType, ComponentState state, string name, string description = null)
        {
            var rootNode = new TreeNodeDataDto
            {
                Id = System.Guid.NewGuid().ToString(),
                HardwareType = hardwareType
            };

            switch (hardwareType)
            {
                case HardwareType.Equipment:
                    rootNode.EquipmentData = new EquipmentDto { Id = workflowId, Name = name, Description = description, State = state };
                    break;
                case HardwareType.System:
                    rootNode.SystemData = new SystemDto { Id = workflowId, Name = name, Description = description, State = state };
                    break;
                case HardwareType.Unit:
                    rootNode.UnitData = new UnitDto { Id = workflowId, Name = name, Description = description, State = state };
                    break;
                case HardwareType.Device:
                    rootNode.DeviceData = new DeviceDto { Id = workflowId, Name = name, Description = description, State = state };
                    break;
            }

            return new HardwareDefinition
            {
                Id = workflowId,
                HardwareType = hardwareType,
                State = state,
                TreeNodes = new List<TreeNodeDataDto> { rootNode }
            };
        }

        #endregion

        #region Collection Initialization Tests

        [Fact]
        public void Constructor_Default_EquipmentsCollectionIsNotNull()
        {
            var viewModel = new ComponentListViewModel();
            viewModel.Equipments.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_Default_SystemsCollectionIsNotNull()
        {
            var viewModel = new ComponentListViewModel();
            viewModel.Systems.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_Default_UnitsCollectionIsNotNull()
        {
            var viewModel = new ComponentListViewModel();
            viewModel.Units.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_Default_DevicesCollectionIsNotNull()
        {
            var viewModel = new ComponentListViewModel();
            viewModel.Devices.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_Default_ViewComponentCommandIsNotNull()
        {
            var viewModel = new ComponentListViewModel();
            viewModel.ViewComponentCommand.Should().NotBeNull();
        }

        #endregion

        #region LoadComponentsAsync Filtering Tests

        [Fact]
        public async Task LoadComponentsAsync_IncludesEquipmentWithReadyState()
        {
            // Arrange
            ServiceLocator.Reset();
            ServiceLocator.ConfigureForTesting();
            var repository = ServiceLocator.GetService<IUploadedWorkflowRepository>();
            var dataStore = await repository.LoadAsync();
            dataStore.WorkflowSessions.Add(CreateWorkflowSession("1", HardwareType.Equipment, ComponentState.Ready, "ReadyEquip"));
            await repository.SaveAsync(dataStore);

            // Act
            var viewModel = new ComponentListViewModel();
            await Task.Delay(1000);

            // Assert
            viewModel.Equipments.Should().HaveCount(1);
            viewModel.Equipments.First().Name.Should().Be("ReadyEquip");
        }

        [Fact]
        public async Task LoadComponentsAsync_IncludesEquipmentWithUploadedState()
        {
            // Arrange
            ServiceLocator.Reset();
            ServiceLocator.ConfigureForTesting();
            var repository = ServiceLocator.GetService<IUploadedWorkflowRepository>();
            var dataStore = await repository.LoadAsync();
            dataStore.WorkflowSessions.Add(CreateWorkflowSession("1", HardwareType.Equipment, ComponentState.Uploaded, "UploadedEquip"));
            await repository.SaveAsync(dataStore);

            // Act
            var viewModel = new ComponentListViewModel();
            await Task.Delay(1000);

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
            dataStore.WorkflowSessions.Add(CreateWorkflowSession("1", HardwareType.Equipment, ComponentState.Validated, "ValidatedEquip"));
            await repository.SaveAsync(dataStore);

            // Act
            var viewModel = new ComponentListViewModel();
            await Task.Delay(1000);

            // Assert
            viewModel.Equipments.Should().HaveCount(1);
            viewModel.Equipments.First().Name.Should().Be("ValidatedEquip");
        }

        [Fact]
        public async Task LoadComponentsAsync_ExcludesEquipmentWithDraftState()
        {
            // Arrange
            ServiceLocator.Reset();
            ServiceLocator.ConfigureForTesting();
            var repository = ServiceLocator.GetService<IUploadedWorkflowRepository>();
            var dataStore = await repository.LoadAsync();
            dataStore.WorkflowSessions.Add(CreateWorkflowSession("1", HardwareType.Equipment, ComponentState.Draft, "DraftEquip"));
            await repository.SaveAsync(dataStore);

            // Act
            var viewModel = new ComponentListViewModel();
            await Task.Delay(1000);

            // Assert
            viewModel.Equipments.Should().HaveCount(0);
        }

        [Fact]
        public async Task LoadComponentsAsync_IncludesSystemWithReadyState()
        {
            // Arrange
            ServiceLocator.Reset();
            ServiceLocator.ConfigureForTesting();
            var repository = ServiceLocator.GetService<IUploadedWorkflowRepository>();
            var dataStore = await repository.LoadAsync();
            dataStore.WorkflowSessions.Add(CreateWorkflowSession("1", HardwareType.System, ComponentState.Ready, "ReadySystem"));
            await repository.SaveAsync(dataStore);

            // Act
            var viewModel = new ComponentListViewModel();
            await Task.Delay(1000);

            // Assert
            viewModel.Systems.Should().HaveCount(1);
            viewModel.Systems.First().Name.Should().Be("ReadySystem");
        }

        [Fact]
        public async Task LoadComponentsAsync_IncludesUnitWithReadyState()
        {
            // Arrange
            ServiceLocator.Reset();
            ServiceLocator.ConfigureForTesting();
            var repository = ServiceLocator.GetService<IUploadedWorkflowRepository>();
            var dataStore = await repository.LoadAsync();
            dataStore.WorkflowSessions.Add(CreateWorkflowSession("1", HardwareType.Unit, ComponentState.Ready, "ReadyUnit"));
            await repository.SaveAsync(dataStore);

            // Act
            var viewModel = new ComponentListViewModel();
            await Task.Delay(1000);

            // Assert
            viewModel.Units.Should().HaveCount(1);
            viewModel.Units.First().Name.Should().Be("ReadyUnit");
        }

        [Fact]
        public async Task LoadComponentsAsync_IncludesDeviceWithReadyState()
        {
            // Arrange
            ServiceLocator.Reset();
            ServiceLocator.ConfigureForTesting();
            var repository = ServiceLocator.GetService<IUploadedWorkflowRepository>();
            var dataStore = await repository.LoadAsync();
            dataStore.WorkflowSessions.Add(CreateWorkflowSession("1", HardwareType.Device, ComponentState.Ready, "ReadyDevice"));
            await repository.SaveAsync(dataStore);

            // Act
            var viewModel = new ComponentListViewModel();
            await Task.Delay(1000);

            // Assert
            viewModel.Devices.Should().HaveCount(1);
            viewModel.Devices.First().Name.Should().Be("ReadyDevice");
        }

        #endregion

        #region LoadComponentsAsync Routing Tests

        [Fact]
        public async Task LoadComponentsAsync_RoutesEquipmentToEquipmentsCollection()
        {
            // Arrange
            ServiceLocator.Reset();
            ServiceLocator.ConfigureForTesting();
            var repository = ServiceLocator.GetService<IUploadedWorkflowRepository>();
            var dataStore = await repository.LoadAsync();
            dataStore.WorkflowSessions.Add(CreateWorkflowSession("equip-1", HardwareType.Equipment, ComponentState.Ready, "TestEquip"));
            await repository.SaveAsync(dataStore);

            // Act
            var viewModel = new ComponentListViewModel();
            await Task.Delay(1000);

            // Assert
            viewModel.Equipments.Should().HaveCount(1);
            viewModel.Systems.Should().HaveCount(0);
            viewModel.Units.Should().HaveCount(0);
            viewModel.Devices.Should().HaveCount(0);
        }

        [Fact]
        public async Task LoadComponentsAsync_RoutesSystemToSystemsCollection()
        {
            // Arrange
            ServiceLocator.Reset();
            ServiceLocator.ConfigureForTesting();
            var repository = ServiceLocator.GetService<IUploadedWorkflowRepository>();
            var dataStore = await repository.LoadAsync();
            dataStore.WorkflowSessions.Add(CreateWorkflowSession("sys-1", HardwareType.System, ComponentState.Ready, "TestSystem"));
            await repository.SaveAsync(dataStore);

            // Act
            var viewModel = new ComponentListViewModel();
            await Task.Delay(1000);

            // Assert
            viewModel.Equipments.Should().HaveCount(0);
            viewModel.Systems.Should().HaveCount(1);
            viewModel.Units.Should().HaveCount(0);
            viewModel.Devices.Should().HaveCount(0);
        }

        [Fact]
        public async Task LoadComponentsAsync_RoutesUnitToUnitsCollection()
        {
            // Arrange
            ServiceLocator.Reset();
            ServiceLocator.ConfigureForTesting();
            var repository = ServiceLocator.GetService<IUploadedWorkflowRepository>();
            var dataStore = await repository.LoadAsync();
            dataStore.WorkflowSessions.Add(CreateWorkflowSession("unit-1", HardwareType.Unit, ComponentState.Ready, "TestUnit"));
            await repository.SaveAsync(dataStore);

            // Act
            var viewModel = new ComponentListViewModel();
            await Task.Delay(1000);

            // Assert
            viewModel.Equipments.Should().HaveCount(0);
            viewModel.Systems.Should().HaveCount(0);
            viewModel.Units.Should().HaveCount(1);
            viewModel.Devices.Should().HaveCount(0);
        }

        [Fact]
        public async Task LoadComponentsAsync_RoutesDeviceToDevicesCollection()
        {
            // Arrange
            ServiceLocator.Reset();
            ServiceLocator.ConfigureForTesting();
            var repository = ServiceLocator.GetService<IUploadedWorkflowRepository>();
            var dataStore = await repository.LoadAsync();
            dataStore.WorkflowSessions.Add(CreateWorkflowSession("dev-1", HardwareType.Device, ComponentState.Ready, "TestDevice"));
            await repository.SaveAsync(dataStore);

            // Act
            var viewModel = new ComponentListViewModel();
            await Task.Delay(1000);

            // Assert
            viewModel.Equipments.Should().HaveCount(0);
            viewModel.Systems.Should().HaveCount(0);
            viewModel.Units.Should().HaveCount(0);
            viewModel.Devices.Should().HaveCount(1);
        }

        #endregion

        #region Count Property Tests

        [Fact]
        public async Task EquipmentsCount_AfterLoading_ReturnsCorrectCount()
        {
            // Arrange
            ServiceLocator.Reset();
            ServiceLocator.ConfigureForTesting();
            var repository = ServiceLocator.GetService<IUploadedWorkflowRepository>();
            var dataStore = await repository.LoadAsync();
            dataStore.WorkflowSessions.Add(CreateWorkflowSession("1", HardwareType.Equipment, ComponentState.Ready, "Equip1"));
            dataStore.WorkflowSessions.Add(CreateWorkflowSession("2", HardwareType.Equipment, ComponentState.Uploaded, "Equip2"));
            await repository.SaveAsync(dataStore);

            // Act
            var viewModel = new ComponentListViewModel();
            await Task.Delay(1000);

            // Assert
            viewModel.EquipmentsCount.Should().Be(2);
        }

        [Fact]
        public async Task SystemsCount_AfterLoading_ReturnsCorrectCount()
        {
            // Arrange
            ServiceLocator.Reset();
            ServiceLocator.ConfigureForTesting();
            var repository = ServiceLocator.GetService<IUploadedWorkflowRepository>();
            var dataStore = await repository.LoadAsync();
            dataStore.WorkflowSessions.Add(CreateWorkflowSession("1", HardwareType.System, ComponentState.Ready, "Sys1"));
            await repository.SaveAsync(dataStore);

            // Act
            var viewModel = new ComponentListViewModel();
            await Task.Delay(1000);

            // Assert
            viewModel.SystemsCount.Should().Be(1);
        }

        [Fact]
        public async Task UnitsCount_AfterLoading_ReturnsCorrectCount()
        {
            // Arrange
            ServiceLocator.Reset();
            ServiceLocator.ConfigureForTesting();
            var repository = ServiceLocator.GetService<IUploadedWorkflowRepository>();
            var dataStore = await repository.LoadAsync();
            dataStore.WorkflowSessions.Add(CreateWorkflowSession("1", HardwareType.Unit, ComponentState.Ready, "Unit1"));
            await repository.SaveAsync(dataStore);

            // Act
            var viewModel = new ComponentListViewModel();
            await Task.Delay(1000);

            // Assert
            viewModel.UnitsCount.Should().Be(1);
        }

        [Fact]
        public async Task DevicesCount_AfterLoading_ReturnsCorrectCount()
        {
            // Arrange
            ServiceLocator.Reset();
            ServiceLocator.ConfigureForTesting();
            var repository = ServiceLocator.GetService<IUploadedWorkflowRepository>();
            var dataStore = await repository.LoadAsync();
            dataStore.WorkflowSessions.Add(CreateWorkflowSession("1", HardwareType.Device, ComponentState.Ready, "Dev1"));
            await repository.SaveAsync(dataStore);

            // Act
            var viewModel = new ComponentListViewModel();
            await Task.Delay(1000);

            // Assert
            viewModel.DevicesCount.Should().Be(1);
        }

        #endregion

        #region Empty State Visibility Tests

        [Fact]
        public void HasNoEquipments_WhenEmpty_ReturnsTrue()
        {
            ServiceLocator.Reset();
            ServiceLocator.ConfigureForTesting();
            var viewModel = new ComponentListViewModel();
            viewModel.HasNoEquipments.Should().BeTrue();
        }

        [Fact]
        public async Task HasNoEquipments_WhenHasItems_ReturnsFalse()
        {
            // Arrange
            ServiceLocator.Reset();
            ServiceLocator.ConfigureForTesting();
            var repository = ServiceLocator.GetService<IUploadedWorkflowRepository>();
            var dataStore = await repository.LoadAsync();
            dataStore.WorkflowSessions.Add(CreateWorkflowSession("1", HardwareType.Equipment, ComponentState.Ready, "Equip1"));
            await repository.SaveAsync(dataStore);

            // Act
            var viewModel = new ComponentListViewModel();
            await Task.Delay(1000);

            // Assert
            viewModel.HasNoEquipments.Should().BeFalse();
        }

        [Fact]
        public void HasNoSystems_WhenEmpty_ReturnsTrue()
        {
            ServiceLocator.Reset();
            ServiceLocator.ConfigureForTesting();
            var viewModel = new ComponentListViewModel();
            viewModel.HasNoSystems.Should().BeTrue();
        }

        [Fact]
        public void HasNoUnits_WhenEmpty_ReturnsTrue()
        {
            ServiceLocator.Reset();
            ServiceLocator.ConfigureForTesting();
            var viewModel = new ComponentListViewModel();
            viewModel.HasNoUnits.Should().BeTrue();
        }

        [Fact]
        public void HasNoDevices_WhenEmpty_ReturnsTrue()
        {
            ServiceLocator.Reset();
            ServiceLocator.ConfigureForTesting();
            var viewModel = new ComponentListViewModel();
            viewModel.HasNoDevices.Should().BeTrue();
        }

        #endregion

        #region ViewComponentCommand Tests

        [Fact]
        public async Task ViewComponentCommand_Execute_TriggersNavigationWithCorrectId()
        {
            // Arrange
            ServiceLocator.Reset();
            ServiceLocator.ConfigureForTesting();
            var viewModel = new ComponentListViewModel();
            NavigationTarget capturedTarget = null;
            NavigationService.Instance.ViewComponentRequested += target => capturedTarget = target;

            var componentItem = new ComponentItem
            {
                Id = "test-123",
                HardwareType = HardwareType.Equipment,
                Name = "Test"
            };

            // Act
            viewModel.ViewComponentCommand.Execute(componentItem);

            // Assert
            capturedTarget.Should().NotBeNull();
            capturedTarget.ComponentId.Should().Be("test-123");
        }

        [Fact]
        public async Task ViewComponentCommand_Execute_TriggersNavigationWithCorrectHardwareType()
        {
            // Arrange
            ServiceLocator.Reset();
            ServiceLocator.ConfigureForTesting();
            var viewModel = new ComponentListViewModel();
            NavigationTarget capturedTarget = null;
            NavigationService.Instance.ViewComponentRequested += target => capturedTarget = target;

            var componentItem = new ComponentItem
            {
                Id = "test-456",
                HardwareType = HardwareType.System,
                Name = "TestSystem"
            };

            // Act
            viewModel.ViewComponentCommand.Execute(componentItem);

            // Assert
            capturedTarget.Should().NotBeNull();
            capturedTarget.HardwareType.Should().Be(HardwareType.System);
        }

        #endregion

        #region RefreshAsync Tests

        [Fact]
        public async Task RefreshAsync_ReloadsAllComponentCollections()
        {
            // Arrange
            ServiceLocator.Reset();
            ServiceLocator.ConfigureForTesting();
            var viewModel = new ComponentListViewModel();
            await Task.Delay(500);

            // Initially empty
            viewModel.Equipments.Should().HaveCount(0);

            // Add data
            var repository = ServiceLocator.GetService<IUploadedWorkflowRepository>();
            var dataStore = await repository.LoadAsync();
            dataStore.WorkflowSessions.Add(CreateWorkflowSession("1", HardwareType.Equipment, ComponentState.Ready, "NewEquip"));
            await repository.SaveAsync(dataStore);

            // Act
            viewModel.RefreshAsync();
            await Task.Delay(1000);

            // Assert
            viewModel.Equipments.Should().HaveCount(1);
        }

        #endregion

        #region NavigateBackCommand Tests

        [Fact]
        public void Constructor_Default_NavigateBackCommandIsNotNull()
        {
            ServiceLocator.Reset();
            ServiceLocator.ConfigureForTesting();
            var viewModel = new ComponentListViewModel();
            viewModel.NavigateBackCommand.Should().NotBeNull();
        }

        [Fact]
        public void NavigateBackCommand_CanExecute_ReturnsTrue()
        {
            ServiceLocator.Reset();
            ServiceLocator.ConfigureForTesting();
            var viewModel = new ComponentListViewModel();
            viewModel.NavigateBackCommand.CanExecute(null).Should().BeTrue();
        }

        [Fact]
        public void NavigateBackCommand_Execute_NavigatesToDashboard()
        {
            // Arrange
            ServiceLocator.Reset();
            ServiceLocator.ConfigureForTesting();
            var viewModel = new ComponentListViewModel();
            bool dashboardNavigationRequested = false;
            NavigationService.Instance.NavigateToDashboardRequested += () => dashboardNavigationRequested = true;

            // Act
            viewModel.NavigateBackCommand.Execute(null);

            // Assert
            dashboardNavigationRequested.Should().BeTrue();
        }

        #endregion
    }
}