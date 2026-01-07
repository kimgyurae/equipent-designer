using System;
using System.Threading.Tasks;
using EquipmentDesigner.Models.Storage;

namespace EquipmentDesigner.Services.Storage
{
    /// <summary>
    /// Generic typed data repository interface for type-safe data persistence.
    /// Supports different data store types with separate file paths.
    /// </summary>
    /// <typeparam name="T">The type of data store to persist</typeparam>
    public interface ITypedDataRepository<T> where T : class, new()
    {
        /// <summary>
        /// Loads the data store from persistent storage.
        /// Returns a new empty instance if file does not exist or is corrupted.
        /// </summary>
        Task<T> LoadAsync();

        /// <summary>
        /// Saves the data store to persistent storage.
        /// Creates directory if not exists.
        /// </summary>
        Task SaveAsync(T dataStore);

        /// <summary>
        /// Enables automatic saving at specified intervals when dirty.
        /// </summary>
        void EnableAutoSave(TimeSpan interval);

        /// <summary>
        /// Disables automatic saving.
        /// </summary>
        void DisableAutoSave();

        /// <summary>
        /// Marks the repository as having unsaved changes.
        /// </summary>
        void MarkDirty();

        /// <summary>
        /// Gets whether there are unsaved changes.
        /// </summary>
        bool IsDirty { get; }
    }

    /// <summary>
    /// Repository interface for incomplete workflow data.
    /// Used to differentiate from uploaded workflow repository.
    /// </summary>
    public interface IWorkflowRepository : ITypedDataRepository<HardwareDefinitionDataStore>
    {
    }

    /// <summary>
    /// Repository interface for uploaded workflow data.
    /// Used to differentiate from incomplete workflow repository.
    /// </summary>
    public interface IUploadedWorkflowRepository : ITypedDataRepository<HardwareDefinitionDataStore>
    {
    }
}