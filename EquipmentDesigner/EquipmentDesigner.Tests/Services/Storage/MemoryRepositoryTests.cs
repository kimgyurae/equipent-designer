using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using EquipmentDesigner.Services.Storage;
using EquipmentDesigner.Models.Storage;
using EquipmentDesigner.Models.Dtos;

namespace EquipmentDesigner.Tests.Services.Storage
{
    public class MemoryRepositoryTests
    {
        [Fact]
        public async Task MemoryRepository_StoresDataInMemoryWithoutFileIO()
        {
            // Arrange
            var repository = new MemoryRepository();
            var dataStore = new SharedMemoryDataStore();
            dataStore.Equipments.Add(new EquipmentDto { Id = "eq-001", Name = "Test" });

            // Act
            await repository.SaveAsync(dataStore);
            var loaded = await repository.LoadAsync();

            // Assert
            loaded.Should().NotBeNull();
            loaded.Equipments.Should().HaveCount(1);
            loaded.Equipments[0].Id.Should().Be("eq-001");
        }

        [Fact]
        public async Task LoadAsync_ReturnsStoredSharedMemoryDataStore()
        {
            // Arrange
            var repository = new MemoryRepository();
            var dataStore = new SharedMemoryDataStore { Version = "2.0" };
            await repository.SaveAsync(dataStore);

            // Act
            var loaded = await repository.LoadAsync();

            // Assert
            loaded.Version.Should().Be("2.0");
        }

        [Fact]
        public async Task LoadAsync_WhenNeverSaved_ReturnsNewDataStore()
        {
            // Arrange
            var repository = new MemoryRepository();

            // Act
            var loaded = await repository.LoadAsync();

            // Assert
            loaded.Should().NotBeNull();
            loaded.Version.Should().Be("1.0");
        }

        [Fact]
        public async Task SaveAsync_StoresSharedMemoryDataStore()
        {
            // Arrange
            var repository = new MemoryRepository();
            var dataStore = new SharedMemoryDataStore();
            dataStore.Systems.Add(new SystemDto { Id = "sys-001", Name = "TestSystem" });

            // Act
            await repository.SaveAsync(dataStore);
            var loaded = await repository.LoadAsync();

            // Assert
            loaded.Systems.Should().HaveCount(1);
            loaded.Systems[0].Name.Should().Be("TestSystem");
        }

        [Fact]
        public void IsDirty_InitiallyFalse()
        {
            // Arrange
            var repository = new MemoryRepository();

            // Assert
            repository.IsDirty.Should().BeFalse();
        }

        [Fact]
        public void MarkDirty_SetsIsDirtyToTrue()
        {
            // Arrange
            var repository = new MemoryRepository();

            // Act
            repository.MarkDirty();

            // Assert
            repository.IsDirty.Should().BeTrue();
        }

        [Fact]
        public async Task SaveAsync_ResetsIsDirty()
        {
            // Arrange
            var repository = new MemoryRepository();
            repository.MarkDirty();

            // Act
            await repository.SaveAsync(new SharedMemoryDataStore());

            // Assert
            repository.IsDirty.Should().BeFalse();
        }
    }
}
