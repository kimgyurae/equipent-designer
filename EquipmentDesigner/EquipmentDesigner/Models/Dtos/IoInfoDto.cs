namespace EquipmentDesigner.Models
{
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
