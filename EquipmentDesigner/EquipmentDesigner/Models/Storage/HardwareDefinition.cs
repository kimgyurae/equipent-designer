using System;
using System.Collections.Generic;

namespace EquipmentDesigner.Models
{
    /// <summary>
    /// 하드웨어 DTO
    /// </summary>
    public class HardwareDefinition : IIdentifiable
    {
        /// <summary>
        /// 정의된 하드웨어 조립체 아이디
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 하드웨어 타입 (Equipment, System, Unit, Device)
        /// </summary>
        public HardwareType HardwareType { get; set; }

        /// <summary>
        /// 하드웨어 고유 식별 키 - 같은 하드웨어의 모든 버전이 동일한 키를 공유
        /// null인 경우 루트 노드의 Name을 기본값으로 사용 (하위 호환성)
        /// </summary>
        public string HardwareKey { get; set; }

        /// <summary>
        /// 최상위 하드웨어의 버전 정보 (예: 1.0.0)
        /// </summary>
        public string Version { get; set; } = "1.0.0";

        public ComponentState State { get; set; }
        public DateTime LastModifiedAt { get; set; }

        /// 트리 구조의 모든 노드 데이터.
        /// 다중 인스턴스 지원을 위해 전체 트리 구조를 저장합니다.
        /// </summary>
        public List<TreeNodeDataDto> TreeNodes { get; set; } = new List<TreeNodeDataDto>();

    }
}