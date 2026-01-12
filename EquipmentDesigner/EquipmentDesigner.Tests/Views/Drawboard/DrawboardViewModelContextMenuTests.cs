using System.Linq;
using System.Windows;
using FluentAssertions;
using EquipmentDesigner.Models;
using EquipmentDesigner.ViewModels;
using Xunit;

namespace EquipmentDesigner.Tests.Views.Drawboard
{
    /// <summary>
    /// TDD tests for DrawboardViewModel context menu operations.
    /// Tests cover clipboard, Z-Order, lock/unlock, duplicate, and delete operations.
    /// </summary>
    public class DrawboardViewModelContextMenuTests
    {
        #region Test Helper

        private static DrawboardViewModel CreateViewModel()
        {
            return new DrawboardViewModel(showBackButton: false);
        }

        private static ContextMenuTestElement CreateTestElement(
            double x = 100, double y = 100, double width = 100, double height = 100, int zIndex = 1)
        {
            return new ContextMenuTestElement
            {
                X = x,
                Y = y,
                Width = width,
                Height = height,
                ZIndex = zIndex
            };
        }

        private static DrawboardViewModel CreateViewModelWithElement(
            double x = 100, double y = 100, double width = 100, double height = 100)
        {
            var viewModel = CreateViewModel();
            var element = CreateTestElement(x, y, width, height);
            viewModel.Elements.Add(element);
            return viewModel;
        }

        private static DrawboardViewModel CreateViewModelWithSelectedElement(
            double x = 100, double y = 100, double width = 100, double height = 100)
        {
            var viewModel = CreateViewModelWithElement(x, y, width, height);
            viewModel.SelectElement(viewModel.Elements[0]);
            return viewModel;
        }

        #endregion

        #region Clipboard Operations - Copy

        [Fact]
        public void CopyToClipboard_WithSingleSelectedElement_StoresElementDataInClipboard()
        {
            // Arrange
            var viewModel = CreateViewModelWithSelectedElement();

            // Act
            viewModel.CopyToClipboard();

            // Assert
            viewModel.HasClipboardData.Should().BeTrue();
        }

        [Fact]
        public void CopyToClipboard_WithSingleElement_StoresCorrectPosition()
        {
            // Arrange
            var viewModel = CreateViewModelWithSelectedElement(x: 150, y: 200);

            // Act
            viewModel.CopyToClipboard();
            viewModel.ClearAllSelections();
            viewModel.PasteFromClipboard();

            // Assert
            var pastedElement = viewModel.Elements.Last();
            pastedElement.X.Should().Be(160); // 150 + 10 offset
            pastedElement.Y.Should().Be(210); // 200 + 10 offset
        }

        [Fact]
        public void CopyToClipboard_WithMultipleSelectedElements_StoresAllElementData()
        {
            // Arrange
            var viewModel = CreateViewModel();
            var element1 = CreateTestElement(100, 100);
            var element2 = CreateTestElement(200, 200);
            viewModel.Elements.Add(element1);
            viewModel.Elements.Add(element2);
            viewModel.ToggleSelection(element1);
            viewModel.ToggleSelection(element2);

            // Act
            viewModel.CopyToClipboard();

            // Assert
            viewModel.HasClipboardData.Should().BeTrue();
            viewModel.ClipboardElementCount.Should().Be(2);
        }

        [Fact]
        public void CopyToClipboard_WithNoSelection_DoesNotStoreData()
        {
            // Arrange
            var viewModel = CreateViewModelWithElement();
            // Element exists but is not selected

            // Act
            viewModel.CopyToClipboard();

            // Assert
            viewModel.HasClipboardData.Should().BeFalse();
        }

        #endregion

        #region Clipboard Operations - Paste

        [Fact]
        public void PasteFromClipboard_WhenClipboardEmpty_DoesNothing()
        {
            // Arrange
            var viewModel = CreateViewModel();
            var initialCount = viewModel.Elements.Count;

            // Act
            viewModel.PasteFromClipboard();

            // Assert
            viewModel.Elements.Count.Should().Be(initialCount);
        }

