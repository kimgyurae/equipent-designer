// API 서버가 준비되지 않은 경우 USE_MOCK_DATA 심볼을 정의하여 하드코딩된 Mock 데이터 사용
// csproj에서 <DefineConstants>USE_MOCK_DATA</DefineConstants> 추가 또는
// 빌드 시 /p:DefineConstants="USE_MOCK_DATA" 사용
#define USE_MOCK_DATA

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using EquipmentDesigner.Constants;
using EquipmentDesigner.Models;
using EquipmentDesigner.Models.Dtos;

namespace EquipmentDesigner.Services.Api
{
    /// <summary>
    /// Equipment API 서비스 구현체
    /// USE_MOCK_DATA 전처리문이 정의되면 하드코딩된 Mock 데이터를 반환합니다.
    /// </summary>
    public class EquipmentApiService : IEquipmentApiService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

#if USE_MOCK_DATA
        // Mock 데이터 저장소
        private static readonly List<EquipmentDto> MockEquipments = new List<EquipmentDto>();
        private static readonly List<SystemDto> MockSystems = new List<SystemDto>();
        private static readonly List<UnitDto> MockUnits = new List<UnitDto>();
        private static readonly List<DeviceDto> MockDevices = new List<DeviceDto>();
        private static bool _isInitialized = false;
#endif

        public EquipmentApiService()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(ApiConstants.ApiBaseUrl),
                Timeout = TimeSpan.FromSeconds(ApiConstants.DefaultTimeoutSeconds)
            };

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

#if USE_MOCK_DATA
            InitializeMockData();
#endif
        }

        public EquipmentApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

#if USE_MOCK_DATA
            InitializeMockData();
#endif
        }

