using EquipmentDesigner.Models;
using EquipmentDesigner.Models.Drawboard;
using FluentAssertions;
using System;
using Xunit;

namespace EquipmentDesigner.Tests.Models.Drawboard
{
    public class WorkspaceManagerTests
    {
        #region Initialization Tests

        [Fact]
        public void WorkspaceManager_InitializesAllPackMlStates()
        {
            // Arrange & Act
            var manager = new WorkspaceManager();

            // Assert - Should have all 17 PackML states
            foreach (PackMlState state in Enum.GetValues(typeof(PackMlState)))
            {
                manager.GetWorkspace(state).Should().NotBeNull();
            }
        }

        [Fact]
        public void WorkspaceManager_InitializesWithEmptyWorkspaces()
        {
            // Arrange & Act
            var manager = new WorkspaceManager();

            // Assert
            foreach (PackMlState state in Enum.GetValues(typeof(PackMlState)))
            {
                manager.GetWorkspace(state).Elements.Should().BeEmpty();
            }
        }

        #endregion

        #region CurrentState Tests

        [Fact]
        public void CurrentState_DefaultsToIdle()
        {
            // Arrange & Act
            var manager = new WorkspaceManager();

            // Assert
            manager.CurrentState.Should().Be(PackMlState.Idle);
        }

        [Fact]
        public void CurrentState_TracksActiveState()
        {
            // Arrange
            var manager = new WorkspaceManager();

            // Act
            manager.SwitchState(PackMlState.Execute);

            // Assert
            manager.CurrentState.Should().Be(PackMlState.Execute);
        }

        #endregion

        #region CurrentWorkspace Tests

        [Fact]
        public void CurrentWorkspace_ReturnsWorkspaceForCurrentState()
        {
            // Arrange
            var manager = new WorkspaceManager();

            // Act
            var workspace = manager.CurrentWorkspace;

            // Assert
            workspace.State.Should().Be(PackMlState.Idle);
        }

        [Fact]
        public void CurrentWorkspace_UpdatesAfterStateSwitch()
        {
            // Arrange
            var manager = new WorkspaceManager();

            // Act
            manager.SwitchState(PackMlState.Starting);

            // Assert
            manager.CurrentWorkspace.State.Should().Be(PackMlState.Starting);
        }

        #endregion

        #region SwitchState Tests

        [Fact]
        public void SwitchState_ChangesCurrentState()
        {
            // Arrange
            var manager = new WorkspaceManager();

            // Act
            manager.SwitchState(PackMlState.Complete);

            // Assert
            manager.CurrentState.Should().Be(PackMlState.Complete);
        }

        [Fact]
        public void SwitchState_ReturnsNewWorkspace()
        {
            // Arrange
            var manager = new WorkspaceManager();

            // Act
            var workspace = manager.SwitchState(PackMlState.Aborting);

            // Assert
            workspace.State.Should().Be(PackMlState.Aborting);
        }

        [Fact]
        public void SwitchState_PreservesElementsInPreviousState()
        {
            // Arrange
            var manager = new WorkspaceManager();
            var element = new ActionShape();
            manager.CurrentWorkspace.AddElement(element);

            // Act
            manager.SwitchState(PackMlState.Execute);
            manager.SwitchState(PackMlState.Idle);

            // Assert
            manager.CurrentWorkspace.Elements.Should().Contain(element);
        }

        #endregion

        #region GetWorkspace Tests

        [Fact]
        public void GetWorkspace_ReturnsCorrectWorkspace()
        {
            // Arrange
            var manager = new WorkspaceManager();
            var element = new ActionShape();
            manager.GetWorkspace(PackMlState.Held).AddElement(element);

            // Act
            var workspace = manager.GetWorkspace(PackMlState.Held);

            // Assert
            workspace.State.Should().Be(PackMlState.Held);
            workspace.Elements.Should().Contain(element);
        }

        #endregion
    }
}
