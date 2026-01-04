using System.Collections.Generic;
using System.Threading.Tasks;
using EquipmentDesigner.Models;
using EquipmentDesigner.Models.Dtos;

namespace EquipmentDesigner.Services.Api
{
    /// <summary>
    /// Equipment API 서비스 인터페이스
    /// </summary>
    public interface IEquipmentApiService
    {
        #region Equipment Operations

        /// <summary>
        /// 모든 Equipment 조회
        /// </summary>
        Task<ApiResponse<List<EquipmentDto>>> GetAllEquipmentsAsync();

        /// <summary>
        /// Equipment ID로 조회
        /// </summary>
        Task<ApiResponse<EquipmentDto>> GetEquipmentByIdAsync(string id);

        /// <summary>
        /// Equipment 생성
        /// </summary>
        Task<ApiResponse<EquipmentDto>> CreateEquipmentAsync(EquipmentDto equipment);

        /// <summary>
        /// Equipment 수정
        /// </summary>
        Task<ApiResponse<EquipmentDto>> UpdateEquipmentAsync(string id, EquipmentDto equipment);

        /// <summary>
        /// Equipment 삭제
        /// </summary>
        Task<ApiResponse<bool>> DeleteEquipmentAsync(string id);

        /// <summary>
        /// Equipment 검증
        /// </summary>
        Task<ApiResponse<bool>> ValidateEquipmentAsync(string id);

        /// <summary>
        /// Equipment 서버 업로드
        /// </summary>
        Task<ApiResponse<EquipmentDto>> UploadEquipmentAsync(string id);

        /// <summary>
        /// 서버에서 Predefined Equipment 검색
        /// </summary>
        Task<ApiResponse<List<EquipmentDto>>> SearchPredefinedEquipmentsAsync(string query);

        #endregion

        #region System Operations

        /// <summary>
        /// 모든 System 조회
        /// </summary>
        Task<ApiResponse<List<SystemDto>>> GetAllSystemsAsync();

        /// <summary>
        /// System ID로 조회
        /// </summary>
        Task<ApiResponse<SystemDto>> GetSystemByIdAsync(string id);

        /// <summary>
        /// Equipment에 속한 System 목록 조회
        /// </summary>
        Task<ApiResponse<List<SystemDto>>> GetSystemsByEquipmentIdAsync(string equipmentId);

        /// <summary>
        /// System 생성
        /// </summary>
        Task<ApiResponse<SystemDto>> CreateSystemAsync(SystemDto system);

        /// <summary>
        /// System 수정
        /// </summary>
        Task<ApiResponse<SystemDto>> UpdateSystemAsync(string id, SystemDto system);

        /// <summary>
        /// System 삭제
        /// </summary>
        Task<ApiResponse<bool>> DeleteSystemAsync(string id);

        /// <summary>
        /// System 검증
        /// </summary>
        Task<ApiResponse<bool>> ValidateSystemAsync(string id);

        /// <summary>
        /// 서버에서 Predefined System 검색
        /// </summary>
        Task<ApiResponse<List<SystemDto>>> SearchPredefinedSystemsAsync(string query);

        #endregion

        #region Unit Operations

        /// <summary>
        /// 모든 Unit 조회
        /// </summary>
        Task<ApiResponse<List<UnitDto>>> GetAllUnitsAsync();

        /// <summary>
        /// Unit ID로 조회
        /// </summary>
        Task<ApiResponse<UnitDto>> GetUnitByIdAsync(string id);

        /// <summary>
        /// System에 속한 Unit 목록 조회
        /// </summary>
        Task<ApiResponse<List<UnitDto>>> GetUnitsBySystemIdAsync(string systemId);

        /// <summary>
        /// Unit 생성
        /// </summary>
        Task<ApiResponse<UnitDto>> CreateUnitAsync(UnitDto unit);

        /// <summary>
        /// Unit 수정
        /// </summary>
        Task<ApiResponse<UnitDto>> UpdateUnitAsync(string id, UnitDto unit);

        /// <summary>
        /// Unit 삭제
        /// </summary>
        Task<ApiResponse<bool>> DeleteUnitAsync(string id);

        /// <summary>
        /// Unit 검증
        /// </summary>
        Task<ApiResponse<bool>> ValidateUnitAsync(string id);

        /// <summary>
        /// 서버에서 Predefined Unit 검색
        /// </summary>
        Task<ApiResponse<List<UnitDto>>> SearchPredefinedUnitsAsync(string query);

        #endregion

        #region Device Operations

        /// <summary>
        /// 모든 Device 조회
        /// </summary>
        Task<ApiResponse<List<DeviceDto>>> GetAllDevicesAsync();

        /// <summary>
        /// Device ID로 조회
        /// </summary>
        Task<ApiResponse<DeviceDto>> GetDeviceByIdAsync(string id);

        /// <summary>
        /// Unit에 속한 Device 목록 조회
        /// </summary>
        Task<ApiResponse<List<DeviceDto>>> GetDevicesByUnitIdAsync(string unitId);

        /// <summary>
        /// Device 생성
        /// </summary>
        Task<ApiResponse<DeviceDto>> CreateDeviceAsync(DeviceDto device);

        /// <summary>
        /// Device 수정
        /// </summary>
        Task<ApiResponse<DeviceDto>> UpdateDeviceAsync(string id, DeviceDto device);

        /// <summary>
        /// Device 삭제
        /// </summary>
        Task<ApiResponse<bool>> DeleteDeviceAsync(string id);

        /// <summary>
        /// Device 검증
        /// </summary>
        Task<ApiResponse<bool>> ValidateDeviceAsync(string id);

        /// <summary>
        /// 서버에서 Predefined Device 검색
        /// </summary>
        Task<ApiResponse<List<DeviceDto>>> SearchPredefinedDevicesAsync(string query);

        #endregion
    }
}
