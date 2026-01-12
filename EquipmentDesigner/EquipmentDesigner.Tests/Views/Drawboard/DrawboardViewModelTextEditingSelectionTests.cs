using System.Linq;
using FluentAssertions;
using EquipmentDesigner.Models;
using EquipmentDesigner.ViewModels;
using Xunit;

namespace EquipmentDesigner.Tests.Views.Drawboard
{
    /// <summary>
    /// TDD tests for text editing selection behavior.
    /// Tests verify that selection (single and multi) is cleared before entering text edit mode.
    /// This ensures adorners are removed before the text editing TextBox is displayed.
    /// </summary>
    public class DrawboardViewModelTextEditingSelectionTests
    {
        #region Test Helpers

        private static DrawboardViewModel CreateViewModel()
        {
            return new DrawboardViewModel(showBackButton: false);
        }

        private static ActionElement CreateActionElement(
            double x = 100, double y = 100, double width = 100, double height = 50, string text = "Test")
        {
            return new ActionElement
            {
                X = x,
                Y = y,
                Width = width,
                Height = height,
                Text = text,
                ZIndex = 1
            };
        }

        private static TextboxElement CreateTextboxElement(
            double x = 100, double y = 100, double width = 150, double height = 30, string text = "Test")
        {
            return new TextboxElement
            {
                X = x,
                Y = y,
                Width = width,
                Height = height,
                Text = text,
                ZIndex = 1
            };
        }

        #endregion

        #region Single Selection Clearing Before Text Edit

        /// <summary>
        /// When starting text editing on a selected element,
        /// the selection should be cleared first to remove the adorner.
        /// </summary>
        [Fact]
        public void TryStartTextEditing_ClearsSingleSelectionBeforeEnteringTextEditMode()
        {
            // Arrange
            var viewModel = CreateViewModel();
            var element = CreateActionElement();
            viewModel.Elements.Add(element);
            viewModel.SelectElement(element);

            // Verify initial state
            viewModel.SelectedElement.Should().Be(element);
            element.IsSelected.Should().BeTrue();
            viewModel.EditModeState.Should().Be(EditModeState.Selected);

            // Act
            var result = viewModel.TryStartTextEditing(element);

            // Assert
            result.Should().BeTrue();
            viewModel.IsTextEditing.Should().BeTrue();
            viewModel.SelectedElement.Should().BeNull("Selection should be cleared before text editing");
            element.IsSelected.Should().BeFalse("IsSelected flag should be cleared");
            viewModel.EditModeState.Should().Be(EditModeState.TextEditing);
        }

        /// <summary>
        /// After committing text editing, EditModeState should transition to None
        /// since selection was cleared before text editing started.
        /// </summary>
        [Fact]
        public void CommitTextEditing_TransitionsToNoneState_AfterSelectionWasCleared()
        {
            // Arrange
            var viewModel = CreateViewModel();
            var element = CreateActionElement();
            viewModel.Elements.Add(element);
            viewModel.SelectElement(element);
            viewModel.TryStartTextEditing(element);

            // Act
            viewModel.CommitTextEditing("New Text");

            // Assert
            viewModel.EditModeState.Should().Be(EditModeState.None);
            viewModel.SelectedElement.Should().BeNull();
        }

        #endregion

        #region Multi-Selection Clearing Before Text Edit

        /// <summary>
        /// When starting text editing from multi-selection mode,
        /// all selections should be cleared first to remove the multi-selection adorner.
        /// </summary>
        [Fact]
        public void TryStartTextEditing_ClearsMultiSelectionBeforeEnteringTextEditMode()
        {
            // Arrange
            var viewModel = CreateViewModel();
            var elementA = CreateActionElement(100, 100);
            var elementB = CreateActionElement(300, 300);
            viewModel.Elements.Add(elementA);
            viewModel.Elements.Add(elementB);

            // Set up multi-selection
            viewModel.AddToSelection(elementA);
            viewModel.AddToSelection(elementB);

            // Verify initial multi-selection state
            viewModel.IsMultiSelectionMode.Should().BeTrue();
            viewModel.SelectedElements.Count.Should().Be(2);
            viewModel.EditModeState.Should().Be(EditModeState.MultiSelected);
            elementA.IsSelected.Should().BeTrue();
            elementB.IsSelected.Should().BeTrue();

            // Act - Start text editing on elementA (one of the selected elements)
            var result = viewModel.TryStartTextEditing(elementA);

            // Assert
            result.Should().BeTrue("Text editing should be allowed from multi-selection mode");
            viewModel.IsTextEditing.Should().BeTrue();
            viewModel.IsMultiSelectionMode.Should().BeFalse("Multi-selection should be cleared");
            viewModel.SelectedElements.Should().BeEmpty("All selected elements should be cleared");
            viewModel.SelectedElement.Should().BeNull("Single selection should also be null");
            elementA.IsSelected.Should().BeFalse("ElementA's IsSelected flag should be cleared");
            elementB.IsSelected.Should().BeFalse("ElementB's IsSelected flag should be cleared");
            viewModel.EditModeState.Should().Be(EditModeState.TextEditing);
        }

