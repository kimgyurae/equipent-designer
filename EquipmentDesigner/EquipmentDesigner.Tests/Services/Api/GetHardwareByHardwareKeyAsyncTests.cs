using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EquipmentDesigner.Models;
using EquipmentDesigner.Services;
using FluentAssertions;
using Xunit;

namespace EquipmentDesigner.Tests.Services.Api
{
    /// <summary>
    /// Tests for the GetHardwareByHardwareKeyAsync API method.
    /// This method returns a paginated list of hardware versions filtered by HardwareKey.
    /// </summary>
    public class GetHardwareByHardwareKeyAsyncTests
    {
        #region Helper Methods

        private HardwareDefinition CreateHardwareDefinition(
            string id,
            HardwareType hardwareType,
            string hardwareKey,
            string name,
            string version,
            ComponentState state,
            string description = null)
        {
            return new HardwareDefinition
            {
                Id = id,
                HardwareType = hardwareType,
                HardwareKey = hardwareKey,
                Name = name,
                Version = version,
                State = state,
                Description = description,
                LastModifiedAt = DateTime.Now
            };
        }

        #endregion

        #region Interface Contract Tests

        [Fact]
        public void IHardwareApiService_ShouldDefineGetHardwareByHardwareKeyAsyncMethod()
        {
            // Arrange
            var serviceType = typeof(IHardwareApiService);

            // Act
            var method = serviceType.GetMethod("GetHardwareByHardwareKeyAsync");

            // Assert
            method.Should().NotBeNull("IHardwareApiService should define GetHardwareByHardwareKeyAsync method");
            method.GetParameters().Should().HaveCount(3);
            method.GetParameters()[0].ParameterType.Should().Be(typeof(string), "First parameter should be hardwareKey");
            method.GetParameters()[1].ParameterType.Should().Be(typeof(int), "Second parameter should be page");
            method.GetParameters()[2].ParameterType.Should().Be(typeof(int), "Third parameter should be pageSize");
        }

        #endregion

        #region Basic Functionality Tests

        [Fact]
        public async Task GetHardwareByHardwareKeyAsync_ReturnsEmptyList_WhenNoMatchingHardwareKeyExists()
        {
            // Arrange
            var repository = new MemoryUploadedWorkflowRepository();
            var service = new MockHardwareApiService(repository);

            // Act
            var response = await service.GetHardwareByHardwareKeyAsync("non-existent-key");

            // Assert
            response.Success.Should().BeTrue();
            response.Data.Should().NotBeNull();
            response.Data.Should().BeEmpty();
            response.TotalCount.Should().Be(0);
        }

        [Fact]
        public async Task GetHardwareByHardwareKeyAsync_ReturnsMatchingVersions_WhenHardwareKeyExists()
        {
            // Arrange
            var repository = new MemoryUploadedWorkflowRepository();
            var dataStore = new List<HardwareDefinition>();
            dataStore.Add(CreateHardwareDefinition(
                "wf-1", HardwareType.Equipment, "auto-assembler", "Auto Assembler",
                "v2.0.0", ComponentState.Validated, "Latest version"));
            dataStore.Add(CreateHardwareDefinition(
                "wf-2", HardwareType.Equipment, "auto-assembler", "Auto Assembler",
                "v1.0.0", ComponentState.Uploaded, "Initial release"));
            await repository.SaveAsync(dataStore);
            var service = new MockHardwareApiService(repository);

            // Act
            var response = await service.GetHardwareByHardwareKeyAsync("auto-assembler");

            // Assert
            response.Success.Should().BeTrue();
            response.Data.Should().HaveCount(2);
            response.TotalCount.Should().Be(2);
        }

        [Fact]
        public async Task GetHardwareByHardwareKeyAsync_FiltersAcrossAllHardwareTypes()
        {
            // Arrange - Same HardwareKey across different hardware types
            var repository = new MemoryUploadedWorkflowRepository();
            var dataStore = new List<HardwareDefinition>();
            dataStore.Add(CreateHardwareDefinition(
                "wf-1", HardwareType.Equipment, "test-key", "Test",
                "v1.0.0", ComponentState.Validated));
            dataStore.Add(CreateHardwareDefinition(
                "wf-2", HardwareType.System, "test-key", "Test",
                "v1.0.0", ComponentState.Validated));
            dataStore.Add(CreateHardwareDefinition(
                "wf-3", HardwareType.Unit, "test-key", "Test",
                "v1.0.0", ComponentState.Validated));
            await repository.SaveAsync(dataStore);
            var service = new MockHardwareApiService(repository);

            // Act
            var response = await service.GetHardwareByHardwareKeyAsync("test-key");

            // Assert
            response.Success.Should().BeTrue();
            response.Data.Should().HaveCount(3);
        }

