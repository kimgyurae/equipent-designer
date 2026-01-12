using System.Linq;
using System.Windows;
using FluentAssertions;
using EquipmentDesigner.Models;
using EquipmentDesigner.ViewModels;
using Xunit;

namespace EquipmentDesigner.Tests.Views.Drawboard
{
    /// <summary>
    /// TDD tests for TextboxElement auto-deletion feature.
    /// Tests verify that TextboxElement is automatically removed from canvas when text content is empty.
    /// </summary>
    public class DrawboardViewModelTextboxAutoDeleteTests
    {
        #region Test Helpers

        private static DrawboardViewModel CreateViewModel()
        {
            return new DrawboardViewModel(showBackButton: false);
        }

        private static TextboxElement CreateTextboxElement(
            double x = 100, double y = 100, double width = 150, double height = 30, string text = "")
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

        private static ActionElement CreateActionElement(
            double x = 100, double y = 100, double width = 100, double height = 50, string text = "")
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

        #endregion

        #region Core Deletion Logic

        /// <summary>
        /// When CommitTextEditing is called with an empty string on a TextboxElement,
        /// the element should be automatically removed from the Elements collection.
        /// </summary>
        [Fact]
        public void CommitTextEditing_WithEmptyText_RemovesTextboxElementFromElements()
        {
            // Arrange
            var viewModel = CreateViewModel();
            var textbox = CreateTextboxElement(text: "");
            viewModel.Elements.Add(textbox);

            // Select and start text editing
            viewModel.SelectElement(textbox);
            viewModel.TryStartTextEditing(textbox);
            viewModel.IsTextEditing.Should().BeTrue();
            viewModel.Elements.Should().Contain(textbox);

            // Act - Commit with empty text
            viewModel.CommitTextEditing("");

            // Assert
            viewModel.Elements.Should().NotContain(textbox);
            viewModel.IsTextEditing.Should().BeFalse();
        }

        /// <summary>
        /// When CommitTextEditing is called with whitespace-only string on a TextboxElement,
        /// the element should be automatically removed from the Elements collection.
        /// </summary>
        [Fact]
        public void CommitTextEditing_WithWhitespaceOnlyText_RemovesTextboxElementFromElements()
        {
            // Arrange
            var viewModel = CreateViewModel();
            var textbox = CreateTextboxElement(text: "");
            viewModel.Elements.Add(textbox);
            viewModel.SelectElement(textbox);
            viewModel.TryStartTextEditing(textbox);

            // Act - Commit with whitespace-only text
            viewModel.CommitTextEditing("   ");

            // Assert
            viewModel.Elements.Should().NotContain(textbox);
        }

        /// <summary>
        /// CommitTextEditing should clear SelectedElement when auto-deleting the selected TextboxElement.
        /// Note: Selection is already cleared when entering text edit mode (to remove adorner),
        /// so this verifies that SelectedElement remains null after auto-deletion.
        /// </summary>
        [Fact]
        public void CommitTextEditing_WithEmptyText_ClearsSelectedElement()
        {
            // Arrange
            var viewModel = CreateViewModel();
            var textbox = CreateTextboxElement(text: "");
            viewModel.Elements.Add(textbox);
            viewModel.SelectElement(textbox);
            viewModel.TryStartTextEditing(textbox);
            // Selection is cleared when entering text edit mode (to remove adorner)
            viewModel.SelectedElement.Should().BeNull();

            // Act
            viewModel.CommitTextEditing("");

            // Assert
            viewModel.SelectedElement.Should().BeNull();
        }

        /// <summary>
        /// CommitTextEditing should NOT remove non-Textbox elements even with empty text.
        /// Other element types (Action, Decision, etc.) should retain empty text.
        /// </summary>
        [Fact]
        public void CommitTextEditing_WithEmptyText_DoesNotRemoveNonTextboxElements()
        {
            // Arrange
            var viewModel = CreateViewModel();
            var actionElement = CreateActionElement(text: "Initial");
            viewModel.Elements.Add(actionElement);
            viewModel.SelectElement(actionElement);
            viewModel.TryStartTextEditing(actionElement);

            // Act - Commit with empty text
            viewModel.CommitTextEditing("");

            // Assert - ActionElement should still exist
            viewModel.Elements.Should().Contain(actionElement);
            actionElement.Text.Should().Be("");
        }

        #endregion

        #region Text Content Validation

        /// <summary>
        /// Non-empty text should preserve the TextboxElement in the Elements collection.
        /// </summary>
        [Fact]
        public void CommitTextEditing_WithNonEmptyText_PreservesTextboxElement()
        {
            // Arrange
            var viewModel = CreateViewModel();
            var textbox = CreateTextboxElement(text: "");
            viewModel.Elements.Add(textbox);
            viewModel.SelectElement(textbox);
            viewModel.TryStartTextEditing(textbox);

            // Act - Commit with non-empty text
            viewModel.CommitTextEditing("Hello World");

            // Assert
            viewModel.Elements.Should().Contain(textbox);
            textbox.Text.Should().Be("Hello World");
        }

