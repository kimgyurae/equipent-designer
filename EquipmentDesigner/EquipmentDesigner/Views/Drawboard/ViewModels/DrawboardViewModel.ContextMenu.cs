using System;
using System.Collections.Generic;
using System.Linq;
using EquipmentDesigner.Models;
using EquipmentDesigner.Controls;

namespace EquipmentDesigner.ViewModels
{
    /// <summary>
    /// Partial class containing context menu operations for DrawboardViewModel.
    /// Includes clipboard, Z-Order, lock/unlock, duplicate, and delete operations.
    /// </summary>
    public partial class DrawboardViewModel
    {
        #region Clipboard Fields

        private List<DrawingElementSnapshot> _clipboard = new List<DrawingElementSnapshot>();

        #endregion

        #region Clipboard Properties

        /// <summary>
        /// Whether the clipboard contains data that can be pasted.
        /// </summary>
        public bool HasClipboardData => _clipboard.Count > 0;

        /// <summary>
        /// Number of elements currently in the clipboard.
        /// </summary>
        public int ClipboardElementCount => _clipboard.Count;

        /// <summary>
        /// Whether paste operation is available.
        /// </summary>
        public bool CanPaste => HasClipboardData;

        /// <summary>
        /// Whether context menu can be shown (requires selection).
        /// </summary>
        public bool CanShowContextMenu => SelectedElement != null || IsMultiSelectionMode;

        /// <summary>
        /// Whether all selected elements are locked.
        /// When true, only Unlock operation should be enabled in context menu.
        /// </summary>
        public bool IsAllSelectedLocked
        {
            get
            {
                var selectedElements = GetSelectedElementsForOperation().ToList();
                if (!selectedElements.Any()) return false;
                return selectedElements.All(e => e.IsLocked);
            }
        }

        #endregion

        #region Clipboard Operations

        /// <summary>
        /// Copies the currently selected element(s) to the internal clipboard.
        /// </summary>
        public void CopyToClipboard()
        {
            _clipboard.Clear();

            var elementsToCopy = GetSelectedElementsForOperation();
            if (!elementsToCopy.Any()) return;

            foreach (var element in elementsToCopy)
            {
                _clipboard.Add(new DrawingElementSnapshot(element));
            }
        }

        /// <summary>
        /// Pastes elements from the clipboard with an offset.
        /// </summary>
        public void PasteFromClipboard()
        {
            if (!HasClipboardData) return;

            const double offset = 10;
            var newElements = new List<DrawingElement>();

            foreach (var snapshot in _clipboard)
            {
                var newElement = snapshot.CreateElement();
                newElement.X += offset;
                newElement.Y += offset;
                newElement.ZIndex = _nextZIndex++;
                Elements.Add(newElement);
                AddStepToCurrentWorkflow(newElement);
                newElements.Add(newElement);

                // Update snapshot coordinates for cumulative offset on next paste
                snapshot.X += offset;
                snapshot.Y += offset;
            }

            // Auto-select the newly created elements
            ClearAllSelections();
            if (newElements.Count == 1)
            {
                SelectElement(newElements[0]);
            }
            else
            {
                foreach (var element in newElements)
                {
                    AddToSelection(element);
                }
            }
        }

        #endregion

        #region Z-Order Operations

        /// <summary>
        /// Moves selected element(s) one step backward in Z-order (swap with element below).
        /// </summary>
        public void SendBackward()
        {
            var selectedElements = GetSelectedElementsForOperation();
            if (!selectedElements.Any()) return;

            foreach (var selectedElement in selectedElements.OrderBy(e => e.ZIndex))
            {
                var elementBelow = Elements
                    .Where(e => e.ZIndex < selectedElement.ZIndex && !selectedElements.Contains(e))
                    .OrderByDescending(e => e.ZIndex)
                    .FirstOrDefault();

                if (elementBelow != null)
                {
                    // Swap ZIndex values
                    var temp = selectedElement.ZIndex;
                    selectedElement.ZIndex = elementBelow.ZIndex;
                    elementBelow.ZIndex = temp;
                }
            }
        }

        /// <summary>
        /// Moves selected element(s) one step forward in Z-order (swap with element above).
        /// </summary>
        public void BringForward()
        {
            var selectedElements = GetSelectedElementsForOperation();
            if (!selectedElements.Any()) return;

            foreach (var selectedElement in selectedElements.OrderByDescending(e => e.ZIndex))
            {
                var elementAbove = Elements
                    .Where(e => e.ZIndex > selectedElement.ZIndex && !selectedElements.Contains(e))
                    .OrderBy(e => e.ZIndex)
                    .FirstOrDefault();

                if (elementAbove != null)
                {
                    // Swap ZIndex values
                    var temp = selectedElement.ZIndex;
                    selectedElement.ZIndex = elementAbove.ZIndex;
                    elementAbove.ZIndex = temp;
                }
            }
        }

