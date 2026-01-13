using System;
using System.Collections.Generic;
using System.IO;
using EquipmentDesigner.Models;

namespace EquipmentDesigner.Services
{
    /// <summary>
    /// Repository for managing remote/uploaded process data.
    /// Persists to process.json in LocalApplicationData/remote folder.
    /// </summary>
    public class RemoteProcessRepository : TypedJsonFileRepository<List<Process>>, IRemoteProcessRepository
    {
        public RemoteProcessRepository() : base(null)
        {
        }

        public RemoteProcessRepository(string filePath) : base(filePath)
        {
        }

        protected override string GetDefaultFilePath()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var appFolder = Path.Combine(appData, "EquipmentDesigner", "remote");
            return Path.Combine(appFolder, "process.json");
        }

        protected override void UpdateLastSavedAt(List<Process> dataStore, DateTime timestamp)
        {
            // No-op: List<Process> doesn't have LastSavedAt property
            // Individual items can track their own LastModifiedAt if needed
        }
    }
}
