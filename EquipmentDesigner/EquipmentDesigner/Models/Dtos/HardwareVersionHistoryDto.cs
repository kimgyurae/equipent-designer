using System.Collections.Generic;

namespace EquipmentDesigner.Models
{
    /// <summary>
    /// 하드웨어 버전 히스토리 전체 정보
    /// </summary>
    public class HardwareVersionHistoryDto
    {
        /// <summary>
        /// 하드웨어 고유 식별 키
        /// </summary>
        public string HardwareKey { get; set; }

        /// <summary>
        /// 하드웨어 레이어 (Equipment, System, Unit, Device)
        /// </summary>
        public HardwareLayer HardwareLayer { get; set; }

        /// <summary>
        /// 화면 표시용 이름 (예: "AutoAssembler")
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// 전체 버전 수
        /// </summary>
        public int TotalVersionCount { get; set; }

        /// <summary>
        /// 버전 목록 (최신순 정렬)
        /// </summary>
        public List<HardwareVersionSummaryDto> Versions { get; set; } = new List<HardwareVersionSummaryDto>();
    }
}
