namespace EquipmentDesigner.Views.HardwareDefineWorkflow
{
    /// <summary>
    /// Defines the starting point of the hardware definition workflow.
    /// </summary>
    public enum WorkflowStartType
    {
        /// <summary>
        /// Start from Equipment definition (Equipment → System → Unit → Device).
        /// </summary>
        Equipment,

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
