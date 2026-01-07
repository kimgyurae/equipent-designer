namespace EquipmentDesigner.Models.Dtos
{
    /// <summary>
    /// 컴포넌트 상태 정의
    /// </summary>
    public enum ComponentState
    {
        /// <summary>
        /// 데이터가 생성되었음.
        /// Ready에서 다시 편집 창으로 돌아오면 Draft로 복귀
        /// </summary>
        Draft = 0,

        /// <summary>
        /// 모든 필수 항목이 채워지고 Confirmation Page로 이동 시 자동 변경
        /// </summary>
        Ready = 1,

        /// <summary>
        /// 서버에 저장 완료됨
        /// </summary>
        Uploaded = 2,

        /// <summary>
        /// 서버에서 운영자에 의해 
        /// 데이터 검증이 완료됨
        /// </summary>
        Validated = 3
    }
}
