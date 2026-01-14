using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using EquipmentDesigner.Converters;

namespace EquipmentDesigner.Services
{
    /// <summary>
    /// Abstract base class for typed JSON file repositories.
    /// Provides common file I/O operations for different data store types.
    /// </summary>
    /// <typeparam name="T">The type of data store to persist</typeparam>
    public abstract class TypedJsonFileRepository<T> : ITypedDataRepository<T> where T : class, new()
    {
        private readonly string _filePath;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly object _lock = new object();
        private Timer _autoSaveTimer;
        private T _cachedData;

        public bool IsDirty { get; private set; }

        protected TypedJsonFileRepository(string filePath = null)
        {
            _filePath = filePath ?? GetDefaultFilePath();
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
                Converters = { new DrawingElementJsonConverter() }
            };
        }

        /// <summary>
        /// Gets the default file path for this repository type.
        /// Must be implemented by derived classes.
        /// </summary>
        protected abstract string GetDefaultFilePath();

        /// <summary>
        /// Called before saving to update timestamps or perform other pre-save operations.
        /// </summary>
        protected abstract void UpdateLastSavedAt(T dataStore, DateTime timestamp);

        public async Task<T> LoadAsync()
        {
            try
            {
                if (!File.Exists(_filePath))
                {
                    _cachedData = new T();
                    return _cachedData;
                }

                var json = await File.ReadAllTextAsync(_filePath);
                _cachedData = JsonSerializer.Deserialize<T>(json, _jsonOptions);

                if (_cachedData == null)
                {
                    _cachedData = new T();
                }

                return _cachedData;
            }
            catch (JsonException)
            {
                _cachedData = new T();
                return _cachedData;
            }
            catch (Exception e)
            {
                _cachedData = new T();
                return _cachedData;
            }
        }

        public async Task SaveAsync(T dataStore)
        {
            lock (_lock)
            {
                UpdateLastSavedAt(dataStore, DateTime.Now);
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

        /// <summary>
        /// Gets the file path for testing purposes.
        /// </summary>
        internal string FilePath => _filePath;
    }
}