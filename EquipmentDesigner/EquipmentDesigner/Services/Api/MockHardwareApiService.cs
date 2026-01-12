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
                var dataStore = await _repository.LoadAsync();
                LoadingSpinnerService.Instance.Hide();
                return ApiResponse<List<HardwareDefinition>>.Ok(
                    dataStore.WorkflowSessions?.ToList() ?? new List<HardwareDefinition>());
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
                var dataStore = await _repository.LoadAsync();
                var filtered = dataStore.WorkflowSessions?
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
                var dataStore = await _repository.LoadAsync();
                var session = dataStore.WorkflowSessions?.FirstOrDefault(s => s.Id == workflowId);

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
                var dataStore = await _repository.LoadAsync();

                var existingIndex = dataStore.WorkflowSessions.FindIndex(s => s.Id == session.Id);
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
                var dataStore = await _repository.LoadAsync();
                var session = dataStore.WorkflowSessions?.FirstOrDefault(s => s.Id == workflowId);

                if (session == null)
                {
                    LoadingSpinnerService.Instance.Hide();
                    return ApiResponse<HardwareDefinition>.Fail(
                        $"Session with ID '{workflowId}' not found", "NOT_FOUND");
                }

                session.State = newState;
                session.LastModifiedAt = DateTime.Now;
                await _repository.SaveAsync(dataStore);
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
                var dataStore = await _repository.LoadAsync();
                var session = dataStore.WorkflowSessions?.FirstOrDefault(s => s.Id == workflowId);

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

        public async Task<ApiResponse<HardwareVersionHistoryDto>> GetVersionHistoryAsync(
            string hardwareKey,
            HardwareType hardwareType)
        {
            LoadingSpinnerService.Instance.Show();
            await Task.Delay(SimulatedDelay);

            try
            {
                var dataStore = await _repository.LoadAsync();

                // 같은 HardwareKey + HardwareType를 가진 모든 세션 필터링
                var matchingSessions = dataStore.WorkflowSessions?
                    .Where(s => s.HardwareType == hardwareType)
                    .Where(s => GetEffectiveHardwareKey(s) == hardwareKey)
                    .OrderByDescending(s => ParseVersion(GetVersion(s)))
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
                    DisplayName = GetDisplayName(matchingSessions.First()),
                    TotalVersionCount = matchingSessions.Count,
                    Versions = matchingSessions.Select((s, index) => new HardwareVersionSummaryDto
                    {
                        WorkflowId = s.Id,
                        Version = GetVersion(s),
                        State = s.State,
                        LastModifiedAt = s.LastModifiedAt,
                        IsLatest = index == 0,
                        Description = GetDescription(s)
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
                var dataStore = await _repository.LoadAsync();

                var distinctKeys = dataStore.WorkflowSessions?
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
            var rootNode = session.TreeNodes?.FirstOrDefault();
            if (rootNode == null) return null;

            return session.HardwareType switch
            {
                HardwareType.Equipment => rootNode.EquipmentData?.HardwareKey ?? rootNode.EquipmentData?.Name,
                HardwareType.System => rootNode.SystemData?.HardwareKey ?? rootNode.SystemData?.Name,
                HardwareType.Unit => rootNode.UnitData?.HardwareKey ?? rootNode.UnitData?.Name,
                HardwareType.Device => rootNode.DeviceData?.HardwareKey ?? rootNode.DeviceData?.Name,
                _ => null
            };
        }

        /// <summary>
        /// 세션에서 버전 정보를 가져옵니다.
        /// </summary>
        private string GetVersion(HardwareDefinition session)
        {
            var rootNode = session.TreeNodes?.FirstOrDefault();
            if (rootNode == null) return "v0.0.0";

            return session.HardwareType switch
            {
                HardwareType.Equipment => rootNode.EquipmentData?.Version ?? "v0.0.0",
                HardwareType.System => rootNode.SystemData?.Version ?? "v0.0.0",
                HardwareType.Unit => rootNode.UnitData?.Version ?? "v0.0.0",
                HardwareType.Device => rootNode.DeviceData?.Version ?? "v0.0.0",
                _ => "v0.0.0"
            };
        }

        /// <summary>
        /// 세션에서 표시용 이름을 가져옵니다.
        /// </summary>
        private string GetDisplayName(HardwareDefinition session)
        {
            var rootNode = session.TreeNodes?.FirstOrDefault();
            if (rootNode == null) return "Unknown";

            return session.HardwareType switch
            {
                HardwareType.Equipment => rootNode.EquipmentData?.DisplayName ?? rootNode.EquipmentData?.Name ?? "Unknown",
                HardwareType.System => rootNode.SystemData?.DisplayName ?? rootNode.SystemData?.Name ?? "Unknown",
                HardwareType.Unit => rootNode.UnitData?.DisplayName ?? rootNode.UnitData?.Name ?? "Unknown",
                HardwareType.Device => rootNode.DeviceData?.DisplayName ?? rootNode.DeviceData?.Name ?? "Unknown",
                _ => "Unknown"
            };
        }

        /// <summary>
        /// 세션에서 설명을 가져옵니다.
        /// </summary>
        private string GetDescription(HardwareDefinition session)
        {
            var rootNode = session.TreeNodes?.FirstOrDefault();
            if (rootNode == null) return null;

            return session.HardwareType switch
            {
                HardwareType.Equipment => rootNode.EquipmentData?.Description,
                HardwareType.System => rootNode.SystemData?.Description,
                HardwareType.Unit => rootNode.UnitData?.Description,
                HardwareType.Device => rootNode.DeviceData?.Description,
                _ => null
            };
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