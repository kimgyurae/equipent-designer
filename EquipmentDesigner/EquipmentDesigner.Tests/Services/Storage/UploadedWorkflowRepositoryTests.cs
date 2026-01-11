using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using EquipmentDesigner.Services;
using EquipmentDesigner.Models;

namespace EquipmentDesigner.Tests.Services.Storage
{
    public class UploadedWorkflowRepositoryTests : IDisposable
    {
        private readonly string _testFilePath;
        private readonly UploadedWorkflowRepository _repository;

        public UploadedWorkflowRepositoryTests()
        {
            _testFilePath = Path.Combine(Path.GetTempPath(), $"test_uploaded_workflows_{Guid.NewGuid()}.json");
            _repository = new UploadedWorkflowRepository(_testFilePath);
        }

        public void Dispose()
        {
            if (File.Exists(_testFilePath))
            {
                File.Delete(_testFilePath);
            }
        }

        #region HardwareDefinitionDataStore Structure Tests

        [Fact]
        public void HardwareDefinitionDataStore_Version_HasDefaultValue1Point0()
        {
            // Arrange & Act
            var dataStore = new HardwareDefinitionDataStore();

            // Assert
            dataStore.Version.Should().Be("1.0");
        }

        [Fact]
        public void HardwareDefinitionDataStore_LastSavedAt_IsDateTimeProperty()
        {
            // Arrange & Act
            var dataStore = new HardwareDefinitionDataStore();
            var testTime = DateTime.Now;
            dataStore.LastSavedAt = testTime;

            // Assert
            dataStore.LastSavedAt.Should().Be(testTime);
        }

        [Fact]
        public void HardwareDefinitionDataStore_WorkflowSessions_IsInitializedAsEmptyList()
        {
            // Arrange & Act
            var dataStore = new HardwareDefinitionDataStore();

            // Assert
            dataStore.WorkflowSessions.Should().NotBeNull();
            dataStore.WorkflowSessions.Should().BeEmpty();
        }

        #endregion

        #region Repository Interface Implementation Tests

        [Fact]
        public void UploadedWorkflowRepository_ImplementsITypedDataRepository()
        {
            // Assert
            _repository.Should().BeAssignableTo<IUploadedWorkflowRepository>();
        }

        [Fact]
        public void Repository_UsesCorrectDefaultFilePath()
        {
            // Arrange
            var repository = new UploadedWorkflowRepository();
            
            // Assert - The repository should be created without exceptions
            repository.Should().NotBeNull();
        }

        #endregion

        #region LoadAsync Tests

        [Fact]
        public async Task LoadAsync_WhenFileNotExists_ReturnsNewEmptyDataStore()
        {
            // Arrange - ensure file doesn't exist
            if (File.Exists(_testFilePath))
                File.Delete(_testFilePath);

            // Act
            var result = await _repository.LoadAsync();

            // Assert
            result.Should().NotBeNull();
            result.Version.Should().Be("1.0");
            result.WorkflowSessions.Should().NotBeNull();
            result.WorkflowSessions.Should().BeEmpty();
        }

        [Fact]
        public async Task LoadAsync_WhenJsonCorrupted_ReturnsEmptyDataStoreGracefully()
        {
            // Arrange
            await File.WriteAllTextAsync(_testFilePath, "{ invalid json content");

            // Act
            var result = await _repository.LoadAsync();

            // Assert
            result.Should().NotBeNull();
            result.Version.Should().Be("1.0");
            result.WorkflowSessions.Should().BeEmpty();
        }

        [Fact]
        public async Task LoadAsync_DeserializesNestedTreeNodeDataDtoStructure()
        {
            // Arrange
            var dataStore = new HardwareDefinitionDataStore();
            var session = new WorkflowSessionDto
            {
                Id = "wf-tree",
                HardwareType = HardwareLayer.Equipment,
                State = ComponentState.Uploaded,
                TreeNodes = new System.Collections.Generic.List<TreeNodeDataDto>
                {
                    new TreeNodeDataDto
                    {
                        Id = "node-1",
                        HardwareLayer = HardwareLayer.Equipment,
                        EquipmentData = new EquipmentDto { Name = "TestEquipment" },
                        Children = new System.Collections.Generic.List<TreeNodeDataDto>
                        {
                            new TreeNodeDataDto
                            {
                                Id = "node-2",
                                HardwareLayer = HardwareLayer.System,
                                SystemData = new SystemDto { Name = "TestSystem" }
                            }
                        }
                    }
                }
            };
            dataStore.WorkflowSessions.Add(session);
            await _repository.SaveAsync(dataStore);

            // Act
            var loaded = await _repository.LoadAsync();

            // Assert
            loaded.WorkflowSessions.Should().HaveCount(1);
            loaded.WorkflowSessions[0].TreeNodes.Should().HaveCount(1);
            loaded.WorkflowSessions[0].TreeNodes[0].Id.Should().Be("node-1");
            loaded.WorkflowSessions[0].TreeNodes[0].EquipmentData.Name.Should().Be("TestEquipment");
            loaded.WorkflowSessions[0].TreeNodes[0].Children.Should().HaveCount(1);
            loaded.WorkflowSessions[0].TreeNodes[0].Children[0].Id.Should().Be("node-2");
            loaded.WorkflowSessions[0].TreeNodes[0].Children[0].SystemData.Name.Should().Be("TestSystem");
        }

