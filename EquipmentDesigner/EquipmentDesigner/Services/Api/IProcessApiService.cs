using System.Threading.Tasks;
using EquipmentDesigner.Models;

namespace EquipmentDesigner.Services
{
    /// <summary>
    /// Process API service interface for managing uploaded process definitions.
    /// Provides process retrieval operations with REST API semantics.
    /// </summary>
    public interface IProcessApiService
    {
        /// <summary>
        /// Gets a specific process by ID.
        /// </summary>
        /// <param name="processId">The process ID to retrieve.</param>
        /// <returns>API response containing the process, or error if not found.</returns>
        Task<ApiResponse<Process>> GetProcessByIdAsync(string processId);

        /// <summary>
        /// Creates or updates a process.
        /// If process with same ID exists, it will be replaced.
        /// </summary>
        /// <param name="process">The process to save.</param>
        /// <returns>API response containing the saved process.</returns>
        Task<ApiResponse<Process>> SaveProcessAsync(Process process);
    }
}