using System.Collections.Generic;

namespace EquipmentDesigner.Models
{
    /// <summary>
    /// API 응답 래퍼 클래스
    /// </summary>
    /// <typeparam name="T">응답 데이터 타입</typeparam>
    public class ApiResponse<T>
    {
        /// <summary>
        /// 요청 성공 여부
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 응답 데이터
        /// </summary>
        public T Data { get; set; }

        /// <summary>
        /// 오류 메시지 (실패 시)
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// 오류 코드 (실패 시)
        /// </summary>
        public string ErrorCode { get; set; }

        /// <summary>
        /// 유효성 검사 오류 목록
        /// </summary>
        public List<ValidationError> ValidationErrors { get; set; }

        /// <summary>
        /// 성공 응답 생성
        /// </summary>
        public static ApiResponse<T> Ok(T data)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Data = data,
                ValidationErrors = new List<ValidationError>()
            };
        }

        /// <summary>
        /// 실패 응답 생성
        /// </summary>
        public static ApiResponse<T> Fail(string errorMessage, string errorCode = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                ErrorMessage = errorMessage,
                ErrorCode = errorCode,
                ValidationErrors = new List<ValidationError>()
            };
        }

        /// <summary>
        /// 유효성 검사 실패 응답 생성
        /// </summary>
        public static ApiResponse<T> ValidationFail(List<ValidationError> errors)
        {
            return new ApiResponse<T>
            {
                Success = false,
                ErrorMessage = "Validation failed",
                ErrorCode = "VALIDATION_ERROR",
                ValidationErrors = errors
            };
        }
    }

    /// <summary>
    /// 유효성 검사 오류 정보
    /// </summary>
    public class ValidationError
    {
        /// <summary>
        /// 오류가 발생한 필드명
        /// </summary>
        public string Field { get; set; }

        /// <summary>
        /// 오류 메시지
        /// </summary>
        public string Message { get; set; }
    }

    /// <summary>
    /// 페이징된 API 응답
    /// </summary>
    /// <typeparam name="T">응답 데이터 타입</typeparam>
    public class PagedApiResponse<T> : ApiResponse<List<T>>
    {
        /// <summary>
        /// 현재 페이지 번호 (1부터 시작)
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// 페이지당 항목 수
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// 전체 항목 수
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 전체 페이지 수
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// 다음 페이지 존재 여부
        /// </summary>
        public bool HasNextPage => Page < TotalPages;

        /// <summary>
        /// 이전 페이지 존재 여부
        /// </summary>
        public bool HasPreviousPage => Page > 1;
    }
}