        #endregion

        #region SaveAsync Tests

        [Fact]
        public async Task SaveAsync_CreatesDirectoryIfNotExists()
        {
            // Arrange
            var nestedPath = Path.Combine(Path.GetTempPath(), $"nested_{Guid.NewGuid()}", "data", "uploaded-hardwares.json");
            var repository = new UploadedWorkflowRepository(nestedPath);
            var dataStore = new HardwareDefinitionDataStore();

            try
            {
                // Act
                await repository.SaveAsync(dataStore);

                // Assert
                File.Exists(nestedPath).Should().BeTrue();
            }
            finally
            {
                // Cleanup
                var dir = Path.GetDirectoryName(nestedPath);
                if (Directory.Exists(dir))
                    Directory.Delete(dir, true);
            }
        }

        [Fact]
        public async Task SaveAsync_UpdatesLastSavedAtTimestamp()
        {
            // Arrange
            var dataStore = new HardwareDefinitionDataStore();
            var beforeSave = DateTime.Now.AddSeconds(-1);

            // Act
            await _repository.SaveAsync(dataStore);
            var loaded = await _repository.LoadAsync();

            // Assert
            loaded.LastSavedAt.Should().BeAfter(beforeSave);
        }

        [Fact]
        public async Task SaveAsync_SerializesWithCamelCaseAndIndented()
        {
            // Arrange
            var dataStore = new HardwareDefinitionDataStore();
            dataStore.WorkflowSessions.Add(new WorkflowSessionDto
            {
                Id = "test-workflow-1",
                HardwareType = HardwareLayer.Equipment,
                State = ComponentState.Uploaded
            });

            // Act
            await _repository.SaveAsync(dataStore);
            var json = await File.ReadAllTextAsync(_testFilePath);

            // Assert - camelCase property names
            json.Should().Contain("workflowId");
            json.Should().Contain("startType");
            // Assert - indented (contains newlines)
            json.Should().Contain("\n");
        }

        #endregion

        #region Workflow Session Management Tests

        [Fact]
        public async Task SaveAndLoad_PreservesWorkflowSessionsWithUploadedState()
        {
            // Arrange
            var dataStore = new HardwareDefinitionDataStore();
            dataStore.WorkflowSessions.Add(new WorkflowSessionDto
            {
                Id = "wf-001",
                HardwareType = HardwareLayer.Equipment,
                State = ComponentState.Uploaded,
                LastModifiedAt = DateTime.Now
            });
            dataStore.WorkflowSessions.Add(new WorkflowSessionDto
            {
                Id = "wf-002",
                HardwareType = HardwareLayer.System,
                State = ComponentState.Validated,
                LastModifiedAt = DateTime.Now
            });

            // Act
            await _repository.SaveAsync(dataStore);
            var loaded = await _repository.LoadAsync();

            // Assert
            loaded.WorkflowSessions.Should().HaveCount(2);
            loaded.WorkflowSessions[0].Id.Should().Be("wf-001");
            loaded.WorkflowSessions[0].State.Should().Be(ComponentState.Uploaded);
            loaded.WorkflowSessions[1].Id.Should().Be("wf-002");
            loaded.WorkflowSessions[1].State.Should().Be(ComponentState.Validated);
        }

        [Fact]
        public async Task FindWorkflowById_ReturnsCorrectSession()
        {
            // Arrange
            var dataStore = new HardwareDefinitionDataStore();
            dataStore.WorkflowSessions.Add(new WorkflowSessionDto { Id = "wf-001", State = ComponentState.Uploaded });
            dataStore.WorkflowSessions.Add(new WorkflowSessionDto { Id = "wf-002", State = ComponentState.Uploaded });
            dataStore.WorkflowSessions.Add(new WorkflowSessionDto { Id = "wf-003", State = ComponentState.Uploaded });
            await _repository.SaveAsync(dataStore);

            // Act
            var loaded = await _repository.LoadAsync();
            var found = loaded.WorkflowSessions.Find(s => s.Id == "wf-002");

            // Assert
            found.Should().NotBeNull();
            found.Id.Should().Be("wf-002");
        }

        [Fact]
        public async Task UpdateExistingSession_ReplacesOnlyThatSession()
        {
            // Arrange
            var dataStore = new HardwareDefinitionDataStore();
            dataStore.WorkflowSessions.Add(new WorkflowSessionDto
            {
                Id = "wf-update",
                State = ComponentState.Uploaded
            });
            dataStore.WorkflowSessions.Add(new WorkflowSessionDto
            {
                Id = "wf-unchanged",
                State = ComponentState.Uploaded
            });
            await _repository.SaveAsync(dataStore);

            // Act - load, update, save
            var loaded = await _repository.LoadAsync();
            var existingIndex = loaded.WorkflowSessions.FindIndex(s => s.Id == "wf-update");
            loaded.WorkflowSessions[existingIndex].State = ComponentState.Validated;
            await _repository.SaveAsync(loaded);

            // Assert
            var reloaded = await _repository.LoadAsync();
            reloaded.WorkflowSessions.Find(s => s.Id == "wf-update").State
                .Should().Be(ComponentState.Validated);
            reloaded.WorkflowSessions.Find(s => s.Id == "wf-unchanged").State
                .Should().Be(ComponentState.Uploaded);
        }

        #endregion
    }
}