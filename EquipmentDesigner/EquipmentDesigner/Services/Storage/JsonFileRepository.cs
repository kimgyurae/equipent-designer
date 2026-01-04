using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using EquipmentDesigner.Models.Storage;

namespace EquipmentDesigner.Services.Storage
{
    /// <summary>
    /// JSON 파일 기반 저장소 구현
    /// </summary>
    public class JsonFileRepository : IDataRepository
    {
        private readonly string _filePath;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly object _lock = new object();
        private Timer _autoSaveTimer;
        private SharedMemoryDataStore _cachedData;

        public bool IsDirty { get; private set; }

        public JsonFileRepository() : this(null)
        {
        }

        public JsonFileRepository(string filePath)
        {
            _filePath = filePath ?? GetDefaultFilePath();
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
        }

        private static string GetDefaultFilePath()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var appFolder = Path.Combine(appData, "EquipmentDesigner");
            return Path.Combine(appFolder, "equipment_data.json");
        }

        public async Task<SharedMemoryDataStore> LoadAsync()
        {
            try
            {
                if (!File.Exists(_filePath))
                {
                    _cachedData = new SharedMemoryDataStore();
                    return _cachedData;
                }

                var json = await File.ReadAllTextAsync(_filePath);
                _cachedData = JsonSerializer.Deserialize<SharedMemoryDataStore>(json, _jsonOptions);

                if (_cachedData == null)
                {
                    _cachedData = new SharedMemoryDataStore();
                }

                return _cachedData;
            }
            catch (JsonException)
            {
                // Corrupted JSON - return empty datastore
                _cachedData = new SharedMemoryDataStore();
                return _cachedData;
            }
            catch (Exception)
            {
                _cachedData = new SharedMemoryDataStore();
                return _cachedData;
            }
        }

        public async Task SaveAsync(SharedMemoryDataStore dataStore)
        {
            lock (_lock)
            {
                dataStore.LastSavedAt = DateTime.Now;
                _cachedData = dataStore;
            }

            var directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonSerializer.Serialize(dataStore, _jsonOptions);
            await File.WriteAllTextAsync(_filePath, json);

            lock (_lock)
            {
                IsDirty = false;
            }
        }

        public void EnableAutoSave(TimeSpan interval)
        {
            DisableAutoSave();
            _autoSaveTimer = new Timer(async _ =>
            {
                if (IsDirty && _cachedData != null)
                {
                    await SaveAsync(_cachedData);
                }
            }, null, interval, interval);
        }

        public void DisableAutoSave()
        {
            _autoSaveTimer?.Dispose();
            _autoSaveTimer = null;
        }

        public void MarkDirty()
        {
            lock (_lock)
            {
                IsDirty = true;
            }
        }
    }
}
