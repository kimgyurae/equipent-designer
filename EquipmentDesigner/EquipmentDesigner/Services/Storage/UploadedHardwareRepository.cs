using System;
using System.IO;
using EquipmentDesigner.Models.Storage;

namespace EquipmentDesigner.Services.Storage
{
    /// <summary>
    /// Repository for managing uploaded hardware data.
    /// Persists to uploaded-hardwares.json in LocalApplicationData folder.
    /// </summary>
    public class UploadedHardwareRepository : TypedJsonFileRepository<UploadedHardwareDataStore>
    {
        public UploadedHardwareRepository() : base(null)
        {
        }

        public UploadedHardwareRepository(string filePath) : base(filePath)
        {
        }

        protected override string GetDefaultFilePath()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var appFolder = Path.Combine(appData, "EquipmentDesigner");
            return Path.Combine(appFolder, "uploaded-hardwares.json");
        }

        protected override void UpdateLastSavedAt(UploadedHardwareDataStore dataStore, DateTime timestamp)
        {
            dataStore.LastSavedAt = timestamp;
        }

        /// <summary>
        /// Gets the default file path for uploaded-hardwares.json.
        /// Exposed for testing purposes.
        /// </summary>
        public static string GetDefaultUploadedHardwareFilePath()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var appFolder = Path.Combine(appData, "EquipmentDesigner");
            return Path.Combine(appFolder, "uploaded-hardwares.json");
        }
    }
}
