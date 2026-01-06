using System;
using System.Collections.Generic;
using EquipmentDesigner.Models.Dtos;

namespace EquipmentDesigner.Models.Storage
{
    /// <summary>
    /// SharedMemory 파일에 저장되는 루트 데이터 구조
    /// 모든 컴포넌트 데이터를 하나의 단위로 관리
    /// </summary>
    public class IncompleteWorkflowDataStore
    {
        /// <summary>
        /// 데이터 스토어 버전 (마이그레이션 지원)
        /// </summary>
        public string Version { get; set; } = "1.0";

        /// <summary>
        /// 마지막 저장 시각
        /// </summary>
        public DateTime LastSavedAt { get; set; }

        /// <summary>
        /// 미완료 워크플로우 세션 목록 (Resume Tasks용)
        /// </summary>
        public List<WorkflowSessionDto2> WorkflowSessions { get; set; } = new List<WorkflowSessionDto2>();
    }
}