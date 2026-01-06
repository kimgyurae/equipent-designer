using System;
using System.Collections.Generic;
using EquipmentDesigner.Models.Dtos;

namespace EquipmentDesigner.Models.Storage
{
    /// <summary>
    /// 작업 세션 컨텍스트 - 앱 재시작 시 마지막 작업 위치 복구용
    /// </summary>
    public class WorkSessionContext
    {
        /// <summary>
        /// 마지막으로 작업 중이던 워크플로우 시작 레이어
        /// </summary>
        public HardwareLayer? LastWorkflowType { get; set; }

        /// <summary>
        /// 마지막으로 편집 중이던 컴포넌트 ID
        /// </summary>
        public string LastEditingComponentId { get; set; }

        /// <summary>
        /// 마지막으로 편집 중이던 컴포넌트 타입
        /// </summary>
        public HardwareLayer? LastEditingHardwareLayer { get; set; }

        /// <summary>
        /// 현재 워크플로우 단계 (0-based index)
        /// </summary>
        public int CurrentWorkflowStep { get; set; }

        /// <summary>
        /// 미완성 워크플로우 목록 (Resume Tasks용)
        /// </summary>
        public List<IncompleteWorkflowInfo> IncompleteWorkflows { get; set; } = new List<IncompleteWorkflowInfo>();
    }
    /// <summary>
    /// 미완성 워크플로우 정보
    /// </summary>
    public class IncompleteWorkflowInfo
    {
        /// <summary>
        /// 워크플로우 세션 고유 식별자
        /// </summary>
        public string WorkflowId { get; set; }

        /// <summary>
        /// 워크플로우 시작 레이어
        /// </summary>
        public HardwareLayer StartType { get; set; }

        /// <summary>
        /// 현재 단계 이름 (Dashboard 표시용)
        /// </summary>
        public string CurrentStepName { get; set; }

        public string ComponentId { get; set; }
        public HardwareLayer HardwareLayer { get; set; }
        public ComponentState State { get; set; }
        public DateTime LastModifiedAt { get; set; }
        public int CompletedFields { get; set; }
        public int TotalFields { get; set; }
    }
}