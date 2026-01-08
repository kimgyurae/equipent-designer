namespace EquipmentDesigner.Models
{
    /// <summary>
    /// Command Parameter DTO
    /// </summary>
    public class ParameterDto
    {
        /// <summary>
        /// 파라미터 이름 (필수)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 파라미터 타입 (필수): String, Int, Float, Bool 등
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 파라미터 설명 (필수)
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 필수 여부
        /// </summary>
        public bool Required { get; set; } = true;

        /// <summary>
        /// 기본값 (선택)
        /// </summary>
        public object DefaultValue { get; set; }
    }
}
