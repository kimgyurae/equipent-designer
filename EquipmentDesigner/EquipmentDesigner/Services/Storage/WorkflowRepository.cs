using System;
using System.IO;
using EquipmentDesigner.Models.Storage;

namespace EquipmentDesigner.Services.Storage
{
    /// <summary>
    /// Repository for managing incomplete workflow data.
    /// Persists to workflows.json in LocalApplicationData folder.
    /// </summary>
    public class WorkflowRepository : TypedJsonFileRepository<IncompleteWorkflowDataStore>
    {
        public WorkflowRepository() : base(null)
        {
        }

        public WorkflowRepository(string filePath) : base(filePath)
        {
        }

        protected override string GetDefaultFilePath()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var appFolder = Path.Combine(appData, "EquipmentDesigner");
            return Path.Combine(appFolder, "workflows.json");
        }

        protected override void UpdateLastSavedAt(IncompleteWorkflowDataStore dataStore, DateTime timestamp)
        {
            dataStore.LastSavedAt = timestamp;
        }

        /// <summary>
        /// Gets the default file path for workflows.json.
        /// Exposed for testing purposes.
        /// </summary>
        public static string GetDefaultWorkflowFilePath()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var appFolder = Path.Combine(appData, "EquipmentDesigner");
            return Path.Combine(appFolder, "workflows.json");
        }
    }
}
