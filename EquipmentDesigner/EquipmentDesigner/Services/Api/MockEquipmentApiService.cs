using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EquipmentDesigner.Models;
using EquipmentDesigner.Models.Dtos;
using EquipmentDesigner.Models.Storage;
using EquipmentDesigner.Services.Storage;

namespace EquipmentDesigner.Services.Api
{
    /// <summary>
    /// UploadedHardwareDataStore 기반 Mock API 서비스
    /// USE_MOCK_DATA 환경에서 사용
    /// </summary>
    public class MockEquipmentApiService : IEquipmentApiService
    {
        private readonly ITypedDataRepository<UploadedHardwareDataStore> _repository;
        private UploadedHardwareDataStore _dataStore;
        private readonly SemaphoreSlim _initLock = new SemaphoreSlim(1, 1);
        private bool _initialized;

        public MockEquipmentApiService(ITypedDataRepository<UploadedHardwareDataStore> repository)
        {
            _repository = repository;
        }

        private async Task EnsureInitializedAsync()
        {
            if (_initialized) return;

            await _initLock.WaitAsync();
            try
            {
                if (_initialized) return;

                _dataStore = await _repository.LoadAsync();
                if (_dataStore.Equipments.Count == 0)
                {
                    InitializeSampleData();
                    await _repository.SaveAsync(_dataStore);
                }
                _initialized = true;
            }
            finally
            {
                _initLock.Release();
            }
        }

        private void InitializeSampleData()
        {
            // Sample Equipment
            var equipment = new EquipmentDto
            {
                Id = "eq-001",
                EquipmentType = "LaserProcessing",
                Name = "LaserBurrMachine",
                DisplayName = "PFS Laser Burr 사상기",
                Description = "Laser burr removal equipment for gasket processing",
                Customer = "Samsung SDI",
                Process = "Deburring",
                State = ComponentState.Defined,
                ProgramRoot = "D:/LaserBurrMachine",
                CreatedAt = DateTime.Now.AddDays(-30),
                UpdatedAt = DateTime.Now
            };

            // Sample System
            var loadingSystem = new SystemDto
            {
                Id = "sys-001",
                EquipmentId = "eq-001",
                Name = "Loading",
                DisplayName = "Loading System",
                Description = "Material loading system",
                State = ComponentState.Defined,
                CreatedAt = DateTime.Now.AddDays(-25),
                UpdatedAt = DateTime.Now
            };

            // Sample Unit
            var opShutterUnit = new UnitDto
            {
                Id = "unit-001",
                SystemId = "sys-001",
                Name = "OPShutter",
                DisplayName = "OP Side Shutter Unit",
                State = ComponentState.Defined,
                CreatedAt = DateTime.Now.AddDays(-20),
                UpdatedAt = DateTime.Now
            };

            // Sample Device
            var opShutterCylinder = new DeviceDto
            {
                Id = "dev-001",
                UnitId = "unit-001",
                Name = "OPShutterCylinder",
                DisplayName = "OP Shutter Cylinder",
                DeviceType = "Generic",
                State = ComponentState.Defined,
                CreatedAt = DateTime.Now.AddDays(-15),
                UpdatedAt = DateTime.Now
            };

            _dataStore.Equipments.Add(equipment);
            _dataStore.Systems.Add(loadingSystem);
            _dataStore.Units.Add(opShutterUnit);
            _dataStore.Devices.Add(opShutterCylinder);
        }

        #region Equipment Operations

        public async Task<ApiResponse<List<EquipmentDto>>> GetAllEquipmentsAsync()
        {
            await EnsureInitializedAsync();
            await Task.Delay(50);
            return ApiResponse<List<EquipmentDto>>.Ok(_dataStore.Equipments.ToList());
        }

        public async Task<ApiResponse<EquipmentDto>> GetEquipmentByIdAsync(string id)
        {
            await EnsureInitializedAsync();
            await Task.Delay(50);
            var equipment = _dataStore.Equipments.FirstOrDefault(e => e.Id == id);
            if (equipment == null)
                return ApiResponse<EquipmentDto>.Fail("Equipment not found", "NOT_FOUND");
            return ApiResponse<EquipmentDto>.Ok(equipment);
        }

        public async Task<ApiResponse<EquipmentDto>> CreateEquipmentAsync(EquipmentDto equipment)
        {
            await EnsureInitializedAsync();
            await Task.Delay(50);
            equipment.Id = $"eq-{Guid.NewGuid():N}".Substring(0, 10);
            equipment.CreatedAt = DateTime.Now;
            equipment.UpdatedAt = DateTime.Now;
            equipment.State = ComponentState.Undefined;
            _dataStore.Equipments.Add(equipment);
            _repository.MarkDirty();
            return ApiResponse<EquipmentDto>.Ok(equipment);
        }