        [Fact]
        public async Task GetHardwareByHardwareKeyAsync_UsesNameAsFallback_WhenHardwareKeyIsNull()
        {
            // Arrange
            var repository = new MemoryUploadedWorkflowRepository();
            var dataStore = new List<HardwareDefinition>();
            dataStore.Add(CreateHardwareDefinition(
                "wf-1", HardwareType.Equipment, null, "Auto Assembler",
                "v1.0.0", ComponentState.Validated));
            await repository.SaveAsync(dataStore);
            var service = new MockHardwareApiService(repository);

            // Act - Search by Name since HardwareKey is null
            var response = await service.GetHardwareByHardwareKeyAsync("Auto Assembler");

            // Assert
            response.Success.Should().BeTrue();
            response.Data.Should().HaveCount(1);
        }

        #endregion

        #region Version Sorting Tests

        [Fact]
        public async Task GetHardwareByHardwareKeyAsync_SortsVersionsDescending()
        {
            // Arrange
            var repository = new MemoryUploadedWorkflowRepository();
            var dataStore = new List<HardwareDefinition>();
            dataStore.Add(CreateHardwareDefinition(
                "wf-1", HardwareType.Equipment, "auto-assembler", "Auto Assembler",
                "v1.0.0", ComponentState.Validated));
            dataStore.Add(CreateHardwareDefinition(
                "wf-2", HardwareType.Equipment, "auto-assembler", "Auto Assembler",
                "v2.7.1", ComponentState.Validated));
            dataStore.Add(CreateHardwareDefinition(
                "wf-3", HardwareType.Equipment, "auto-assembler", "Auto Assembler",
                "v2.7.0", ComponentState.Uploaded));
            await repository.SaveAsync(dataStore);
            var service = new MockHardwareApiService(repository);

            // Act
            var response = await service.GetHardwareByHardwareKeyAsync("auto-assembler");

            // Assert
            response.Success.Should().BeTrue();
            response.Data[0].Version.Should().Be("v2.7.1");
            response.Data[1].Version.Should().Be("v2.7.0");
            response.Data[2].Version.Should().Be("v1.0.0");
        }

        [Fact]
        public async Task GetHardwareByHardwareKeyAsync_SetsIsLatestTrueOnlyForFirstItem()
        {
            // Arrange
            var repository = new MemoryUploadedWorkflowRepository();
            var dataStore = new List<HardwareDefinition>();
            dataStore.Add(CreateHardwareDefinition(
                "wf-1", HardwareType.Equipment, "auto-assembler", "Auto Assembler",
                "v2.0.0", ComponentState.Validated));
            dataStore.Add(CreateHardwareDefinition(
                "wf-2", HardwareType.Equipment, "auto-assembler", "Auto Assembler",
                "v1.0.0", ComponentState.Uploaded));
            await repository.SaveAsync(dataStore);
            var service = new MockHardwareApiService(repository);

            // Act
            var response = await service.GetHardwareByHardwareKeyAsync("auto-assembler");

            // Assert
            response.Data[0].IsLatest.Should().BeTrue();
            response.Data[1].IsLatest.Should().BeFalse();
        }

        #endregion

        #region Pagination Tests

        [Fact]
        public async Task GetHardwareByHardwareKeyAsync_ReturnsCorrectPageOfResults()
        {
            // Arrange
            var repository = new MemoryUploadedWorkflowRepository();
            var dataStore = new List<HardwareDefinition>();
            for (int i = 1; i <= 5; i++)
            {
                dataStore.Add(CreateHardwareDefinition(
                    $"wf-{i}", HardwareType.Equipment, "auto-assembler", "Auto Assembler",
                    $"v{i}.0.0", ComponentState.Validated));
            }
            await repository.SaveAsync(dataStore);
            var service = new MockHardwareApiService(repository);

            // Act - Get first page with 2 items per page
            var response = await service.GetHardwareByHardwareKeyAsync("auto-assembler", page: 1, pageSize: 2);

            // Assert
            response.Success.Should().BeTrue();
            response.Data.Should().HaveCount(2);
            response.Page.Should().Be(1);
            response.PageSize.Should().Be(2);
            response.TotalCount.Should().Be(5);
            response.TotalPages.Should().Be(3);
            response.HasNextPage.Should().BeTrue();
            response.HasPreviousPage.Should().BeFalse();
        }

        [Fact]
        public async Task GetHardwareByHardwareKeyAsync_ReturnsSecondPageCorrectly()
        {
            // Arrange
            var repository = new MemoryUploadedWorkflowRepository();
            var dataStore = new List<HardwareDefinition>();
            for (int i = 1; i <= 5; i++)
            {
                dataStore.Add(CreateHardwareDefinition(
                    $"wf-{i}", HardwareType.Equipment, "auto-assembler", "Auto Assembler",
                    $"v{i}.0.0", ComponentState.Validated));
            }
            await repository.SaveAsync(dataStore);
            var service = new MockHardwareApiService(repository);

            // Act - Get second page
            var response = await service.GetHardwareByHardwareKeyAsync("auto-assembler", page: 2, pageSize: 2);

            // Assert
            response.Success.Should().BeTrue();
            response.Data.Should().HaveCount(2);
            response.Page.Should().Be(2);
            response.HasNextPage.Should().BeTrue();
            response.HasPreviousPage.Should().BeTrue();
        }

