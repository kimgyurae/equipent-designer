using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EquipmentDesigner.Models;
using EquipmentDesigner.Services;
using FluentAssertions;
using Xunit;

namespace EquipmentDesigner.Tests.Services.Api
{
    /// <summary>
    /// TDD Tests for Hardware Version Selection feature - API Service layer
    /// RED Phase: These tests should fail initially until implementation is complete
    /// </summary>
    public class HardwareVersionApiTests
    {
        #region Helper Methods

        private WorkflowSessionDto CreateWorkflowSession(
            string id,
            HardwareLayer layer,
            string hardwareKey,
            string name,
            string version,
            ComponentState state,
            DateTime lastModifiedAt)
        {
            var session = new WorkflowSessionDto
            {
                Id = id,
                HardwareType = layer,
                State = state,
                LastModifiedAt = lastModifiedAt,
                TreeNodes = new List<TreeNodeDataDto>()
            };

            var treeNode = new TreeNodeDataDto
            {
                Id = $"node-{id}",
                HardwareLayer = layer
            };

            switch (layer)
            {
                case HardwareLayer.Equipment:
                    treeNode.EquipmentData = new EquipmentDto
                    {
                        Id = $"eq-{id}",
                        Name = name,
                        HardwareKey = hardwareKey,
                        Version = version
                    };
                    break;
                case HardwareLayer.System:
                    treeNode.SystemData = new SystemDto
                    {
                        Id = $"sys-{id}",
                        Name = name,
                        HardwareKey = hardwareKey,
                        Version = version
                    };
                    break;
                case HardwareLayer.Unit:
                    treeNode.UnitData = new UnitDto
                    {
                        Id = $"unit-{id}",
                        Name = name,
                        HardwareKey = hardwareKey,
                        Version = version
                    };
                    break;
                case HardwareLayer.Device:
                    treeNode.DeviceData = new DeviceDto
                    {
                        Id = $"dev-{id}",
                        Name = name,
                        HardwareKey = hardwareKey,
                        Version = version
                    };
                    break;
            }

            session.TreeNodes.Add(treeNode);
            return session;
        }

        #endregion

        #region IHardwareApiService Interface Tests

        [Fact]
        public void IHardwareApiService_ShouldDefineGetVersionHistoryAsyncMethod()
        {
            // Arrange
            var serviceType = typeof(IHardwareApiService);

            // Act
            var method = serviceType.GetMethod("GetVersionHistoryAsync");

            // Assert
            method.Should().NotBeNull("IHardwareApiService should define GetVersionHistoryAsync method");
            method.GetParameters().Should().HaveCount(2);
            method.GetParameters()[0].ParameterType.Should().Be(typeof(string), "First parameter should be hardwareKey");
            method.GetParameters()[1].ParameterType.Should().Be(typeof(HardwareLayer), "Second parameter should be hardwareLayer");
        }

        [Fact]
        public void IHardwareApiService_ShouldDefineGetDistinctHardwareKeysAsyncMethod()
        {
            // Arrange
            var serviceType = typeof(IHardwareApiService);

            // Act
            var method = serviceType.GetMethod("GetDistinctHardwareKeysAsync");

            // Assert
            method.Should().NotBeNull("IHardwareApiService should define GetDistinctHardwareKeysAsync method");
            method.GetParameters().Should().HaveCount(1);
            method.GetParameters()[0].ParameterType.Should().Be(typeof(HardwareLayer), "Parameter should be hardwareLayer");
        }

        #endregion

        #region GetVersionHistoryAsync Tests