#if USE_MOCK_DATA
        /// <summary>
        /// Mock 데이터 초기화 (LaserBurr 설비 예제 데이터 기반)
        /// </summary>
        private static void InitializeMockData()
        {
            if (_isInitialized) return;

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

            // Sample Systems
            var loadingSystem = new SystemDto
            {
                Id = "sys-001",
                EquipmentId = "eq-001",
                Name = "Loading",
                DisplayName = "Loading System",
                Description = "Material loading system managing OP side shutter, gasket detection, and safety interlocks",
                State = ComponentState.Defined,
                ImplementationInstructions = new List<string>
                {
                    "This System handles material loading from the operator side.",
                    "Coordinates with Safety interlocks during loading operations."
                },
                Commands = new List<CommandDto>
                {
                    new CommandDto
                    {
                        Name = "OpenForLoading",
                        Description = "Open OP shutter and prepare for material loading",
                        Parameters = new List<ParameterDto>()
                    },
                    new CommandDto
                    {
                        Name = "CloseAfterLoading",
                        Description = "Close OP shutter after material loaded",
                        Parameters = new List<ParameterDto>()
                    }
                },
                CreatedAt = DateTime.Now.AddDays(-25),
                UpdatedAt = DateTime.Now
            };

            var stageSystem = new SystemDto
            {
                Id = "sys-002",
                EquipmentId = "eq-001",
                Name = "Stage",
                DisplayName = "Stage System",
                Description = "Motion control system managing all 6 axes for workpiece and head positioning",
                State = ComponentState.Defined,
                ImplementationInstructions = new List<string>
                {
                    "This System controls all motion axes using WMX3 motion controller.",
                    "Homing sequence: Head Z -> Head X -> Stage Y -> Table X -> Table Y -> Table T"
                },
                Commands = new List<CommandDto>
                {
                    new CommandDto
                    {
                        Name = "HomeAll",
                        Description = "Execute home sequence for all axes in defined order",
                        Parameters = new List<ParameterDto>()
                    },
                    new CommandDto
                    {
                        Name = "StopAll",
                        Description = "Stop all axis motion immediately",
                        Parameters = new List<ParameterDto>()
                    }
                },
                CreatedAt = DateTime.Now.AddDays(-25),
                UpdatedAt = DateTime.Now
            };

            var laserSystem = new SystemDto
            {
                Id = "sys-003",
                EquipmentId = "eq-001",
                Name = "Laser",
                DisplayName = "Laser System",
                Description = "Laser processing system managing laser shutter, ready status, and chiller cooling",
                State = ComponentState.Defined,
                Commands = new List<CommandDto>
                {
                    new CommandDto
                    {
                        Name = "PrepareForProcessing",
                        Description = "Prepare laser for processing (check ready, open shutter)",
                        Parameters = new List<ParameterDto>()
                    }
                },
                CreatedAt = DateTime.Now.AddDays(-20),
                UpdatedAt = DateTime.Now
            };

            // Sample Units
            var opShutterUnit = new UnitDto
            {
                Id = "unit-001",
                SystemId = "sys-001",
                Name = "OPShutter",
                DisplayName = "OP Side Shutter Unit",
                Description = "Operator side shutter for material access control",
                State = ComponentState.Defined,
                ImplementationInstructions = new List<string>
                {
                    "Controls OP side shutter for gasket loading.",
                    "Shutter UP = Open (allow material access), DOWN = Closed (safety)."
                },
                Commands = new List<CommandDto>
                {
                    new CommandDto
                    {
                        Name = "Open",
                        Description = "Open OP side shutter (UP position)",
                        Parameters = new List<ParameterDto>
                        {
                            new ParameterDto
                            {
                                Name = "wait",
                                Type = "bool",
                                Description = "Wait for position confirmation",
                                Required = false,
                                DefaultValue = true
                            }
                        }
                    },
                    new CommandDto
                    {
                        Name = "Close",
                        Description = "Close OP side shutter (DOWN position)",
                        Parameters = new List<ParameterDto>
                        {
                            new ParameterDto
                            {
                                Name = "wait",
                                Type = "bool",
                                Description = "Wait for position confirmation",
                                Required = false,
                                DefaultValue = true
                            }
                        }
                    }
                },
                CreatedAt = DateTime.Now.AddDays(-20),
                UpdatedAt = DateTime.Now
            };

            var headUnit = new UnitDto
            {
                Id = "unit-002",
                SystemId = "sys-002",
                Name = "Head",
                DisplayName = "Head Unit",
                Description = "Laser head positioning unit with Z (vertical) and X (horizontal) axes",
                State = ComponentState.Defined,
                Commands = new List<CommandDto>
                {
                    new CommandDto
                    {
                        Name = "Home",
                        Description = "Execute home sequence for Head axes (Z first, then X)",
                        Parameters = new List<ParameterDto>()
                    },
                    new CommandDto
                    {
                        Name = "MoveTo",
                        Description = "Move head to specified position",
                        Parameters = new List<ParameterDto>
                        {
                            new ParameterDto { Name = "posZ", Type = "double", Description = "Z-axis target position (mm)", Required = true },
                            new ParameterDto { Name = "posX", Type = "double", Description = "X-axis target position (mm)", Required = true }
                        }
                    }
                },
                CreatedAt = DateTime.Now.AddDays(-18),
                UpdatedAt = DateTime.Now
            };

            // Sample Devices
            var opShutterCylinder = new DeviceDto
            {
                Id = "dev-001",
                UnitId = "unit-001",
                Name = "OPShutterCylinder",
                DisplayName = "OP Shutter Cylinder",
                Description = "OP side shutter dual solenoid cylinder",
                DeviceType = "Generic",
                State = ComponentState.Defined,
                ImplementationInstructions = new List<string>
                {
                    "IO binding:",
                    "  Output: OUT_OP_SHUTTER_UP_SOL(0x0010), OUT_OP_SHUTTER_DN_SOL(0x0011)",
                    "  Input: IN_OP_SIDE_SHUTTER_UP(0x0020), IN_OP_SIDE_SHUTTER_DN(0x0021)"
                },
                Commands = new List<CommandDto>
                {
                    new CommandDto
                    {
                        Name = "Extend",
                        Description = "Open shutter (UP)",
                        Parameters = new List<ParameterDto>
                        {
                            new ParameterDto { Name = "wait", Type = "bool", Description = "Wait for position confirmation", Required = false, DefaultValue = true }
                        }
                    },
                    new CommandDto
                    {
                        Name = "Retract",
                        Description = "Close shutter (DOWN)",
                        Parameters = new List<ParameterDto>
                        {
                            new ParameterDto { Name = "wait", Type = "bool", Description = "Wait for position confirmation", Required = false, DefaultValue = true }
                        }
                    }
                },
                IoInfo = new List<IoInfoDto>
                {
                    new IoInfoDto { Name = "OUT_OP_SHUTTER_UP_SOL", IoType = "Output", Address = "0x0010", Description = "Shutter UP solenoid" },
                    new IoInfoDto { Name = "OUT_OP_SHUTTER_DN_SOL", IoType = "Output", Address = "0x0011", Description = "Shutter DOWN solenoid" },
                    new IoInfoDto { Name = "IN_OP_SIDE_SHUTTER_UP", IoType = "Input", Address = "0x0020", Description = "Shutter UP sensor" },
                    new IoInfoDto { Name = "IN_OP_SIDE_SHUTTER_DN", IoType = "Input", Address = "0x0021", Description = "Shutter DOWN sensor" }
                },
                CreatedAt = DateTime.Now.AddDays(-15),
                UpdatedAt = DateTime.Now
            };

            var headZAxis = new DeviceDto
            {
                Id = "dev-002",
                UnitId = "unit-002",
                Name = "HeadZAxis",
                DisplayName = "Head Z Axis",
                Description = "Head vertical positioning axis",
                DeviceType = "Generic",
                State = ComponentState.Defined,
                ImplementationInstructions = new List<string>
                {
                    "WMX3 axis control for Head Z movement.",
                    "Axis Index: AXIS_HEAD_Z = 5",
                    "Home position is UP (away from workpiece for safety)."
                },
                Commands = new List<CommandDto>
                {
                    new CommandDto { Name = "Home", Description = "Execute home sequence for this axis", Parameters = new List<ParameterDto>() },
                    new CommandDto
                    {
                        Name = "MoveTo",
                        Description = "Move to target position",
                        Parameters = new List<ParameterDto>
                        {
                            new ParameterDto { Name = "position", Type = "double", Description = "Target position (mm)", Required = true }
                        }
                    },
                    new CommandDto { Name = "Stop", Description = "Stop axis motion", Parameters = new List<ParameterDto>() },
                    new CommandDto { Name = "GetPosition", Description = "Get current position", Parameters = new List<ParameterDto>() }
                },
                CreatedAt = DateTime.Now.AddDays(-15),
                UpdatedAt = DateTime.Now
            };

            // Add to mock storage
            MockEquipments.Add(equipment);
            MockSystems.AddRange(new[] { loadingSystem, stageSystem, laserSystem });
            MockUnits.AddRange(new[] { opShutterUnit, headUnit });
            MockDevices.AddRange(new[] { opShutterCylinder, headZAxis });

            _isInitialized = true;
        }