        [Fact]
        public async Task GetHardwareByHardwareKeyAsync_ReturnsLastPageCorrectly()
        {
            // Arrange
            var repository = new MemoryUploadedWorkflowRepository();
            var dataStore = new List<HardwareDefinition>();
            for (int i = 1; i <= 5; i++)
            {
                dataStore.Add(CreateHardwareDefinition(
                    $"wf-{i}", HardwareType.Equipment, "auto-assembler", "Auto Assembler",
                    $"v{i}.0.0", ComponentState.Validated));
            }
            await repository.SaveAsync(dataStore);
            var service = new MockHardwareApiService(repository);

            // Act - Get last page (page 3 with 2 items per page = 1 item)
            var response = await service.GetHardwareByHardwareKeyAsync("auto-assembler", page: 3, pageSize: 2);

            // Assert
            response.Success.Should().BeTrue();
            response.Data.Should().HaveCount(1);
            response.Page.Should().Be(3);
            response.HasNextPage.Should().BeFalse();
            response.HasPreviousPage.Should().BeTrue();
        }

        [Fact]
        public async Task GetHardwareByHardwareKeyAsync_IsLatestIsFalse_WhenNotOnFirstPage()
        {
            // Arrange
            var repository = new MemoryUploadedWorkflowRepository();
            var dataStore = new List<HardwareDefinition>();
            for (int i = 1; i <= 5; i++)
            {
                dataStore.Add(CreateHardwareDefinition(
                    $"wf-{i}", HardwareType.Equipment, "auto-assembler", "Auto Assembler",
                    $"v{i}.0.0", ComponentState.Validated));
            }
            await repository.SaveAsync(dataStore);
            var service = new MockHardwareApiService(repository);

            // Act - Get second page
            var response = await service.GetHardwareByHardwareKeyAsync("auto-assembler", page: 2, pageSize: 2);

            // Assert
            response.Success.Should().BeTrue();
            response.Data.Should().OnlyContain(v => v.IsLatest == false);
        }

        [Fact]
        public async Task GetHardwareByHardwareKeyAsync_UsesDefaultPagination_WhenNotSpecified()
        {
            // Arrange
            var repository = new MemoryUploadedWorkflowRepository();
            var dataStore = new List<HardwareDefinition>();
            dataStore.Add(CreateHardwareDefinition(
                "wf-1", HardwareType.Equipment, "auto-assembler", "Auto Assembler",
                "v1.0.0", ComponentState.Validated));
            await repository.SaveAsync(dataStore);
            var service = new MockHardwareApiService(repository);

            // Act - Call without pagination parameters
            var response = await service.GetHardwareByHardwareKeyAsync("auto-assembler");

            // Assert
            response.Success.Should().BeTrue();
            response.Page.Should().Be(1);
            response.PageSize.Should().Be(10); // Default page size
        }

        #endregion

        #region Response Data Tests

        [Fact]
        public async Task GetHardwareByHardwareKeyAsync_PopulatesAllRequiredFields()
        {
            // Arrange
            var repository = new MemoryUploadedWorkflowRepository();
            var dataStore = new List<HardwareDefinition>();
            dataStore.Add(CreateHardwareDefinition(
                "wf-1", HardwareType.Equipment, "auto-assembler", "Auto Assembler",
                "v1.0.0", ComponentState.Validated, "Test description"));
            await repository.SaveAsync(dataStore);
            var service = new MockHardwareApiService(repository);

            // Act
            var response = await service.GetHardwareByHardwareKeyAsync("auto-assembler");

            // Assert
            response.Success.Should().BeTrue();
            response.Data.Should().HaveCount(1);
            var version = response.Data[0];
            version.WorkflowId.Should().Be("wf-1");
            version.Version.Should().Be("v1.0.0");
            version.State.Should().Be(ComponentState.Validated);
            version.Description.Should().Be("Test description");
            version.IsLatest.Should().BeTrue();
        }

        [Fact]
        public async Task GetHardwareByHardwareKeyAsync_HandlesNullVersion()
        {
            // Arrange
            var repository = new MemoryUploadedWorkflowRepository();
            var dataStore = new List<HardwareDefinition>();
            dataStore.Add(new HardwareDefinition
            {
                Id = "wf-1",
                HardwareType = HardwareType.Equipment,
                HardwareKey = "auto-assembler",
                Name = "Auto Assembler",
                Version = null,
                State = ComponentState.Validated,
                LastModifiedAt = DateTime.Now
            });
            await repository.SaveAsync(dataStore);
            var service = new MockHardwareApiService(repository);

            // Act
            var response = await service.GetHardwareByHardwareKeyAsync("auto-assembler");

            // Assert
            response.Success.Should().BeTrue();
            response.Data[0].Version.Should().Be("v0.0.0");
        }

        #endregion
    }
}
