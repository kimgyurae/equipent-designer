using FluentAssertions;
using Xunit;

namespace EquipmentDesigner.Tests.Services.Storage
{
    public class SharedMemoryDataStoreTests
    {
        [Fact]
        public void Constructor_WhenCreated_InitializesVersionTo1Point0()
        {
            // Arrange & Act
            var dataStore = new EquipmentDesigner.Models.Storage.SharedMemoryDataStore();

            // Assert
            dataStore.Version.Should().Be("1.0");
        }

        [Fact]
        public void Constructor_WhenCreated_InitializesEmptyEquipmentsList()
        {
            // Arrange & Act
            var dataStore = new EquipmentDesigner.Models.Storage.SharedMemoryDataStore();

            // Assert
            dataStore.Equipments.Should().NotBeNull();
            dataStore.Equipments.Should().BeEmpty();
        }

        [Fact]
        public void Constructor_WhenCreated_InitializesEmptySystemsList()
        {
            // Arrange & Act
            var dataStore = new EquipmentDesigner.Models.Storage.SharedMemoryDataStore();

            // Assert
            dataStore.Systems.Should().NotBeNull();
            dataStore.Systems.Should().BeEmpty();
        }

        [Fact]
        public void Constructor_WhenCreated_InitializesEmptyUnitsList()
        {
            // Arrange & Act
            var dataStore = new EquipmentDesigner.Models.Storage.SharedMemoryDataStore();

            // Assert
            dataStore.Units.Should().NotBeNull();
            dataStore.Units.Should().BeEmpty();
        }

        [Fact]
        public void Constructor_WhenCreated_InitializesEmptyDevicesList()
        {
            // Arrange & Act
            var dataStore = new EquipmentDesigner.Models.Storage.SharedMemoryDataStore();

            // Assert
            dataStore.Devices.Should().NotBeNull();
            dataStore.Devices.Should().BeEmpty();
        }

        [Fact]
        public void Constructor_WhenCreated_SessionContextIsNull()
        {
            // Arrange & Act
            var dataStore = new EquipmentDesigner.Models.Storage.SharedMemoryDataStore();

            // Assert
            dataStore.SessionContext.Should().BeNull();
        }

        [Fact]
        public void LastSavedAt_WhenSet_StoresCorrectValue()
        {
            // Arrange
            var dataStore = new EquipmentDesigner.Models.Storage.SharedMemoryDataStore();
            var expectedTime = new System.DateTime(2026, 1, 4, 15, 30, 0);

            // Act
            dataStore.LastSavedAt = expectedTime;

            // Assert
            dataStore.LastSavedAt.Should().Be(expectedTime);
        }
    }
}
