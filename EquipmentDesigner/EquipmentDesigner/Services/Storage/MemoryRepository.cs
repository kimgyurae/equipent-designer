using System;
using System.Threading.Tasks;
using EquipmentDesigner.Models.Storage;

namespace EquipmentDesigner.Services.Storage
{
    /// <summary>
    /// 메모리 기반 저장소 (테스트용)
    /// </summary>
    public class MemoryRepository : IDataRepository
    {
        private SharedMemoryDataStore _dataStore;

        public bool IsDirty { get; private set; }

        public Task<SharedMemoryDataStore> LoadAsync()
        {
            if (_dataStore == null)
            {
                _dataStore = new SharedMemoryDataStore();
            }
            return Task.FromResult(_dataStore);
        }

        public Task SaveAsync(SharedMemoryDataStore dataStore)
        {
            _dataStore = dataStore;
            _dataStore.LastSavedAt = DateTime.Now;
            IsDirty = false;
            return Task.CompletedTask;
        }

        public void EnableAutoSave(TimeSpan interval)
        {
            // No-op for memory repository
        }

        public void DisableAutoSave()
        {
            // No-op for memory repository
        }

        public void MarkDirty()
        {
            IsDirty = true;
        }
    }
}