        public async Task<ApiResponse<EquipmentDto>> UpdateEquipmentAsync(string id, EquipmentDto equipment)
        {
            await EnsureInitializedAsync();
            await Task.Delay(50);
            var index = _dataStore.Equipments.FindIndex(e => e.Id == id);
            if (index < 0)
                return ApiResponse<EquipmentDto>.Fail("Equipment not found", "NOT_FOUND");

            equipment.Id = id;
            equipment.UpdatedAt = DateTime.Now;
            _dataStore.Equipments[index] = equipment;
            _repository.MarkDirty();

            return ApiResponse<EquipmentDto>.Ok(equipment);
        }

        public async Task<ApiResponse<bool>> DeleteEquipmentAsync(string id)
        {
            await EnsureInitializedAsync();
            await Task.Delay(50);
            var removed = _dataStore.Equipments.RemoveAll(e => e.Id == id) > 0;
            if (!removed)
                return ApiResponse<bool>.Fail("Equipment not found", "NOT_FOUND");

            _repository.MarkDirty();
            return ApiResponse<bool>.Ok(true);
        }

        public async Task<ApiResponse<bool>> ValidateEquipmentAsync(string id)
        {
            await EnsureInitializedAsync();
            await Task.Delay(50);
            var equipment = _dataStore.Equipments.FirstOrDefault(e => e.Id == id);
            if (equipment == null)
                return ApiResponse<bool>.Fail("Equipment not found", "NOT_FOUND");

            var errors = new List<ValidationError>();
            if (string.IsNullOrEmpty(equipment.Name))
                errors.Add(new ValidationError { Field = "Name", Message = "Name is required" });
            if (string.IsNullOrEmpty(equipment.EquipmentType))
                errors.Add(new ValidationError { Field = "EquipmentType", Message = "Equipment type is required" });

            if (errors.Any())
                return ApiResponse<bool>.ValidationFail(errors);

            equipment.State = ComponentState.Validated;
            _repository.MarkDirty();
            return ApiResponse<bool>.Ok(true);
        }

        public async Task<ApiResponse<EquipmentDto>> UploadEquipmentAsync(string id)
        {
            await EnsureInitializedAsync();
            await Task.Delay(100);
            var equipment = _dataStore.Equipments.FirstOrDefault(e => e.Id == id);
            if (equipment == null)
                return ApiResponse<EquipmentDto>.Fail("Equipment not found", "NOT_FOUND");

            equipment.State = ComponentState.Uploaded;
            equipment.UpdatedAt = DateTime.Now;
            _repository.MarkDirty();
            return ApiResponse<EquipmentDto>.Ok(equipment);
        }

        public async Task<ApiResponse<List<EquipmentDto>>> SearchPredefinedEquipmentsAsync(string query)
        {
            await EnsureInitializedAsync();
            await Task.Delay(50);
            var results = _dataStore.Equipments
                .Where(e => e.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                           (e.DisplayName?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false))
                .ToList();
            return ApiResponse<List<EquipmentDto>>.Ok(results);
        }

        #endregion

        #region System Operations

        public async Task<ApiResponse<List<SystemDto>>> GetAllSystemsAsync()
        {
            await EnsureInitializedAsync();
            await Task.Delay(50);
            return ApiResponse<List<SystemDto>>.Ok(_dataStore.Systems.ToList());
        }

        public async Task<ApiResponse<SystemDto>> GetSystemByIdAsync(string id)
        {
            await EnsureInitializedAsync();
            await Task.Delay(50);
            var system = _dataStore.Systems.FirstOrDefault(s => s.Id == id);
            if (system == null)
                return ApiResponse<SystemDto>.Fail("System not found", "NOT_FOUND");
            return ApiResponse<SystemDto>.Ok(system);
        }

        public async Task<ApiResponse<List<SystemDto>>> GetSystemsByEquipmentIdAsync(string equipmentId)
        {
            await EnsureInitializedAsync();
            await Task.Delay(50);
            var systems = _dataStore.Systems.Where(s => s.EquipmentId == equipmentId).ToList();
            return ApiResponse<List<SystemDto>>.Ok(systems);
        }

        public async Task<ApiResponse<SystemDto>> CreateSystemAsync(SystemDto system)
        {
            await EnsureInitializedAsync();
            await Task.Delay(50);
            system.Id = $"sys-{Guid.NewGuid():N}".Substring(0, 10);
            system.CreatedAt = DateTime.Now;
            system.UpdatedAt = DateTime.Now;
            system.State = ComponentState.Undefined;
            _dataStore.Systems.Add(system);
            _repository.MarkDirty();
            return ApiResponse<SystemDto>.Ok(system);
        }

