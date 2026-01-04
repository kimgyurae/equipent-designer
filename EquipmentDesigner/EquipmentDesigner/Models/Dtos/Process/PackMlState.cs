namespace EquipmentDesigner.Models.Dtos.Process
{
    /// <summary>
    /// PackML standard 17 states enum
    /// </summary>
    public enum PackMlState
    {
        Idle,
        Starting,
        Execute,
        Suspending,
        Suspended,
        Unsuspending,
        Holding,
        Held,
        Unholding,
        Aborting,
        Aborted,
        Clearing,
        Resetting,
        Stopping,
        Stopped,
        Completing,
        Complete
    }
}
