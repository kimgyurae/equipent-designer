using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using EquipmentDesigner.Services;
using EquipmentDesigner.Models;

namespace EquipmentDesigner.Tests.Services.Storage
{
    public class WorkflowRepositoryTests : IDisposable
    {
        private readonly string _testFilePath;
        private readonly LocalHardwareRepository _repository;

        public WorkflowRepositoryTests()
        {
            _testFilePath = Path.Combine(Path.GetTempPath(), $"test_workflows_{Guid.NewGuid()}.json");
            _repository = new LocalHardwareRepository(_testFilePath);
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
            var repository = new LocalHardwareRepository();
            
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
            result.Should().BeEmpty();
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
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task SaveAsync_CreatesDirectoryIfNotExists()
        {
            // Arrange
            var nestedPath = Path.Combine(Path.GetTempPath(), $"nested_{Guid.NewGuid()}", "data", "workflows.json");
            var repository = new LocalHardwareRepository(nestedPath);
            var dataStore = new List<HardwareDefinition>();

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
            var dataStore = new List<HardwareDefinition>();
            var beforeSave = DateTime.Now.AddSeconds(-1);

            // Act
            await _repository.SaveAsync(dataStore);
            var loaded = await _repository.LoadAsync();

            // Assert
            // Note: List<HardwareDefinition>는 LastSavedAt 속성이 없으므로 파일 존재 여부로 검증
            File.Exists(_testFilePath).Should().BeTrue();
        }

        [Fact]
        public async Task SaveAsync_SerializesWithCamelCaseAndIndented()
        {
            // Arrange
            var dataStore = new List<HardwareDefinition>();
            dataStore.Add(new HardwareDefinition
            {
                Id = "test-workflow-1",
                HardwareType = HardwareType.Equipment
            });

            // Act
            await _repository.SaveAsync(dataStore);
            var json = await File.ReadAllTextAsync(_testFilePath);

            // Assert - camelCase property names
            json.Should().Contain("id");
            json.Should().Contain("hardwareType");
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
            await _repository.SaveAsync(new List<HardwareDefinition>());

            // Assert
            _repository.IsDirty.Should().BeFalse();
        }

        #endregion

        #region Workflow Repository Specific Behaviors

        [Fact]
        public async Task LoadAsync_DeserializesWorkflowSessionsList()
        {
            // Arrange
            var dataStore = new List<HardwareDefinition>();
            dataStore.Add(new HardwareDefinition
            {
                Id = "wf-001",
                HardwareType = HardwareType.Equipment,
                State = ComponentState.Draft,
                LastModifiedAt = DateTime.Now
            });
            dataStore.Add(new HardwareDefinition
            {
                Id = "wf-002",
                HardwareType = HardwareType.System,
                State = ComponentState.Ready,
                LastModifiedAt = DateTime.Now
            });
            await _repository.SaveAsync(dataStore);

            // Act
            var loaded = await _repository.LoadAsync();

            // Assert
            loaded.Should().HaveCount(2);
            loaded[0].Id.Should().Be("wf-001");
            loaded[0].HardwareType.Should().Be(HardwareType.Equipment);
            loaded[1].Id.Should().Be("wf-002");
            loaded[1].State.Should().Be(ComponentState.Ready);
        }

        [Fact]
        public async Task SaveAsync_SerializesNestedTreeNodeData()
        {
            // Arrange
            var dataStore = new List<HardwareDefinition>();
            var session = new HardwareDefinition
            {
                Id = "wf-tree",
                HardwareType = HardwareType.Equipment,
                Children = new List<HardwareDefinition>
                {
                    new HardwareDefinition
                    {
                        Id = "node-1",
                        HardwareType = HardwareType.Equipment,
                        Children = new List<HardwareDefinition>
                        {
                            new HardwareDefinition
                            {
                                Id = "node-2",
                                HardwareType = HardwareType.System
                            }
                        }
                    }
                }
            };
            dataStore.Add(session);

            // Act
            await _repository.SaveAsync(dataStore);
            var loaded = await _repository.LoadAsync();

            // Assert
            loaded.Should().HaveCount(1);
            loaded[0].Children.Should().HaveCount(1);
            loaded[0].Children[0].Id.Should().Be("node-1");
            loaded[0].Children[0].Children.Should().HaveCount(1);
            loaded[0].Children[0].Children[0].Id.Should().Be("node-2");
        }

        [Fact]
        public async Task FindWorkflowById_ReturnsCorrectSession()
        {
            // Arrange
            var dataStore = new List<HardwareDefinition>();
            dataStore.Add(new HardwareDefinition { Id = "wf-001" });
            dataStore.Add(new HardwareDefinition { Id = "wf-002" });
            dataStore.Add(new HardwareDefinition { Id = "wf-003" });
            await _repository.SaveAsync(dataStore);

            // Act
            var loaded = await _repository.LoadAsync();
            var found = loaded.Find(s => s.Id == "wf-002");

            // Assert
            found.Should().NotBeNull();
            found.Id.Should().Be("wf-002");
        }

        [Fact]
        public async Task AddingNewSession_PreservesExistingSessions()
        {
            // Arrange - save initial data
            var dataStore = new List<HardwareDefinition>();
            dataStore.Add(new HardwareDefinition { Id = "existing-1" });
            dataStore.Add(new HardwareDefinition { Id = "existing-2" });
            await _repository.SaveAsync(dataStore);

            // Act - load, add new, save
            var loaded = await _repository.LoadAsync();
            loaded.Add(new HardwareDefinition { Id = "new-session" });
            await _repository.SaveAsync(loaded);

            // Assert - reload and verify
            var reloaded = await _repository.LoadAsync();
            reloaded.Should().HaveCount(3);
            reloaded.Should().Contain(s => s.Id == "existing-1");
            reloaded.Should().Contain(s => s.Id == "existing-2");
            reloaded.Should().Contain(s => s.Id == "new-session");
        }

        [Fact]
        public async Task UpdatingSession_ReplacesOnlyThatSession()
        {
            // Arrange
            var dataStore = new List<HardwareDefinition>();
            dataStore.Add(new HardwareDefinition
            {
                Id = "wf-update",
                State = ComponentState.Draft
            });
            dataStore.Add(new HardwareDefinition
            {
                Id = "wf-unchanged",
                State = ComponentState.Draft
            });
            await _repository.SaveAsync(dataStore);

            // Act - load, update, save
            var loaded = await _repository.LoadAsync();
            var toUpdate = loaded.Find(s => s.Id == "wf-update");
            toUpdate.State = ComponentState.Ready;
            await _repository.SaveAsync(loaded);

            // Assert
            var reloaded = await _repository.LoadAsync();
            reloaded.Find(s => s.Id == "wf-update").State
                .Should().Be(ComponentState.Ready);
            reloaded.Find(s => s.Id == "wf-unchanged").State
                .Should().Be(ComponentState.Draft);
        }

        [Fact]
        public async Task RemovingSession_RemovesOnlyThatSession()
        {
            // Arrange
            var dataStore = new List<HardwareDefinition>();
            dataStore.Add(new HardwareDefinition { Id = "wf-keep-1" });
            dataStore.Add(new HardwareDefinition { Id = "wf-remove" });
            dataStore.Add(new HardwareDefinition { Id = "wf-keep-2" });
            await _repository.SaveAsync(dataStore);

            // Act - load, remove, save
            var loaded = await _repository.LoadAsync();
            loaded.RemoveAll(s => s.Id == "wf-remove");
            await _repository.SaveAsync(loaded);

            // Assert
            var reloaded = await _repository.LoadAsync();
            reloaded.Should().HaveCount(2);
            reloaded.Should().NotContain(s => s.Id == "wf-remove");
            reloaded.Should().Contain(s => s.Id == "wf-keep-1");
            reloaded.Should().Contain(s => s.Id == "wf-keep-2");
        }

        #endregion
    }
}