using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using EquipmentDesigner.Services;
using EquipmentDesigner.Models;
using EquipmentDesigner.Models;
using EquipmentDesigner.Models;

namespace EquipmentDesigner.Tests.Services.Storage
{
    public class WorkflowRepositoryTests : IDisposable
    {
        private readonly string _testFilePath;
        private readonly WorkflowRepository _repository;

        public WorkflowRepositoryTests()
        {
            _testFilePath = Path.Combine(Path.GetTempPath(), $"test_workflows_{Guid.NewGuid()}.json");
            _repository = new WorkflowRepository(_testFilePath);
        }

        public void Dispose()
        {
            if (File.Exists(_testFilePath))
            {
                File.Delete(_testFilePath);
            }
        }

        #region Generic Repository Infrastructure Tests

        [Fact]
        public void ITypedDataRepository_Interface_ProvidesGenericLoadSaveOperations()
        {
            // Assert - WorkflowRepository implements the generic interface
            _repository.Should().BeAssignableTo<IWorkflowRepository>();
        }

        [Fact]
        public void Repository_UsesCorrectDefaultFilePath()
        {
            // Arrange
            var repository = new WorkflowRepository();
            
            // Assert - The repository should be created without exceptions
            repository.Should().NotBeNull();
        }

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
        public async Task SaveAsync_CreatesDirectoryIfNotExists()
        {
            // Arrange
            var nestedPath = Path.Combine(Path.GetTempPath(), $"nested_{Guid.NewGuid()}", "data", "workflows.json");
            var repository = new WorkflowRepository(nestedPath);
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
                HardwareType = HardwareLayer.Equipment
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

        [Fact]
        public void IsDirty_InitiallyFalse()
        {
            // Assert
            _repository.IsDirty.Should().BeFalse();
        }

        [Fact]
        public void MarkDirty_SetsIsDirtyToTrue()
        {
            // Act
            _repository.MarkDirty();

            // Assert
            _repository.IsDirty.Should().BeTrue();
        }

        [Fact]
        public async Task SaveAsync_ResetsIsDirtyToFalse()
        {
            // Arrange
            _repository.MarkDirty();
            _repository.IsDirty.Should().BeTrue();

            // Act
            await _repository.SaveAsync(new HardwareDefinitionDataStore());

            // Assert
            _repository.IsDirty.Should().BeFalse();
        }

        #endregion

        #region Workflow Repository Specific Behaviors

        [Fact]
        public async Task LoadAsync_DeserializesWorkflowSessionsList()
        {
            // Arrange
            var originalDataStore = new HardwareDefinitionDataStore();
            originalDataStore.WorkflowSessions.Add(new WorkflowSessionDto
            {
                Id = "wf-001",
                HardwareType = HardwareLayer.Equipment,
                State = ComponentState.Draft,
                LastModifiedAt = DateTime.Now
            });
            originalDataStore.WorkflowSessions.Add(new WorkflowSessionDto
            {
                Id = "wf-002",
                HardwareType = HardwareLayer.System,
                State = ComponentState.Ready,
                LastModifiedAt = DateTime.Now
            });
            await _repository.SaveAsync(originalDataStore);

            // Act
            var loaded = await _repository.LoadAsync();

            // Assert
            loaded.WorkflowSessions.Should().HaveCount(2);
            loaded.WorkflowSessions[0].Id.Should().Be("wf-001");
            loaded.WorkflowSessions[0].HardwareType.Should().Be(HardwareLayer.Equipment);
            loaded.WorkflowSessions[1].Id.Should().Be("wf-002");
            loaded.WorkflowSessions[1].State.Should().Be(ComponentState.Ready);
        }

        [Fact]
        public async Task SaveAsync_SerializesNestedTreeNodeData()
        {
            // Arrange
            var dataStore = new HardwareDefinitionDataStore();
            var session = new WorkflowSessionDto
            {
                Id = "wf-tree",
                HardwareType = HardwareLayer.Equipment,
                TreeNodes = new System.Collections.Generic.List<TreeNodeDataDto>
                {
                    new TreeNodeDataDto
                    {
                        Id = "node-1",
                        HardwareLayer = HardwareLayer.Equipment,
                        Children = new System.Collections.Generic.List<TreeNodeDataDto>
                        {
                            new TreeNodeDataDto
                            {
                                Id = "node-2",
                                HardwareLayer = HardwareLayer.System
                            }
                        }
                    }
                }
            };
            dataStore.WorkflowSessions.Add(session);

            // Act
            await _repository.SaveAsync(dataStore);
            var loaded = await _repository.LoadAsync();

            // Assert
            loaded.WorkflowSessions.Should().HaveCount(1);
            loaded.WorkflowSessions[0].TreeNodes.Should().HaveCount(1);
            loaded.WorkflowSessions[0].TreeNodes[0].Id.Should().Be("node-1");
            loaded.WorkflowSessions[0].TreeNodes[0].Children.Should().HaveCount(1);
            loaded.WorkflowSessions[0].TreeNodes[0].Children[0].Id.Should().Be("node-2");
        }

        [Fact]
        public async Task FindWorkflowById_ReturnsCorrectSession()
        {
            // Arrange
            var dataStore = new HardwareDefinitionDataStore();
            dataStore.WorkflowSessions.Add(new WorkflowSessionDto { Id = "wf-001" });
            dataStore.WorkflowSessions.Add(new WorkflowSessionDto { Id = "wf-002" });
            dataStore.WorkflowSessions.Add(new WorkflowSessionDto { Id = "wf-003" });
            await _repository.SaveAsync(dataStore);

            // Act
            var loaded = await _repository.LoadAsync();
            var found = loaded.WorkflowSessions.Find(s => s.Id == "wf-002");

            // Assert
            found.Should().NotBeNull();
            found.Id.Should().Be("wf-002");
        }

        [Fact]
        public async Task AddingNewSession_PreservesExistingSessions()
        {
            // Arrange - save initial data
            var dataStore = new HardwareDefinitionDataStore();
            dataStore.WorkflowSessions.Add(new WorkflowSessionDto { Id = "existing-1" });
            dataStore.WorkflowSessions.Add(new WorkflowSessionDto { Id = "existing-2" });
            await _repository.SaveAsync(dataStore);

            // Act - load, add new, save
            var loaded = await _repository.LoadAsync();
            loaded.WorkflowSessions.Add(new WorkflowSessionDto { Id = "new-session" });
            await _repository.SaveAsync(loaded);

            // Assert - reload and verify
            var reloaded = await _repository.LoadAsync();
            reloaded.WorkflowSessions.Should().HaveCount(3);
            reloaded.WorkflowSessions.Should().Contain(s => s.Id == "existing-1");
            reloaded.WorkflowSessions.Should().Contain(s => s.Id == "existing-2");
            reloaded.WorkflowSessions.Should().Contain(s => s.Id == "new-session");
        }

        [Fact]
        public async Task UpdatingSession_ReplacesOnlyThatSession()
        {
            // Arrange
            var dataStore = new HardwareDefinitionDataStore();
            dataStore.WorkflowSessions.Add(new WorkflowSessionDto
            {
                Id = "wf-update",
                State = ComponentState.Draft
            });
            dataStore.WorkflowSessions.Add(new WorkflowSessionDto
            {
                Id = "wf-unchanged",
                State = ComponentState.Draft
            });
            await _repository.SaveAsync(dataStore);

            // Act - load, update, save
            var loaded = await _repository.LoadAsync();
            var toUpdate = loaded.WorkflowSessions.Find(s => s.Id == "wf-update");
            toUpdate.State = ComponentState.Ready;
            await _repository.SaveAsync(loaded);

            // Assert
            var reloaded = await _repository.LoadAsync();
            reloaded.WorkflowSessions.Find(s => s.Id == "wf-update").State
                .Should().Be(ComponentState.Ready);
            reloaded.WorkflowSessions.Find(s => s.Id == "wf-unchanged").State
                .Should().Be(ComponentState.Draft);
        }

        [Fact]
        public async Task RemovingSession_RemovesOnlyThatSession()
        {
            // Arrange
            var dataStore = new HardwareDefinitionDataStore();
            dataStore.WorkflowSessions.Add(new WorkflowSessionDto { Id = "wf-keep-1" });
            dataStore.WorkflowSessions.Add(new WorkflowSessionDto { Id = "wf-remove" });
            dataStore.WorkflowSessions.Add(new WorkflowSessionDto { Id = "wf-keep-2" });
            await _repository.SaveAsync(dataStore);

            // Act - load, remove, save
            var loaded = await _repository.LoadAsync();
            loaded.WorkflowSessions.RemoveAll(s => s.Id == "wf-remove");
            await _repository.SaveAsync(loaded);

            // Assert
            var reloaded = await _repository.LoadAsync();
            reloaded.WorkflowSessions.Should().HaveCount(2);
            reloaded.WorkflowSessions.Should().NotContain(s => s.Id == "wf-remove");
            reloaded.WorkflowSessions.Should().Contain(s => s.Id == "wf-keep-1");
            reloaded.WorkflowSessions.Should().Contain(s => s.Id == "wf-keep-2");
        }

        #endregion
    }
}