using System.Collections.Generic;

namespace EquipmentDesigner.Models.Commands
{
    /// <summary>
    /// Manages undo/redo history for edit commands with a maximum of 20 steps
    /// </summary>
    public class EditHistoryManager
    {
        private const int MaxHistorySize = 20;

        private readonly LinkedList<IEditCommand> _undoStack = new LinkedList<IEditCommand>();
        private readonly LinkedList<IEditCommand> _redoStack = new LinkedList<IEditCommand>();

        /// <summary>
        /// Returns true if there are commands that can be undone
        /// </summary>
        public bool CanUndo => _undoStack.Count > 0;

        /// <summary>
        /// Returns true if there are commands that can be redone
        /// </summary>
        public bool CanRedo => _redoStack.Count > 0;

        /// <summary>
        /// Executes a command and adds it to the undo stack.
        /// Clears the redo stack and drops oldest command if stack exceeds 20.
        /// </summary>
        public void ExecuteCommand(IEditCommand command)
        {
            command.Execute();

            _undoStack.AddLast(command);
            _redoStack.Clear();

            // Drop oldest command if exceeds max size
            if (_undoStack.Count > MaxHistorySize)
            {
                _undoStack.RemoveFirst();
            }
        }

        /// <summary>
        /// Undoes the last command and moves it to the redo stack.
        /// Returns false if there are no commands to undo.
        /// </summary>
        public bool Undo()
        {
            if (_undoStack.Count == 0)
            {
                return false;
            }

            var command = _undoStack.Last.Value;
            _undoStack.RemoveLast();

            command.Undo();
            _redoStack.AddLast(command);

            return true;
        }

        /// <summary>
        /// Redoes the last undone command and moves it back to the undo stack.
        /// Returns false if there are no commands to redo.
        /// </summary>
        public bool Redo()
        {
            if (_redoStack.Count == 0)
            {
                return false;
            }

            var command = _redoStack.Last.Value;
            _redoStack.RemoveLast();

            command.Execute();
            _undoStack.AddLast(command);

            return true;
        }

        /// <summary>
        /// Clears all commands from both undo and redo stacks.
        /// </summary>
        public void Clear()
        {
            _undoStack.Clear();
            _redoStack.Clear();
        }
    }
}
