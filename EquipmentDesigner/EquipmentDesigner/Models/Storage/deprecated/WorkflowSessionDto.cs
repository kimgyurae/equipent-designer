using System;
using System.Collections.Generic;
using EquipmentDesigner.Models.Dtos;

namespace EquipmentDesigner.Models.Storage
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
        public HardwareLayer StartType { get; set; }

        /// <summary>
        /// 현재 워크플로우 단계 인덱스 (0-based)
        /// </summary>
        public int CurrentStepIndex { get; set; }

        /// <summary>
        /// 마지막 수정 시각
        /// </summary>
        public DateTime LastModifiedAt { get; set; }

        /// <summary>
        /// Equipment 컴포넌트 데이터 (StartType이 Equipment일 때 사용)
        /// </summary>
        public EquipmentDto EquipmentData { get; set; }

        /// <summary>
        /// System 컴포넌트 데이터
        /// </summary>
        public SystemDto SystemData { get; set; }

        /// <summary>
        /// Unit 컴포넌트 데이터
        /// </summary>
        public UnitDto UnitData { get; set; }

        /// <summary>
        /// Device 컴포넌트 데이터
        /// </summary>
        public DeviceDto DeviceData { get; set; }

        /// <summary>
        /// 트리 구조의 모든 노드 데이터.
        /// 다중 인스턴스 지원을 위해 전체 트리 구조를 저장합니다.
        /// </summary>
        public List<TreeNodeDataDto> TreeNodes { get; set; } = new List<TreeNodeDataDto>();

        /// <summary>
        /// 현재 단계 이름 반환 (Dashboard 표시용)
        /// </summary>
        public string GetCurrentStepName()
        {
            return StartType switch
            {
                HardwareLayer.Equipment => CurrentStepIndex switch
                {
                    0 => "Equipment",
                    1 => "System",
                    2 => "Unit",
                    3 => "Device",
                    _ => "Unknown"
                },
                HardwareLayer.System => CurrentStepIndex switch
                {
                    0 => "System",
                    1 => "Unit",
                    2 => "Device",
                    _ => "Unknown"
                },
                HardwareLayer.Unit => CurrentStepIndex switch
                {
                    0 => "Unit",
                    1 => "Device",
                    _ => "Unknown"
                },
                HardwareLayer.Device => "Device",
                _ => "Unknown"
            };
        }
    }
}