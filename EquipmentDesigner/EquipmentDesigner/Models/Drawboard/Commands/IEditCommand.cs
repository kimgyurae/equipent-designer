namespace EquipmentDesigner.Models.Commands
{
    /// <summary>
    /// Interface for undoable edit commands (Command Pattern)
    /// </summary>
    public interface IEditCommand
    {
        /// <summary>
        /// Executes the command
        /// </summary>
        void Execute();

        /// <summary>
        /// Reverses the command execution
        /// </summary>
        void Undo();

        /// <summary>
        /// Human-readable description of the command for display
        /// </summary>
        string Description { get; }
    }
}
