using System.Collections.Generic;
using System.Threading.Tasks;
using EquipmentDesigner.Models;
using EquipmentDesigner.Models.Dtos;
using EquipmentDesigner.Models.Storage;

namespace EquipmentDesigner.Services.Api
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
        Task<ApiResponse<List<WorkflowSessionDto>>> GetAllSessionsAsync();

        /// <summary>
        /// Gets workflow sessions filtered by state.
        /// </summary>
        /// <param name="states">States to filter by.</param>
        /// <returns>API response containing filtered workflow sessions.</returns>
        Task<ApiResponse<List<WorkflowSessionDto>>> GetSessionsByStateAsync(params ComponentState[] states);

        /// <summary>
        /// Gets a specific workflow session by ID.
        /// </summary>
        /// <param name="workflowId">The workflow ID to retrieve.</param>
        /// <returns>API response containing the workflow session, or error if not found.</returns>
        Task<ApiResponse<WorkflowSessionDto>> GetSessionByIdAsync(string workflowId);

        /// <summary>
        /// Creates or updates a workflow session.
        /// If session with same WorkflowId exists, it will be replaced.
        /// </summary>
        /// <param name="session">The workflow session to save.</param>
        /// <returns>API response containing the saved session.</returns>
        Task<ApiResponse<WorkflowSessionDto>> SaveSessionAsync(WorkflowSessionDto session);

        /// <summary>
        /// Updates the state of an existing workflow session.
        /// </summary>
        /// <param name="workflowId">The workflow ID to update.</param>
        /// <param name="newState">The new state to set.</param>
        /// <returns>API response containing the updated session, or error if not found.</returns>
        Task<ApiResponse<WorkflowSessionDto>> UpdateSessionStateAsync(string workflowId, ComponentState newState);

        /// <summary>
        /// Deletes a workflow session by ID.
        /// </summary>
        /// <param name="workflowId">The workflow ID to delete.</param>
        /// <returns>API response indicating success or failure.</returns>
        Task<ApiResponse<bool>> DeleteSessionAsync(string workflowId);
    }
}
