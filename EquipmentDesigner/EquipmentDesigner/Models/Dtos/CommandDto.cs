using System.Collections.Generic;

namespace EquipmentDesigner.Models
{
    /// <summary>
    /// Command DTO
    /// System, Unit, Device가 가질 수 있는 명령어 정의
    /// </summary>
    public class CommandDto
    {
        /// <summary>
        /// 명령어 이름 (필수)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 명령어 설명 (필수)
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 파라미터 목록 (필수, 1개 이상)
        /// </summary>
        public List<ParameterDto> Parameters { get; set; } = new List<ParameterDto>();
    }
}
