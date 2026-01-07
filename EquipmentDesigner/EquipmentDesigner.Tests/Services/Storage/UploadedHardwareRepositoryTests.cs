using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using EquipmentDesigner.Services.Storage;
using EquipmentDesigner.Models.Storage;
using EquipmentDesigner.Models.Dtos;
using EquipmentDesigner.Models;

namespace EquipmentDesigner.Tests.Services.Storage
{
    public class UploadedHardwareRepositoryTests : IDisposable
    {
        private readonly string _testFilePath;
        private readonly UploadedHardwareRepository _repository;

        public UploadedHardwareRepositoryTests()
        {
            _testFilePath = Path.Combine(Path.GetTempPath(), $"test_uploaded_hardwares_{Guid.NewGuid()}.json");
            _repository = new UploadedHardwareRepository(_testFilePath);
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
            // Assert
            _repository.Should().BeAssignableTo<ITypedDataRepository<UploadedHardwareDataStore>>();
        }

        [Fact]
        public void GetDefaultFilePath_ReturnsUploadedHardwaresJsonPath()
        {
            // Act
            var defaultPath = UploadedHardwareRepository.GetDefaultUploadedHardwareFilePath();

            // Assert
            var expectedPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "EquipmentDesigner",
                "uploaded-hardwares.json");
            defaultPath.Should().Be(expectedPath);
        }

        [Fact]
        public async Task LoadAsync_WhenFileNotExists_ReturnsNewEmptyDataStore()
        {
            // Arrange
            if (File.Exists(_testFilePath))
                File.Delete(_testFilePath);

            // Act
            var result = await _repository.LoadAsync();

            // Assert
            result.Should().NotBeNull();
            result.Version.Should().Be("1.0");
            result.Equipments.Should().NotBeNull().And.BeEmpty();
            result.Systems.Should().NotBeNull().And.BeEmpty();
            result.Units.Should().NotBeNull().And.BeEmpty();
            result.Devices.Should().NotBeNull().And.BeEmpty();
        }

        [Fact]
        public async Task LoadAsync_WhenJsonCorrupted_ReturnsEmptyDataStoreGracefully()
        {
            // Arrange
            await File.WriteAllTextAsync(_testFilePath, "not valid json { broken");

            // Act
            var result = await _repository.LoadAsync();

            // Assert
            result.Should().NotBeNull();
            result.Version.Should().Be("1.0");
            result.Equipments.Should().BeEmpty();
        }

        #endregion

        #region Uploaded Hardware Repository Specific Behaviors

        [Fact]
        public async Task LoadAsync_DeserializesAllHardwareLists()
        {
            // Arrange
            var dataStore = new UploadedHardwareDataStore();
            dataStore.Equipments.Add(new EquipmentDto { Id = "eq-1", Name = "Equipment1" });
            dataStore.Systems.Add(new SystemDto { Id = "sys-1", Name = "System1", EquipmentId = "eq-1" });
            dataStore.Units.Add(new UnitDto { Id = "unit-1", Name = "Unit1", SystemId = "sys-1" });
            dataStore.Devices.Add(new DeviceDto { Id = "dev-1", Name = "Device1", UnitId = "unit-1" });
            await _repository.SaveAsync(dataStore);

            // Act
            var loaded = await _repository.LoadAsync();

            // Assert
            loaded.Equipments.Should().HaveCount(1);
            loaded.Equipments[0].Name.Should().Be("Equipment1");
            loaded.Systems.Should().HaveCount(1);
            loaded.Systems[0].EquipmentId.Should().Be("eq-1");
            loaded.Units.Should().HaveCount(1);
            loaded.Units[0].SystemId.Should().Be("sys-1");
            loaded.Devices.Should().HaveCount(1);
            loaded.Devices[0].UnitId.Should().Be("unit-1");
        }

        [Fact]
        public async Task SaveAsync_SerializesAllFourHardwareComponentLists()
        {
            // Arrange
            var dataStore = new UploadedHardwareDataStore();
            dataStore.Equipments.Add(new EquipmentDto { Id = "eq-1", Name = "E1" });
            dataStore.Systems.Add(new SystemDto { Id = "sys-1", Name = "S1" });
            dataStore.Units.Add(new UnitDto { Id = "unit-1", Name = "U1" });
            dataStore.Devices.Add(new DeviceDto { Id = "dev-1", Name = "D1" });

            // Act
            await _repository.SaveAsync(dataStore);
            var json = await File.ReadAllTextAsync(_testFilePath);

            // Assert
            json.Should().Contain("equipments");
            json.Should().Contain("systems");
            json.Should().Contain("units");
            json.Should().Contain("devices");
        }