        /// <summary>
        /// Moves selected element(s) to the back (lowest Z-index).
        /// </summary>
        public void SendToBack()
        {
            var selectedElements = GetSelectedElementsForOperation();
            if (!selectedElements.Any()) return;

            var minZIndex = Elements.Min(e => e.ZIndex);
            foreach (var element in selectedElements)
            {
                element.ZIndex = minZIndex - 1;
                minZIndex--;
            }
        }

        /// <summary>
        /// Moves selected element(s) to the front (highest Z-index).
        /// </summary>
        public void BringToFront()
        {
            var selectedElements = GetSelectedElementsForOperation();
            if (!selectedElements.Any()) return;

            var maxZIndex = Elements.Max(e => e.ZIndex);
            foreach (var element in selectedElements)
            {
                element.ZIndex = maxZIndex + 1;
                maxZIndex++;
            }
        }

        #endregion

        #region Duplicate Operation

        /// <summary>
        /// Creates a duplicate of selected element(s) with offset positioning.
        /// </summary>
        public void Duplicate()
        {
            CopyToClipboard();
            PasteFromClipboard();
        }

        #endregion

        #region Lock/Unlock Operations

        /// <summary>
        /// Locks all selected elements (prevents move, resize, delete).
        /// </summary>
        public void LockSelectedElements()
        {
            var elements = GetSelectedElementsForOperation();
            foreach (var element in elements)
            {
                element.IsLocked = true;
            }
            DeferNotifyUnlockButtonPropertiesChanged();
        }

        /// <summary>
        /// Unlocks all selected elements.
        /// </summary>
        public void UnlockSelectedElements()
        {
            var elements = GetSelectedElementsForOperation();
            foreach (var element in elements)
            {
                element.IsLocked = false;
            }
            DeferNotifyUnlockButtonPropertiesChanged();
        }

        /// <summary>
        /// Unlocks the single selected element (used by floating unlock button).
        /// </summary>
        public void UnlockSingleSelectedElement()
        {
            if (SelectedElement != null && SelectedElement.IsLocked)
            {
                SelectedElement.IsLocked = false;
                DeferNotifyUnlockButtonPropertiesChanged();
            }
        }

        /// <summary>
        /// Toggles the lock state of selected elements.
        /// If any selected element is unlocked, locks all. Otherwise unlocks all.
        /// </summary>
        public void ToggleLock()
        {
            var elements = GetSelectedElementsForOperation();
            if (!elements.Any()) return;

            // If any element is unlocked, lock all. Otherwise unlock all.
            bool shouldLock = elements.Any(e => !e.IsLocked);
            foreach (var element in elements)
            {
                element.IsLocked = shouldLock;
            }
            DeferNotifyUnlockButtonPropertiesChanged();
        }

        /// <summary>
        /// Gets the appropriate menu text based on current lock state.
        /// </summary>
        public string GetLockMenuText()
        {
            var elements = GetSelectedElementsForOperation();
            if (!elements.Any()) return "Lock";

            // If all elements are locked, show "Unlock". Otherwise show "Lock".
            return elements.All(e => e.IsLocked) ? "Unlock" : "Lock";
        }

        #endregion

        #region Delete Operation

        /// <summary>
        /// Deletes all selected elements that are not locked.
        /// </summary>
        public void DeleteSelectedElements()
        {
            var elementsToDelete = GetSelectedElementsForOperation()
                .Where(e => !e.IsLocked)
                .ToList();

            ClearAllSelections();

            foreach (var element in elementsToDelete)
            {
                RemoveStepFromCurrentWorkflow(element);
                Elements.Remove(element);
            }
        }

        #endregion

        #region Context Menu Builder

