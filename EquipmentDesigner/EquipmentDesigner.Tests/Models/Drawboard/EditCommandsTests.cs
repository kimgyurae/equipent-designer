using EquipmentDesigner.Models;
using EquipmentDesigner.Models.Drawboard;
using EquipmentDesigner.Models.Drawboard.Commands;
using FluentAssertions;
using System;
using Xunit;

namespace EquipmentDesigner.Tests.Models.Drawboard
{
    public class EditCommandsTests
    {
        #region IEditCommand Interface Tests

        [Fact]
        public void AddElementCommand_ImplementsIEditCommand()
        {
            // Arrange & Act
            var workspace = new StateWorkspace(PackMlState.Idle);
            var element = new ActionShape();
            var command = new AddElementCommand(workspace, element);

            // Assert
            command.Should().BeAssignableTo<IEditCommand>();
        }

        #endregion

        #region AddElementCommand Tests

        [Fact]
        public void AddElementCommand_Execute_AddsElementToWorkspace()
        {
            // Arrange
            var workspace = new StateWorkspace(PackMlState.Idle);
            var element = new ActionShape();
            var command = new AddElementCommand(workspace, element);

            // Act
            command.Execute();

            // Assert
            workspace.Elements.Should().Contain(element);
        }

        [Fact]
        public void AddElementCommand_Undo_RemovesElementFromWorkspace()
        {
            // Arrange
            var workspace = new StateWorkspace(PackMlState.Idle);
            var element = new ActionShape();
            var command = new AddElementCommand(workspace, element);
            command.Execute();

            // Act
            command.Undo();

            // Assert
            workspace.Elements.Should().NotContain(element);
        }

        [Fact]
        public void AddElementCommand_Description_ReturnsAddShapeType()
        {
            // Arrange
            var workspace = new StateWorkspace(PackMlState.Idle);
            var element = new ActionShape();
            var command = new AddElementCommand(workspace, element);

            // Assert
            command.Description.Should().Be("Add Action");
        }

        #endregion

        #region DeleteElementCommand Tests

        [Fact]
        public void DeleteElementCommand_Execute_RemovesElementFromWorkspace()
        {
            // Arrange
            var workspace = new StateWorkspace(PackMlState.Idle);
            var element = new ActionShape();
            workspace.AddElement(element);
            var command = new DeleteElementCommand(workspace, element.Id);

            // Act
            command.Execute();

            // Assert
            workspace.Elements.Should().NotContain(element);
        }

        [Fact]
        public void DeleteElementCommand_Execute_ThrowsWhenElementNotFound()
        {
            // Arrange
            var workspace = new StateWorkspace(PackMlState.Idle);
            var command = new DeleteElementCommand(workspace, "non-existent-id");

            // Act & Assert
            command.Invoking(c => c.Execute())
                .Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void DeleteElementCommand_Undo_RestoresElementToWorkspace()
        {
            // Arrange
            var workspace = new StateWorkspace(PackMlState.Idle);
            var element = new ActionShape { ZIndex = 5 };
            workspace.AddElement(element);
            var originalZIndex = element.ZIndex;
            var command = new DeleteElementCommand(workspace, element.Id);
            command.Execute();

            // Act
            command.Undo();

            // Assert
            var restored = workspace.GetElementById(element.Id);
            restored.Should().NotBeNull();
            restored.ZIndex.Should().Be(originalZIndex);
        }

        [Fact]
        public void DeleteElementCommand_Description_ReturnsDeleteShapeType()
        {
            // Arrange
            var workspace = new StateWorkspace(PackMlState.Idle);
            var element = new DecisionShape();
            workspace.AddElement(element);
            var command = new DeleteElementCommand(workspace, element.Id);

            // Assert
            command.Description.Should().Be("Delete Decision");
        }

        #endregion

        #region MoveElementCommand Tests

        [Fact]
        public void MoveElementCommand_Execute_UpdatesPosition()
        {
            // Arrange
            var workspace = new StateWorkspace(PackMlState.Idle);
            var element = new ActionShape { X = 10, Y = 20 };
            workspace.AddElement(element);
            var command = new MoveElementCommand(workspace, element.Id, 10, 20, 100, 200);

            // Act
            command.Execute();

            // Assert
            element.X.Should().Be(100);
            element.Y.Should().Be(200);
        }

        [Fact]
        public void MoveElementCommand_Execute_ThrowsWhenElementNotFound()
        {
            // Arrange
            var workspace = new StateWorkspace(PackMlState.Idle);
            var command = new MoveElementCommand(workspace, "non-existent-id", 0, 0, 100, 200);

            // Act & Assert
            command.Invoking(c => c.Execute())
                .Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void MoveElementCommand_Undo_RestoresOriginalPosition()
        {
            // Arrange
            var workspace = new StateWorkspace(PackMlState.Idle);
            var element = new ActionShape { X = 10, Y = 20 };
            workspace.AddElement(element);
            var command = new MoveElementCommand(workspace, element.Id, 10, 20, 100, 200);
            command.Execute();

            // Act
            command.Undo();

            // Assert
            element.X.Should().Be(10);
            element.Y.Should().Be(20);
        }

        [Fact]
        public void MoveElementCommand_Description_ReturnsMoveShapeType()
        {
            // Arrange
            var workspace = new StateWorkspace(PackMlState.Idle);
            var element = new InitialShape();
            workspace.AddElement(element);
            var command = new MoveElementCommand(workspace, element.Id, 0, 0, 100, 100);

            // Assert
            command.Description.Should().Be("Move Initial");
        }

        #endregion

        #region ResizeElementCommand Tests

        [Fact]
        public void ResizeElementCommand_Execute_UpdatesSize()
        {
            // Arrange
            var workspace = new StateWorkspace(PackMlState.Idle);
            var element = new ActionShape { Width = 50, Height = 30 };
            workspace.AddElement(element);
            var command = new ResizeElementCommand(workspace, element.Id, 50, 30, 100, 80);

            // Act
            command.Execute();

            // Assert
            element.Width.Should().Be(100);
            element.Height.Should().Be(80);
        }

        [Fact]
        public void ResizeElementCommand_Execute_ThrowsWhenElementNotFound()
        {
            // Arrange
            var workspace = new StateWorkspace(PackMlState.Idle);
            var command = new ResizeElementCommand(workspace, "non-existent-id", 50, 30, 100, 80);

            // Act & Assert
            command.Invoking(c => c.Execute())
                .Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void ResizeElementCommand_Undo_RestoresOriginalSize()
        {
            // Arrange
            var workspace = new StateWorkspace(PackMlState.Idle);
            var element = new ActionShape { Width = 50, Height = 30 };
            workspace.AddElement(element);
            var command = new ResizeElementCommand(workspace, element.Id, 50, 30, 100, 80);
            command.Execute();

            // Act
            command.Undo();

            // Assert
            element.Width.Should().Be(50);
            element.Height.Should().Be(30);
        }

        [Fact]
        public void ResizeElementCommand_Description_ReturnsResizeShapeType()
        {
            // Arrange
            var workspace = new StateWorkspace(PackMlState.Idle);
            var element = new TerminalShape();
            workspace.AddElement(element);
            var command = new ResizeElementCommand(workspace, element.Id, 50, 30, 100, 80);

            // Assert
            command.Description.Should().Be("Resize Terminal");
        }

        #endregion
    }
}