        [Fact]
        public async Task AddingEquipment_PreservesOtherEquipments()
        {
            // Arrange
            var dataStore = new UploadedHardwareDataStore();
            dataStore.Equipments.Add(new EquipmentDto { Id = "eq-existing", Name = "Existing" });
            await _repository.SaveAsync(dataStore);

            // Act
            var loaded = await _repository.LoadAsync();
            loaded.Equipments.Add(new EquipmentDto { Id = "eq-new", Name = "New" });
            await _repository.SaveAsync(loaded);

            // Assert
            var reloaded = await _repository.LoadAsync();
            reloaded.Equipments.Should().HaveCount(2);
            reloaded.Equipments.Should().Contain(e => e.Id == "eq-existing");
            reloaded.Equipments.Should().Contain(e => e.Id == "eq-new");
        }

        [Fact]
        public async Task AddingSystem_WithValidEquipmentId_MaintainsReferentialIntegrity()
        {
            // Arrange
            var dataStore = new UploadedHardwareDataStore();
            dataStore.Equipments.Add(new EquipmentDto { Id = "eq-parent", Name = "Parent Equipment" });
            await _repository.SaveAsync(dataStore);

            // Act
            var loaded = await _repository.LoadAsync();
            loaded.Systems.Add(new SystemDto
            {
                Id = "sys-child",
                Name = "Child System",
                EquipmentId = "eq-parent"
            });
            await _repository.SaveAsync(loaded);

            // Assert
            var reloaded = await _repository.LoadAsync();
            var system = reloaded.Systems[0];
            system.EquipmentId.Should().Be("eq-parent");
            reloaded.Equipments.Should().Contain(e => e.Id == system.EquipmentId);
        }

        [Fact]
        public async Task AddingUnit_WithValidSystemId_MaintainsReferentialIntegrity()
        {
            // Arrange
            var dataStore = new UploadedHardwareDataStore();
            dataStore.Systems.Add(new SystemDto { Id = "sys-parent", Name = "Parent System" });
            await _repository.SaveAsync(dataStore);

            // Act
            var loaded = await _repository.LoadAsync();
            loaded.Units.Add(new UnitDto
            {
                Id = "unit-child",
                Name = "Child Unit",
                SystemId = "sys-parent"
            });
            await _repository.SaveAsync(loaded);

            // Assert
            var reloaded = await _repository.LoadAsync();
            var unit = reloaded.Units[0];
            unit.SystemId.Should().Be("sys-parent");
            reloaded.Systems.Should().Contain(s => s.Id == unit.SystemId);
        }

        [Fact]
        public async Task AddingDevice_WithValidUnitId_MaintainsReferentialIntegrity()
        {
            // Arrange
            var dataStore = new UploadedHardwareDataStore();
            dataStore.Units.Add(new UnitDto { Id = "unit-parent", Name = "Parent Unit" });
            await _repository.SaveAsync(dataStore);

            // Act
            var loaded = await _repository.LoadAsync();
            loaded.Devices.Add(new DeviceDto
            {
                Id = "dev-child",
                Name = "Child Device",
                UnitId = "unit-parent"
            });
            await _repository.SaveAsync(loaded);

            // Assert
            var reloaded = await _repository.LoadAsync();
            var device = reloaded.Devices[0];
            device.UnitId.Should().Be("unit-parent");
            reloaded.Units.Should().Contain(u => u.Id == device.UnitId);
        }

        [Fact]
        public async Task SaveAndLoad_PreservesCompleteHardwareHierarchy()
        {
            // Arrange - full hierarchy
            var dataStore = new UploadedHardwareDataStore();
            dataStore.Equipments.Add(new EquipmentDto
            {
                Id = "eq-root",
                Name = "Root Equipment",
                EquipmentType = "Manufacturing",
                State = ComponentState.Uploaded
            });
            dataStore.Systems.Add(new SystemDto
            {
                Id = "sys-1",
                EquipmentId = "eq-root",
                Name = "System One"
            });
            dataStore.Systems.Add(new SystemDto
            {
                Id = "sys-2",
                EquipmentId = "eq-root",
                Name = "System Two"
            });
            dataStore.Units.Add(new UnitDto
            {
                Id = "unit-1",
                SystemId = "sys-1",
                Name = "Unit One"
            });
            dataStore.Devices.Add(new DeviceDto
            {
                Id = "dev-1",
                UnitId = "unit-1",
                Name = "Device One"
            });
            dataStore.Devices.Add(new DeviceDto
            {
                Id = "dev-2",
                UnitId = "unit-1",
                Name = "Device Two"
            });

            // Act
            await _repository.SaveAsync(dataStore);
            var loaded = await _repository.LoadAsync();

            // Assert
            loaded.Equipments.Should().HaveCount(1);
            loaded.Systems.Should().HaveCount(2);
            loaded.Units.Should().HaveCount(1);
            loaded.Devices.Should().HaveCount(2);

            // Verify hierarchy references
            loaded.Systems.Should().OnlyContain(s => s.EquipmentId == "eq-root");
            loaded.Units.Should().OnlyContain(u => u.SystemId == "sys-1");
            loaded.Devices.Should().OnlyContain(d => d.UnitId == "unit-1");
        }

        #endregion
    }
}
