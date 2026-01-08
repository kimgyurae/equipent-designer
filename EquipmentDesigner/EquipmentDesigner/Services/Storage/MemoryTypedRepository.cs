using System;
using System.Threading.Tasks;
using EquipmentDesigner.Models;

namespace EquipmentDesigner.Services
{
    /// <summary>
    /// Memory-based typed repository for testing purposes.
    /// Generic implementation that works with any data store type.
    /// </summary>
    /// <typeparam name="T">The data store type.</typeparam>
    public class MemoryTypedRepository<T> : ITypedDataRepository<T> where T : class, new()
    {
        private T _dataStore;

        public bool IsDirty { get; private set; }

        public Task<T> LoadAsync()
        {
            if (_dataStore == null)
            {
                _dataStore = new T();
            }
            return Task.FromResult(_dataStore);
        }

        public Task SaveAsync(T dataStore)
        {
            _dataStore = dataStore;
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

    /// <summary>
    /// Memory-based workflow repository for testing purposes.
    /// Implements IWorkflowRepository for incomplete workflow data.
    /// </summary>
    public class MemoryWorkflowRepository : MemoryTypedRepository<HardwareDefinitionDataStore>, IWorkflowRepository
    {
    }

    /// <summary>
    /// Memory-based uploaded workflow repository for testing purposes.
    /// Implements IUploadedWorkflowRepository for uploaded workflow data.
    /// </summary>
    public class MemoryUploadedWorkflowRepository : MemoryTypedRepository<HardwareDefinitionDataStore>, IUploadedWorkflowRepository
    {
    }
}