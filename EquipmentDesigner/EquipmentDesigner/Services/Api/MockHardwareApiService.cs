using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EquipmentDesigner.Models;
using EquipmentDesigner.Models.Dtos;
using EquipmentDesigner.Models.Storage;
using EquipmentDesigner.Services.Storage;
using EquipmentDesigner.Views.ReusableComponents.Spinner;
namespace EquipmentDesigner.Services.Api
{
    /// <summary>
    /// Mock implementation of IHardwareApiService for development.
    /// Internally uses UploadedWorkflowRepository with simulated network delay.
    /// </summary>
    public class MockHardwareApiService : IHardwareApiService
    {
        private readonly IUploadedWorkflowRepository _repository;
        private static readonly TimeSpan SimulatedDelay = TimeSpan.FromMilliseconds(1500);

    
        public MockHardwareApiService(IUploadedWorkflowRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<ApiResponse<List<WorkflowSessionDto>>> GetAllSessionsAsync()
        {
            LoadingSpinnerService.Instance.Show();
            await Task.Delay(SimulatedDelay);

            try
            {
                var dataStore = await _repository.LoadAsync();
                LoadingSpinnerService.Instance.Hide();
                return ApiResponse<List<WorkflowSessionDto>>.Ok(
                    dataStore.WorkflowSessions?.ToList() ?? new List<WorkflowSessionDto>());
            }
            catch (Exception ex)
            {
                LoadingSpinnerService.Instance.Hide();
                return ApiResponse<List<WorkflowSessionDto>>.Fail(ex.Message, "LOAD_ERROR");
            }
        }

        public async Task<ApiResponse<List<WorkflowSessionDto>>> GetSessionsByStateAsync(params ComponentState[] states)
        {
            LoadingSpinnerService.Instance.Show();
            await Task.Delay(SimulatedDelay);

            try
            {
                var dataStore = await _repository.LoadAsync();
                var filtered = dataStore.WorkflowSessions?
                    .Where(s => states.Contains(s.State))
                    .ToList() ?? new List<WorkflowSessionDto>();
                LoadingSpinnerService.Instance.Hide();
                return ApiResponse<List<WorkflowSessionDto>>.Ok(filtered);
            }
            catch (Exception ex)
            {
                LoadingSpinnerService.Instance.Hide();
                return ApiResponse<List<WorkflowSessionDto>>.Fail(ex.Message, "LOAD_ERROR");
            }
        }

        public async Task<ApiResponse<WorkflowSessionDto>> GetSessionByIdAsync(string workflowId)
        {
            LoadingSpinnerService.Instance.Show();
            await Task.Delay(SimulatedDelay);

            if (string.IsNullOrEmpty(workflowId))
            {
                LoadingSpinnerService.Instance.Hide();
                return ApiResponse<WorkflowSessionDto>.Fail("Workflow ID is required", "INVALID_ID");
            }

            try
            {
                var dataStore = await _repository.LoadAsync();
                var session = dataStore.WorkflowSessions?.FirstOrDefault(s => s.WorkflowId == workflowId);

                if (session == null)
                {
                    LoadingSpinnerService.Instance.Hide();
                    return ApiResponse<WorkflowSessionDto>.Fail(
                        $"Session with ID '{workflowId}' not found", "NOT_FOUND");
                }
                LoadingSpinnerService.Instance.Hide();
                return ApiResponse<WorkflowSessionDto>.Ok(session);
            }
            catch (Exception ex)
            {
                LoadingSpinnerService.Instance.Hide();
                return ApiResponse<WorkflowSessionDto>.Fail(ex.Message, "LOAD_ERROR");
            }
        }

        public async Task<ApiResponse<WorkflowSessionDto>> SaveSessionAsync(WorkflowSessionDto session)
        {
            LoadingSpinnerService.Instance.Show();
            await Task.Delay(SimulatedDelay);

            if (session == null)
            {
                LoadingSpinnerService.Instance.Hide();
                return ApiResponse<WorkflowSessionDto>.Fail("Session is required", "INVALID_SESSION");
            }

            if (string.IsNullOrEmpty(session.WorkflowId))
            {
                LoadingSpinnerService.Instance.Hide();
                return ApiResponse<WorkflowSessionDto>.Fail("Workflow ID is required", "INVALID_ID");
            }

            try
            {
                var dataStore = await _repository.LoadAsync();

                var existingIndex = dataStore.WorkflowSessions.FindIndex(s => s.WorkflowId == session.WorkflowId);
                if (existingIndex >= 0)
                {
                    dataStore.WorkflowSessions[existingIndex] = session;
                }
                else
                {
                    dataStore.WorkflowSessions.Add(session);
                }

                session.LastModifiedAt = DateTime.Now;
                await _repository.SaveAsync(dataStore);
                LoadingSpinnerService.Instance.Hide();
                return ApiResponse<WorkflowSessionDto>.Ok(session);
            }
            catch (Exception ex)
            {
                LoadingSpinnerService.Instance.Hide();
                return ApiResponse<WorkflowSessionDto>.Fail(ex.Message, "SAVE_ERROR");
            }
        }

        public async Task<ApiResponse<WorkflowSessionDto>> UpdateSessionStateAsync(string workflowId, ComponentState newState)
        {
            LoadingSpinnerService.Instance.Show();
            await Task.Delay(SimulatedDelay);

            if (string.IsNullOrEmpty(workflowId))
            {
                LoadingSpinnerService.Instance.Hide();
                return ApiResponse<WorkflowSessionDto>.Fail("Workflow ID is required", "INVALID_ID");
            }

            try
            {
                var dataStore = await _repository.LoadAsync();
                var session = dataStore.WorkflowSessions?.FirstOrDefault(s => s.WorkflowId == workflowId);

                if (session == null)
                {
                    LoadingSpinnerService.Instance.Hide();
                    return ApiResponse<WorkflowSessionDto>.Fail(
                        $"Session with ID '{workflowId}' not found", "NOT_FOUND");
                }

                session.State = newState;
                session.LastModifiedAt = DateTime.Now;
                await _repository.SaveAsync(dataStore);
                LoadingSpinnerService.Instance.Hide();
                return ApiResponse<WorkflowSessionDto>.Ok(session);
            }
            catch (Exception ex)
            {
                LoadingSpinnerService.Instance.Hide();
                return ApiResponse<WorkflowSessionDto>.Fail(ex.Message, "UPDATE_ERROR");
            }
        }

        public async Task<ApiResponse<bool>> DeleteSessionAsync(string workflowId)
        {
            LoadingSpinnerService.Instance.Show();
            await Task.Delay(SimulatedDelay);

            if (string.IsNullOrEmpty(workflowId))
            {
                return ApiResponse<bool>.Fail("Workflow ID is required", "INVALID_ID");
            }

            try
            {
                var dataStore = await _repository.LoadAsync();
                var session = dataStore.WorkflowSessions?.FirstOrDefault(s => s.WorkflowId == workflowId);

                if (session == null)
                {
                    LoadingSpinnerService.Instance.Hide();
                    return ApiResponse<bool>.Fail(
                        $"Session with ID '{workflowId}' not found", "NOT_FOUND");
                }

                dataStore.WorkflowSessions.Remove(session);
                await _repository.SaveAsync(dataStore);
                LoadingSpinnerService.Instance.Hide();
                return ApiResponse<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                LoadingSpinnerService.Instance.Hide();
                return ApiResponse<bool>.Fail(ex.Message, "DELETE_ERROR");
            }
        }
    }
}
