using System.Linq;
using FluentAssertions;
using EquipmentDesigner.Models;
using EquipmentDesigner.ViewModels;
using Xunit;

namespace EquipmentDesigner.Tests.Views.Drawboard
{
    /// <summary>
    /// TDD tests for DrawboardViewModel multi-selection operations.
    /// Tests cover Shift+Click behavior, selection state transitions, and edge cases.
    /// </summary>
    public class DrawboardViewModelSelectionTests
    {
        #region Test Helpers

        private static DrawboardViewModel CreateViewModel()
        {
            return new DrawboardViewModel("test-process-id", showBackButton: false);
        }

        private static SelectionTestElement CreateTestElement(
            double x = 100, double y = 100, double width = 100, double height = 100, int zIndex = 1)
        {
            return new SelectionTestElement
            {
                X = x,
                Y = y,
                Width = width,
                Height = height,
                ZIndex = zIndex
            };
        }

        private static DrawboardViewModel CreateViewModelWithTwoElements()
        {
            var viewModel = CreateViewModel();
            viewModel.CurrentSteps.Add(CreateTestElement(100, 100)); // Element A
            viewModel.CurrentSteps.Add(CreateTestElement(300, 300)); // Element B
            return viewModel;
        }

        #endregion

        #region API-Level Tests - Multi-Selection Shift+Click Scenario

        /// <summary>
        /// API-Level Test: Reproduces the bug scenario from user report.
        /// Bug: When shape A is selected without Shift, then user holds Shift and clicks shape B,
        /// shape B should be ADDED to selection (multi-select), not replace shape A.
        /// </summary>
        [Fact]
        public void ShiftClick_WithExistingSingleSelection_ShouldAddToMultiSelection()
        {
            // Arrange - Shape A is already selected (without Shift)
            var viewModel = CreateViewModelWithTwoElements();
            var elementA = viewModel.CurrentSteps[0];
            var elementB = viewModel.CurrentSteps[1];

            // User clicks element A without Shift - single selection
            viewModel.SelectElement(elementA);
            viewModel.SelectedElement.Should().Be(elementA);
            viewModel.IsMultiSelectionMode.Should().BeFalse();

            // Act - User holds Shift and clicks element B
            // This simulates what SHOULD happen when HandleSelectionClick receives isShiftPressed=true
            viewModel.AddToSelection(elementB);

            // Assert - Both elements should be selected (multi-selection)
            viewModel.IsMultiSelectionMode.Should().BeTrue();
            viewModel.SelectedElements.Should().Contain(elementA);
            viewModel.SelectedElements.Should().Contain(elementB);
            viewModel.SelectedElements.Count.Should().Be(2);
            elementA.IsSelected.Should().BeTrue();
            elementB.IsSelected.Should().BeTrue();
        }

        /// <summary>
        /// API-Level Test: Verifies that selecting without Shift replaces existing selection.
        /// This is the expected behavior when NOT holding Shift.
        /// </summary>
        [Fact]
        public void Click_WithExistingSingleSelection_WithoutShift_ShouldReplaceSingleSelection()
        {
            // Arrange - Shape A is already selected
            var viewModel = CreateViewModelWithTwoElements();
            var elementA = viewModel.CurrentSteps[0];
            var elementB = viewModel.CurrentSteps[1];

            viewModel.SelectElement(elementA);
            viewModel.SelectedElement.Should().Be(elementA);

            // Act - User clicks element B without Shift
            viewModel.SelectElement(elementB);

            // Assert - Only element B should be selected
            viewModel.SelectedElement.Should().Be(elementB);
            viewModel.IsMultiSelectionMode.Should().BeFalse();
            elementA.IsSelected.Should().BeFalse();
            elementB.IsSelected.Should().BeTrue();
        }

        #endregion

        #region Multi-Selection Toggle Behavior

