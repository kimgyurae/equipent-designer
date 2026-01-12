using System;
using System.Linq;
using System.Threading.Tasks;
using EquipmentDesigner.Models;
using EquipmentDesigner.Controls;

namespace EquipmentDesigner.Services
{
    /// <summary>
    /// Mock implementation of IProcessApiService for development.
    /// Internally uses RemoteProcessRepository with simulated network delay.
    /// </summary>
    public class MockProcessApiService : IProcessApiService
    {
        private readonly IRemoteProcessRepository _repository;
        private static readonly TimeSpan SimulatedDelay = TimeSpan.FromMilliseconds(400);

        public MockProcessApiService(IRemoteProcessRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<ApiResponse<Process>> GetProcessByIdAsync(string processId)
        {
            LoadingSpinnerService.Instance.Show();
            await Task.Delay(SimulatedDelay);

            if (string.IsNullOrEmpty(processId))
            {
                LoadingSpinnerService.Instance.Hide();
                return ApiResponse<Process>.Fail("Process ID is required", "INVALID_ID");
            }

            try
            {
                var processes = await _repository.LoadAsync();
                var process = processes?.FirstOrDefault(p => p.Id == processId);

                if (process == null)
                {
                    LoadingSpinnerService.Instance.Hide();
                    return ApiResponse<Process>.Fail(
                        $"Process with ID '{processId}' not found", "NOT_FOUND");
                }

                LoadingSpinnerService.Instance.Hide();
                return ApiResponse<Process>.Ok(process);
            }
            catch (Exception ex)
            {
                LoadingSpinnerService.Instance.Hide();
                return ApiResponse<Process>.Fail(ex.Message, "LOAD_ERROR");
            }
        }

        public async Task<ApiResponse<Process>> SaveProcessAsync(Process process)
        {
            LoadingSpinnerService.Instance.Show();
            await Task.Delay(SimulatedDelay);

            if (process == null)
            {
                LoadingSpinnerService.Instance.Hide();
                return ApiResponse<Process>.Fail("Process is required", "INVALID_PROCESS");
            }

            if (string.IsNullOrEmpty(process.Id))
            {
                LoadingSpinnerService.Instance.Hide();
                return ApiResponse<Process>.Fail("Process ID is required", "INVALID_ID");
            }

            try
            {
                var processes = await _repository.LoadAsync();

                var existingIndex = processes.FindIndex(p => p.Id == process.Id);
                if (existingIndex >= 0)
                {
                    processes[existingIndex] = process;
                }
                else
                {
                    processes.Add(process);
                }

                await _repository.SaveAsync(processes);
                LoadingSpinnerService.Instance.Hide();
                return ApiResponse<Process>.Ok(process);
            }
            catch (Exception ex)
            {
                LoadingSpinnerService.Instance.Hide();
                return ApiResponse<Process>.Fail(ex.Message, "SAVE_ERROR");
            }
        }
    }
}