        public async Task<ApiResponse<SystemDto>> UpdateSystemAsync(string id, SystemDto system)
        {
            await EnsureInitializedAsync();
            await Task.Delay(50);
            var index = _dataStore.Systems.FindIndex(s => s.Id == id);
            if (index < 0)
                return ApiResponse<SystemDto>.Fail("System not found", "NOT_FOUND");

            system.Id = id;
            system.UpdatedAt = DateTime.Now;
            _dataStore.Systems[index] = system;
            _repository.MarkDirty();

            return ApiResponse<SystemDto>.Ok(system);
        }

        public async Task<ApiResponse<bool>> DeleteSystemAsync(string id)
        {
            await EnsureInitializedAsync();
            await Task.Delay(50);
            var removed = _dataStore.Systems.RemoveAll(s => s.Id == id) > 0;
            if (!removed)
                return ApiResponse<bool>.Fail("System not found", "NOT_FOUND");

            _repository.MarkDirty();
            return ApiResponse<bool>.Ok(true);
        }

        public async Task<ApiResponse<bool>> ValidateSystemAsync(string id)
        {
            await EnsureInitializedAsync();
            await Task.Delay(50);
            var system = _dataStore.Systems.FirstOrDefault(s => s.Id == id);
            if (system == null)
                return ApiResponse<bool>.Fail("System not found", "NOT_FOUND");

            var errors = new List<ValidationError>();
            if (string.IsNullOrEmpty(system.Name))
                errors.Add(new ValidationError { Field = "Name", Message = "Name is required" });

            if (errors.Any())
                return ApiResponse<bool>.ValidationFail(errors);

            system.State = ComponentState.Validated;
            _repository.MarkDirty();
            return ApiResponse<bool>.Ok(true);
        }

        public async Task<ApiResponse<List<SystemDto>>> SearchPredefinedSystemsAsync(string query)
        {
            await EnsureInitializedAsync();
            await Task.Delay(50);
            var results = _dataStore.Systems
                .Where(s => s.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                           (s.DisplayName?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false))
                .ToList();
            return ApiResponse<List<SystemDto>>.Ok(results);
        }

        #endregion

        #region Unit Operations

        public async Task<ApiResponse<List<UnitDto>>> GetAllUnitsAsync()
        {
            await EnsureInitializedAsync();
            await Task.Delay(50);
            return ApiResponse<List<UnitDto>>.Ok(_dataStore.Units.ToList());
        }

        public async Task<ApiResponse<UnitDto>> GetUnitByIdAsync(string id)
        {
            await EnsureInitializedAsync();
            await Task.Delay(50);
            var unit = _dataStore.Units.FirstOrDefault(u => u.Id == id);
            if (unit == null)
                return ApiResponse<UnitDto>.Fail("Unit not found", "NOT_FOUND");
            return ApiResponse<UnitDto>.Ok(unit);
        }

        public async Task<ApiResponse<List<UnitDto>>> GetUnitsBySystemIdAsync(string systemId)
        {
            await EnsureInitializedAsync();
            await Task.Delay(50);
            var units = _dataStore.Units.Where(u => u.SystemId == systemId).ToList();
            return ApiResponse<List<UnitDto>>.Ok(units);
        }

        public async Task<ApiResponse<UnitDto>> CreateUnitAsync(UnitDto unit)
        {
            await EnsureInitializedAsync();
            await Task.Delay(50);
            unit.Id = $"unit-{Guid.NewGuid():N}".Substring(0, 12);
            unit.CreatedAt = DateTime.Now;
            unit.UpdatedAt = DateTime.Now;
            unit.State = ComponentState.Undefined;
            _dataStore.Units.Add(unit);
            _repository.MarkDirty();
            return ApiResponse<UnitDto>.Ok(unit);
        }

        public async Task<ApiResponse<UnitDto>> UpdateUnitAsync(string id, UnitDto unit)
        {
            await EnsureInitializedAsync();
            await Task.Delay(50);
            var index = _dataStore.Units.FindIndex(u => u.Id == id);
            if (index < 0)
                return ApiResponse<UnitDto>.Fail("Unit not found", "NOT_FOUND");

            unit.Id = id;
            unit.UpdatedAt = DateTime.Now;
            _dataStore.Units[index] = unit;
            _repository.MarkDirty();

            return ApiResponse<UnitDto>.Ok(unit);
        }

        public async Task<ApiResponse<bool>> DeleteUnitAsync(string id)
        {
            await EnsureInitializedAsync();
            await Task.Delay(50);
            var removed = _dataStore.Units.RemoveAll(u => u.Id == id) > 0;
            if (!removed)
                return ApiResponse<bool>.Fail("Unit not found", "NOT_FOUND");

            _repository.MarkDirty();
            return ApiResponse<bool>.Ok(true);
        }

