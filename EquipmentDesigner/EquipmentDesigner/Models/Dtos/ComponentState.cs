namespace EquipmentDesigner.Models.Dtos
{
    /// <summary>
    /// 컴포넌트 상태 정의
    /// </summary>
    public enum ComponentState
    {
        /// <summary>
        /// 데이터가 생성되었으나 필수 값이 부족함
        /// </summary>
        Undefined = 0,

        /// <summary>
        /// 모든 필수 입력 항목이 채워짐
        /// </summary>
        Defined = 1,

        /// <summary>
        /// 서버에 저장 완료됨
        /// </summary>
        Uploaded = 2,

        /// <summary>
        /// 데이터 검증이 완료됨
        /// </summary>
        Validated = 3
    }
}