#endif

        #region Equipment Operations

        public async Task<ApiResponse<List<EquipmentDto>>> GetAllEquipmentsAsync()
        {
#if USE_MOCK_DATA
            await Task.Delay(100); // Simulate network delay
            return ApiResponse<List<EquipmentDto>>.Ok(MockEquipments.ToList());
#else
            try
            {
                var response = await _httpClient.GetAsync(ApiConstants.Equipment.GetAll);
                return await ParseResponseAsync<List<EquipmentDto>>(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<EquipmentDto>>.Fail($"Failed to get equipments: {ex.Message}");
            }
#endif
        }

        public async Task<ApiResponse<EquipmentDto>> GetEquipmentByIdAsync(string id)
        {
#if USE_MOCK_DATA
            await Task.Delay(50);
            var equipment = MockEquipments.FirstOrDefault(e => e.Id == id);
            if (equipment == null)
                return ApiResponse<EquipmentDto>.Fail("Equipment not found", "NOT_FOUND");
            return ApiResponse<EquipmentDto>.Ok(equipment);
#else
            try
            {
                var response = await _httpClient.GetAsync(ApiConstants.Equipment.GetById(id));
                return await ParseResponseAsync<EquipmentDto>(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<EquipmentDto>.Fail($"Failed to get equipment: {ex.Message}");
            }
#endif
        }

        public async Task<ApiResponse<EquipmentDto>> CreateEquipmentAsync(EquipmentDto equipment)
        {
#if USE_MOCK_DATA
            await Task.Delay(100);
            equipment.Id = $"eq-{Guid.NewGuid():N}".Substring(0, 10);
            equipment.CreatedAt = DateTime.Now;
            equipment.UpdatedAt = DateTime.Now;
            equipment.State = ComponentState.Undefined;
            MockEquipments.Add(equipment);
            return ApiResponse<EquipmentDto>.Ok(equipment);
#else
            try
            {
                var content = new StringContent(JsonSerializer.Serialize(equipment, _jsonOptions), Encoding.UTF8, ApiConstants.ContentTypes.Json);
                var response = await _httpClient.PostAsync(ApiConstants.Equipment.Create, content);
                return await ParseResponseAsync<EquipmentDto>(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<EquipmentDto>.Fail($"Failed to create equipment: {ex.Message}");
            }
#endif
        }

        public async Task<ApiResponse<EquipmentDto>> UpdateEquipmentAsync(string id, EquipmentDto equipment)
        {
#if USE_MOCK_DATA
            await Task.Delay(100);
            var index = MockEquipments.FindIndex(e => e.Id == id);
            if (index < 0)
                return ApiResponse<EquipmentDto>.Fail("Equipment not found", "NOT_FOUND");

            equipment.Id = id;
            equipment.UpdatedAt = DateTime.Now;
            MockEquipments[index] = equipment;
            return ApiResponse<EquipmentDto>.Ok(equipment);
#else
            try
            {
                var content = new StringContent(JsonSerializer.Serialize(equipment, _jsonOptions), Encoding.UTF8, ApiConstants.ContentTypes.Json);
                var response = await _httpClient.PutAsync(ApiConstants.Equipment.Update(id), content);
                return await ParseResponseAsync<EquipmentDto>(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<EquipmentDto>.Fail($"Failed to update equipment: {ex.Message}");
            }
#endif
        }

        public async Task<ApiResponse<bool>> DeleteEquipmentAsync(string id)
        {
#if USE_MOCK_DATA
            await Task.Delay(100);
            var removed = MockEquipments.RemoveAll(e => e.Id == id) > 0;
            return removed
                ? ApiResponse<bool>.Ok(true)
                : ApiResponse<bool>.Fail("Equipment not found", "NOT_FOUND");
#else
            try
            {
                var response = await _httpClient.DeleteAsync(ApiConstants.Equipment.Delete(id));
                return await ParseResponseAsync<bool>(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Fail($"Failed to delete equipment: {ex.Message}");
            }
#endif
        }

        public async Task<ApiResponse<bool>> ValidateEquipmentAsync(string id)
        {
#if USE_MOCK_DATA
            await Task.Delay(100);
            var equipment = MockEquipments.FirstOrDefault(e => e.Id == id);
            if (equipment == null)
                return ApiResponse<bool>.Fail("Equipment not found", "NOT_FOUND");

            // Simple validation: check required fields
            var errors = new List<ValidationError>();
            if (string.IsNullOrEmpty(equipment.Name))
                errors.Add(new ValidationError { Field = "Name", Message = "Name is required" });
            if (string.IsNullOrEmpty(equipment.EquipmentType))
                errors.Add(new ValidationError { Field = "EquipmentType", Message = "Equipment type is required" });

            if (errors.Any())
                return ApiResponse<bool>.ValidationFail(errors);

            equipment.State = ComponentState.Validated;
            return ApiResponse<bool>.Ok(true);
#else
            try
            {
                var response = await _httpClient.PostAsync(ApiConstants.Equipment.Validate(id), null);
                return await ParseResponseAsync<bool>(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Fail($"Failed to validate equipment: {ex.Message}");
            }
#endif
        }

        public async Task<ApiResponse<EquipmentDto>> UploadEquipmentAsync(string id)
        {
#if USE_MOCK_DATA
            await Task.Delay(200);
            var equipment = MockEquipments.FirstOrDefault(e => e.Id == id);
            if (equipment == null)
                return ApiResponse<EquipmentDto>.Fail("Equipment not found", "NOT_FOUND");

            equipment.State = ComponentState.Uploaded;
            equipment.UpdatedAt = DateTime.Now;
            return ApiResponse<EquipmentDto>.Ok(equipment);
#else
            try
            {
                var response = await _httpClient.PostAsync(ApiConstants.Equipment.Upload(id), null);
                return await ParseResponseAsync<EquipmentDto>(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<EquipmentDto>.Fail($"Failed to upload equipment: {ex.Message}");
            }
#endif
        }

        public async Task<ApiResponse<List<EquipmentDto>>> SearchPredefinedEquipmentsAsync(string query)
        {
#if USE_MOCK_DATA
            await Task.Delay(100);
            var results = MockEquipments
                .Where(e => e.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                           (e.DisplayName?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false))
                .ToList();
            return ApiResponse<List<EquipmentDto>>.Ok(results);
#else
            try
            {
                var response = await _httpClient.GetAsync(ApiConstants.Import.SearchEquipments(query));
                return await ParseResponseAsync<List<EquipmentDto>>(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<EquipmentDto>>.Fail($"Failed to search equipments: {ex.Message}");
            }
#endif
        }

        #endregion

        #region System Operations

        public async Task<ApiResponse<List<SystemDto>>> GetAllSystemsAsync()
        {
#if USE_MOCK_DATA
            await Task.Delay(100);
            return ApiResponse<List<SystemDto>>.Ok(MockSystems.ToList());
#else
            try
            {
                var response = await _httpClient.GetAsync(ApiConstants.System.GetAll);
                return await ParseResponseAsync<List<SystemDto>>(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<SystemDto>>.Fail($"Failed to get systems: {ex.Message}");
            }
#endif
        }

        public async Task<ApiResponse<SystemDto>> GetSystemByIdAsync(string id)
        {
#if USE_MOCK_DATA
            await Task.Delay(50);
            var system = MockSystems.FirstOrDefault(s => s.Id == id);
            if (system == null)
                return ApiResponse<SystemDto>.Fail("System not found", "NOT_FOUND");
            return ApiResponse<SystemDto>.Ok(system);
#else
            try
            {
                var response = await _httpClient.GetAsync(ApiConstants.System.GetById(id));
                return await ParseResponseAsync<SystemDto>(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<SystemDto>.Fail($"Failed to get system: {ex.Message}");
            }
#endif
        }

        public async Task<ApiResponse<List<SystemDto>>> GetSystemsByEquipmentIdAsync(string equipmentId)
        {
#if USE_MOCK_DATA
            await Task.Delay(100);
            var systems = MockSystems.Where(s => s.EquipmentId == equipmentId).ToList();
            return ApiResponse<List<SystemDto>>.Ok(systems);
#else
            try
            {
                var response = await _httpClient.GetAsync(ApiConstants.System.GetByEquipmentId(equipmentId));
                return await ParseResponseAsync<List<SystemDto>>(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<SystemDto>>.Fail($"Failed to get systems: {ex.Message}");
            }
#endif
        }

        public async Task<ApiResponse<SystemDto>> CreateSystemAsync(SystemDto system)
        {
#if USE_MOCK_DATA
            await Task.Delay(100);
            system.Id = $"sys-{Guid.NewGuid():N}".Substring(0, 10);
            system.CreatedAt = DateTime.Now;
            system.UpdatedAt = DateTime.Now;
            system.State = ComponentState.Undefined;
            MockSystems.Add(system);
            return ApiResponse<SystemDto>.Ok(system);
#else
            try
            {
                var content = new StringContent(JsonSerializer.Serialize(system, _jsonOptions), Encoding.UTF8, ApiConstants.ContentTypes.Json);
                var response = await _httpClient.PostAsync(ApiConstants.System.Create, content);
                return await ParseResponseAsync<SystemDto>(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<SystemDto>.Fail($"Failed to create system: {ex.Message}");
            }
#endif
        }

        public async Task<ApiResponse<SystemDto>> UpdateSystemAsync(string id, SystemDto system)
        {
#if USE_MOCK_DATA
            await Task.Delay(100);
            var index = MockSystems.FindIndex(s => s.Id == id);
            if (index < 0)
                return ApiResponse<SystemDto>.Fail("System not found", "NOT_FOUND");

            system.Id = id;
            system.UpdatedAt = DateTime.Now;
            MockSystems[index] = system;
            return ApiResponse<SystemDto>.Ok(system);
#else
            try
            {
                var content = new StringContent(JsonSerializer.Serialize(system, _jsonOptions), Encoding.UTF8, ApiConstants.ContentTypes.Json);
                var response = await _httpClient.PutAsync(ApiConstants.System.Update(id), content);
                return await ParseResponseAsync<SystemDto>(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<SystemDto>.Fail($"Failed to update system: {ex.Message}");
            }
#endif
        }

        public async Task<ApiResponse<bool>> DeleteSystemAsync(string id)
        {
#if USE_MOCK_DATA
            await Task.Delay(100);
            var removed = MockSystems.RemoveAll(s => s.Id == id) > 0;
            return removed
                ? ApiResponse<bool>.Ok(true)
                : ApiResponse<bool>.Fail("System not found", "NOT_FOUND");
#else
            try
            {
                var response = await _httpClient.DeleteAsync(ApiConstants.System.Delete(id));
                return await ParseResponseAsync<bool>(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Fail($"Failed to delete system: {ex.Message}");
            }
#endif
        }

        public async Task<ApiResponse<bool>> ValidateSystemAsync(string id)
        {
#if USE_MOCK_DATA
            await Task.Delay(100);
            var system = MockSystems.FirstOrDefault(s => s.Id == id);
            if (system == null)
                return ApiResponse<bool>.Fail("System not found", "NOT_FOUND");

            var errors = new List<ValidationError>();
            if (string.IsNullOrEmpty(system.Name))
                errors.Add(new ValidationError { Field = "Name", Message = "Name is required" });

            if (errors.Any())
                return ApiResponse<bool>.ValidationFail(errors);

            system.State = ComponentState.Validated;
            return ApiResponse<bool>.Ok(true);
#else
            try
            {
                var response = await _httpClient.PostAsync(ApiConstants.System.Validate(id), null);
                return await ParseResponseAsync<bool>(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Fail($"Failed to validate system: {ex.Message}");
            }
#endif
        }

        public async Task<ApiResponse<List<SystemDto>>> SearchPredefinedSystemsAsync(string query)
        {
#if USE_MOCK_DATA
            await Task.Delay(100);
            var results = MockSystems
                .Where(s => s.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                           (s.DisplayName?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false))
                .ToList();
            return ApiResponse<List<SystemDto>>.Ok(results);
#else
            try
            {
                var response = await _httpClient.GetAsync(ApiConstants.Import.SearchSystems(query));
                return await ParseResponseAsync<List<SystemDto>>(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<SystemDto>>.Fail($"Failed to search systems: {ex.Message}");
            }
#endif
        }

        #endregion

        #region Unit Operations

        public async Task<ApiResponse<List<UnitDto>>> GetAllUnitsAsync()
        {
#if USE_MOCK_DATA
            await Task.Delay(100);
            return ApiResponse<List<UnitDto>>.Ok(MockUnits.ToList());
#else
            try
            {
                var response = await _httpClient.GetAsync(ApiConstants.Unit.GetAll);
                return await ParseResponseAsync<List<UnitDto>>(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<UnitDto>>.Fail($"Failed to get units: {ex.Message}");
            }
#endif
        }

        public async Task<ApiResponse<UnitDto>> GetUnitByIdAsync(string id)
        {
#if USE_MOCK_DATA
            await Task.Delay(50);
            var unit = MockUnits.FirstOrDefault(u => u.Id == id);
            if (unit == null)
                return ApiResponse<UnitDto>.Fail("Unit not found", "NOT_FOUND");
            return ApiResponse<UnitDto>.Ok(unit);
#else
            try
            {
                var response = await _httpClient.GetAsync(ApiConstants.Unit.GetById(id));
                return await ParseResponseAsync<UnitDto>(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<UnitDto>.Fail($"Failed to get unit: {ex.Message}");
            }
#endif
        }

        public async Task<ApiResponse<List<UnitDto>>> GetUnitsBySystemIdAsync(string systemId)
        {
#if USE_MOCK_DATA
            await Task.Delay(100);
            var units = MockUnits.Where(u => u.SystemId == systemId).ToList();
            return ApiResponse<List<UnitDto>>.Ok(units);
#else
            try
            {
                var response = await _httpClient.GetAsync(ApiConstants.Unit.GetBySystemId(systemId));
                return await ParseResponseAsync<List<UnitDto>>(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<UnitDto>>.Fail($"Failed to get units: {ex.Message}");
            }
#endif
        }

        public async Task<ApiResponse<UnitDto>> CreateUnitAsync(UnitDto unit)
        {
#if USE_MOCK_DATA
            await Task.Delay(100);
            unit.Id = $"unit-{Guid.NewGuid():N}".Substring(0, 12);
            unit.CreatedAt = DateTime.Now;
            unit.UpdatedAt = DateTime.Now;
            unit.State = ComponentState.Undefined;
            MockUnits.Add(unit);
            return ApiResponse<UnitDto>.Ok(unit);
#else
            try
            {
                var content = new StringContent(JsonSerializer.Serialize(unit, _jsonOptions), Encoding.UTF8, ApiConstants.ContentTypes.Json);
                var response = await _httpClient.PostAsync(ApiConstants.Unit.Create, content);
                return await ParseResponseAsync<UnitDto>(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<UnitDto>.Fail($"Failed to create unit: {ex.Message}");
            }
#endif
        }

        public async Task<ApiResponse<UnitDto>> UpdateUnitAsync(string id, UnitDto unit)
        {
#if USE_MOCK_DATA
            await Task.Delay(100);
            var index = MockUnits.FindIndex(u => u.Id == id);
            if (index < 0)
                return ApiResponse<UnitDto>.Fail("Unit not found", "NOT_FOUND");

            unit.Id = id;
            unit.UpdatedAt = DateTime.Now;
            MockUnits[index] = unit;
            return ApiResponse<UnitDto>.Ok(unit);
#else
            try
            {
                var content = new StringContent(JsonSerializer.Serialize(unit, _jsonOptions), Encoding.UTF8, ApiConstants.ContentTypes.Json);
                var response = await _httpClient.PutAsync(ApiConstants.Unit.Update(id), content);
                return await ParseResponseAsync<UnitDto>(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<UnitDto>.Fail($"Failed to update unit: {ex.Message}");
            }
#endif
        }

        public async Task<ApiResponse<bool>> DeleteUnitAsync(string id)
        {
#if USE_MOCK_DATA
            await Task.Delay(100);
            var removed = MockUnits.RemoveAll(u => u.Id == id) > 0;
            return removed
                ? ApiResponse<bool>.Ok(true)
                : ApiResponse<bool>.Fail("Unit not found", "NOT_FOUND");
#else
            try
            {
                var response = await _httpClient.DeleteAsync(ApiConstants.Unit.Delete(id));
                return await ParseResponseAsync<bool>(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Fail($"Failed to delete unit: {ex.Message}");
            }
#endif
        }

        public async Task<ApiResponse<bool>> ValidateUnitAsync(string id)
        {
#if USE_MOCK_DATA
            await Task.Delay(100);
            var unit = MockUnits.FirstOrDefault(u => u.Id == id);
            if (unit == null)
                return ApiResponse<bool>.Fail("Unit not found", "NOT_FOUND");

            var errors = new List<ValidationError>();
            if (string.IsNullOrEmpty(unit.Name))
                errors.Add(new ValidationError { Field = "Name", Message = "Name is required" });

            if (errors.Any())
                return ApiResponse<bool>.ValidationFail(errors);

            unit.State = ComponentState.Validated;
            return ApiResponse<bool>.Ok(true);
#else
            try
            {
                var response = await _httpClient.PostAsync(ApiConstants.Unit.Validate(id), null);
                return await ParseResponseAsync<bool>(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Fail($"Failed to validate unit: {ex.Message}");
            }
#endif
        }

        public async Task<ApiResponse<List<UnitDto>>> SearchPredefinedUnitsAsync(string query)
        {
#if USE_MOCK_DATA
            await Task.Delay(100);
            var results = MockUnits
                .Where(u => u.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                           (u.DisplayName?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false))
                .ToList();
            return ApiResponse<List<UnitDto>>.Ok(results);
#else
            try
            {
                var response = await _httpClient.GetAsync(ApiConstants.Import.SearchUnits(query));
                return await ParseResponseAsync<List<UnitDto>>(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<UnitDto>>.Fail($"Failed to search units: {ex.Message}");
            }
#endif
        }

        #endregion

        #region Device Operations

        public async Task<ApiResponse<List<DeviceDto>>> GetAllDevicesAsync()
        {
#if USE_MOCK_DATA
            await Task.Delay(100);
            return ApiResponse<List<DeviceDto>>.Ok(MockDevices.ToList());
#else
            try
            {
                var response = await _httpClient.GetAsync(ApiConstants.Device.GetAll);
                return await ParseResponseAsync<List<DeviceDto>>(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<DeviceDto>>.Fail($"Failed to get devices: {ex.Message}");
            }
#endif
        }

        public async Task<ApiResponse<DeviceDto>> GetDeviceByIdAsync(string id)
        {
#if USE_MOCK_DATA
            await Task.Delay(50);
            var device = MockDevices.FirstOrDefault(d => d.Id == id);
            if (device == null)
                return ApiResponse<DeviceDto>.Fail("Device not found", "NOT_FOUND");
            return ApiResponse<DeviceDto>.Ok(device);
#else
            try
            {
                var response = await _httpClient.GetAsync(ApiConstants.Device.GetById(id));
                return await ParseResponseAsync<DeviceDto>(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<DeviceDto>.Fail($"Failed to get device: {ex.Message}");
            }
#endif
        }

        public async Task<ApiResponse<List<DeviceDto>>> GetDevicesByUnitIdAsync(string unitId)
        {
#if USE_MOCK_DATA
            await Task.Delay(100);
            var devices = MockDevices.Where(d => d.UnitId == unitId).ToList();
            return ApiResponse<List<DeviceDto>>.Ok(devices);
#else
            try
            {
                var response = await _httpClient.GetAsync(ApiConstants.Device.GetByUnitId(unitId));
                return await ParseResponseAsync<List<DeviceDto>>(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<DeviceDto>>.Fail($"Failed to get devices: {ex.Message}");
            }
#endif
        }

        public async Task<ApiResponse<DeviceDto>> CreateDeviceAsync(DeviceDto device)
        {
#if USE_MOCK_DATA
            await Task.Delay(100);
            device.Id = $"dev-{Guid.NewGuid():N}".Substring(0, 11);
            device.CreatedAt = DateTime.Now;
            device.UpdatedAt = DateTime.Now;
            device.State = ComponentState.Undefined;
            MockDevices.Add(device);
            return ApiResponse<DeviceDto>.Ok(device);
#else
            try
            {
                var content = new StringContent(JsonSerializer.Serialize(device, _jsonOptions), Encoding.UTF8, ApiConstants.ContentTypes.Json);
                var response = await _httpClient.PostAsync(ApiConstants.Device.Create, content);
                return await ParseResponseAsync<DeviceDto>(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<DeviceDto>.Fail($"Failed to create device: {ex.Message}");
            }
#endif
        }

        public async Task<ApiResponse<DeviceDto>> UpdateDeviceAsync(string id, DeviceDto device)
        {
#if USE_MOCK_DATA
            await Task.Delay(100);
            var index = MockDevices.FindIndex(d => d.Id == id);
            if (index < 0)
                return ApiResponse<DeviceDto>.Fail("Device not found", "NOT_FOUND");

            device.Id = id;
            device.UpdatedAt = DateTime.Now;
            MockDevices[index] = device;
            return ApiResponse<DeviceDto>.Ok(device);
#else
            try
            {
                var content = new StringContent(JsonSerializer.Serialize(device, _jsonOptions), Encoding.UTF8, ApiConstants.ContentTypes.Json);
                var response = await _httpClient.PutAsync(ApiConstants.Device.Update(id), content);
                return await ParseResponseAsync<DeviceDto>(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<DeviceDto>.Fail($"Failed to update device: {ex.Message}");
            }
#endif
        }

        public async Task<ApiResponse<bool>> DeleteDeviceAsync(string id)
        {
#if USE_MOCK_DATA
            await Task.Delay(100);
            var removed = MockDevices.RemoveAll(d => d.Id == id) > 0;
            return removed
                ? ApiResponse<bool>.Ok(true)
                : ApiResponse<bool>.Fail("Device not found", "NOT_FOUND");
#else
            try
            {
                var response = await _httpClient.DeleteAsync(ApiConstants.Device.Delete(id));
                return await ParseResponseAsync<bool>(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Fail($"Failed to delete device: {ex.Message}");
            }
#endif
        }

        public async Task<ApiResponse<bool>> ValidateDeviceAsync(string id)
        {
#if USE_MOCK_DATA
            await Task.Delay(100);
            var device = MockDevices.FirstOrDefault(d => d.Id == id);
            if (device == null)
                return ApiResponse<bool>.Fail("Device not found", "NOT_FOUND");

            var errors = new List<ValidationError>();
            if (string.IsNullOrEmpty(device.Name))
                errors.Add(new ValidationError { Field = "Name", Message = "Name is required" });

            if (errors.Any())
                return ApiResponse<bool>.ValidationFail(errors);

            device.State = ComponentState.Validated;
            return ApiResponse<bool>.Ok(true);
#else
            try
            {
                var response = await _httpClient.PostAsync(ApiConstants.Device.Validate(id), null);
                return await ParseResponseAsync<bool>(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Fail($"Failed to validate device: {ex.Message}");
            }
#endif
        }

        public async Task<ApiResponse<List<DeviceDto>>> SearchPredefinedDevicesAsync(string query)
        {
#if USE_MOCK_DATA
            await Task.Delay(100);
            var results = MockDevices
                .Where(d => d.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                           (d.DisplayName?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false))
                .ToList();
            return ApiResponse<List<DeviceDto>>.Ok(results);
#else
            try
            {
                var response = await _httpClient.GetAsync(ApiConstants.Import.SearchDevices(query));
                return await ParseResponseAsync<List<DeviceDto>>(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<DeviceDto>>.Fail($"Failed to search devices: {ex.Message}");
            }
#endif
        }

        #endregion

        #region Helper Methods

        private async Task<ApiResponse<T>> ParseResponseAsync<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var data = JsonSerializer.Deserialize<T>(content, _jsonOptions);
                    return ApiResponse<T>.Ok(data);
                }
                catch (JsonException ex)
                {
                    return ApiResponse<T>.Fail($"Failed to parse response: {ex.Message}", "PARSE_ERROR");
                }
            }

            return ApiResponse<T>.Fail($"API request failed with status {response.StatusCode}: {content}", response.StatusCode.ToString());
        }

        #endregion
    }
}
