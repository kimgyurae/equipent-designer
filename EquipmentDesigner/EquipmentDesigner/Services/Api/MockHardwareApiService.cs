using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EquipmentDesigner.Models;
using EquipmentDesigner.Controls;

namespace EquipmentDesigner.Services
{
    /// <summary>
    /// Mock implementation of IHardwareApiService for development.
    /// Internally uses UploadedWorkflowRepository with simulated network delay.
    /// </summary>
    public class MockHardwareApiService : IHardwareApiService
    {
        private readonly IUploadedWorkflowRepository _repository;
        private static readonly TimeSpan SimulatedDelay = TimeSpan.FromMilliseconds(400);

    
        public MockHardwareApiService(IUploadedWorkflowRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<ApiResponse<List<HardwareDefinition>>> GetAllSessionsAsync()
        {
            LoadingSpinnerService.Instance.Show();
            await Task.Delay(SimulatedDelay);

            try
            {
                var sessions = await _repository.LoadAsync();
                LoadingSpinnerService.Instance.Hide();
                return ApiResponse<List<HardwareDefinition>>.Ok(sessions?.ToList() ?? new List<HardwareDefinition>());
            }
            catch (Exception ex)
            {
                LoadingSpinnerService.Instance.Hide();
                return ApiResponse<List<HardwareDefinition>>.Fail(ex.Message, "LOAD_ERROR");
            }
        }

        public async Task<ApiResponse<List<HardwareDefinition>>> GetSessionsByStateAsync(params ComponentState[] states)
        {
            LoadingSpinnerService.Instance.Show();
            await Task.Delay(SimulatedDelay);

            try
            {
                var sessions = await _repository.LoadAsync();
                var filtered = sessions?
                    .Where(s => states.Contains(s.State))
                    .ToList() ?? new List<HardwareDefinition>();
                LoadingSpinnerService.Instance.Hide();
                return ApiResponse<List<HardwareDefinition>>.Ok(filtered);
            }
            catch (Exception ex)
            {
                LoadingSpinnerService.Instance.Hide();
                return ApiResponse<List<HardwareDefinition>>.Fail(ex.Message, "LOAD_ERROR");
            }
        }

        public async Task<ApiResponse<HardwareDefinition>> GetSessionByIdAsync(string workflowId)
        {
            LoadingSpinnerService.Instance.Show();
            await Task.Delay(SimulatedDelay);

            if (string.IsNullOrEmpty(workflowId))
            {
                LoadingSpinnerService.Instance.Hide();
                return ApiResponse<HardwareDefinition>.Fail("Workflow ID is required", "INVALID_ID");
            }

            try
            {
                var sessions = await _repository.LoadAsync();
                var session = sessions?.FirstOrDefault(s => s.Id == workflowId);

                if (session == null)
                {
                    LoadingSpinnerService.Instance.Hide();
                    return ApiResponse<HardwareDefinition>.Fail(
                        $"Session with ID '{workflowId}' not found", "NOT_FOUND");
                }
                LoadingSpinnerService.Instance.Hide();
                return ApiResponse<HardwareDefinition>.Ok(session);
            }
            catch (Exception ex)
            {
                LoadingSpinnerService.Instance.Hide();
                return ApiResponse<HardwareDefinition>.Fail(ex.Message, "LOAD_ERROR");
            }
        }

        public async Task<ApiResponse<HardwareDefinition>> SaveSessionAsync(HardwareDefinition session)
        {
            LoadingSpinnerService.Instance.Show();
            await Task.Delay(SimulatedDelay);

            if (session == null)
            {
                LoadingSpinnerService.Instance.Hide();
                return ApiResponse<HardwareDefinition>.Fail("Session is required", "INVALID_SESSION");
            }

            if (string.IsNullOrEmpty(session.Id))
            {
                LoadingSpinnerService.Instance.Hide();
                return ApiResponse<HardwareDefinition>.Fail("Workflow ID is required", "INVALID_ID");
            }

            try
            {
                var sessions = await _repository.LoadAsync();

                var existingIndex = sessions.FindIndex(s => s.Id == session.Id);
                if (existingIndex >= 0)
                {
                    sessions[existingIndex] = session;
                }
                else
                {
                    sessions.Add(session);
                }

                session.LastModifiedAt = DateTime.Now;
                await _repository.SaveAsync(sessions);
                LoadingSpinnerService.Instance.Hide();
                return ApiResponse<HardwareDefinition>.Ok(session);
            }
            catch (Exception ex)
            {
                LoadingSpinnerService.Instance.Hide();
                return ApiResponse<HardwareDefinition>.Fail(ex.Message, "SAVE_ERROR");
            }
        }

        public async Task<ApiResponse<HardwareDefinition>> UpdateSessionStateAsync(string workflowId, ComponentState newState)
        {
            LoadingSpinnerService.Instance.Show();
            await Task.Delay(SimulatedDelay);

            if (string.IsNullOrEmpty(workflowId))
            {
                LoadingSpinnerService.Instance.Hide();
                return ApiResponse<HardwareDefinition>.Fail("Workflow ID is required", "INVALID_ID");
            }

            try
            {
                var sessions = await _repository.LoadAsync();
                var session = sessions?.FirstOrDefault(s => s.Id == workflowId);

                if (session == null)
                {
                    LoadingSpinnerService.Instance.Hide();
                    return ApiResponse<HardwareDefinition>.Fail(
                        $"Session with ID '{workflowId}' not found", "NOT_FOUND");
                }

                session.State = newState;
                session.LastModifiedAt = DateTime.Now;
                await _repository.SaveAsync(sessions);
                LoadingSpinnerService.Instance.Hide();
                return ApiResponse<HardwareDefinition>.Ok(session);
            }
            catch (Exception ex)
            {
                LoadingSpinnerService.Instance.Hide();
                return ApiResponse<HardwareDefinition>.Fail(ex.Message, "UPDATE_ERROR");
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
                var sessions = await _repository.LoadAsync();
                var session = sessions?.FirstOrDefault(s => s.Id == workflowId);

                if (session == null)
                {
                    LoadingSpinnerService.Instance.Hide();
                    return ApiResponse<bool>.Fail(
                        $"Session with ID '{workflowId}' not found", "NOT_FOUND");
                }

                sessions.Remove(session);
                await _repository.SaveAsync(sessions);
                LoadingSpinnerService.Instance.Hide();
                return ApiResponse<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                LoadingSpinnerService.Instance.Hide();
                return ApiResponse<bool>.Fail(ex.Message, "DELETE_ERROR");
            }
        }

        public async Task<ApiResponse<HardwareVersionHistoryDto>> GetVersionHistoryAsync(
            string hardwareKey,
            HardwareType hardwareType)
        {
            LoadingSpinnerService.Instance.Show();
            await Task.Delay(SimulatedDelay);

            try
            {
                var sessions = await _repository.LoadAsync();

                // 같은 HardwareKey + HardwareType를 가진 모든 세션 필터링
                var matchingSessions = sessions?
                    .Where(s => s.HardwareType == hardwareType)
                    .Where(s => GetEffectiveHardwareKey(s) == hardwareKey)
                    .OrderByDescending(s => ParseVersion(s.Version))
                    .ToList() ?? new List<HardwareDefinition>();

                if (!matchingSessions.Any())
                {
                    LoadingSpinnerService.Instance.Hide();
                    return ApiResponse<HardwareVersionHistoryDto>.Fail(
                        $"No versions found for '{hardwareKey}'", "NOT_FOUND");
                }

                var history = new HardwareVersionHistoryDto
                {
                    HardwareKey = hardwareKey,
                    HardwareType = hardwareType,
                    DisplayName = matchingSessions.First().DisplayName ?? matchingSessions.First().Name ?? "Unknown",
                    TotalVersionCount = matchingSessions.Count,
                    Versions = matchingSessions.Select((s, index) => new HardwareVersionSummaryDto
                    {
                        WorkflowId = s.Id,
                        Version = s.Version ?? "v0.0.0",
                        State = s.State,
                        LastModifiedAt = s.LastModifiedAt,
                        IsLatest = index == 0,
                        Description = s.Description
                    }).ToList()
                };

                LoadingSpinnerService.Instance.Hide();
                return ApiResponse<HardwareVersionHistoryDto>.Ok(history);
            }
            catch (Exception ex)
            {
                LoadingSpinnerService.Instance.Hide();
                return ApiResponse<HardwareVersionHistoryDto>.Fail(ex.Message, "LOAD_ERROR");
            }
        }

        public async Task<ApiResponse<List<string>>> GetDistinctHardwareKeysAsync(HardwareType hardwareType)
        {
            LoadingSpinnerService.Instance.Show();
            await Task.Delay(SimulatedDelay);

            try
            {
                var sessions = await _repository.LoadAsync();

                var distinctKeys = sessions?
                    .Where(s => s.HardwareType == hardwareType)
                    .Select(s => GetEffectiveHardwareKey(s))
                    .Where(key => !string.IsNullOrEmpty(key))
                    .Distinct()
                    .ToList() ?? new List<string>();

                LoadingSpinnerService.Instance.Hide();
                return ApiResponse<List<string>>.Ok(distinctKeys);
            }
            catch (Exception ex)
            {
                LoadingSpinnerService.Instance.Hide();
                return ApiResponse<List<string>>.Fail(ex.Message, "LOAD_ERROR");
            }
        }

        #region Version History Helper Methods

        /// <summary>
        /// 세션에서 유효한 HardwareKey를 가져옵니다. null인 경우 Name을 반환합니다.
        /// </summary>
        private string GetEffectiveHardwareKey(HardwareDefinition session)
        {
            return session.HardwareKey ?? session.Name;
        }

        /// <summary>
        /// 버전 문자열을 파싱하여 비교 가능한 Version 객체로 변환합니다.
        /// </summary>
        private Version ParseVersion(string versionString)
        {
            if (string.IsNullOrEmpty(versionString))
                return new Version(0, 0, 0);

            // 'v' 또는 'V' 접두사 제거
            var normalized = versionString.TrimStart('v', 'V');

            // 버전 형식 맞추기 (X.Y → X.Y.0)
            var parts = normalized.Split('.');
            while (parts.Length < 3)
            {
                normalized += ".0";
                parts = normalized.Split('.');
            }

            if (Version.TryParse(normalized, out var version))
                return version;

            return new Version(0, 0, 0);
        }

        #endregion
    }
}