        [Fact]
        public void PasteFromClipboard_WhenClipboardHasData_CreatesNewElement()
        {
            // Arrange
            var viewModel = CreateViewModelWithSelectedElement();
            viewModel.CopyToClipboard();
            var countBeforePaste = viewModel.Elements.Count;

            // Act
            viewModel.PasteFromClipboard();

            // Assert
            viewModel.Elements.Count.Should().Be(countBeforePaste + 1);
        }

        [Fact]
        public void PasteFromClipboard_CreatesElementWithNewId()
        {
            // Arrange
            var viewModel = CreateViewModelWithSelectedElement();
            var originalElement = viewModel.SelectedElement;
            viewModel.CopyToClipboard();

            // Act
            viewModel.PasteFromClipboard();

            // Assert
            var pastedElement = viewModel.Elements.Last();
            pastedElement.Id.Should().NotBe(originalElement.Id);
        }

        [Fact]
        public void PasteFromClipboard_AppliesOffset()
        {
            // Arrange
            var viewModel = CreateViewModelWithSelectedElement(x: 100, y: 100);
            viewModel.CopyToClipboard();

            // Act
            viewModel.PasteFromClipboard();

            // Assert
            var pastedElement = viewModel.Elements.Last();
            pastedElement.X.Should().Be(110); // 100 + 10
            pastedElement.Y.Should().Be(110); // 100 + 10
        }

        [Fact]
        public void PasteFromClipboard_WithMultipleElements_MaintainsRelativePositioning()
        {
            // Arrange
            var viewModel = CreateViewModel();
            var element1 = CreateTestElement(100, 100);
            var element2 = CreateTestElement(200, 150); // 100px right, 50px down from element1
            viewModel.Elements.Add(element1);
            viewModel.Elements.Add(element2);
            viewModel.ToggleSelection(element1);
            viewModel.ToggleSelection(element2);
            viewModel.CopyToClipboard();

            // Act
            viewModel.PasteFromClipboard();

            // Assert
            var pastedElements = viewModel.Elements.Skip(2).ToList();
            pastedElements.Should().HaveCount(2);

            var relativeX = pastedElements[1].X - pastedElements[0].X;
            var relativeY = pastedElements[1].Y - pastedElements[0].Y;
            relativeX.Should().Be(100); // Same relative X difference
            relativeY.Should().Be(50);  // Same relative Y difference
        }

        [Fact]
        public void PasteFromClipboard_AutoSelectsNewlyCreatedElements()
        {
            // Arrange
            var viewModel = CreateViewModelWithSelectedElement();
            viewModel.CopyToClipboard();

            // Act
            viewModel.PasteFromClipboard();

            // Assert
            var pastedElement = viewModel.Elements.Last();
            viewModel.SelectedElement.Should().Be(pastedElement);
            pastedElement.IsSelected.Should().BeTrue();
        }

        #endregion

        #region Z-Order Operations - Send Backward

        [Fact]
        public void SendBackward_SwapsWithElementBelowInZOrder()
        {
            // Arrange
            var viewModel = CreateViewModel();
            var bottomElement = CreateTestElement(zIndex: 1);
            var topElement = CreateTestElement(zIndex: 2);
            viewModel.Elements.Add(bottomElement);
            viewModel.Elements.Add(topElement);
            viewModel.SelectElement(topElement);

            // Act
            viewModel.SendBackward();

            // Assert
            topElement.ZIndex.Should().Be(1);
            bottomElement.ZIndex.Should().Be(2);
        }

        [Fact]
        public void SendBackward_WhenAlreadyAtMinimum_DoesNothing()
        {
            // Arrange
            var viewModel = CreateViewModel();
            var bottomElement = CreateTestElement(zIndex: 1);
            var topElement = CreateTestElement(zIndex: 2);
            viewModel.Elements.Add(bottomElement);
            viewModel.Elements.Add(topElement);
            viewModel.SelectElement(bottomElement);

            // Act
            viewModel.SendBackward();

            // Assert
            bottomElement.ZIndex.Should().Be(1);
            topElement.ZIndex.Should().Be(2);
        }

