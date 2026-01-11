using System.Collections.Generic;

namespace EquipmentDesigner.Models
{
    /// <summary>
    /// Data transfer object for serializing tree node structure and associated ViewModel data.
    /// Used for persisting and restoring workflow sessions with full tree hierarchy.
    /// </summary>
    public class TreeNodeDataDto: IIdentifiable
    {

        public string Version = "0.0.1";
        /// <summary>
        /// Unique identifier for this node.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The hardware layer type (Equipment, System, Unit, Device).
        /// </summary>
        public HardwareLayer HardwareLayer { get; set; }

        /// <summary>
        /// Equipment data if this is an Equipment node, null otherwise.
        /// </summary>
        public EquipmentDto EquipmentData { get; set; }

        /// <summary>
        /// System data if this is a System node, null otherwise.
        /// </summary>
        public SystemDto SystemData { get; set; }

        /// <summary>
        /// Unit data if this is a Unit node, null otherwise.
        /// </summary>
        public UnitDto UnitData { get; set; }

        /// <summary>
        /// Device data if this is a Device node, null otherwise.
        /// </summary>
        public DeviceDto DeviceData { get; set; }

        /// <summary>
        /// Child nodes in the hierarchy.
        /// </summary>
        public List<TreeNodeDataDto> Children { get; set; } = new List<TreeNodeDataDto>();
    }
}
