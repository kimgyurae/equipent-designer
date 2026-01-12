using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using EquipmentDesigner.Models;
using EquipmentDesigner.Services;
using FluentAssertions;
using Xunit;

namespace EquipmentDesigner.Tests.Services.Api
{
    public class HardwareApiServiceTests
    {
        #region Interface Contract Tests

        [Fact]
        public void MockHardwareApiService_ImplementsIHardwareApiService()
        {
            // Arrange
            var repository = new MemoryUploadedWorkflowRepository();

            // Act
            var service = new MockHardwareApiService(repository);

            // Assert
            service.Should().BeAssignableTo<IHardwareApiService>();
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenRepositoryIsNull()
        {
            // Arrange & Act
            Action act = () => new MockHardwareApiService(null);

            // Assert
            act.Should().Throw<ArgumentNullException>()
               .WithParameterName("repository");
        }

        #endregion

        #region GetAllSessionsAsync Tests

        [Fact]
        public async Task GetAllSessionsAsync_ReturnsEmptyList_WhenNoSessionsExist()
        {
            // Arrange
            var repository = new MemoryUploadedWorkflowRepository();
            var service = new MockHardwareApiService(repository);

            // Act
            var response = await service.GetAllSessionsAsync();

            // Assert
            response.Success.Should().BeTrue();
            response.Data.Should().NotBeNull();
            response.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllSessionsAsync_ReturnsAllSessions_WhenMultipleSessionsExist()
        {
            // Arrange
            var repository = new MemoryUploadedWorkflowRepository();
            var dataStore = new List<HardwareDefinition>();
            dataStore.Add(new HardwareDefinition { Id = "wf-1" });
            dataStore.Add(new HardwareDefinition { Id = "wf-2" });
            await repository.SaveAsync(dataStore);
            var service = new MockHardwareApiService(repository);

            // Act
            var response = await service.GetAllSessionsAsync();

            // Assert
            response.Success.Should().BeTrue();
            response.Data.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetAllSessionsAsync_IncludesSimulatedDelay()
        {
            // Arrange
            var repository = new MemoryUploadedWorkflowRepository();
            var service = new MockHardwareApiService(repository);
            var expectedMinDelay = TimeSpan.FromMilliseconds(250);

            // Act
            var sw = Stopwatch.StartNew();
            await service.GetAllSessionsAsync();
            sw.Stop();

            // Assert
            sw.Elapsed.Should().BeGreaterThan(expectedMinDelay);
        }

        #endregion

        #region GetSessionsByStateAsync Tests

        [Fact]
        public async Task GetSessionsByStateAsync_ReturnsOnlyMatchingSessions()
        {
            // Arrange
            var repository = new MemoryUploadedWorkflowRepository();
            var dataStore = new List<HardwareDefinition>();
            dataStore.Add(new HardwareDefinition { Id = "wf-1", State = ComponentState.Uploaded });
            dataStore.Add(new HardwareDefinition { Id = "wf-2", State = ComponentState.Validated });
            dataStore.Add(new HardwareDefinition { Id = "wf-3", State = ComponentState.Draft });
            await repository.SaveAsync(dataStore);
            var service = new MockHardwareApiService(repository);

            // Act
            var response = await service.GetSessionsByStateAsync(ComponentState.Uploaded, ComponentState.Validated);

            // Assert
            response.Success.Should().BeTrue();
            response.Data.Should().HaveCount(2);
            response.Data.Should().OnlyContain(s =>
                s.State == ComponentState.Uploaded || s.State == ComponentState.Validated);
        }

        [Fact]
        public async Task GetSessionsByStateAsync_ReturnsEmptyList_WhenNoSessionsMatchFilter()
        {
            // Arrange
            var repository = new MemoryUploadedWorkflowRepository();
            var dataStore = new List<HardwareDefinition>();
            dataStore.Add(new HardwareDefinition { Id = "wf-1", State = ComponentState.Draft });
            await repository.SaveAsync(dataStore);
            var service = new MockHardwareApiService(repository);

            // Act
            var response = await service.GetSessionsByStateAsync(ComponentState.Uploaded);

            // Assert
            response.Success.Should().BeTrue();
            response.Data.Should().BeEmpty();
        }

        #endregion

        #region GetSessionByIdAsync Tests

        [Fact]
        public async Task GetSessionByIdAsync_ReturnsSession_WhenIdExists()
        {
            // Arrange
            var repository = new MemoryUploadedWorkflowRepository();
            var dataStore = new List<HardwareDefinition>();
            dataStore.Add(new HardwareDefinition { Id = "wf-target" });
            await repository.SaveAsync(dataStore);
            var service = new MockHardwareApiService(repository);

            // Act
            var response = await service.GetSessionByIdAsync("wf-target");

            // Assert
            response.Success.Should().BeTrue();
            response.Data.Id.Should().Be("wf-target");
        }

        [Fact]
        public async Task GetSessionByIdAsync_ReturnsNotFound_WhenIdDoesNotExist()
        {
            // Arrange
            var repository = new MemoryUploadedWorkflowRepository();
            var service = new MockHardwareApiService(repository);

            // Act
            var response = await service.GetSessionByIdAsync("non-existent");

            // Assert
            response.Success.Should().BeFalse();
            response.ErrorCode.Should().Be("NOT_FOUND");
        }

        [Fact]
        public async Task GetSessionByIdAsync_ReturnsInvalidId_WhenIdIsNull()
        {
            // Arrange
            var repository = new MemoryUploadedWorkflowRepository();
            var service = new MockHardwareApiService(repository);

            // Act
            var response = await service.GetSessionByIdAsync(null);

            // Assert
            response.Success.Should().BeFalse();
            response.ErrorCode.Should().Be("INVALID_ID");
        }

        [Fact]
        public async Task GetSessionByIdAsync_ReturnsInvalidId_WhenIdIsEmpty()
        {
            // Arrange
            var repository = new MemoryUploadedWorkflowRepository();
            var service = new MockHardwareApiService(repository);

            // Act
            var response = await service.GetSessionByIdAsync("");

            // Assert
            response.Success.Should().BeFalse();
            response.ErrorCode.Should().Be("INVALID_ID");
        }

        #endregion

        #region SaveSessionAsync Tests

        [Fact]
        public async Task SaveSessionAsync_AddsNewSession_WhenIdDoesNotExist()
        {
            // Arrange
            var repository = new MemoryUploadedWorkflowRepository();
            var service = new MockHardwareApiService(repository);
            var session = new HardwareDefinition { Id = "new-wf", State = ComponentState.Uploaded };

            // Act
            var response = await service.SaveSessionAsync(session);

            // Assert
            response.Success.Should().BeTrue();
            var allSessions = await service.GetAllSessionsAsync();
            allSessions.Data.Should().HaveCount(1);
            allSessions.Data.First().Id.Should().Be("new-wf");
        }

        [Fact]
        public async Task SaveSessionAsync_UpdatesExistingSession_WhenIdExists()
        {
            // Arrange
            var repository = new MemoryUploadedWorkflowRepository();
            var dataStore = new List<HardwareDefinition>();
            dataStore.Add(new HardwareDefinition
            {
                Id = "existing-wf",
                State = ComponentState.Uploaded
            });
            await repository.SaveAsync(dataStore);
            var service = new MockHardwareApiService(repository);

            // Act
            var updatedSession = new HardwareDefinition
            {
                Id = "existing-wf",
                State = ComponentState.Validated
            };
            var response = await service.SaveSessionAsync(updatedSession);

            // Assert
            response.Success.Should().BeTrue();
            var loaded = await service.GetSessionByIdAsync("existing-wf");
            loaded.Data.State.Should().Be(ComponentState.Validated);
        }

        [Fact]
        public async Task SaveSessionAsync_SetsLastModifiedAt()
        {
            // Arrange
            var repository = new MemoryUploadedWorkflowRepository();
            var service = new MockHardwareApiService(repository);
            var beforeSave = DateTime.Now;
            var session = new HardwareDefinition { Id = "new-wf" };

            // Act
            var response = await service.SaveSessionAsync(session);

            // Assert
            response.Success.Should().BeTrue();
            response.Data.LastModifiedAt.Should().BeOnOrAfter(beforeSave);
        }

        [Fact]
        public async Task SaveSessionAsync_ReturnsInvalidSession_WhenSessionIsNull()
        {
            // Arrange
            var repository = new MemoryUploadedWorkflowRepository();
            var service = new MockHardwareApiService(repository);

            // Act
            var response = await service.SaveSessionAsync(null);

            // Assert
            response.Success.Should().BeFalse();
            response.ErrorCode.Should().Be("INVALID_SESSION");
        }

        [Fact]
        public async Task SaveSessionAsync_ReturnsInvalidId_WhenWorkflowIdIsNull()
        {
            // Arrange
            var repository = new MemoryUploadedWorkflowRepository();
            var service = new MockHardwareApiService(repository);
            var session = new HardwareDefinition { Id = null };

            // Act
            var response = await service.SaveSessionAsync(session);

            // Assert
            response.Success.Should().BeFalse();
            response.ErrorCode.Should().Be("INVALID_ID");
        }

        #endregion

        #region UpdateSessionStateAsync Tests

        [Fact]
        public async Task UpdateSessionStateAsync_UpdatesState_WhenSessionExists()
        {
            // Arrange
            var repository = new MemoryUploadedWorkflowRepository();
            var dataStore = new List<HardwareDefinition>();
            dataStore.Add(new HardwareDefinition
            {
                Id = "wf-1",
                State = ComponentState.Uploaded
            });
            await repository.SaveAsync(dataStore);
            var service = new MockHardwareApiService(repository);

            // Act
            var response = await service.UpdateSessionStateAsync("wf-1", ComponentState.Draft);

            // Assert
            response.Success.Should().BeTrue();
            response.Data.State.Should().Be(ComponentState.Draft);
        }

        [Fact]
        public async Task UpdateSessionStateAsync_UpdatesLastModifiedAt()
        {
            // Arrange
            var repository = new MemoryUploadedWorkflowRepository();
            var originalTime = DateTime.Now.AddDays(-1);
            var dataStore = new List<HardwareDefinition>();
            dataStore.Add(new HardwareDefinition
            {
                Id = "wf-1",
                State = ComponentState.Uploaded,
                LastModifiedAt = originalTime
            });
            await repository.SaveAsync(dataStore);
            var service = new MockHardwareApiService(repository);

            // Act
            var response = await service.UpdateSessionStateAsync("wf-1", ComponentState.Draft);

            // Assert
            response.Success.Should().BeTrue();
            response.Data.LastModifiedAt.Should().BeAfter(originalTime);
        }

        [Fact]
        public async Task UpdateSessionStateAsync_ReturnsNotFound_WhenSessionDoesNotExist()
        {
            // Arrange
            var repository = new MemoryUploadedWorkflowRepository();
            var service = new MockHardwareApiService(repository);

            // Act
            var response = await service.UpdateSessionStateAsync("non-existent", ComponentState.Draft);

            // Assert
            response.Success.Should().BeFalse();
            response.ErrorCode.Should().Be("NOT_FOUND");
        }

        [Fact]
        public async Task UpdateSessionStateAsync_ReturnsInvalidId_WhenIdIsNull()
        {
            // Arrange
            var repository = new MemoryUploadedWorkflowRepository();
            var service = new MockHardwareApiService(repository);

            // Act
            var response = await service.UpdateSessionStateAsync(null, ComponentState.Draft);

            // Assert
            response.Success.Should().BeFalse();
            response.ErrorCode.Should().Be("INVALID_ID");
        }

        #endregion

        #region DeleteSessionAsync Tests

        [Fact]
        public async Task DeleteSessionAsync_RemovesSession_WhenIdExists()
        {
            // Arrange
            var repository = new MemoryUploadedWorkflowRepository();
            var dataStore = new List<HardwareDefinition>();
            dataStore.Add(new HardwareDefinition { Id = "wf-to-delete" });
            await repository.SaveAsync(dataStore);
            var service = new MockHardwareApiService(repository);

            // Act
            var response = await service.DeleteSessionAsync("wf-to-delete");

            // Assert
            response.Success.Should().BeTrue();
            response.Data.Should().BeTrue();
            var allSessions = await service.GetAllSessionsAsync();
            allSessions.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task DeleteSessionAsync_ReturnsNotFound_WhenIdDoesNotExist()
        {
            // Arrange
            var repository = new MemoryUploadedWorkflowRepository();
            var service = new MockHardwareApiService(repository);

            // Act
            var response = await service.DeleteSessionAsync("non-existent");

            // Assert
            response.Success.Should().BeFalse();
            response.ErrorCode.Should().Be("NOT_FOUND");
        }

        [Fact]
        public async Task DeleteSessionAsync_ReturnsInvalidId_WhenIdIsNull()
        {
            // Arrange
            var repository = new MemoryUploadedWorkflowRepository();
            var service = new MockHardwareApiService(repository);

            // Act
            var response = await service.DeleteSessionAsync(null);

            // Assert
            response.Success.Should().BeFalse();
            response.ErrorCode.Should().Be("INVALID_ID");
        }

        #endregion
    }
}