using System;
using System.Threading.Tasks;
using EquipmentDesigner.Models.Storage;

namespace EquipmentDesigner.Services.Storage
{
    /// <summary>
    /// 데이터 저장소 인터페이스 - SharedMemory/REST API 전환 추상화
    /// </summary>
    public interface IDataRepository
    {
        /// <summary>
        /// 데이터 스토어 전체 로드
        /// </summary>
        Task<SharedMemoryDataStore> LoadAsync();

        /// <summary>
        /// 데이터 스토어 전체 저장
        /// </summary>
        Task SaveAsync(SharedMemoryDataStore dataStore);

        /// <summary>
        /// 자동 저장 활성화
        /// </summary>
        /// <param name="interval">저장 간격</param>
        void EnableAutoSave(TimeSpan interval);

        /// <summary>
        /// 자동 저장 비활성화
        /// </summary>
        void DisableAutoSave();

        /// <summary>
        /// 변경 플래그 설정
        /// </summary>
        void MarkDirty();

        /// <summary>
        /// 데이터 변경 여부
        /// </summary>
        bool IsDirty { get; }
    }
}