        [Fact]
        public void AddToSelection_AddsElementToExistingSelectionWithoutClearing()
        {
            // Arrange
            var viewModel = CreateViewModelWithTwoElements();
            var elementA = viewModel.CurrentSteps[0];
            var elementB = viewModel.CurrentSteps[1];
            viewModel.SelectElement(elementA);

            // Act
            viewModel.AddToSelection(elementB);

            // Assert
            viewModel.SelectedElements.Should().Contain(elementA);
            viewModel.SelectedElements.Should().Contain(elementB);
        }

        [Fact]
        public void AddToSelection_WithSingleExistingSelection_TransitionsToMultiSelectionMode()
        {
            // Arrange
            var viewModel = CreateViewModelWithTwoElements();
            var elementA = viewModel.CurrentSteps[0];
            var elementB = viewModel.CurrentSteps[1];
            viewModel.SelectElement(elementA);
            viewModel.IsMultiSelectionMode.Should().BeFalse();

            // Act
            viewModel.AddToSelection(elementB);

            // Assert
            viewModel.IsMultiSelectionMode.Should().BeTrue();
            viewModel.EditModeState.Should().Be(EditModeState.MultiSelected);
        }

        [Fact]
        public void AddToSelection_WithMultipleExistingSelections_MaintainsMultiSelectionMode()
        {
            // Arrange
            var viewModel = CreateViewModel();
            var elementA = CreateTestElement(100, 100);
            var elementB = CreateTestElement(200, 200);
            var elementC = CreateTestElement(300, 300);
            viewModel.CurrentSteps.Add(elementA);
            viewModel.CurrentSteps.Add(elementB);
            viewModel.CurrentSteps.Add(elementC);

            viewModel.AddToSelection(elementA);
            viewModel.AddToSelection(elementB);
            viewModel.IsMultiSelectionMode.Should().BeTrue();

            // Act
            viewModel.AddToSelection(elementC);

            // Assert
            viewModel.IsMultiSelectionMode.Should().BeTrue();
            viewModel.SelectedElements.Count.Should().Be(3);
        }

        [Fact]
        public void AddToSelection_SetsEditModeStateToMultiSelected_WhenCountExceedsOne()
        {
            // Arrange
            var viewModel = CreateViewModelWithTwoElements();
            var elementA = viewModel.CurrentSteps[0];
            var elementB = viewModel.CurrentSteps[1];
            viewModel.AddToSelection(elementA);
            viewModel.EditModeState.Should().Be(EditModeState.Selected);

            // Act
            viewModel.AddToSelection(elementB);

            // Assert
            viewModel.EditModeState.Should().Be(EditModeState.MultiSelected);
        }

        [Fact]
        public void AddToSelection_DoesNotAddDuplicateElements()
        {
            // Arrange
            var viewModel = CreateViewModelWithTwoElements();
            var elementA = viewModel.CurrentSteps[0];
            viewModel.AddToSelection(elementA);
            var initialCount = viewModel.SelectedElements.Count;

            // Act
            viewModel.AddToSelection(elementA); // Try to add same element again

            // Assert
            viewModel.SelectedElements.Count.Should().Be(initialCount);
        }

        #endregion

        #region Selection State Consistency

        [Fact]
        public void SelectElement_WithoutShift_ClearsAllPreviousSelections()
        {
            // Arrange
            var viewModel = CreateViewModel();
            var elementA = CreateTestElement(100, 100);
            var elementB = CreateTestElement(200, 200);
            var elementC = CreateTestElement(300, 300);
            viewModel.CurrentSteps.Add(elementA);
            viewModel.CurrentSteps.Add(elementB);
            viewModel.CurrentSteps.Add(elementC);

            // Start with multi-selection
            viewModel.AddToSelection(elementA);
            viewModel.AddToSelection(elementB);
            viewModel.IsMultiSelectionMode.Should().BeTrue();

            // Act - Select element C without Shift (should clear previous selection)
            viewModel.ClearAllSelections();
            viewModel.SelectElement(elementC);

            // Assert
            viewModel.SelectedElement.Should().Be(elementC);
            viewModel.IsMultiSelectionMode.Should().BeFalse();
            elementA.IsSelected.Should().BeFalse();
            elementB.IsSelected.Should().BeFalse();
            elementC.IsSelected.Should().BeTrue();
        }