        [Fact]
        public async Task GetVersionHistoryAsync_ReturnsSuccessWithMatchingSessions_WhenHardwareKeyExists()
        {
            // Arrange
            var repository = new MemoryUploadedWorkflowRepository();
            var dataStore = new HardwareDefinitionDataStore();
            dataStore.WorkflowSessions.Add(CreateWorkflowSession(
                "wf-1", HardwareLayer.Equipment, "auto-assembler", "Auto Assembler",
                "v2.0.0", ComponentState.Validated, DateTime.Now));
            dataStore.WorkflowSessions.Add(CreateWorkflowSession(
                "wf-2", HardwareLayer.Equipment, "auto-assembler", "Auto Assembler",
                "1.0.0", ComponentState.Uploaded, DateTime.Now.AddDays(-30)));
            await repository.SaveAsync(dataStore);
            var service = new MockHardwareApiService(repository);

            // Act
            var response = await service.GetVersionHistoryAsync("auto-assembler", HardwareLayer.Equipment);

            // Assert
            response.Success.Should().BeTrue();
            response.Data.Should().NotBeNull();
            response.Data.HardwareKey.Should().Be("auto-assembler");
            response.Data.Versions.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetVersionHistoryAsync_ReturnsFailureWithNotFoundCode_WhenHardwareKeyDoesNotExist()
        {
            // Arrange
            var repository = new MemoryUploadedWorkflowRepository();
            var service = new MockHardwareApiService(repository);

            // Act
            var response = await service.GetVersionHistoryAsync("non-existent-key", HardwareLayer.Equipment);

            // Assert
            response.Success.Should().BeFalse();
            response.ErrorCode.Should().Be("NOT_FOUND");
        }

        [Fact]
        public async Task GetVersionHistoryAsync_FiltersSessionsByHardwareLayerCorrectly()
        {
            // Arrange
            var repository = new MemoryUploadedWorkflowRepository();
            var dataStore = new HardwareDefinitionDataStore();
            // Same HardwareKey but different layers
            dataStore.WorkflowSessions.Add(CreateWorkflowSession(
                "wf-1", HardwareLayer.Equipment, "test-key", "Test",
                "1.0.0", ComponentState.Validated, DateTime.Now));
            dataStore.WorkflowSessions.Add(CreateWorkflowSession(
                "wf-2", HardwareLayer.System, "test-key", "Test",
                "1.0.0", ComponentState.Validated, DateTime.Now));
            await repository.SaveAsync(dataStore);
            var service = new MockHardwareApiService(repository);

            // Act
            var response = await service.GetVersionHistoryAsync("test-key", HardwareLayer.Equipment);

            // Assert
            response.Success.Should().BeTrue();
            response.Data.Versions.Should().HaveCount(1);
            response.Data.HardwareLayer.Should().Be(HardwareLayer.Equipment);
        }

        [Fact]
        public async Task GetVersionHistoryAsync_SortsVersionsBySemanticVersionDescending()
        {
            // Arrange
            var repository = new MemoryUploadedWorkflowRepository();
            var dataStore = new HardwareDefinitionDataStore();
            dataStore.WorkflowSessions.Add(CreateWorkflowSession(
                "wf-1", HardwareLayer.Equipment, "auto-assembler", "Auto Assembler",
                "1.0.0", ComponentState.Validated, DateTime.Now));
            dataStore.WorkflowSessions.Add(CreateWorkflowSession(
                "wf-2", HardwareLayer.Equipment, "auto-assembler", "Auto Assembler",
                "v2.7.1", ComponentState.Validated, DateTime.Now));
            dataStore.WorkflowSessions.Add(CreateWorkflowSession(
                "wf-3", HardwareLayer.Equipment, "auto-assembler", "Auto Assembler",
                "v2.7.0", ComponentState.Validated, DateTime.Now));
            await repository.SaveAsync(dataStore);
            var service = new MockHardwareApiService(repository);

            // Act
            var response = await service.GetVersionHistoryAsync("auto-assembler", HardwareLayer.Equipment);

            // Assert
            response.Success.Should().BeTrue();
            response.Data.Versions.Should().HaveCount(3);
            response.Data.Versions[0].Version.Should().Be("v2.7.1");
            response.Data.Versions[1].Version.Should().Be("v2.7.0");
            response.Data.Versions[2].Version.Should().Be("1.0.0");
        }

        [Fact]
        public async Task GetVersionHistoryAsync_SetsIsLatestTrueOnlyForFirstItem()
        {
            // Arrange
            var repository = new MemoryUploadedWorkflowRepository();
            var dataStore = new HardwareDefinitionDataStore();
            dataStore.WorkflowSessions.Add(CreateWorkflowSession(
                "wf-1", HardwareLayer.Equipment, "auto-assembler", "Auto Assembler",
                "v2.0.0", ComponentState.Validated, DateTime.Now));
            dataStore.WorkflowSessions.Add(CreateWorkflowSession(
                "wf-2", HardwareLayer.Equipment, "auto-assembler", "Auto Assembler",
                "1.0.0", ComponentState.Validated, DateTime.Now.AddDays(-30)));
            await repository.SaveAsync(dataStore);
            var service = new MockHardwareApiService(repository);

            // Act
            var response = await service.GetVersionHistoryAsync("auto-assembler", HardwareLayer.Equipment);

            // Assert
            response.Success.Should().BeTrue();
            response.Data.Versions[0].IsLatest.Should().BeTrue();
            response.Data.Versions[1].IsLatest.Should().BeFalse();
        }

        [Fact]
        public async Task GetVersionHistoryAsync_UsesNameAsHardwareKey_WhenHardwareKeyIsNull()
        {
            // Arrange
            var repository = new MemoryUploadedWorkflowRepository();
            var dataStore = new HardwareDefinitionDataStore();
            // Create session with null HardwareKey - should use Name as fallback
            dataStore.WorkflowSessions.Add(CreateWorkflowSession(
                "wf-1", HardwareLayer.Equipment, null, "Auto Assembler",
                "1.0.0", ComponentState.Validated, DateTime.Now));
            await repository.SaveAsync(dataStore);
            var service = new MockHardwareApiService(repository);

            // Act - Search by Name since HardwareKey is null
            var response = await service.GetVersionHistoryAsync("Auto Assembler", HardwareLayer.Equipment);

            // Assert
            response.Success.Should().BeTrue();
            response.Data.Versions.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetVersionHistoryAsync_PopulatesDisplayNameFromFirstSession()
        {
            // Arrange
            var repository = new MemoryUploadedWorkflowRepository();
            var dataStore = new HardwareDefinitionDataStore();
            dataStore.WorkflowSessions.Add(CreateWorkflowSession(
                "wf-1", HardwareLayer.Equipment, "auto-assembler", "Auto Assembler Display",
                "1.0.0", ComponentState.Validated, DateTime.Now));
            await repository.SaveAsync(dataStore);
            var service = new MockHardwareApiService(repository);

            // Act
            var response = await service.GetVersionHistoryAsync("auto-assembler", HardwareLayer.Equipment);

            // Assert
            response.Success.Should().BeTrue();
            response.Data.DisplayName.Should().Be("Auto Assembler Display");
        }

        [Fact]
        public async Task GetVersionHistoryAsync_SetsTotalVersionCountCorrectly()
        {
            // Arrange
            var repository = new MemoryUploadedWorkflowRepository();
            var dataStore = new HardwareDefinitionDataStore();
            dataStore.WorkflowSessions.Add(CreateWorkflowSession(
                "wf-1", HardwareLayer.Equipment, "auto-assembler", "Auto Assembler",
                "v3.0.0", ComponentState.Validated, DateTime.Now));
            dataStore.WorkflowSessions.Add(CreateWorkflowSession(
                "wf-2", HardwareLayer.Equipment, "auto-assembler", "Auto Assembler",
                "v2.0.0", ComponentState.Validated, DateTime.Now));
            dataStore.WorkflowSessions.Add(CreateWorkflowSession(
                "wf-3", HardwareLayer.Equipment, "auto-assembler", "Auto Assembler",
                "1.0.0", ComponentState.Validated, DateTime.Now));
            await repository.SaveAsync(dataStore);
            var service = new MockHardwareApiService(repository);

            // Act
            var response = await service.GetVersionHistoryAsync("auto-assembler", HardwareLayer.Equipment);

            // Assert
            response.Success.Should().BeTrue();
            response.Data.TotalVersionCount.Should().Be(3);
        }

        #endregion

        #region GetDistinctHardwareKeysAsync Tests

        [Fact]
        public async Task GetDistinctHardwareKeysAsync_ReturnsEmptyList_WhenNoSessionsExist()
        {
            // Arrange
            var repository = new MemoryUploadedWorkflowRepository();
            var service = new MockHardwareApiService(repository);

            // Act
            var response = await service.GetDistinctHardwareKeysAsync(HardwareLayer.Equipment);

            // Assert
            response.Success.Should().BeTrue();
            response.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task GetDistinctHardwareKeysAsync_ReturnsUniqueHardwareKeys()
        {
            // Arrange
            var repository = new MemoryUploadedWorkflowRepository();
            var dataStore = new HardwareDefinitionDataStore();
            dataStore.WorkflowSessions.Add(CreateWorkflowSession(
                "wf-1", HardwareLayer.Equipment, "key-a", "Item A",
                "1.0.0", ComponentState.Validated, DateTime.Now));
            dataStore.WorkflowSessions.Add(CreateWorkflowSession(
                "wf-2", HardwareLayer.Equipment, "key-b", "Item B",
                "1.0.0", ComponentState.Validated, DateTime.Now));
            dataStore.WorkflowSessions.Add(CreateWorkflowSession(
                "wf-3", HardwareLayer.Equipment, "key-a", "Item A",
                "v2.0.0", ComponentState.Validated, DateTime.Now)); // Same key, different version
            await repository.SaveAsync(dataStore);
            var service = new MockHardwareApiService(repository);

            // Act
            var response = await service.GetDistinctHardwareKeysAsync(HardwareLayer.Equipment);

            // Assert
            response.Success.Should().BeTrue();
            response.Data.Should().HaveCount(2);
            response.Data.Should().Contain("key-a");
            response.Data.Should().Contain("key-b");
        }

        [Fact]
        public async Task GetDistinctHardwareKeysAsync_UsesNameAsHardwareKey_WhenHardwareKeyIsNull()
        {
            // Arrange
            var repository = new MemoryUploadedWorkflowRepository();
            var dataStore = new HardwareDefinitionDataStore();
            dataStore.WorkflowSessions.Add(CreateWorkflowSession(
                "wf-1", HardwareLayer.Equipment, null, "Item Name",
                "1.0.0", ComponentState.Validated, DateTime.Now));
            await repository.SaveAsync(dataStore);
            var service = new MockHardwareApiService(repository);

            // Act
            var response = await service.GetDistinctHardwareKeysAsync(HardwareLayer.Equipment);

            // Assert
            response.Success.Should().BeTrue();
            response.Data.Should().Contain("Item Name");
        }

        [Fact]
        public async Task GetDistinctHardwareKeysAsync_FiltersOnlySpecifiedLayer()
        {
            // Arrange
            var repository = new MemoryUploadedWorkflowRepository();
            var dataStore = new HardwareDefinitionDataStore();
            dataStore.WorkflowSessions.Add(CreateWorkflowSession(
                "wf-1", HardwareLayer.Equipment, "eq-key", "Equipment",
                "1.0.0", ComponentState.Validated, DateTime.Now));
            dataStore.WorkflowSessions.Add(CreateWorkflowSession(
                "wf-2", HardwareLayer.System, "sys-key", "System",
                "1.0.0", ComponentState.Validated, DateTime.Now));
            await repository.SaveAsync(dataStore);
            var service = new MockHardwareApiService(repository);

            // Act
            var response = await service.GetDistinctHardwareKeysAsync(HardwareLayer.Equipment);

            // Assert
            response.Success.Should().BeTrue();
            response.Data.Should().HaveCount(1);
            response.Data.Should().Contain("eq-key");
            response.Data.Should().NotContain("sys-key");
        }

        #endregion

        #region Semantic Version Parsing Tests

        [Fact]
        public async Task GetVersionHistoryAsync_ParsesVersionWithVPrefix()
        {
            // Arrange
            var repository = new MemoryUploadedWorkflowRepository();
            var dataStore = new HardwareDefinitionDataStore();
            dataStore.WorkflowSessions.Add(CreateWorkflowSession(
                "wf-1", HardwareLayer.Equipment, "test-key", "Test",
                "1.0.0", ComponentState.Validated, DateTime.Now));
            await repository.SaveAsync(dataStore);
            var service = new MockHardwareApiService(repository);

            // Act
            var response = await service.GetVersionHistoryAsync("test-key", HardwareLayer.Equipment);

            // Assert
            response.Success.Should().BeTrue();
            response.Data.Versions[0].Version.Should().Be("1.0.0");
        }

        [Fact]
        public async Task GetVersionHistoryAsync_ParsesVersionWithoutVPrefix()
        {
            // Arrange
            var repository = new MemoryUploadedWorkflowRepository();
            var dataStore = new HardwareDefinitionDataStore();
            dataStore.WorkflowSessions.Add(CreateWorkflowSession(
                "wf-1", HardwareLayer.Equipment, "test-key", "Test",
                "1.0.0", ComponentState.Validated, DateTime.Now));
            await repository.SaveAsync(dataStore);
            var service = new MockHardwareApiService(repository);

            // Act
            var response = await service.GetVersionHistoryAsync("test-key", HardwareLayer.Equipment);

            // Assert
            response.Success.Should().BeTrue();
            response.Data.Versions[0].Version.Should().Be("1.0.0");
        }

        [Fact]
        public async Task GetVersionHistoryAsync_HandlesVersionWithDifferentComponentCounts()
        {
            // Arrange
            var repository = new MemoryUploadedWorkflowRepository();
            var dataStore = new HardwareDefinitionDataStore();
            dataStore.WorkflowSessions.Add(CreateWorkflowSession(
                "wf-1", HardwareLayer.Equipment, "test-key", "Test",
                "v1.0", ComponentState.Validated, DateTime.Now));
            dataStore.WorkflowSessions.Add(CreateWorkflowSession(
                "wf-2", HardwareLayer.Equipment, "test-key", "Test",
                "1.0.0", ComponentState.Validated, DateTime.Now));
            await repository.SaveAsync(dataStore);
            var service = new MockHardwareApiService(repository);

            // Act
            var response = await service.GetVersionHistoryAsync("test-key", HardwareLayer.Equipment);

            // Assert
            response.Success.Should().BeTrue();
            response.Data.Versions.Should().HaveCount(2);
        }

        #endregion
    }
}
