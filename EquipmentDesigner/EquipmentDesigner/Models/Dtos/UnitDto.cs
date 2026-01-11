using System;
using System.Collections.Generic;

namespace EquipmentDesigner.Models
{
    /// <summary>
    /// Unit DTO - System 하위의 유닛 정의
    /// Equipment ⊃ System ⊃ Unit ⊃ Device
    /// </summary>
    public class UnitDto: IIdentifiable
    {
        /// <summary>
        /// 고유 식별자
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 소속 System ID
        /// </summary>
        public string SystemId { get; set; }

        /// <summary>
        /// Unit 이름 (필수)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 화면 표시명 (선택)
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// 설명 (선택)
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 버전 정보 (예: v1.0.0)
        /// </summary>
        public string Version { get; set; } = "1.0.0";

        /// <summary>
        /// 하드웨어 고유 식별 키 - 같은 하드웨어의 모든 버전이 동일한 키를 공유
        /// null인 경우 Name을 기본값으로 사용 (하위 호환성)
        /// </summary>
        public string HardwareKey { get; set; }

        /// <summary>
        /// 구현 가이드라인 (선택)
        /// </summary>
        public List<string> ImplementationInstructions { get; set; } = new List<string>();

        /// <summary>
        /// 명령어 목록
        /// </summary>
        public List<CommandDto> Commands { get; set; } = new List<CommandDto>();

        /// <summary>
        /// 프로세스 정보 (선택)
        /// </summary>
        public string ProcessInfo { get; set; }

        /// <summary>
        /// 컴포넌트 상태
        /// </summary>
        public ComponentState State { get; set; } = ComponentState.Draft;

        /// <summary>
        /// 하위 Device 목록
        /// </summary>
        public List<DeviceDto> Devices { get; set; } = new List<DeviceDto>();

        /// <summary>
        /// 생성 일시
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 수정 일시
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }
}