        #endregion

        #region Z-Order Operations - Bring Forward

        [Fact]
        public void BringForward_SwapsWithElementAboveInZOrder()
        {
            // Arrange
            var viewModel = CreateViewModel();
            var bottomElement = CreateTestElement(zIndex: 1);
            var topElement = CreateTestElement(zIndex: 2);
            viewModel.Elements.Add(bottomElement);
            viewModel.Elements.Add(topElement);
            viewModel.SelectElement(bottomElement);

            // Act
            viewModel.BringForward();

            // Assert
            bottomElement.ZIndex.Should().Be(2);
            topElement.ZIndex.Should().Be(1);
        }

        [Fact]
        public void BringForward_WhenAlreadyAtMaximum_DoesNothing()
        {
            // Arrange
            var viewModel = CreateViewModel();
            var bottomElement = CreateTestElement(zIndex: 1);
            var topElement = CreateTestElement(zIndex: 2);
            viewModel.Elements.Add(bottomElement);
            viewModel.Elements.Add(topElement);
            viewModel.SelectElement(topElement);

            // Act
            viewModel.BringForward();

            // Assert
            topElement.ZIndex.Should().Be(2);
            bottomElement.ZIndex.Should().Be(1);
        }

        #endregion

        #region Z-Order Operations - Send to Back

        [Fact]
        public void SendToBack_SetsZIndexToMinimumMinusOne()
        {
            // Arrange
            var viewModel = CreateViewModel();
            var element1 = CreateTestElement(zIndex: 5);
            var element2 = CreateTestElement(zIndex: 10);
            var element3 = CreateTestElement(zIndex: 15);
            viewModel.Elements.Add(element1);
            viewModel.Elements.Add(element2);
            viewModel.Elements.Add(element3);
            viewModel.SelectElement(element3);

            // Act
            viewModel.SendToBack();

            // Assert
            element3.ZIndex.Should().Be(4); // Min(5) - 1
        }

        #endregion

        #region Z-Order Operations - Bring to Front

        [Fact]
        public void BringToFront_SetsZIndexToMaximumPlusOne()
        {
            // Arrange
            var viewModel = CreateViewModel();
            var element1 = CreateTestElement(zIndex: 5);
            var element2 = CreateTestElement(zIndex: 10);
            var element3 = CreateTestElement(zIndex: 15);
            viewModel.Elements.Add(element1);
            viewModel.Elements.Add(element2);
            viewModel.Elements.Add(element3);
            viewModel.SelectElement(element1);

            // Act
            viewModel.BringToFront();

            // Assert
            element1.ZIndex.Should().Be(16); // Max(15) + 1
        }

        #endregion

        #region Management - Duplicate

        [Fact]
        public void Duplicate_CreatesNewElementWithNewId()
        {
            // Arrange
            var viewModel = CreateViewModelWithSelectedElement();
            var originalElement = viewModel.SelectedElement;
            var originalCount = viewModel.Elements.Count;

            // Act
            viewModel.Duplicate();

            // Assert
            viewModel.Elements.Count.Should().Be(originalCount + 1);
            var duplicatedElement = viewModel.Elements.Last();
            duplicatedElement.Id.Should().NotBe(originalElement.Id);
        }

        [Fact]
        public void Duplicate_PositionsCopyWithOffset()
        {
            // Arrange
            var viewModel = CreateViewModelWithSelectedElement(x: 100, y: 100);

            // Act
            viewModel.Duplicate();

            // Assert
            var duplicatedElement = viewModel.Elements.Last();
            duplicatedElement.X.Should().Be(110); // 100 + 10
            duplicatedElement.Y.Should().Be(110); // 100 + 10
        }