        /// <summary>
        /// Shows the context menu for the currently selected element(s).
        /// When all selected elements are locked, only Unlock is enabled.
        /// </summary>
        public void ShowContextMenu()
        {
            if (!CanShowContextMenu) return;

            var lockText = GetLockMenuText();
            var isLocked = IsAllSelectedLocked;

            ContextMenuService.Instance
                .Create()
                // Clipboard Group
                .AddItem("Copy", CopyToClipboard, isEnabled: !isLocked)
                .AddItem("Paste", PasteFromClipboard, isEnabled: CanPaste && !isLocked)
                .AddSeparator()
                // Z-Order Group
                .AddItem("Send backward", SendBackward, isEnabled: !isLocked)
                .AddItem("Bring forward", BringForward, isEnabled: !isLocked)
                .AddItem("Send to back", SendToBack, isEnabled: !isLocked)
                .AddItem("Bring to front", BringToFront, isEnabled: !isLocked)
                .AddSeparator()
                // Management Group
                .AddItem("Duplicate", Duplicate, isEnabled: !isLocked)
                .AddItem(lockText, ToggleLock)
                .AddSeparator()
                // Delete Group
                .AddDestructiveItem("Delete", DeleteSelectedElements, isEnabled: !isLocked)
                .Show();
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Gets all currently selected elements for context menu operations.
        /// Returns multi-selected elements if in multi-selection mode, otherwise single selected element.
        /// </summary>
        private IEnumerable<DrawingElement> GetSelectedElementsForOperation()
        {
            if (IsMultiSelectionMode)
            {
                return SelectedElements.ToList();
            }
            else if (SelectedElement != null)
            {
                return new[] { SelectedElement };
            }
            return Enumerable.Empty<DrawingElement>();
        }

        /// <summary>
        /// Notifies property changes for unlock button properties.
        /// </summary>
        private void NotifyUnlockButtonPropertiesChanged()
        {
            OnPropertyChanged(nameof(ShowUnlockButton));
            OnPropertyChanged(nameof(UnlockButtonX));
            OnPropertyChanged(nameof(UnlockButtonY));
            OnPropertyChanged(nameof(ScreenUnlockButtonX));
            OnPropertyChanged(nameof(ScreenUnlockButtonY));
        }

        /// <summary>
        /// Defers unlock button property notifications to prevent UI update conflicts.
        /// Uses Dispatcher to ensure notifications happen after current operation completes.
        /// </summary>
        private void DeferNotifyUnlockButtonPropertiesChanged()
        {
            System.Windows.Application.Current?.Dispatcher.BeginInvoke(
                new System.Action(NotifyUnlockButtonPropertiesChanged),
                System.Windows.Threading.DispatcherPriority.Input);
        }

        /// <summary>
        /// Updates scroll offset and notifies position change.
        /// Called from View when ScrollViewer scroll changes.
        /// </summary>
        public void UpdateScrollOffset(double horizontalOffset, double verticalOffset)
        {
            ScrollOffsetX = horizontalOffset;
            ScrollOffsetY = verticalOffset;
            OnPropertyChanged(nameof(ScreenUnlockButtonX));
            OnPropertyChanged(nameof(ScreenUnlockButtonY));
        }

        #endregion

        #region Nested Types

        /// <summary>
        /// Stores a snapshot of a DrawingElement for clipboard operations.
        /// </summary>
        private class DrawingElementSnapshot
        {
            public double X { get; set; }
            public double Y { get; set; }
            public double Width { get; }
            public double Height { get; }
            public double Opacity { get; }
            public string Text { get; }
            public TextFontSize FontSize { get; }
            public TextAlignment TextAlign { get; }
            public System.Windows.Media.Color TextColor { get; }
            public double TextOpacity { get; }
            public DrawingShapeType ShapeType { get; }

            public DrawingElementSnapshot(DrawingElement element)
            {
                X = element.X;
                Y = element.Y;
                Width = element.Width;
                Height = element.Height;
                Opacity = element.Opacity;
                Text = element.Text;
                FontSize = element.FontSize;
                TextAlign = element.TextAlign;
                TextColor = element.TextColor;
                TextOpacity = element.TextOpacity;
                ShapeType = element.ShapeType;
            }

            public DrawingElement CreateElement()
            {
                DrawingElement element = ShapeType switch
                {
                    DrawingShapeType.Initial => new InitialElement(),
                    DrawingShapeType.Action => new ActionElement(),
                    DrawingShapeType.Decision => new DecisionElement(),
                    DrawingShapeType.Terminal => new TerminalElement(),
                    DrawingShapeType.PredefinedAction => new PredefinedActionElement(),
                    DrawingShapeType.Textbox => new TextboxElement(),
                    _ => new ActionElement()
                };

                element.X = X;
                element.Y = Y;
                element.Width = Width;
                element.Height = Height;
                element.Opacity = Opacity;
                element.Text = Text;
                element.FontSize = FontSize;
                element.TextAlign = TextAlign;
                element.TextColor = TextColor;
                element.TextOpacity = TextOpacity;

                return element;
            }
        }

        #endregion
    }
}