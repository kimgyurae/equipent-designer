using System;

namespace EquipmentDesigner.Models
{
    /// <summary>
    /// 버전 선택 다이얼로그용 버전 요약 정보
    /// </summary>
    public class HardwareVersionSummaryDto
    {
        /// <summary>
        /// 해당 버전의 WorkflowId (선택 시 이 ID로 상세 페이지 이동)
        /// </summary>
        public string WorkflowId { get; set; }

        /// <summary>
        /// 버전 문자열 (예: "v2.7.1")
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// 컴포넌트 상태 (Ready, Uploaded, Validated)
        /// </summary>
        public ComponentState State { get; set; }

        /// <summary>
        /// 마지막 수정 일시
        /// </summary>
        public DateTime LastModifiedAt { get; set; }

        /// <summary>
        /// 최신 버전 여부
        /// </summary>
        public bool IsLatest { get; set; }

        /// <summary>
        /// 설명 (선택)
        /// </summary>
        public string Description { get; set; }
    }
}
