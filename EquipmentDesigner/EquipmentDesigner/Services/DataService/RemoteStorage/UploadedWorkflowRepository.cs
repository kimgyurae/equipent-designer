using System;
using System.Collections.Generic;
using System.IO;
using EquipmentDesigner.Models;

namespace EquipmentDesigner.Services
{
    /// <summary>
    /// Repository for managing uploaded workflow data.
    /// Persists to uploaded-hardwares.json in LocalApplicationData folder.
    /// </summary>
    public class UploadedWorkflowRepository : TypedJsonFileRepository<List<HardwareDefinition>>, IUploadedWorkflowRepository
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
            var appFolder = Path.Combine(appData, "EquipmentDesigner", "remote");
            return Path.Combine(appFolder, "uploaded-hardwares.json");
        }

        protected override void UpdateLastSavedAt(List<HardwareDefinition> dataStore, DateTime timestamp)
        {
            // No-op: List<HardwareDefinition> doesn't have LastSavedAt property
            // Individual items can track their own LastModifiedAt if needed
        }
    }
}