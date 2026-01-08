using EquipmentDesigner.Models;
using EquipmentDesigner.Models.Drawboard;
using EquipmentDesigner.Models.Drawboard.Commands;
using FluentAssertions;
using Xunit;

namespace EquipmentDesigner.Tests.Models.Drawboard
{
    public class EditHistoryManagerTests
    {
        #region Initialization Tests

        [Fact]
        public void EditHistoryManager_InitializesWithEmptyStacks()
        {
            // Arrange & Act
            var manager = new EditHistoryManager();

            // Assert
            manager.CanUndo.Should().BeFalse();
            manager.CanRedo.Should().BeFalse();
        }

        #endregion

        #region ExecuteCommand Tests

        [Fact]
        public void ExecuteCommand_ExecutesCommandAndPushesToUndoStack()
        {
            // Arrange
            var manager = new EditHistoryManager();
            var workspace = new StateWorkspace(PackMlState.Idle);
            var element = new ActionShape();
            var command = new AddElementCommand(workspace, element);

            // Act
            manager.ExecuteCommand(command);

            // Assert
            manager.CanUndo.Should().BeTrue();
            workspace.Elements.Should().Contain(element);
        }

        [Fact]
        public void ExecuteCommand_ClearsRedoStack()
        {
            // Arrange
            var manager = new EditHistoryManager();
            var workspace = new StateWorkspace(PackMlState.Idle);
            var element1 = new ActionShape();
            var element2 = new ActionShape();

            manager.ExecuteCommand(new AddElementCommand(workspace, element1));
            manager.Undo();
            manager.CanRedo.Should().BeTrue();

            // Act
            manager.ExecuteCommand(new AddElementCommand(workspace, element2));

            // Assert
            manager.CanRedo.Should().BeFalse();
        }

        [Fact]
        public void ExecuteCommand_DropsOldestWhenUndoStackExceeds20()
        {
            // Arrange
            var manager = new EditHistoryManager();
            var workspace = new StateWorkspace(PackMlState.Idle);

            // Add 21 commands
            for (int i = 0; i < 21; i++)
            {
                manager.ExecuteCommand(new AddElementCommand(workspace, new ActionShape()));
            }

            // Act - Undo all
            int undoCount = 0;
            while (manager.CanUndo)
            {
                manager.Undo();
                undoCount++;
            }

            // Assert - Should only be able to undo 20 times
            undoCount.Should().Be(20);
        }

        #endregion

        #region Undo Tests

        [Fact]
        public void Undo_PopsFromUndoStackAndCallsUndo()
        {
            // Arrange
            var manager = new EditHistoryManager();
            var workspace = new StateWorkspace(PackMlState.Idle);
            var element = new ActionShape();
            manager.ExecuteCommand(new AddElementCommand(workspace, element));

            // Act
            var result = manager.Undo();

            // Assert
            result.Should().BeTrue();
            workspace.Elements.Should().NotContain(element);
        }

        [Fact]
        public void Undo_PushesToRedoStack()
        {
            // Arrange
            var manager = new EditHistoryManager();
            var workspace = new StateWorkspace(PackMlState.Idle);
            var element = new ActionShape();
            manager.ExecuteCommand(new AddElementCommand(workspace, element));

            // Act
            manager.Undo();

            // Assert
            manager.CanRedo.Should().BeTrue();
        }

        [Fact]
        public void Undo_ReturnsFalseWhenStackEmpty()
        {
            // Arrange
            var manager = new EditHistoryManager();

            // Act
            var result = manager.Undo();

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region Redo Tests

        [Fact]
        public void Redo_PopsFromRedoStackAndCallsExecute()
        {
            // Arrange
            var manager = new EditHistoryManager();
            var workspace = new StateWorkspace(PackMlState.Idle);
            var element = new ActionShape();
            manager.ExecuteCommand(new AddElementCommand(workspace, element));
            manager.Undo();

            // Act
            var result = manager.Redo();

            // Assert
            result.Should().BeTrue();
            workspace.Elements.Should().Contain(element);
        }

        [Fact]
        public void Redo_PushesToUndoStack()
        {
            // Arrange
            var manager = new EditHistoryManager();
            var workspace = new StateWorkspace(PackMlState.Idle);
            var element = new ActionShape();
            manager.ExecuteCommand(new AddElementCommand(workspace, element));
            manager.Undo();
            manager.CanUndo.Should().BeFalse();

            // Act
            manager.Redo();

            // Assert
            manager.CanUndo.Should().BeTrue();
        }

        [Fact]
        public void Redo_ReturnsFalseWhenStackEmpty()
        {
            // Arrange
            var manager = new EditHistoryManager();

            // Act
            var result = manager.Redo();

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region CanUndo/CanRedo Tests

        [Fact]
        public void CanUndo_ReturnsTrueWhenUndoStackHasCommands()
        {
            // Arrange
            var manager = new EditHistoryManager();
            var workspace = new StateWorkspace(PackMlState.Idle);
            manager.ExecuteCommand(new AddElementCommand(workspace, new ActionShape()));

            // Assert
            manager.CanUndo.Should().BeTrue();
        }

        [Fact]
        public void CanRedo_ReturnsTrueWhenRedoStackHasCommands()
        {
            // Arrange
            var manager = new EditHistoryManager();
            var workspace = new StateWorkspace(PackMlState.Idle);
            manager.ExecuteCommand(new AddElementCommand(workspace, new ActionShape()));
            manager.Undo();

            // Assert
            manager.CanRedo.Should().BeTrue();
        }

        #endregion

        #region Clear Tests

        [Fact]
        public void Clear_RemovesAllCommandsFromBothStacks()
        {
            // Arrange
            var manager = new EditHistoryManager();
            var workspace = new StateWorkspace(PackMlState.Idle);
            manager.ExecuteCommand(new AddElementCommand(workspace, new ActionShape()));
            manager.ExecuteCommand(new AddElementCommand(workspace, new ActionShape()));
            manager.Undo();

            // Act
            manager.Clear();

            // Assert
            manager.CanUndo.Should().BeFalse();
            manager.CanRedo.Should().BeFalse();
        }

        #endregion

        #region Rapid Sequential Operations Tests

        [Fact]
        public void HandleRapidSequentialOperations_Correctly()
        {
            // Arrange
            var manager = new EditHistoryManager();
            var workspace = new StateWorkspace(PackMlState.Idle);

            // Act - Rapid add, undo, redo operations
            for (int i = 0; i < 10; i++)
            {
                manager.ExecuteCommand(new AddElementCommand(workspace, new ActionShape()));
            }

            for (int i = 0; i < 5; i++)
            {
                manager.Undo();
            }

            for (int i = 0; i < 3; i++)
            {
                manager.Redo();
            }

            // Assert
            workspace.Elements.Should().HaveCount(8); // 10 - 5 + 3 = 8
        }

        #endregion
    }
}
