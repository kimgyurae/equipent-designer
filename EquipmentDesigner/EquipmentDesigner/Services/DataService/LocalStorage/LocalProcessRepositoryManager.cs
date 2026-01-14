using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EquipmentDesigner.Models;

namespace EquipmentDesigner.Services
{
    /// <summary>
    /// Manager interface for LocalProcessRepository operations.
    /// </summary>
    public interface ILocalProcessRepositoryManager
    {
        /// <summary>
        /// Loads all processes from persistent storage.
        /// </summary>
        Task<List<Process>> LoadAsync();

        /// <summary>
        /// Saves all processes to persistent storage.
        /// </summary>
        Task SaveAsync(List<Process> processes);

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

        /// <summary>
        /// Gets a specific UMLWorkflow by process ID and PackML state.
        /// </summary>
        /// <param name="id">The process identifier.</param>
        /// <param name="packMlState">The PackML state to retrieve the workflow for.</param>
        /// <returns>The UMLWorkflow if found; otherwise, null.</returns>
        Task<UMLWorkflow> GetProcessAsync(string id, PackMlState packMlState);

        /// <summary>
        /// Gets a Process by its ID.
        /// </summary>
        /// <param name="id">The process identifier.</param>
        /// <returns>The Process if found; otherwise, null.</returns>
        Task<Process> GetProcessByIdAsync(string id);

        /// <summary>
        /// Updates an existing process by replacing it with a new value.
        /// </summary>
        /// <param name="processId">The identifier of the process to update.</param>
        /// <param name="process">The new process value to replace the existing one.</param>
        /// <returns>True if the process was found and updated; otherwise, false.</returns>
        Task<bool> UpdateProcessAsync(string processId, Process process);
    }

    /// <summary>
    /// Manages local process data by encapsulating LocalProcessRepository.
    /// Provides convenient access methods while maintaining full repository functionality.
    /// </summary>
    public class LocalProcessRepositoryManager : ILocalProcessRepositoryManager
    {
        private readonly ILocalProcessRepository _repository;

        /// <summary>
        /// Initializes a new instance with the specified repository.
        /// </summary>
        /// <param name="repository">The local process repository to wrap.</param>
        public LocalProcessRepositoryManager(ILocalProcessRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        /// <summary>
        /// Initializes a new instance with a default LocalProcessRepository.
        /// </summary>
        public LocalProcessRepositoryManager() : this(new LocalProcessRepository())
        {
        }

        /// <inheritdoc />
        public bool IsDirty => _repository.IsDirty;

        /// <inheritdoc />
        public async Task<List<Process>> LoadAsync()
            => await _repository.LoadAsync();

        /// <inheritdoc />
        public async Task SaveAsync(List<Process> processes)
            => await _repository.SaveAsync(processes);

        /// <inheritdoc />
        public void EnableAutoSave(TimeSpan interval)
            => _repository.EnableAutoSave(interval);

        /// <inheritdoc />
        public void DisableAutoSave()
            => _repository.DisableAutoSave();

        /// <inheritdoc />
        public void MarkDirty()
            => _repository.MarkDirty();

        /// <inheritdoc />
        public async Task<UMLWorkflow> GetProcessAsync(string id, PackMlState packMlState)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("Id cannot be null or empty.", nameof(id));
            }

            var processes = await _repository.LoadAsync();
            var process = processes?.FirstOrDefault(p => p.Id == id);

            if (process?.Processes == null)
            {
                return null;
            }

            return process.Processes.TryGetValue(packMlState, out var workflow)
                ? workflow
                : null;
        }

        /// <inheritdoc />
        public async Task<Process> GetProcessByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            var processes = await _repository.LoadAsync();
            return processes?.FirstOrDefault(p => p.Id == id);
        }

        /// <inheritdoc />
        public async Task<bool> UpdateProcessAsync(string processId, Process process)
        {
            if (string.IsNullOrEmpty(processId))
            {
                throw new ArgumentException("Process ID cannot be null or empty.", nameof(processId));
            }

            if (process == null)
            {
                throw new ArgumentNullException(nameof(process));
            }

            var processes = await _repository.LoadAsync();
            if (processes == null)
            {
                return false;
            }

            var index = processes.FindIndex(p => p.Id == processId);
            if (index < 0)
            {
                return false;
            }

            processes[index] = process;
            await _repository.SaveAsync(processes);
            _repository.MarkDirty();

            return true;
        }
    }
}