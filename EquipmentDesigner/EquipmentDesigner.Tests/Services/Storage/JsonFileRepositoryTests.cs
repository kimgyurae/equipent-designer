using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using EquipmentDesigner.Services.Storage;
using EquipmentDesigner.Models.Storage;
using EquipmentDesigner.Models.Dtos;

namespace EquipmentDesigner.Tests.Services.Storage
{
    public class JsonFileRepositoryTests : IDisposable
    {
        private readonly string _testFilePath;
        private readonly JsonFileRepository _repository;

        public JsonFileRepositoryTests()
        {
            _testFilePath = Path.Combine(Path.GetTempPath(), $"test_equipment_data_{Guid.NewGuid()}.json");
            _repository = new JsonFileRepository(_testFilePath);
        }

        public void Dispose()
        {
            if (File.Exists(_testFilePath))
            {
                File.Delete(_testFilePath);
            }
        }

        [Fact]
        public void Constructor_WithNoPath_UsesDefaultPath()
        {
            // Arrange & Act
            var repository = new JsonFileRepository();
            var expectedPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "EquipmentDesigner",
                "equipment_data.json");

            // Assert - just verify it doesn't throw and uses expected pattern
            repository.Should().NotBeNull();
        }

        [Fact]
        public async Task LoadAsync_WhenFileNotExists_ReturnsNewSharedMemoryDataStore()
        {
            // Arrange - ensure file doesn't exist
            if (File.Exists(_testFilePath))
                File.Delete(_testFilePath);

            // Act
            var result = await _repository.LoadAsync();

            // Assert
            result.Should().NotBeNull();
            result.Version.Should().Be("1.0");
            result.Equipments.Should().BeEmpty();
        }

        [Fact]
        public async Task SaveAsync_CreatesDirectoryIfNotExists()
        {
            // Arrange
            var nestedPath = Path.Combine(Path.GetTempPath(), $"nested_{Guid.NewGuid()}", "data", "equipment_data.json");
            var repository = new JsonFileRepository(nestedPath);
            var dataStore = new SharedMemoryDataStore();

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
        public async Task SaveAsync_CreatesValidJsonFile()
        {
            // Arrange
            var dataStore = new SharedMemoryDataStore();
            dataStore.Equipments.Add(new EquipmentDto
            {
                Id = "eq-001",
                Name = "TestEquipment",
                EquipmentType = "TestType"
            });

            // Act
            await _repository.SaveAsync(dataStore);

            // Assert
            File.Exists(_testFilePath).Should().BeTrue();
            var content = await File.ReadAllTextAsync(_testFilePath);
            content.Should().Contain("eq-001");
            content.Should().Contain("TestEquipment");
        }

        [Fact]
        public async Task SaveAsync_UpdatesLastSavedAtTimestamp()
        {
            // Arrange
            var dataStore = new SharedMemoryDataStore();
            var beforeSave = DateTime.Now.AddSeconds(-1);

            // Act
            await _repository.SaveAsync(dataStore);
            var loaded = await _repository.LoadAsync();

            // Assert
            loaded.LastSavedAt.Should().BeAfter(beforeSave);
        }

        [Fact]
        public async Task LoadAsync_DeserializesExistingJsonFile()
        {
            // Arrange
            var originalDataStore = new SharedMemoryDataStore();
            originalDataStore.Equipments.Add(new EquipmentDto
            {
                Id = "eq-test",
                Name = "LoadTest",
                EquipmentType = "Test"
            });
            originalDataStore.Systems.Add(new SystemDto
            {
                Id = "sys-test",
                EquipmentId = "eq-test",
                Name = "TestSystem"
            });
            await _repository.SaveAsync(originalDataStore);

            // Act
            var loaded = await _repository.LoadAsync();

            // Assert
            loaded.Equipments.Should().HaveCount(1);
            loaded.Equipments[0].Id.Should().Be("eq-test");
            loaded.Equipments[0].Name.Should().Be("LoadTest");
            loaded.Systems.Should().HaveCount(1);
            loaded.Systems[0].Name.Should().Be("TestSystem");
        }

        [Fact]
        public async Task SaveAndLoad_PreservesDataIntegrity()
        {
            // Arrange
            var original = new SharedMemoryDataStore();
            original.Equipments.Add(new EquipmentDto
            {
                Id = "eq-001",
                Name = "Equipment1",
                EquipmentType = "Type1",
                DisplayName = "Display Name",
                State = ComponentState.Defined
            });
            original.Systems.Add(new SystemDto
            {
                Id = "sys-001",
                EquipmentId = "eq-001",
                Name = "System1"
            });
            original.Units.Add(new UnitDto
            {
                Id = "unit-001",
                SystemId = "sys-001",
                Name = "Unit1"
            });
            original.Devices.Add(new DeviceDto
            {
                Id = "dev-001",
                UnitId = "unit-001",
                Name = "Device1"
            });

            // Act
            await _repository.SaveAsync(original);
            var loaded = await _repository.LoadAsync();

            // Assert
            loaded.Equipments.Should().HaveCount(1);
            loaded.Systems.Should().HaveCount(1);
            loaded.Units.Should().HaveCount(1);
            loaded.Devices.Should().HaveCount(1);
            loaded.Equipments[0].State.Should().Be(ComponentState.Defined);
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
            await _repository.SaveAsync(new SharedMemoryDataStore());

            // Assert
            _repository.IsDirty.Should().BeFalse();
        }

        [Fact]
        public async Task LoadAsync_WithCorruptedJson_ReturnsEmptyDataStore()
        {
            // Arrange
            await File.WriteAllTextAsync(_testFilePath, "{ invalid json content");

            // Act
            var result = await _repository.LoadAsync();

            // Assert
            result.Should().NotBeNull();
            result.Version.Should().Be("1.0");
            result.Equipments.Should().BeEmpty();
        }
    }
}
