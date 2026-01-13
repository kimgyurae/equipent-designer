using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EquipmentDesigner.Models;
using EquipmentDesigner.Controls;
using EquipmentDesigner.Utils;

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

        public async Task<ApiResponse<List<HardwareDefinition>>> GetHardwaresByStateAsync(params ComponentState[] states)
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

        public async Task<PagedApiResponse<HardwareVersionSummaryDto>> GetHardwareByHardwareKeyAsync(
            string hardwareKey,
            int page = 1,
            int pageSize = 10)
        {
            LoadingSpinnerService.Instance.Show();
            await Task.Delay(SimulatedDelay);

            try
            {
                var sessions = await _repository.LoadAsync();

                // 같은 HardwareKey를 가진 모든 세션 필터링 (HardwareType 무관)
                var matchingSessions = sessions?
                    .Where(s => GetEffectiveHardwareKey(s) == hardwareKey)
                    .OrderByDescending(s => ParseVersion(s.Version))
                    .ToList() ?? new List<HardwareDefinition>();

                var totalCount = matchingSessions.Count;
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                // 페이지네이션 적용
                var pagedSessions = matchingSessions
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var versions = pagedSessions.Select((s, index) => new HardwareVersionSummaryDto
                {
                    WorkflowId = s.Id,
                    Version = s.Version ?? "v0.0.0",
                    State = s.State,
                    LastModifiedAt = s.LastModifiedAt,
                    IsLatest = page == 1 && index == 0,
                    Description = s.Description
                }).ToList();

                LoadingSpinnerService.Instance.Hide();
                return new PagedApiResponse<HardwareVersionSummaryDto>
                {
                    Success = true,
                    Data = versions,
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = totalPages
                };
            }
            catch (Exception ex)
            {
                LoadingSpinnerService.Instance.Hide();
                return new PagedApiResponse<HardwareVersionSummaryDto>
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    ErrorCode = "LOAD_ERROR",
                    Data = new List<HardwareVersionSummaryDto>()
                };
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

        public async Task<ApiResponse<List<HardwareDefinition>>> GetAllHardwaresWithUniqueHardwareKeyAsync()
        {
            LoadingSpinnerService.Instance.Show();
            await Task.Delay(SimulatedDelay);

            try
            {
                var sessions = await _repository.LoadAsync();

                // HardwareKey별로 그룹화하고 각 그룹에서 버전이 가장 높은 것만 선택
                var latestByKey = sessions?
                    .GroupBy(s => GetEffectiveHardwareKey(s))
                    .Where(g => !string.IsNullOrEmpty(g.Key))
                    .Select(g => g
                        .OrderByDescending(s => NormalizeVersionForComparison(s.Version), new VersionComparer())
                        .First())
                    .ToList() ?? new List<HardwareDefinition>();

                LoadingSpinnerService.Instance.Hide();
                return ApiResponse<List<HardwareDefinition>>.Ok(latestByKey);
            }
            catch (Exception ex)
            {
                LoadingSpinnerService.Instance.Hide();
                return ApiResponse<List<HardwareDefinition>>.Fail(ex.Message, "LOAD_ERROR");
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
        /// 버전 문자열을 정규화하여 VersionHelper 비교에 적합한 형식으로 변환합니다.
        /// </summary>
        private string NormalizeVersionForComparison(string versionString)
        {
            if (string.IsNullOrEmpty(versionString))
                return "0.0.0";

            // 'v' 또는 'V' 접두사 제거
            var normalized = versionString.TrimStart('v', 'V').Trim();

            // 버전 형식 맞추기 (X.Y → X.Y.0)
            var parts = normalized.Split('.');
            if (parts.Length == 1)
                normalized = $"{parts[0]}.0.0";
            else if (parts.Length == 2)
                normalized = $"{parts[0]}.{parts[1]}.0";

            // VersionHelper가 처리할 수 있는 형식인지 확인
            if (VersionHelper.IsValid(normalized))
                return normalized;

            return "0.0.0";
        }

        /// <summary>
        /// VersionHelper를 사용하여 버전 문자열을 비교하는 Comparer 클래스입니다.
        /// </summary>
        private class VersionComparer : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                // 둘 다 null이거나 빈 문자열이면 동일
                if (string.IsNullOrEmpty(x) && string.IsNullOrEmpty(y))
                    return 0;

                // null/빈 문자열은 가장 낮은 버전으로 처리
                if (string.IsNullOrEmpty(x))
                    return -1;
                if (string.IsNullOrEmpty(y))
                    return 1;

                try
                {
                    return VersionHelper.Compare(x, y);
                }
                catch
                {
                    // 비교 실패 시 문자열 비교로 폴백
                    return string.Compare(x, y, StringComparison.Ordinal);
                }
            }
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