        [Fact]
        public void ToggleSelection_WithExistingSingleSelection_AddsElementAndTransitionsToMultiSelect()
        {
            // Arrange
            var viewModel = CreateViewModelWithTwoElements();
            var elementA = viewModel.CurrentSteps[0];
            var elementB = viewModel.CurrentSteps[1];
            viewModel.ToggleSelection(elementA);
            viewModel.SelectedElements.Count.Should().Be(1);

            // Act
            viewModel.ToggleSelection(elementB);

            // Assert
            viewModel.IsMultiSelectionMode.Should().BeTrue();
            viewModel.SelectedElements.Count.Should().Be(2);
        }

        [Fact]
        public void ToggleSelection_RemovesElement_WhenAlreadySelected()
        {
            // Arrange
            var viewModel = CreateViewModelWithTwoElements();
            var elementA = viewModel.CurrentSteps[0];
            var elementB = viewModel.CurrentSteps[1];
            viewModel.ToggleSelection(elementA);
            viewModel.ToggleSelection(elementB);
            viewModel.SelectedElements.Count.Should().Be(2);

            // Act
            viewModel.ToggleSelection(elementA);

            // Assert
            viewModel.SelectedElements.Should().NotContain(elementA);
            viewModel.SelectedElements.Should().Contain(elementB);
            elementA.IsSelected.Should().BeFalse();
        }

        [Fact]
        public void ToggleSelection_TransitionsToSingleSelectionMode_WhenReturningToOneElement()
        {
            // Arrange
            var viewModel = CreateViewModelWithTwoElements();
            var elementA = viewModel.CurrentSteps[0];
            var elementB = viewModel.CurrentSteps[1];
            viewModel.ToggleSelection(elementA);
            viewModel.ToggleSelection(elementB);
            viewModel.IsMultiSelectionMode.Should().BeTrue();

            // Act - Remove element B from selection
            viewModel.ToggleSelection(elementB);

            // Assert
            viewModel.IsMultiSelectionMode.Should().BeFalse();
            viewModel.SelectedElement.Should().Be(elementA);
            viewModel.EditModeState.Should().Be(EditModeState.Selected);
        }

        [Fact]
        public void ClearAllSelections_ResetsAllSelectionStates()
        {
            // Arrange
            var viewModel = CreateViewModelWithTwoElements();
            var elementA = viewModel.CurrentSteps[0];
            var elementB = viewModel.CurrentSteps[1];
            viewModel.AddToSelection(elementA);
            viewModel.AddToSelection(elementB);

            // Act
            viewModel.ClearAllSelections();

            // Assert
            viewModel.SelectedElement.Should().BeNull();
            viewModel.SelectedElements.Should().BeEmpty();
            viewModel.IsMultiSelectionMode.Should().BeFalse();
            viewModel.EditModeState.Should().Be(EditModeState.None);
            elementA.IsSelected.Should().BeFalse();
            elementB.IsSelected.Should().BeFalse();
        }

        #endregion

        #region Multi-Selection Mode Transitions

        [Fact]
        public void SingleToMultiSelection_PreservesFirstSelectedElement()
        {
            // Arrange
            var viewModel = CreateViewModelWithTwoElements();
            var elementA = viewModel.CurrentSteps[0];
            var elementB = viewModel.CurrentSteps[1];
            viewModel.SelectElement(elementA);

            // Act
            viewModel.AddToSelection(elementB);

            // Assert
            viewModel.SelectedElements.Should().Contain(elementA);
            viewModel.SelectedElements.First().Should().Be(elementA);
        }

        [Fact]
        public void MultiToSingleSelection_ViaToggle_UpdatesSelectedElementCorrectly()
        {
            // Arrange
            var viewModel = CreateViewModelWithTwoElements();
            var elementA = viewModel.CurrentSteps[0];
            var elementB = viewModel.CurrentSteps[1];
            viewModel.ToggleSelection(elementA);
            viewModel.ToggleSelection(elementB);

            // Act - Toggle off element A
            viewModel.ToggleSelection(elementA);

            // Assert
            viewModel.SelectedElement.Should().Be(elementB);
        }

