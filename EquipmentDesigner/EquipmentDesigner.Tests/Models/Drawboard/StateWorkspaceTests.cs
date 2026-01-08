using EquipmentDesigner.Models;
using EquipmentDesigner.Models.Drawboard;
using FluentAssertions;
using System.Linq;
using Xunit;

namespace EquipmentDesigner.Tests.Models.Drawboard
{
    public class StateWorkspaceTests
    {
        #region State Property Tests

        [Fact]
        public void StateWorkspace_StoresPackMlState()
        {
            // Arrange & Act
            var workspace = new StateWorkspace(PackMlState.Execute);

            // Assert
            workspace.State.Should().Be(PackMlState.Execute);
        }

        #endregion

        #region Elements Collection Tests

        [Fact]
        public void StateWorkspace_ElementsCollection_IsInitializedEmpty()
        {
            // Arrange & Act
            var workspace = new StateWorkspace(PackMlState.Idle);

            // Assert
            workspace.Elements.Should().NotBeNull();
            workspace.Elements.Should().BeEmpty();
        }

        #endregion

        #region AddElement Tests

        [Fact]
        public void AddElement_AddsElementToCollection()
        {
            // Arrange
            var workspace = new StateWorkspace(PackMlState.Idle);
            var element = new ActionShape();

            // Act
            workspace.AddElement(element);

            // Assert
            workspace.Elements.Should().Contain(element);
        }

        [Fact]
        public void AddElement_AssignsNextZIndex()
        {
            // Arrange
            var workspace = new StateWorkspace(PackMlState.Idle);
            var element1 = new ActionShape();
            var element2 = new ActionShape();

            // Act
            workspace.AddElement(element1);
            workspace.AddElement(element2);

            // Assert
            element1.ZIndex.Should().Be(0);
            element2.ZIndex.Should().Be(1);
        }

        [Fact]
        public void AddElement_RejectsDuplicateId()
        {
            // Arrange
            var workspace = new StateWorkspace(PackMlState.Idle);
            var element1 = new ActionShape();
            var element2 = new ActionShape();

            // Force same Id for testing
            typeof(DrawingElement).GetProperty("Id")
                .SetValue(element2, element1.Id);

            // Act
            workspace.AddElement(element1);
            var result = workspace.AddElement(element2);

            // Assert
            result.Should().BeFalse();
            workspace.Elements.Should().HaveCount(1);
        }

        #endregion

        #region RemoveElement Tests

        [Fact]
        public void RemoveElement_RemovesElementById()
        {
            // Arrange
            var workspace = new StateWorkspace(PackMlState.Idle);
            var element = new ActionShape();
            workspace.AddElement(element);

            // Act
            var result = workspace.RemoveElement(element.Id);

            // Assert
            result.Should().BeTrue();
            workspace.Elements.Should().NotContain(element);
        }

        [Fact]
        public void RemoveElement_ReturnsFalseWhenNotFound()
        {
            // Arrange
            var workspace = new StateWorkspace(PackMlState.Idle);

            // Act
            var result = workspace.RemoveElement("non-existent-id");

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region GetElementById Tests

        [Fact]
        public void GetElementById_ReturnsElementWhenFound()
        {
            // Arrange
            var workspace = new StateWorkspace(PackMlState.Idle);
            var element = new ActionShape();
            workspace.AddElement(element);

            // Act
            var result = workspace.GetElementById(element.Id);

            // Assert
            result.Should().Be(element);
        }

        [Fact]
        public void GetElementById_ReturnsNullWhenNotFound()
        {
            // Arrange
            var workspace = new StateWorkspace(PackMlState.Idle);

            // Act
            var result = workspace.GetElementById("non-existent-id");

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region Clear Tests

        [Fact]
        public void Clear_RemovesAllElements()
        {
            // Arrange
            var workspace = new StateWorkspace(PackMlState.Idle);
            workspace.AddElement(new ActionShape());
            workspace.AddElement(new ActionShape());
            workspace.AddElement(new ActionShape());

            // Act
            workspace.Clear();

            // Assert
            workspace.Elements.Should().BeEmpty();
        }

        #endregion

        #region GetElementsOrderedByZIndex Tests

        [Fact]
        public void GetElementsOrderedByZIndex_ReturnsSortedAscending()
        {
            // Arrange
            var workspace = new StateWorkspace(PackMlState.Idle);
            var element1 = new ActionShape();
            var element2 = new ActionShape();
            var element3 = new ActionShape();

            workspace.AddElement(element1);
            workspace.AddElement(element2);
            workspace.AddElement(element3);

            // Modify ZIndex manually for test
            element1.ZIndex = 5;
            element2.ZIndex = 1;
            element3.ZIndex = 3;

            // Act
            var ordered = workspace.GetElementsOrderedByZIndex().ToList();

            // Assert
            ordered[0].Should().Be(element2); // ZIndex 1
            ordered[1].Should().Be(element3); // ZIndex 3
            ordered[2].Should().Be(element1); // ZIndex 5
        }

        #endregion
    }
}