        [Fact]
        public void Duplicate_AutoSelectsNewCopy()
        {
            // Arrange
            var viewModel = CreateViewModelWithSelectedElement();

            // Act
            viewModel.Duplicate();

            // Assert
            var duplicatedElement = viewModel.Elements.Last();
            viewModel.SelectedElement.Should().Be(duplicatedElement);
            duplicatedElement.IsSelected.Should().BeTrue();
        }

        [Fact]
        public void Duplicate_WithMultipleSelection_CreatesAllCopies()
        {
            // Arrange
            var viewModel = CreateViewModel();
            var element1 = CreateTestElement(100, 100);
            var element2 = CreateTestElement(200, 200);
            viewModel.Elements.Add(element1);
            viewModel.Elements.Add(element2);
            viewModel.ToggleSelection(element1);
            viewModel.ToggleSelection(element2);
            var originalCount = viewModel.Elements.Count;

            // Act
            viewModel.Duplicate();

            // Assert
            viewModel.Elements.Count.Should().Be(originalCount + 2);
        }

        #endregion

        #region Management - Lock/Unlock

        [Fact]
        public void LockSelectedElements_SetsIsLockedToTrue()
        {
            // Arrange
            var viewModel = CreateViewModelWithSelectedElement();
            var element = viewModel.SelectedElement;

            // Act
            viewModel.LockSelectedElements();

            // Assert
            element.IsLocked.Should().BeTrue();
        }

        [Fact]
        public void UnlockSelectedElements_SetsIsLockedToFalse()
        {
            // Arrange
            var viewModel = CreateViewModelWithSelectedElement();
            var element = viewModel.SelectedElement;
            element.IsLocked = true;

            // Act
            viewModel.UnlockSelectedElements();

            // Assert
            element.IsLocked.Should().BeFalse();
        }

        [Fact]
        public void ToggleLock_WhenUnlocked_LocksElement()
        {
            // Arrange
            var viewModel = CreateViewModelWithSelectedElement();
            var element = viewModel.SelectedElement;
            element.IsLocked = false;

            // Act
            viewModel.ToggleLock();

            // Assert
            element.IsLocked.Should().BeTrue();
        }

        [Fact]
        public void ToggleLock_WhenLocked_UnlocksElement()
        {
            // Arrange
            var viewModel = CreateViewModelWithSelectedElement();
            var element = viewModel.SelectedElement;
            element.IsLocked = true;

            // Act
            viewModel.ToggleLock();

            // Assert
            element.IsLocked.Should().BeFalse();
        }

        [Fact]
        public void GetLockMenuText_WhenUnlocked_ReturnsLock()
        {
            // Arrange
            var viewModel = CreateViewModelWithSelectedElement();
            viewModel.SelectedElement.IsLocked = false;

            // Act
            var text = viewModel.GetLockMenuText();

            // Assert
            text.Should().Be("Lock");
        }

        [Fact]
        public void GetLockMenuText_WhenLocked_ReturnsUnlock()
        {
            // Arrange
            var viewModel = CreateViewModelWithSelectedElement();
            viewModel.SelectedElement.IsLocked = true;

            // Act
            var text = viewModel.GetLockMenuText();

            // Assert
            text.Should().Be("Unlock");
        }

        [Fact]
        public void LockSelectedElements_WithMultipleSelection_LocksAllElements()
        {
            // Arrange
            var viewModel = CreateViewModel();
            var element1 = CreateTestElement();
            var element2 = CreateTestElement(200, 200);
            viewModel.Elements.Add(element1);
            viewModel.Elements.Add(element2);
            viewModel.ToggleSelection(element1);
            viewModel.ToggleSelection(element2);

            // Act
            viewModel.LockSelectedElements();

            // Assert
            element1.IsLocked.Should().BeTrue();
            element2.IsLocked.Should().BeTrue();
        }

        #endregion

        #region Delete Operation

        [Fact]
        public void DeleteSelectedElements_RemovesElementFromCollection()
        {
            // Arrange
            var viewModel = CreateViewModelWithSelectedElement();
            var element = viewModel.SelectedElement;

            // Act
            viewModel.DeleteSelectedElements();

            // Assert
            viewModel.Elements.Should().NotContain(element);
        }

