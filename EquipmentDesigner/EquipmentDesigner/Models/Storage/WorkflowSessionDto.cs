using System;
using System.Collections.Generic;

namespace EquipmentDesigner.Models
{
    /// <summary>
    /// 워크플로우 세션 DTO - 미완료 워크플로우의 전체 상태 저장
    /// </summary>
    public class WorkflowSessionDto
    {
        /// <summary>
        /// 워크플로우 세션 고유 식별자
        /// </summary>
        public string WorkflowId { get; set; }

        /// <summary>
        /// 워크플로우 시작 레이어 (Equipment, System, Unit, Device)
        /// </summary>
        public HardwareLayer HardwareType { get; set; }

        public ComponentState State { get; set; }
        public DateTime LastModifiedAt { get; set; }

        /// 트리 구조의 모든 노드 데이터.
        /// 다중 인스턴스 지원을 위해 전체 트리 구조를 저장합니다.
        /// </summary>
        public List<TreeNodeDataDto> TreeNodes { get; set; } = new List<TreeNodeDataDto>();

    }
}