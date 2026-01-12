using System;
using System.Collections.Generic;

namespace EquipmentDesigner.Models
{
    /// <summary>
    /// 하드웨어 DTO - 모든 하드웨어 계층(Equipment, System, Unit, Device)을 통합한 모델
    /// </summary>
    public class HardwareDefinition: IIdentifiable
    {
        /// <summary>
        /// 정의된 하드웨어 조립체 아이디
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 하드웨어 고유 식별 키 - 같은 하드웨어의 모든 버전이 동일한 키를 공유
        /// null인 경우 루트 노드의 Name을 기본값으로 사용 (하위 호환성)
        /// </summary>
        public string HardwareKey { get; set; }

        /// <summary>
        /// 하드웨어 타입 (Equipment, System, Unit, Device)
        /// </summary>
        public HardwareType HardwareType { get; set; }

        /// <summary>
        /// 최상위 하드웨어의 버전 정보 (예: 1.0.0)
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// 컴포넌트 상태
        /// </summary>
        public ComponentState State { get; set; }

        /// <summary>
        /// 하드웨어 이름 (필수)
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
        /// 고객사 (선택) - Equipment에서 사용
        /// </summary>
        public string Customer { get; set; }

        /// <summary>
        /// 프로세스 Id (선택. HardwareType이 Equipment, System, Unit일 때 사용)
        /// </summary>
        public string ProcessId { get; set; }

        /// <summary>
        /// 프로세스 정보 (선택) - System, Unit에서 사용
        /// </summary>
        public string ProcessInfo { get; set; }

        /// <summary>
        /// Equipment 종류 (선택) - Equipment에서 사용
        /// </summary>
        public string EquipmentType { get; set; }

        /// <summary>
        /// Device 타입 (선택) - Device에서 사용 (예: Generic, Sensor, Actuator 등)
        /// </summary>
        public string DeviceType { get; set; }

        /// <summary>
        /// 구현 가이드라인 (선택) - System, Unit, Device에서 사용
        /// </summary>
        public List<string> ImplementationInstructions { get; set; } = new List<string>();

        /// <summary>
        /// 명령어 목록 - System, Unit, Device에서 사용
        /// </summary>
        public List<CommandDto> Commands { get; set; } = new List<CommandDto>();

        /// <summary>
        /// IO 정보 목록 - Device에서 사용
        /// </summary>
        public List<IoInfoDto> IoInfo { get; set; } = new List<IoInfoDto>();

        /// <summary>
        /// 첨부 문서 파일 경로 목록 (선택)
        /// 지원 확장자: PDF, PPT, MD, DRAWIO
        /// </summary>
        public List<string> AttachedDocumentsIds { get; set; } = new List<string>();

        /// <summary>
        /// 프로그램 루트 경로 - Equipment에서 사용
        /// </summary>
        public string ProgramRoot { get; set; }

        /// <summary>
        /// 생성 일시
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 수정 일시
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// 마지막 수정 일시 (세션 레벨)
        /// </summary>
        public DateTime LastModifiedAt { get; set; }

        /// <summary>
        /// 하위 하드웨어 목록 (재귀 구조)
        /// Equipment → System → Unit → Device
        /// </summary>
        public List<HardwareDefinition> Children { get; set; } = new List<HardwareDefinition>();
    }
}