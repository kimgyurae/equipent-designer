using System;
using System.Collections.Generic;
using System.IO;
using EquipmentDesigner.Models;

namespace EquipmentDesigner.Services
{
    /// <summary>
    /// Repository for managing incomplete workflow data.
    /// Persists to workflows.json in LocalApplicationData folder.
    /// </summary>
    public class LocalHardwareRepository : TypedJsonFileRepository<List<HardwareDefinition>>, IWorkflowRepository
    {
        public LocalHardwareRepository() : base(null)
        {
        }

        public LocalHardwareRepository(string filePath) : base(filePath)
        {
        }

        protected override string GetDefaultFilePath()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var appFolder = Path.Combine(appData, "EquipmentDesigner", "local");
            return Path.Combine(appFolder, "workflows.json");
        }

        protected override void UpdateLastSavedAt(List<HardwareDefinition> dataStore, DateTime timestamp)
        {
            // No-op: List<HardwareDefinition> doesn't have LastSavedAt property
            // Individual items can track their own LastModifiedAt if needed
        }
    }
}