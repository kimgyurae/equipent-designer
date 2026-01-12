using System.Collections.Generic;
using System.Threading.Tasks;
using EquipmentDesigner.Models;

namespace EquipmentDesigner.Services
{
    /// <summary>
    /// Hardware API service interface for managing uploaded hardware workflow sessions.
    /// Provides session-based CRUD operations with REST API semantics.
    /// </summary>
    public interface IHardwareApiService
    {
        /// <summary>
        /// Gets all workflow sessions.
        /// </summary>
        /// <returns>API response containing all workflow sessions.</returns>
        Task<ApiResponse<List<HardwareDefinition>>> GetAllSessionsAsync();

        /// <summary>
        /// Gets workflow sessions filtered by state.
        /// </summary>
        /// <param name="states">States to filter by.</param>
        /// <returns>API response containing filtered workflow sessions.</returns>
        Task<ApiResponse<List<HardwareDefinition>>> GetSessionsByStateAsync(params ComponentState[] states);

        /// <summary>
        /// Gets a specific workflow session by ID.
        /// </summary>
        /// <param name="workflowId">The workflow ID to retrieve.</param>
        /// <returns>API response containing the workflow session, or error if not found.</returns>
        Task<ApiResponse<HardwareDefinition>> GetSessionByIdAsync(string workflowId);

        /// <summary>
        /// Creates or updates a workflow session.
        /// If session with same WorkflowId exists, it will be replaced.
        /// </summary>
        /// <param name="session">The workflow session to save.</param>
        /// <returns>API response containing the saved session.</returns>
        Task<ApiResponse<HardwareDefinition>> SaveSessionAsync(HardwareDefinition session);

        /// <summary>
        /// Updates the state of an existing workflow session.
        /// </summary>
        /// <param name="workflowId">The workflow ID to update.</param>
        /// <param name="newState">The new state to set.</param>
        /// <returns>API response containing the updated session, or error if not found.</returns>
        Task<ApiResponse<HardwareDefinition>> UpdateSessionStateAsync(string workflowId, ComponentState newState);

        /// <summary>
        /// Deletes a workflow session by ID.
        /// </summary>
        /// <param name="workflowId">The workflow ID to delete.</param>
        /// <returns>API response indicating success or failure.</returns>
        Task<ApiResponse<bool>> DeleteSessionAsync(string workflowId);

        /// <summary>
        /// 특정 하드웨어 키의 모든 버전 히스토리를 조회합니다.
        /// </summary>
        /// <param name="hardwareKey">하드웨어 고유 식별 키</param>
        /// <param name="hardwareType">하드웨어 레이어 (Equipment, System, Unit, Device)</param>
        /// <returns>버전 히스토리 정보</returns>
        Task<ApiResponse<HardwareVersionHistoryDto>> GetVersionHistoryAsync(
            string hardwareKey,
            HardwareType hardwareType);

        /// <summary>
        /// 특정 하드웨어 키를 가진 모든 하드웨어의 버전 목록을 페이지네이션으로 조회합니다.
        /// </summary>
        /// <param name="hardwareKey">하드웨어 고유 식별 키</param>
        /// <param name="page">페이지 번호 (1부터 시작)</param>
        /// <param name="pageSize">페이지당 항목 수</param>
        /// <returns>페이지네이션된 버전 요약 정보</returns>
        Task<PagedApiResponse<HardwareVersionSummaryDto>> GetHardwareByHardwareKeyAsync(
            string hardwareKey,
            int page = 1,
            int pageSize = 10);

        /// <summary>
        /// 특정 레이어의 모든 고유 하드웨어 키 목록을 조회합니다.
        /// (Dashboard에서 그룹화된 표시에 활용 가능)
        /// </summary>
        /// <param name="hardwareType">하드웨어 레이어</param>
        /// <returns>고유 하드웨어 키 목록</returns>
        Task<ApiResponse<List<string>>> GetDistinctHardwareKeysAsync(HardwareType hardwareType);
    }
}