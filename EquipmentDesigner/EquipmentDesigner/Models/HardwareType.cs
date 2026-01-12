namespace EquipmentDesigner.Models
{
    /// <summary>
    /// Defines the starting point of the hardware definition workflow.
    /// </summary>
    public enum HardwareType
    {
        /// <summary>
        /// Start from Equipment definition (Equipment → System → Unit → Device).
        /// </summary>
        Equipment = 0,

        /// <summary>
        /// Start from System definition (System → Unit → Device).
        /// </summary>
        System,

        /// <summary>
        /// Start from Unit definition (Unit → Device).
        /// </summary>
        Unit,

        /// <summary>
        /// Start from Device definition only.
        /// </summary>
        Device
    }
}
