using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using EquipmentDesigner.Services.Api;
using EquipmentDesigner.Services.Storage;
using EquipmentDesigner.Models.Dtos;
using EquipmentDesigner.Models.Storage;

namespace EquipmentDesigner.Tests.Services.Api
{
    public class MockEquipmentApiServiceTests
    {
        private readonly MemoryTypedRepository<UploadedHardwareDataStore> _repository;
        private readonly MockEquipmentApiService _service;

        public MockEquipmentApiServiceTests()
        {
            _repository = new MemoryTypedRepository<UploadedHardwareDataStore>();
            _service = new MockEquipmentApiService(_repository);
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_AcceptsITypedDataRepository()
        {
            // Arrange & Act
            var service = new MockEquipmentApiService(_repository);

            // Assert
            service.Should().NotBeNull();
        }

        [Fact]
        public async Task Constructor_InitializesSampleDataIfEmpty()
        {
            // Arrange
            var emptyRepository = new MemoryTypedRepository<UploadedHardwareDataStore>();
            var service = new MockEquipmentApiService(emptyRepository);

            // Act
            var result = await service.GetAllEquipmentsAsync();

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeEmpty();
        }

        #endregion

        #region Equipment Operations

        [Fact]
        public async Task GetAllEquipmentsAsync_ReturnsEquipmentsFromDatastore()
        {
            // Act
            var result = await _service.GetAllEquipmentsAsync();

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
        }

        [Fact]
        public async Task GetEquipmentByIdAsync_WhenExists_ReturnsEquipment()
        {
            // Arrange
            var createResult = await _service.CreateEquipmentAsync(new EquipmentDto
            {
                Name = "TestEquipment",
                EquipmentType = "TestType"
            });
            var createdId = createResult.Data.Id;

            // Act
            var result = await _service.GetEquipmentByIdAsync(createdId);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data.Name.Should().Be("TestEquipment");
        }

        [Fact]
        public async Task GetEquipmentByIdAsync_WhenNotExists_ReturnsNotFoundError()
        {
            // Act
            var result = await _service.GetEquipmentByIdAsync("non-existent-id");

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be("NOT_FOUND");
        }

        [Fact]
        public async Task CreateEquipmentAsync_AddsEquipmentWithGeneratedId()
        {
            // Arrange
            var equipment = new EquipmentDto
            {
                Name = "NewEquipment",
                EquipmentType = "NewType"
            };

            // Act
            var result = await _service.CreateEquipmentAsync(equipment);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Id.Should().NotBeNullOrEmpty();
            result.Data.Name.Should().Be("NewEquipment");
        }

        [Fact]
        public async Task CreateEquipmentAsync_MarksRepositoryAsDirty()
        {
            // Arrange
            _repository.IsDirty.Should().BeFalse();

            // Act
            await _service.CreateEquipmentAsync(new EquipmentDto
            {
                Name = "Test",
                EquipmentType = "Type"
            });

            // Assert
            _repository.IsDirty.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateEquipmentAsync_UpdatesExistingEquipment()
        {
            // Arrange
            var createResult = await _service.CreateEquipmentAsync(new EquipmentDto
            {
                Name = "Original",
                EquipmentType = "Type"
            });
            var id = createResult.Data.Id;

            // Act
            var updateResult = await _service.UpdateEquipmentAsync(id, new EquipmentDto
            {
                Name = "Updated",
                EquipmentType = "Type"
            });

            // Assert
            updateResult.Success.Should().BeTrue();
            updateResult.Data.Name.Should().Be("Updated");
        }

        [Fact]
        public async Task UpdateEquipmentAsync_MarksRepositoryAsDirty()
        {
            // Arrange
            var createResult = await _service.CreateEquipmentAsync(new EquipmentDto
            {
                Name = "Test",
                EquipmentType = "Type"
            });
            await _repository.SaveAsync(await _repository.LoadAsync()); // Reset dirty
            _repository.IsDirty.Should().BeFalse();

            // Act
            await _service.UpdateEquipmentAsync(createResult.Data.Id, new EquipmentDto
            {
                Name = "Updated",
                EquipmentType = "Type"
            });

            // Assert
            _repository.IsDirty.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteEquipmentAsync_RemovesEquipment()
        {
            // Arrange
            var createResult = await _service.CreateEquipmentAsync(new EquipmentDto
            {
                Name = "ToDelete",
                EquipmentType = "Type"
            });
            var id = createResult.Data.Id;

            // Act
            var deleteResult = await _service.DeleteEquipmentAsync(id);
            var getResult = await _service.GetEquipmentByIdAsync(id);

            // Assert
            deleteResult.Success.Should().BeTrue();
            getResult.Success.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteEquipmentAsync_MarksRepositoryAsDirty()
        {
            // Arrange
            var createResult = await _service.CreateEquipmentAsync(new EquipmentDto
            {
                Name = "Test",
                EquipmentType = "Type"
            });
            await _repository.SaveAsync(await _repository.LoadAsync());
            _repository.IsDirty.Should().BeFalse();

            // Act
            await _service.DeleteEquipmentAsync(createResult.Data.Id);

            // Assert
            _repository.IsDirty.Should().BeTrue();
        }

        [Fact]
        public async Task ValidateEquipmentAsync_ChecksRequiredFieldsAndUpdatesState()
        {
            // Arrange
            var createResult = await _service.CreateEquipmentAsync(new EquipmentDto
            {
                Name = "ValidEquipment",
                EquipmentType = "ValidType"
            });

            // Act
            var validateResult = await _service.ValidateEquipmentAsync(createResult.Data.Id);

            // Assert
            validateResult.Success.Should().BeTrue();
        }

        [Fact]
        public async Task UploadEquipmentAsync_UpdatesStateToUploaded()
        {
            // Arrange
            var createResult = await _service.CreateEquipmentAsync(new EquipmentDto
            {
                Name = "ToUpload",
                EquipmentType = "Type"
            });

            // Act
            var uploadResult = await _service.UploadEquipmentAsync(createResult.Data.Id);

            // Assert
            uploadResult.Success.Should().BeTrue();
            uploadResult.Data.State.Should().Be(ComponentState.Uploaded);
        }

        [Fact]
        public async Task SearchPredefinedEquipmentsAsync_FiltersByNameOrDisplayName()
        {
            // Arrange
            await _service.CreateEquipmentAsync(new EquipmentDto
            {
                Name = "LaserMachine",
                DisplayName = "Laser Processing Machine",
                EquipmentType = "Laser"
            });

            // Act
            var result = await _service.SearchPredefinedEquipmentsAsync("Laser");

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeEmpty();
            result.Data.Any(e => e.Name.Contains("Laser")).Should().BeTrue();
        }

        #endregion

        #region System Operations

        [Fact]
        public async Task GetAllSystemsAsync_ReturnsSystemsFromDatastore()
        {
            // Act
            var result = await _service.GetAllSystemsAsync();

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateSystemAsync_AddsSystemWithGeneratedIdAndMarksDirty()
        {
            // Arrange
            _repository.IsDirty.Should().BeFalse();

            // Act
            var result = await _service.CreateSystemAsync(new SystemDto
            {
                Name = "NewSystem",
                EquipmentId = "eq-001"
            });

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Id.Should().NotBeNullOrEmpty();
            _repository.IsDirty.Should().BeTrue();
        }

        [Fact]
        public async Task GetSystemsByEquipmentIdAsync_FiltersByEquipmentId()
        {
            // Arrange
            await _service.CreateSystemAsync(new SystemDto
            {
                Name = "System1",
                EquipmentId = "eq-specific"
            });
            await _service.CreateSystemAsync(new SystemDto
            {
                Name = "System2",
                EquipmentId = "eq-other"
            });

            // Act
            var result = await _service.GetSystemsByEquipmentIdAsync("eq-specific");

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().OnlyContain(s => s.EquipmentId == "eq-specific");
        }

        #endregion

        #region Unit Operations

        [Fact]
        public async Task GetAllUnitsAsync_ReturnsUnitsFromDatastore()
        {
            // Act
            var result = await _service.GetAllUnitsAsync();

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateUnitAsync_AddsUnitWithGeneratedIdAndMarksDirty()
        {
            // Arrange
            await _repository.SaveAsync(await _repository.LoadAsync());
            _repository.IsDirty.Should().BeFalse();

            // Act
            var result = await _service.CreateUnitAsync(new UnitDto
            {
                Name = "NewUnit",
                SystemId = "sys-001"
            });

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Id.Should().NotBeNullOrEmpty();
            _repository.IsDirty.Should().BeTrue();
        }

        [Fact]
        public async Task GetUnitsBySystemIdAsync_FiltersBySystemId()
        {
            // Arrange
            await _service.CreateUnitAsync(new UnitDto
            {
                Name = "Unit1",
                SystemId = "sys-specific"
            });
            await _service.CreateUnitAsync(new UnitDto
            {
                Name = "Unit2",
                SystemId = "sys-other"
            });

            // Act
            var result = await _service.GetUnitsBySystemIdAsync("sys-specific");

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().OnlyContain(u => u.SystemId == "sys-specific");
        }

        #endregion

        #region Device Operations

        [Fact]
        public async Task GetAllDevicesAsync_ReturnsDevicesFromDatastore()
        {
            // Act
            var result = await _service.GetAllDevicesAsync();

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateDeviceAsync_AddsDeviceWithGeneratedIdAndMarksDirty()
        {
            // Arrange
            await _repository.SaveAsync(await _repository.LoadAsync());
            _repository.IsDirty.Should().BeFalse();

            // Act
            var result = await _service.CreateDeviceAsync(new DeviceDto
            {
                Name = "NewDevice",
                UnitId = "unit-001"
            });

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Id.Should().NotBeNullOrEmpty();
            _repository.IsDirty.Should().BeTrue();
        }

        [Fact]
        public async Task GetDevicesByUnitIdAsync_FiltersByUnitId()
        {
            // Arrange
            await _service.CreateDeviceAsync(new DeviceDto
            {
                Name = "Device1",
                UnitId = "unit-specific"
            });
            await _service.CreateDeviceAsync(new DeviceDto
            {
                Name = "Device2",
                UnitId = "unit-other"
            });

            // Act
            var result = await _service.GetDevicesByUnitIdAsync("unit-specific");

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().OnlyContain(d => d.UnitId == "unit-specific");
        }

        #endregion

        #region Integration Tests

        [Fact]
        public async Task DataPersistsAfterRepositoryReload()
        {
            // Arrange
            await _service.CreateEquipmentAsync(new EquipmentDto
            {
                Name = "PersistentEquipment",
                EquipmentType = "Test"
            });

            // Act - simulate reload by creating new service with same repository
            var newService = new MockEquipmentApiService(_repository);
            var result = await newService.GetAllEquipmentsAsync();

            // Assert
            result.Data.Should().Contain(e => e.Name == "PersistentEquipment");
        }

        [Fact]
        public async Task UpdateAndReloadPreservesChanges()
        {
            // Arrange
            var createResult = await _service.CreateEquipmentAsync(new EquipmentDto
            {
                Name = "Original",
                EquipmentType = "Test"
            });
            await _service.UpdateEquipmentAsync(createResult.Data.Id, new EquipmentDto
            {
                Name = "Modified",
                EquipmentType = "Test"
            });

            // Act
            var newService = new MockEquipmentApiService(_repository);
            var result = await newService.GetEquipmentByIdAsync(createResult.Data.Id);

            // Assert
            result.Data.Name.Should().Be("Modified");
        }

        [Fact]
        public async Task DeleteAndReloadConfirmsDeletion()
        {
            // Arrange
            var createResult = await _service.CreateEquipmentAsync(new EquipmentDto
            {
                Name = "ToDelete",
                EquipmentType = "Test"
            });
            await _service.DeleteEquipmentAsync(createResult.Data.Id);

            // Act
            var newService = new MockEquipmentApiService(_repository);
            var result = await newService.GetEquipmentByIdAsync(createResult.Data.Id);

            // Assert
            result.Success.Should().BeFalse();
        }

        #endregion
    }
}