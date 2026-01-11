using System;
using System.Collections.Generic;

namespace EquipmentDesigner.Models
{
    /// <summary>
    /// Device DTO - Unit 하위의 디바이스 정의
    /// Equipment ⊃ System ⊃ Unit ⊃ Device
    /// </summary>
    public class DeviceDto: IIdentifiable
    {
        /// <summary>
        /// 고유 식별자
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 소속 Unit ID
        /// </summary>
        public string UnitId { get; set; }

        /// <summary>
        /// Device 이름 (필수)
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
        /// Device 타입 (예: Generic, Sensor, Actuator 등)
        /// </summary>
        public string DeviceType { get; set; }

        /// <summary>
        /// 구현 가이드라인 (선택)
        /// </summary>
        public List<string> ImplementationInstructions { get; set; } = new List<string>();

        /// <summary>
        /// 명령어 목록
        /// </summary>
        public List<CommandDto> Commands { get; set; } = new List<CommandDto>();

        /// <summary>
        /// IO 정보 목록 (Device는 IO 정보를 가짐)
        /// </summary>
        public List<IoInfoDto> IoInfo { get; set; } = new List<IoInfoDto>();

        /// <summary>
        /// 컴포넌트 상태
        /// </summary>
        public ComponentState State { get; set; } = ComponentState.Draft;

        /// <summary>
        /// 생성 일시
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 수정 일시
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// IO 정보 DTO
    /// </summary>
    public class IoInfoDto
    {
        /// <summary>
        /// IO 이름
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// IO 타입: Input, Output, AnalogInput, AnalogOutput
        /// </summary>
        public string IoType { get; set; }

        /// <summary>
        /// IO 주소 (예: 0x0010)
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// 설명
        /// </summary>
        public string Description { get; set; }
    }
}