using System;
using System.Collections.Generic;

namespace EquipmentDesigner.Models
{
    /// <summary>
    /// SharedMemory 파일에 저장되는 루트 데이터 구조
    /// 모든 컴포넌트 데이터를 하나의 단위로 관리
    /// </summary>
    public class UploadedHardwareDataStore
    {
        /// <summary>
        /// 데이터 스토어 버전 (마이그레이션 지원)
        /// </summary>
        public string Version { get; set; } = "1.0";

        /// <summary>
        /// 마지막 저장 시각
        /// </summary>
        public DateTime LastSavedAt { get; set; }

      
        /// <summary>
        /// Equipment 목록
        /// </summary>
        public List<EquipmentDto> Equipments { get; set; } = new List<EquipmentDto>();

        /// <summary>
        /// System 목록
        /// </summary>
        public List<SystemDto> Systems { get; set; } = new List<SystemDto>();

        /// <summary>
        /// Unit 목록
        /// </summary>
        public List<UnitDto> Units { get; set; } = new List<UnitDto>();

        /// <summary>
        /// Device 목록
        /// </summary>
        public List<DeviceDto> Devices { get; set; } = new List<DeviceDto>();
    }
}