        /// <summary>
        /// When starting text editing by double-clicking on an element
        /// that is NOT part of the current multi-selection,
        /// all selections should still be cleared first.
        /// </summary>
        [Fact]
        public void TryStartTextEditing_ClearsMultiSelection_EvenWhenTargetElementNotInSelection()
        {
            // Arrange
            var viewModel = CreateViewModel();
            var elementA = CreateActionElement(100, 100);
            var elementB = CreateActionElement(300, 300);
            var elementC = CreateActionElement(500, 500);
            viewModel.Elements.Add(elementA);
            viewModel.Elements.Add(elementB);
            viewModel.Elements.Add(elementC);

            // Set up multi-selection with A and B
            viewModel.AddToSelection(elementA);
            viewModel.AddToSelection(elementB);
            viewModel.IsMultiSelectionMode.Should().BeTrue();

            // Act - Start text editing on elementC (NOT in the selection)
            var result = viewModel.TryStartTextEditing(elementC);

            // Assert
            result.Should().BeTrue();
            viewModel.IsMultiSelectionMode.Should().BeFalse();
            viewModel.SelectedElements.Should().BeEmpty();
            elementA.IsSelected.Should().BeFalse();
            elementB.IsSelected.Should().BeFalse();
            viewModel.TextEditingElement.Should().Be(elementC);
        }

        #endregion

        #region Text Editing State Transitions

        /// <summary>
        /// TryStartTextEditing should work from None state (no selection).
        /// </summary>
        [Fact]
        public void TryStartTextEditing_WorksFromNoneState()
        {
            // Arrange
            var viewModel = CreateViewModel();
            var element = CreateActionElement();
            viewModel.Elements.Add(element);

            // Verify initial state
            viewModel.EditModeState.Should().Be(EditModeState.None);
            viewModel.SelectedElement.Should().BeNull();

            // Act
            var result = viewModel.TryStartTextEditing(element);

            // Assert
            result.Should().BeTrue();
            viewModel.IsTextEditing.Should().BeTrue();
            viewModel.EditModeState.Should().Be(EditModeState.TextEditing);
        }

        /// <summary>
        /// TryStartTextEditing should reject locked elements.
        /// </summary>
        [Fact]
        public void TryStartTextEditing_RejectsLockedElement()
        {
            // Arrange
            var viewModel = CreateViewModel();
            var element = CreateActionElement();
            element.IsLocked = true;
            viewModel.Elements.Add(element);

            // Act
            var result = viewModel.TryStartTextEditing(element);

            // Assert
            result.Should().BeFalse();
            viewModel.IsTextEditing.Should().BeFalse();
        }

        /// <summary>
        /// TryStartTextEditing should reject null elements.
        /// </summary>
        [Fact]
        public void TryStartTextEditing_RejectsNullElement()
        {
            // Arrange
            var viewModel = CreateViewModel();

            // Act
            var result = viewModel.TryStartTextEditing(null);

            // Assert
            result.Should().BeFalse();
            viewModel.IsTextEditing.Should().BeFalse();
        }

        /// <summary>
        /// TryStartTextEditing should reject when in Moving state.
        /// </summary>
        [Fact]
        public void TryStartTextEditing_RejectsDuringMovingState()
        {
            // Arrange
            var viewModel = CreateViewModel();
            var element = CreateActionElement();
            viewModel.Elements.Add(element);
            viewModel.SelectElement(element);

            // Simulate moving state
            viewModel.StartMove(new System.Windows.Point(150, 150));
            viewModel.EditModeState.Should().Be(EditModeState.Moving);

            // Act
            var result = viewModel.TryStartTextEditing(element);

            // Assert
            result.Should().BeFalse();
            viewModel.IsTextEditing.Should().BeFalse();
        }

        #endregion

        #region TextEditingElement Reference

        /// <summary>
        /// After starting text editing, TextEditingElement should reference
        /// the element being edited, even though selection is cleared.
        /// </summary>
        [Fact]
        public void TryStartTextEditing_SetsTextEditingElementReference()
        {
            // Arrange
            var viewModel = CreateViewModel();
            var element = CreateActionElement();
            viewModel.Elements.Add(element);
            viewModel.SelectElement(element);

            // Act
            viewModel.TryStartTextEditing(element);

            // Assert
            viewModel.TextEditingElement.Should().Be(element);
            viewModel.SelectedElement.Should().BeNull("Selection cleared, but TextEditingElement is set");
        }

        /// <summary>
        /// After ending text editing, TextEditingElement should be null.
        /// </summary>
        [Fact]
        public void CommitTextEditing_ClearsTextEditingElementReference()
        {
            // Arrange
            var viewModel = CreateViewModel();
            var element = CreateActionElement();
            viewModel.Elements.Add(element);
            viewModel.SelectElement(element);
            viewModel.TryStartTextEditing(element);

            // Act
            viewModel.CommitTextEditing("New Text");

            // Assert
            viewModel.TextEditingElement.Should().BeNull();
        }

        #endregion

        #region Textbox Element Double-Click Scenario

        /// <summary>
        /// When double-clicking on a Textbox element with existing selection,
        /// the selection should be cleared and text editing should start.
        /// </summary>
        [Fact]
        public void TryStartTextEditing_OnTextboxElement_ClearsSelectionAndStartsEditing()
        {
            // Arrange
            var viewModel = CreateViewModel();
            var textbox = CreateTextboxElement(text: "Existing Text");
            viewModel.Elements.Add(textbox);
            viewModel.SelectElement(textbox);

            // Act
            var result = viewModel.TryStartTextEditing(textbox);

            // Assert
            result.Should().BeTrue();
            viewModel.IsTextEditing.Should().BeTrue();
            viewModel.SelectedElement.Should().BeNull();
            viewModel.TextEditingElement.Should().Be(textbox);
        }

        #endregion
    }
}
