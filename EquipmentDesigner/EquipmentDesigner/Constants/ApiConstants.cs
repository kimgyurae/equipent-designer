namespace EquipmentDesigner.Constants
{
    /// <summary>
    /// API 통신에 사용되는 상수 정의
    /// </summary>
    public static class ApiConstants
    {
        #region Base Configuration

        /// <summary>
        /// API 서버 기본 URL
        /// </summary>
#if DEBUG
        public const string BaseUrl = "http://localhost:5000/api";
#else
        public const string BaseUrl = "https://api.equipment-designer.com/api";
#endif

        /// <summary>
        /// API 버전
        /// </summary>
        public const string ApiVersion = "v1";

        /// <summary>
        /// 전체 API 기본 경로
        /// </summary>
        public static string ApiBaseUrl => $"{BaseUrl}/{ApiVersion}";

        #endregion

        #region Timeout Configuration

        /// <summary>
        /// 기본 요청 타임아웃 (초)
        /// </summary>
        public const int DefaultTimeoutSeconds = 30;

        /// <summary>
        /// 파일 업로드 타임아웃 (초)
        /// </summary>
        public const int FileUploadTimeoutSeconds = 120;

        #endregion

        #region Endpoints - Equipment

        public static class Equipment
        {
            private const string Base = "/equipments";

            /// <summary>
            /// 모든 Equipment 조회
            /// </summary>
            public static string GetAll => Base;

            /// <summary>
            /// Equipment ID로 조회
            /// </summary>
            public static string GetById(string id) => $"{Base}/{id}";

            /// <summary>
            /// Equipment 생성
            /// </summary>
            public static string Create => Base;

            /// <summary>
            /// Equipment 수정
            /// </summary>
            public static string Update(string id) => $"{Base}/{id}";

            /// <summary>
            /// Equipment 삭제
            /// </summary>
            public static string Delete(string id) => $"{Base}/{id}";

            /// <summary>
            /// Equipment 검증
            /// </summary>
            public static string Validate(string id) => $"{Base}/{id}/validate";

            /// <summary>
            /// Equipment 업로드 (서버 저장)
            /// </summary>
            public static string Upload(string id) => $"{Base}/{id}/upload";
        }

        #endregion

        #region Endpoints - System

        public static class System
        {
            private const string Base = "/systems";

            public static string GetAll => Base;
            public static string GetById(string id) => $"{Base}/{id}";
            public static string GetByEquipmentId(string equipmentId) => $"/equipments/{equipmentId}/systems";
            public static string Create => Base;
            public static string Update(string id) => $"{Base}/{id}";
            public static string Delete(string id) => $"{Base}/{id}";
            public static string Validate(string id) => $"{Base}/{id}/validate";
        }

        #endregion

        #region Endpoints - Unit

        public static class Unit
        {
            private const string Base = "/units";

            public static string GetAll => Base;
            public static string GetById(string id) => $"{Base}/{id}";
            public static string GetBySystemId(string systemId) => $"/systems/{systemId}/units";
            public static string Create => Base;
            public static string Update(string id) => $"{Base}/{id}";
            public static string Delete(string id) => $"{Base}/{id}";
            public static string Validate(string id) => $"{Base}/{id}/validate";
        }

        #endregion

        #region Endpoints - Device

        public static class Device
        {
            private const string Base = "/devices";

            public static string GetAll => Base;
            public static string GetById(string id) => $"{Base}/{id}";
            public static string GetByUnitId(string unitId) => $"/units/{unitId}/devices";
            public static string Create => Base;
            public static string Update(string id) => $"{Base}/{id}";
            public static string Delete(string id) => $"{Base}/{id}";
            public static string Validate(string id) => $"{Base}/{id}/validate";
        }

        #endregion

        #region Endpoints - Import (Predefined Components)

        public static class Import
        {
            private const string Base = "/import";

            public static string SearchEquipments(string query) => $"{Base}/equipments?q={query}";
            public static string SearchSystems(string query) => $"{Base}/systems?q={query}";
            public static string SearchUnits(string query) => $"{Base}/units?q={query}";
            public static string SearchDevices(string query) => $"{Base}/devices?q={query}";
        }

        #endregion

        #region HTTP Headers

        public static class Headers
        {
            public const string ContentType = "Content-Type";
            public const string Authorization = "Authorization";
            public const string AcceptLanguage = "Accept-Language";
            public const string ClientVersion = "X-Client-Version";
        }

        public static class ContentTypes
        {
            public const string Json = "application/json";
            public const string FormData = "multipart/form-data";
        }

        #endregion
    }
}
