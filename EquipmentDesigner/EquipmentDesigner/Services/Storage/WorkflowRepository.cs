using System;
using System.IO;
using EquipmentDesigner.Models;

namespace EquipmentDesigner.Services
{
    /// <summary>
    /// Repository for managing incomplete workflow data.
    /// Persists to workflows.json in LocalApplicationData folder.
    /// </summary>
    public class WorkflowRepository : TypedJsonFileRepository<HardwareDefinitionDataStore>, IWorkflowRepository
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
            var appFolder = Path.Combine(appData, "EquipmentDesigner", "local");
            return Path.Combine(appFolder, "workflows.json");
        }

        protected override void UpdateLastSavedAt(HardwareDefinitionDataStore dataStore, DateTime timestamp)
        {
            dataStore.LastSavedAt = timestamp;
        }
    }
}