        [Fact]
        public void DeleteSelectedElements_ClearsSelection()
        {
            // Arrange
            var viewModel = CreateViewModelWithSelectedElement();

            // Act
            viewModel.DeleteSelectedElements();

            // Assert
            viewModel.SelectedElement.Should().BeNull();
        }

        [Fact]
        public void DeleteSelectedElements_WithMultipleSelection_RemovesAllSelectedElements()
        {
            // Arrange
            var viewModel = CreateViewModel();
            var element1 = CreateTestElement();
            var element2 = CreateTestElement(200, 200);
            var element3 = CreateTestElement(300, 300);
            viewModel.Elements.Add(element1);
            viewModel.Elements.Add(element2);
            viewModel.Elements.Add(element3);
            viewModel.ToggleSelection(element1);
            viewModel.ToggleSelection(element2);

            // Act
            viewModel.DeleteSelectedElements();

            // Assert
            viewModel.Elements.Should().NotContain(element1);
            viewModel.Elements.Should().NotContain(element2);
            viewModel.Elements.Should().Contain(element3); // Not selected, should remain
        }

        [Fact]
        public void DeleteSelectedElements_SkipsLockedElements()
        {
            // Arrange
            var viewModel = CreateViewModel();
            var lockedElement = CreateTestElement();
            lockedElement.IsLocked = true;
            var unlockedElement = CreateTestElement(200, 200);
            viewModel.Elements.Add(lockedElement);
            viewModel.Elements.Add(unlockedElement);
            viewModel.AddToSelection(lockedElement);
            viewModel.AddToSelection(unlockedElement);

            // Act
            viewModel.DeleteSelectedElements();

            // Assert
            viewModel.Elements.Should().Contain(lockedElement); // Locked, should remain
            viewModel.Elements.Should().NotContain(unlockedElement); // Unlocked, should be deleted
        }

        #endregion

        #region Context Menu Availability

        [Fact]
        public void CanShowContextMenu_WhenElementSelected_ReturnsTrue()
        {
            // Arrange
            var viewModel = CreateViewModelWithSelectedElement();

            // Act
            var canShow = viewModel.CanShowContextMenu;

            // Assert
            canShow.Should().BeTrue();
        }

        [Fact]
        public void CanShowContextMenu_WhenNoSelection_ReturnsFalse()
        {
            // Arrange
            var viewModel = CreateViewModel();

            // Act
            var canShow = viewModel.CanShowContextMenu;

            // Assert
            canShow.Should().BeFalse();
        }

        [Fact]
        public void CanShowContextMenu_WhenLockedElementSelected_ReturnsTrue()
        {
            // Arrange
            var viewModel = CreateViewModelWithSelectedElement();
            viewModel.SelectedElement.IsLocked = true;

            // Act
            var canShow = viewModel.CanShowContextMenu;

            // Assert
            canShow.Should().BeTrue();
        }

        [Fact]
        public void CanPaste_WhenClipboardEmpty_ReturnsFalse()
        {
            // Arrange
            var viewModel = CreateViewModel();

            // Act
            var canPaste = viewModel.CanPaste;

            // Assert
            canPaste.Should().BeFalse();
        }

        [Fact]
        public void CanPaste_WhenClipboardHasData_ReturnsTrue()
        {
            // Arrange
            var viewModel = CreateViewModelWithSelectedElement();
            viewModel.CopyToClipboard();

            // Act
            var canPaste = viewModel.CanPaste;

            // Assert
            canPaste.Should().BeTrue();
        }

        #endregion

        #region Locked Element Context Menu Behavior

        [Fact]
        public void IsAllSelectedLocked_WhenSingleLockedElement_ReturnsTrue()
        {
            // Arrange
            var viewModel = CreateViewModelWithSelectedElement();
            viewModel.SelectedElement.IsLocked = true;

            // Act
            var isLocked = viewModel.IsAllSelectedLocked;

            // Assert
            isLocked.Should().BeTrue();
        }