        /// <summary>
        /// Single character text should preserve the TextboxElement.
        /// </summary>
        [Fact]
        public void CommitTextEditing_WithSingleCharacter_PreservesTextboxElement()
        {
            // Arrange
            var viewModel = CreateViewModel();
            var textbox = CreateTextboxElement(text: "");
            viewModel.Elements.Add(textbox);
            viewModel.SelectElement(textbox);
            viewModel.TryStartTextEditing(textbox);

            // Act - Commit with single character
            viewModel.CommitTextEditing("A");

            // Assert
            viewModel.Elements.Should().Contain(textbox);
            textbox.Text.Should().Be("A");
        }

        #endregion

        #region Selection State Management

        /// <summary>
        /// After auto-deletion, EditModeState should transition to None.
        /// </summary>
        [Fact]
        public void CommitTextEditing_WithEmptyText_TransitionsEditModeStateToNone()
        {
            // Arrange
            var viewModel = CreateViewModel();
            var textbox = CreateTextboxElement(text: "");
            viewModel.Elements.Add(textbox);
            viewModel.SelectElement(textbox);
            viewModel.TryStartTextEditing(textbox);
            viewModel.EditModeState.Should().Be(EditModeState.TextEditing);

            // Act
            viewModel.CommitTextEditing("");

            // Assert
            viewModel.EditModeState.Should().Be(EditModeState.None);
        }

        /// <summary>
        /// IsSelected property should be set to false on auto-deleted element.
        /// Note: IsSelected is already cleared when entering text edit mode (to remove adorner),
        /// so this verifies that IsSelected remains false after auto-deletion.
        /// </summary>
        [Fact]
        public void CommitTextEditing_WithEmptyText_SetsIsSelectedToFalse()
        {
            // Arrange
            var viewModel = CreateViewModel();
            var textbox = CreateTextboxElement(text: "");
            viewModel.Elements.Add(textbox);
            viewModel.SelectElement(textbox);
            viewModel.TryStartTextEditing(textbox);
            // IsSelected is cleared when entering text edit mode (to remove adorner)
            textbox.IsSelected.Should().BeFalse();

            // Act
            viewModel.CommitTextEditing("");

            // Assert
            textbox.IsSelected.Should().BeFalse();
        }

        #endregion

        #region Creation via Double-Click Scenario

        /// <summary>
        /// TextboxElement created via CreateTextboxAtPosition should be auto-deleted
        /// when text editing ends with no input.
        /// </summary>
        [Fact]
        public void CreateTextboxAtPosition_ThenCommitEmptyText_RemovesTextbox()
        {
            // Arrange
            var viewModel = CreateViewModel();
            var initialCount = viewModel.Elements.Count;
            var position = new Point(200, 200);

            // Act - Create textbox via double-click simulation
            viewModel.CreateTextboxAtPosition(position);
            viewModel.Elements.Count.Should().Be(initialCount + 1);
            var createdTextbox = viewModel.Elements.Last();
            createdTextbox.ShapeType.Should().Be(DrawingShapeType.Textbox);

            // Commit with empty text
            viewModel.CommitTextEditing("");

            // Assert - Textbox should be removed
            viewModel.Elements.Count.Should().Be(initialCount);
            viewModel.Elements.Should().NotContain(createdTextbox);
        }

        #endregion

        #region Edge Cases

        /// <summary>
        /// CancelTextEditing on newly created TextboxElement (with empty originalText)
        /// should trigger deletion since the element was never given content.
        /// </summary>
        [Fact]
        public void CancelTextEditing_OnNewlyCreatedTextbox_WithEmptyOriginalText_RemovesElement()
        {
            // Arrange
            var viewModel = CreateViewModel();
            var position = new Point(200, 200);
            viewModel.CreateTextboxAtPosition(position);
            var createdTextbox = viewModel.Elements.Last();

            // Act - Cancel editing (originalText is empty)
            viewModel.CancelTextEditing();

            // Assert - Textbox should be removed because originalText was empty
            viewModel.Elements.Should().NotContain(createdTextbox);
        }

        /// <summary>
        /// CancelTextEditing on existing TextboxElement with non-empty originalText
        /// should restore the original text and NOT delete the element.
        /// </summary>
        [Fact]
        public void CancelTextEditing_OnExistingTextbox_WithNonEmptyOriginalText_PreservesElement()
        {
            // Arrange
            var viewModel = CreateViewModel();
            var textbox = CreateTextboxElement(text: "Original Content");
            viewModel.Elements.Add(textbox);
            viewModel.SelectElement(textbox);
            viewModel.TryStartTextEditing(textbox);

            // Act - Cancel editing
            viewModel.CancelTextEditing();

            // Assert - Textbox should still exist with original text
            viewModel.Elements.Should().Contain(textbox);
            textbox.Text.Should().Be("Original Content");
        }

        #endregion
    }
}