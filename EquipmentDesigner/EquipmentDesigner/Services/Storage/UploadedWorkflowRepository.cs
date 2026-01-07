using System;
using System.IO;
using EquipmentDesigner.Models.Storage;

namespace EquipmentDesigner.Services.Storage
{
    /// <summary>
    /// Repository for managing uploaded workflow data.
    /// Persists to uploaded-hardwares.json in LocalApplicationData folder.
    /// Uses the same HardwareDefinitionDataStore structure as WorkflowRepository.
    /// </summary>
    public class UploadedWorkflowRepository : TypedJsonFileRepository<HardwareDefinitionDataStore>, IUploadedWorkflowRepository
    {
        public UploadedWorkflowRepository() : base(null)
        {
        }

        public UploadedWorkflowRepository(string filePath) : base(filePath)
        {
        }

        protected override string GetDefaultFilePath()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var appFolder = Path.Combine(appData, "EquipmentDesigner");
            return Path.Combine(appFolder, "uploaded-hardwares.json");
        }

        protected override void UpdateLastSavedAt(HardwareDefinitionDataStore dataStore, DateTime timestamp)
        {
            dataStore.LastSavedAt = timestamp;
        }

        /// <summary>
        /// Gets the default file path for uploaded-hardwares.json.
        /// Exposed for testing purposes.
        /// </summary>
        public static string GetDefaultUploadedWorkflowFilePath()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var appFolder = Path.Combine(appData, "EquipmentDesigner");
            return Path.Combine(appFolder, "uploaded-hardwares.json");
        }
    }
}