        [Fact]
        public void EditModeState_ReflectsSelectedVsMultiSelected_BasedOnSelectionCount()
        {
            // Arrange
            var viewModel = CreateViewModelWithTwoElements();
            var elementA = viewModel.CurrentSteps[0];
            var elementB = viewModel.CurrentSteps[1];

            // Act & Assert - No selection
            viewModel.EditModeState.Should().Be(EditModeState.None);

            // Act & Assert - Single selection
            viewModel.AddToSelection(elementA);
            viewModel.EditModeState.Should().Be(EditModeState.Selected);

            // Act & Assert - Multi selection
            viewModel.AddToSelection(elementB);
            viewModel.EditModeState.Should().Be(EditModeState.MultiSelected);
        }

        [Fact]
        public void IsMultiSelectionMode_ReturnsTrue_OnlyWhenSelectedElementsCountIsGreaterOrEqualToTwo()
        {
            // Arrange
            var viewModel = CreateViewModelWithTwoElements();
            var elementA = viewModel.CurrentSteps[0];
            var elementB = viewModel.CurrentSteps[1];

            // Assert - No selection
            viewModel.IsMultiSelectionMode.Should().BeFalse();

            // Assert - Single selection
            viewModel.AddToSelection(elementA);
            viewModel.IsMultiSelectionMode.Should().BeFalse();

            // Assert - Two elements
            viewModel.AddToSelection(elementB);
            viewModel.IsMultiSelectionMode.Should().BeTrue();
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void AddToSelection_IgnoresNullElements()
        {
            // Arrange
            var viewModel = CreateViewModel();
            var initialCount = viewModel.SelectedElements.Count;

            // Act
            viewModel.AddToSelection(null);

            // Assert
            viewModel.SelectedElements.Count.Should().Be(initialCount);
        }

        [Fact]
        public void AddToSelection_IgnoresLockedElements()
        {
            // Arrange
            var viewModel = CreateViewModel();
            var lockedElement = CreateTestElement();
            lockedElement.IsLocked = true;
            viewModel.CurrentSteps.Add(lockedElement);

            // Act
            viewModel.AddToSelection(lockedElement);

            // Assert
            viewModel.SelectedElements.Should().NotContain(lockedElement);
        }

        [Fact]
        public void ToggleSelection_IgnoresNullElements()
        {
            // Arrange
            var viewModel = CreateViewModelWithTwoElements();
            var elementA = viewModel.CurrentSteps[0];
            viewModel.ToggleSelection(elementA);
            var initialCount = viewModel.SelectedElements.Count;

            // Act
            viewModel.ToggleSelection(null);

            // Assert
            viewModel.SelectedElements.Count.Should().Be(initialCount);
        }

        [Fact]
        public void ToggleSelection_IgnoresLockedElements()
        {
            // Arrange
            var viewModel = CreateViewModel();
            var lockedElement = CreateTestElement();
            lockedElement.IsLocked = true;
            viewModel.CurrentSteps.Add(lockedElement);

            // Act
            viewModel.ToggleSelection(lockedElement);

            // Assert
            viewModel.SelectedElements.Should().NotContain(lockedElement);
        }

        [Fact]
        public void SelectElement_IgnoresLockedElements()
        {
            // Arrange
            var viewModel = CreateViewModel();
            var lockedElement = CreateTestElement();
            lockedElement.IsLocked = true;
            viewModel.CurrentSteps.Add(lockedElement);

            // Act
            viewModel.SelectElement(lockedElement);

            // Assert
            viewModel.SelectedElement.Should().BeNull();
        }

        #endregion
    }

    /// <summary>
    /// Concrete DrawingElement implementation for selection testing.
    /// </summary>
    internal class SelectionTestElement : DrawingElement
    {
        public override DrawingShapeType ShapeType => DrawingShapeType.Action;

        public override int IncomingArrowCount => IncomingSourceIds.Count;
        public override int OutgoingArrowCount => OutgoingArrows.Count;
    }
}