        [Fact]
        public void IsAllSelectedLocked_WhenSingleUnlockedElement_ReturnsFalse()
        {
            // Arrange
            var viewModel = CreateViewModelWithSelectedElement();
            viewModel.SelectedElement.IsLocked = false;

            // Act
            var isLocked = viewModel.IsAllSelectedLocked;

            // Assert
            isLocked.Should().BeFalse();
        }

        [Fact]
        public void IsAllSelectedLocked_WhenAllMultipleElementsLocked_ReturnsTrue()
        {
            // Arrange
            var viewModel = CreateViewModel();
            var element1 = CreateTestElement();
            var element2 = CreateTestElement(200, 200);
            viewModel.Elements.Add(element1);
            viewModel.Elements.Add(element2);
            viewModel.AddToSelection(element1);
            viewModel.AddToSelection(element2);
            // Lock elements AFTER adding to selection (AddToSelection returns early for locked elements)
            element1.IsLocked = true;
            element2.IsLocked = true;

            // Act
            var isLocked = viewModel.IsAllSelectedLocked;

            // Assert
            isLocked.Should().BeTrue();
        }

        [Fact]
        public void IsAllSelectedLocked_WhenSomeElementsLocked_ReturnsFalse()
        {
            // Arrange
            var viewModel = CreateViewModel();
            var element1 = CreateTestElement();
            var element2 = CreateTestElement(200, 200);
            viewModel.Elements.Add(element1);
            viewModel.Elements.Add(element2);
            viewModel.AddToSelection(element1);
            viewModel.AddToSelection(element2);
            // Lock only element1 AFTER adding to selection
            element1.IsLocked = true;
            element2.IsLocked = false;

            // Act
            var isLocked = viewModel.IsAllSelectedLocked;

            // Assert
            isLocked.Should().BeFalse();
        }

        [Fact]
        public void IsAllSelectedLocked_WhenNoSelection_ReturnsFalse()
        {
            // Arrange
            var viewModel = CreateViewModel();

            // Act
            var isLocked = viewModel.IsAllSelectedLocked;

            // Assert
            isLocked.Should().BeFalse();
        }

        #endregion

        #region Locked Element Multi-Selection Prevention

        [Fact]
        public void ToggleSelection_WhenAddingLockedElement_DoesNotAddToMultiSelection()
        {
            // Arrange
            var viewModel = CreateViewModel();
            var unlockedElement = CreateTestElement(100, 100);
            var lockedElement = CreateTestElement(200, 200);
            lockedElement.IsLocked = true;
            viewModel.Elements.Add(unlockedElement);
            viewModel.Elements.Add(lockedElement);
            viewModel.SelectElement(unlockedElement);

            // Act - Try to add locked element via shift+click
            viewModel.ToggleSelection(lockedElement);

            // Assert - Should remain single selection, not multi-selection
            viewModel.IsMultiSelectionMode.Should().BeFalse();
            viewModel.SelectedElement.Should().Be(unlockedElement);
        }

        [Fact]
        public void ToggleSelection_WhenRemovingLockedElement_AllowsRemoval()
        {
            // Arrange
            var viewModel = CreateViewModel();
            var element1 = CreateTestElement(100, 100);
            var element2 = CreateTestElement(200, 200);
            viewModel.Elements.Add(element1);
            viewModel.Elements.Add(element2);
            viewModel.ToggleSelection(element1);
            viewModel.ToggleSelection(element2);
            // Lock element2 AFTER adding to selection
            element2.IsLocked = true;

            // Act - Remove locked element from selection
            viewModel.ToggleSelection(element2);

            // Assert - Should allow removal
            viewModel.SelectedElements.Should().NotContain(element2);
            viewModel.SelectedElements.Count().Should().Be(1);
        }

