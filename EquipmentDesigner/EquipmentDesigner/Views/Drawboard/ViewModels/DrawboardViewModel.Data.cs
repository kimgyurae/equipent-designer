using System.Threading.Tasks;
using EquipmentDesigner.Models;
using EquipmentDesigner.Services;

namespace EquipmentDesigner.ViewModels
{
    /// <summary>
    /// Partial class containing data access operations for Process management.
    /// Handles loading/saving Process data via LocalProcessRepositoryManager.
    /// </summary>
    public partial class DrawboardViewModel
    {
        private readonly ILocalProcessRepositoryManager _processManager;

        /// <summary>
        /// Loads a Process by its ID from the local repository.
        /// </summary>
        /// <param name="processId">The process identifier to load.</param>
        /// <returns>The Process if found; otherwise, null.</returns>
        private async Task<Process> LoadProcessByIdAsync(string processId)
        {
            return await _processManager.GetProcessByIdAsync(processId);
        }
    }
}