        public async Task<ApiResponse<bool>> ValidateUnitAsync(string id)
        {
            await EnsureInitializedAsync();
            await Task.Delay(50);
            var unit = _dataStore.Units.FirstOrDefault(u => u.Id == id);
            if (unit == null)
                return ApiResponse<bool>.Fail("Unit not found", "NOT_FOUND");

            var errors = new List<ValidationError>();
            if (string.IsNullOrEmpty(unit.Name))
                errors.Add(new ValidationError { Field = "Name", Message = "Name is required" });

            if (errors.Any())
                return ApiResponse<bool>.ValidationFail(errors);

            unit.State = ComponentState.Validated;
            _repository.MarkDirty();
            return ApiResponse<bool>.Ok(true);
        }

        public async Task<ApiResponse<List<UnitDto>>> SearchPredefinedUnitsAsync(string query)
        {
            await EnsureInitializedAsync();
            await Task.Delay(50);
            var results = _dataStore.Units
                .Where(u => u.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                           (u.DisplayName?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false))
                .ToList();
            return ApiResponse<List<UnitDto>>.Ok(results);
        }

        #endregion

        #region Device Operations

        public async Task<ApiResponse<List<DeviceDto>>> GetAllDevicesAsync()
        {
            await EnsureInitializedAsync();
            await Task.Delay(50);
            return ApiResponse<List<DeviceDto>>.Ok(_dataStore.Devices.ToList());
        }

        public async Task<ApiResponse<DeviceDto>> GetDeviceByIdAsync(string id)
        {
            await EnsureInitializedAsync();
            await Task.Delay(50);
            var device = _dataStore.Devices.FirstOrDefault(d => d.Id == id);
            if (device == null)
                return ApiResponse<DeviceDto>.Fail("Device not found", "NOT_FOUND");
            return ApiResponse<DeviceDto>.Ok(device);
        }

        public async Task<ApiResponse<List<DeviceDto>>> GetDevicesByUnitIdAsync(string unitId)
        {
            await EnsureInitializedAsync();
            await Task.Delay(50);
            var devices = _dataStore.Devices.Where(d => d.UnitId == unitId).ToList();
            return ApiResponse<List<DeviceDto>>.Ok(devices);
        }

        public async Task<ApiResponse<DeviceDto>> CreateDeviceAsync(DeviceDto device)
        {
            await EnsureInitializedAsync();
            await Task.Delay(50);
            device.Id = $"dev-{Guid.NewGuid():N}".Substring(0, 11);
            device.CreatedAt = DateTime.Now;
            device.UpdatedAt = DateTime.Now;
            device.State = ComponentState.Undefined;
            _dataStore.Devices.Add(device);
            _repository.MarkDirty();
            return ApiResponse<DeviceDto>.Ok(device);
        }

        public async Task<ApiResponse<DeviceDto>> UpdateDeviceAsync(string id, DeviceDto device)
        {
            await EnsureInitializedAsync();
            await Task.Delay(50);
            var index = _dataStore.Devices.FindIndex(d => d.Id == id);
            if (index < 0)
                return ApiResponse<DeviceDto>.Fail("Device not found", "NOT_FOUND");

            device.Id = id;
            device.UpdatedAt = DateTime.Now;
            _dataStore.Devices[index] = device;
            _repository.MarkDirty();

            return ApiResponse<DeviceDto>.Ok(device);
        }

        public async Task<ApiResponse<bool>> DeleteDeviceAsync(string id)
        {
            await EnsureInitializedAsync();
            await Task.Delay(50);
            var removed = _dataStore.Devices.RemoveAll(d => d.Id == id) > 0;
            if (!removed)
                return ApiResponse<bool>.Fail("Device not found", "NOT_FOUND");

            _repository.MarkDirty();
            return ApiResponse<bool>.Ok(true);
        }

        public async Task<ApiResponse<bool>> ValidateDeviceAsync(string id)
        {
            await EnsureInitializedAsync();
            await Task.Delay(50);
            var device = _dataStore.Devices.FirstOrDefault(d => d.Id == id);
            if (device == null)
                return ApiResponse<bool>.Fail("Device not found", "NOT_FOUND");

            var errors = new List<ValidationError>();
            if (string.IsNullOrEmpty(device.Name))
                errors.Add(new ValidationError { Field = "Name", Message = "Name is required" });

            if (errors.Any())
                return ApiResponse<bool>.ValidationFail(errors);

            device.State = ComponentState.Validated;
            _repository.MarkDirty();
            return ApiResponse<bool>.Ok(true);
        }

        public async Task<ApiResponse<List<DeviceDto>>> SearchPredefinedDevicesAsync(string query)
        {
            await EnsureInitializedAsync();
            await Task.Delay(50);
            var results = _dataStore.Devices
                .Where(d => d.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                           (d.DisplayName?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false))
                .ToList();
            return ApiResponse<List<DeviceDto>>.Ok(results);
        }

        #endregion
    }
}