        [Fact]
        public void AddToSelection_WhenElementIsLocked_DoesNotAddToSelection()
        {
            // Arrange
            var viewModel = CreateViewModel();
            var unlockedElement = CreateTestElement(100, 100);
            var lockedElement = CreateTestElement(200, 200);
            lockedElement.IsLocked = true;
            viewModel.Elements.Add(unlockedElement);
            viewModel.Elements.Add(lockedElement);
            viewModel.SelectElement(unlockedElement);

            // Act
            viewModel.AddToSelection(lockedElement);

            // Assert
            viewModel.IsMultiSelectionMode.Should().BeFalse();
            viewModel.SelectedElement.Should().Be(unlockedElement);
        }

        [Fact]
        public void FinishRubberbandSelection_WithLockedElements_ExcludesLockedElements()
        {
            // Arrange
            var viewModel = CreateViewModel();
            var unlockedElement = CreateTestElement(50, 50, 50, 50);
            var lockedElement = CreateTestElement(150, 50, 50, 50);
            lockedElement.IsLocked = true;
            viewModel.Elements.Add(unlockedElement);
            viewModel.Elements.Add(lockedElement);

            // Act - Rubberband select an area containing both elements
            viewModel.StartRubberbandSelection(new Point(0, 0));
            viewModel.UpdateRubberbandSelection(new Point(250, 150));
            viewModel.FinishRubberbandSelection();

            // Assert - Only unlocked element should be selected
            viewModel.SelectedElements.Should().Contain(unlockedElement);
            viewModel.SelectedElements.Should().NotContain(lockedElement);
            viewModel.SelectedElements.Count().Should().Be(1);
        }

        [Fact]
        public void FinishRubberbandSelection_WithOnlyLockedElements_SelectsNothing()
        {
            // Arrange
            var viewModel = CreateViewModel();
            var lockedElement1 = CreateTestElement(50, 50, 50, 50);
            var lockedElement2 = CreateTestElement(150, 50, 50, 50);
            lockedElement1.IsLocked = true;
            lockedElement2.IsLocked = true;
            viewModel.Elements.Add(lockedElement1);
            viewModel.Elements.Add(lockedElement2);

            // Act - Rubberband select an area containing both locked elements
            viewModel.StartRubberbandSelection(new Point(0, 0));
            viewModel.UpdateRubberbandSelection(new Point(250, 150));
            viewModel.FinishRubberbandSelection();

            // Assert - No elements should be selected
            viewModel.SelectedElements.Count().Should().Be(0);
            viewModel.IsMultiSelectionMode.Should().BeFalse();
            viewModel.SelectedElement.Should().BeNull();
        }

        [Fact]
        public void FinishRubberbandSelection_WithMixedElements_SelectsOnlyUnlocked()
        {
            // Arrange
            var viewModel = CreateViewModel();
            var unlocked1 = CreateTestElement(50, 50, 50, 50);
            var unlocked2 = CreateTestElement(150, 50, 50, 50);
            var locked1 = CreateTestElement(250, 50, 50, 50);
            locked1.IsLocked = true;
            viewModel.Elements.Add(unlocked1);
            viewModel.Elements.Add(unlocked2);
            viewModel.Elements.Add(locked1);

            // Act - Rubberband select an area containing all elements
            viewModel.StartRubberbandSelection(new Point(0, 0));
            viewModel.UpdateRubberbandSelection(new Point(350, 150));
            viewModel.FinishRubberbandSelection();

            // Assert - Only unlocked elements should be selected
            viewModel.SelectedElements.Should().Contain(unlocked1);
            viewModel.SelectedElements.Should().Contain(unlocked2);
            viewModel.SelectedElements.Should().NotContain(locked1);
            viewModel.SelectedElements.Count().Should().Be(2);
            viewModel.IsMultiSelectionMode.Should().BeTrue();
        }

        #endregion
    }

    /// <summary>
    /// Concrete DrawingElement implementation for context menu testing.
    /// </summary>
    internal class ContextMenuTestElement : DrawingElement
    {
        public override DrawingShapeType ShapeType => DrawingShapeType.Action;
    }
}