using System;
using System.Collections.Generic;
using System.IO;
using EquipmentDesigner.Models;

namespace EquipmentDesigner.Services
{
    /// <summary>
    /// Repository for managing local process data.
    /// Persists to process.json in LocalApplicationData/local folder.
    /// </summary>
    public class LocalProcessRepository : TypedJsonFileRepository<List<Process>>, ILocalProcessRepository
    {
        public LocalProcessRepository() : base(null)
        {
        }

        public LocalProcessRepository(string filePath) : base(filePath)
        {
        }

        protected override string GetDefaultFilePath()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var appFolder = Path.Combine(appData, "EquipmentDesigner", "local");
            return Path.Combine(appFolder, "process.json");
        }

        protected override void UpdateLastSavedAt(List<Process> dataStore, DateTime timestamp)
        {
            // No-op: List<Process> doesn't have LastSavedAt property
            // Individual items can track their own LastModifiedAt if needed
        }
    }
}
