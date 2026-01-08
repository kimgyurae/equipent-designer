using System;
using System.Collections.Generic;

namespace EquipmentDesigner.Models
{
    /// <summary>
    /// System DTO - Equipment 하위의 시스템 정의
    /// Equipment ⊃ System ⊃ Unit ⊃ Device
    /// </summary>
    public class SystemDto
    {
        /// <summary>
        /// 고유 식별자
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 소속 Equipment ID
        /// </summary>
        public string EquipmentId { get; set; }

        /// <summary>
        /// System 이름 (필수)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 화면 표시명 (선택)
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// 부가 이름 (선택)
        /// </summary>
        public string Subname { get; set; }

        /// <summary>
        /// 설명 (선택)
        /// </summary>
        public string Description { get; set; }

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
        /// 하위 Unit 목록
        /// </summary>
        public List<UnitDto> Units { get; set; } = new List<UnitDto>();

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
