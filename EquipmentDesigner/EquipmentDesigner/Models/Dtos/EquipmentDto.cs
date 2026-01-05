using System;
using System.Collections.Generic;

namespace EquipmentDesigner.Models.Dtos
{
    /// <summary>
    /// Equipment DTO - 최상위 설비 정의
    /// </summary>
    public class EquipmentDto
    {
        /// <summary>
        /// 고유 식별자
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Equipment 종류 (선택)
        /// </summary>
        public string EquipmentType { get; set; }

        /// <summary>
        /// Equipment 이름 (필수)
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
        /// 고객사 (선택)
        /// </summary>
        public string Customer { get; set; }

        /// <summary>
        /// 프로세스 (선택)
        /// </summary>
        public string Process { get; set; }

        /// <summary>
        /// 첨부 문서 파일 경로 목록 (선택)
        /// 지원 확장자: PDF, PPT, MD, DRAWIO
        /// </summary>
        public List<string> AttachedDocuments { get; set; } = new List<string>();

        /// <summary>
        /// 컴포넌트 상태
        /// </summary>
        public ComponentState State { get; set; } = ComponentState.Undefined;

        /// <summary>
        /// 하위 System 목록
        /// </summary>
        public List<SystemDto> Systems { get; set; } = new List<SystemDto>();

        /// <summary>
        /// 생성 일시
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 수정 일시
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        #region Path Definitions

        /// <summary>
        /// 프로그램 루트 경로
        /// </summary>
        public string ProgramRoot { get; set; }

        #endregion
    }
}
