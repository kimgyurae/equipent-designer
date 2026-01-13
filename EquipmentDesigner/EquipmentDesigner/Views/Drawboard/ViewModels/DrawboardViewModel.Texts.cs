using System;
using System.Windows;
using EquipmentDesigner.Models;
using EquipmentDesigner.Views.Drawboard.UMLEngine;
using EquipmentDesigner.Views.Drawboard.UMLEngine.Contexts;

namespace EquipmentDesigner.ViewModels
{
    /// <summary>
    /// Partial class containing text editing operations.
    /// Follows the Engine delegation pattern: Context -> Engine -> Result.
    /// </summary>
    public partial class DrawboardViewModel
    {
        #region Text Editing Fields

        private TextEditContext _textEditContext;
        private DrawingElement _textEditingElement;
        private bool _isTextEditing;
        private Rect _textBoxBounds;
        private string _originalText;
        private double _originalTextOpacity;

        #endregion

        #region Text Editing Properties

        /// <summary>
        /// Whether text editing mode is active.
        /// </summary>
        public bool IsTextEditing
        {
            get => _isTextEditing;
            private set => SetProperty(ref _isTextEditing, value);
        }

        /// <summary>
        /// Bounds for the inline text editing TextBox (in canvas coordinates).
        /// </summary>
        public Rect TextBoxBounds
        {
            get => _textBoxBounds;
            private set => SetProperty(ref _textBoxBounds, value);
        }

        /// <summary>
        /// Element currently being text-edited.
        /// </summary>
        public DrawingElement TextEditingElement => _textEditingElement;

        #endregion

        #region Text Editing Events

        /// <summary>
        /// Raised when text editing starts. View should show TextBox.
        /// </summary>
        public event EventHandler<TextEditingEventArgs> TextEditingStarted;

        /// <summary>
        /// Raised when text editing ends. View should hide TextBox.
        /// </summary>
        public event EventHandler<TextEditingEventArgs> TextEditingEnded;

        /// <summary>
        /// Raised when TextBox bounds need to be updated (e.g., after resize).
        /// </summary>
        public event EventHandler<TextEditingEventArgs> TextBoxBoundsChanged;

        #endregion

        #region Text Editing Public Methods

        /// <summary>
        /// Starts inline text editing for an element.
        /// Called from View on double-click.
        /// Clears all selections before entering text edit mode to remove adorners.
        /// </summary>
        public bool TryStartTextEditing(DrawingElement element)
        {
            if (element == null || element.IsLocked)
                return false;

            // Can't edit text while in other edit modes (except Selected, MultiSelected, or None)
            if (EditModeState != EditModeState.Selected && 
                EditModeState != EditModeState.MultiSelected &&
                EditModeState != EditModeState.None)
                return false;

            // Clear all selections before entering text edit mode
            // This removes the adorner so it doesn't overlay the text editing area
            ClearAllSelections();

            _textEditingElement = element;
            _originalText = element.Text;
            _originalTextOpacity = element.TextOpacity;
            element.TextOpacity = 0;

            // Create context and calculate bounds using Engine
            _textEditContext = TextEditingEngine.CreateTextEditContext(element);
            var result = TextEditingEngine.CalculateTextEditBounds(_textEditContext);

            TextBoxBounds = result.TextSafeBounds;
            IsTextEditing = true;
            EditModeState = EditModeState.TextEditing;

            // Notify View
            TextEditingStarted?.Invoke(this, new TextEditingEventArgs(
                element,
                result.TextSafeBounds,
                element.Text,
                TextEditingEngine.GetFontSizePixels(element.FontSize),
                ConvertTextAlignment(element.TextAlign)));

            return true;
        }

        /// <summary>
        /// Updates text content during editing.
        /// Called from View when TextBox text changes.
        /// </summary>
        public void UpdateTextContent(string newText)
        {
            if (!IsTextEditing || _textEditingElement == null) return;

            // Calculate if resize is needed using Engine
            var result = TextEditingEngine.CalculateRequiredSize(_textEditContext, newText);

            if (result.NeedsResize)
            {
                // Grow element to fit text
                _textEditingElement.Height = result.RequiredHeight;

                // Update context with new bounds
                _textEditContext = TextEditingEngine.CreateTextEditContext(_textEditingElement);

                // Update TextBox bounds
                TextBoxBounds = result.TextSafeBounds;

                // Notify View about bounds change
                TextBoxBoundsChanged?.Invoke(this, new TextEditingEventArgs(
                    _textEditingElement,
                    result.TextSafeBounds,
                    newText,
                    TextEditingEngine.GetFontSizePixels(_textEditingElement.FontSize),
                    ConvertTextAlignment(_textEditingElement.TextAlign)));
            }
        }

        /// <summary>
        /// Commits text changes and ends editing.
        /// Auto-deletes TextboxElement if text is empty or whitespace-only.
        /// </summary>
        public void CommitTextEditing(string finalText)
        {
            if (!IsTextEditing || _textEditingElement == null) return;

            var elementToEdit = _textEditingElement;
            
            // Check if TextboxElement should be auto-deleted (empty or whitespace-only text)
            bool shouldAutoDelete = elementToEdit.ShapeType == DrawingShapeType.Textbox 
                && string.IsNullOrWhiteSpace(finalText);

            if (shouldAutoDelete)
            {
                // Clear selection before removing element
                if (SelectedElement == elementToEdit)
                {
                    SelectedElement = null;
                }
                _selectedElements.Remove(elementToEdit);
                elementToEdit.IsSelected = false;
                
                // End text editing first
                EndTextEditing(committed: false);
                
                // Remove from canvas
                CurrentSteps.Remove(elementToEdit);
                return;
            }

            elementToEdit.Text = finalText;

            // Final size calculation using Engine
            var result = TextEditingEngine.CalculateRequiredSize(_textEditContext, finalText);
            if (result.NeedsResize)
            {
                elementToEdit.Height = result.RequiredHeight;
            }

            EndTextEditing(committed: true);
        }

        /// <summary>
        /// Cancels text editing and restores original text.
        /// Auto-deletes TextboxElement if originalText was empty (newly created element).
        /// </summary>
        public void CancelTextEditing()
        {
            if (!IsTextEditing || _textEditingElement == null) return;

            var elementToEdit = _textEditingElement;
            var originalHeight = _textEditContext.OriginalHeight;
            var originalTextValue = _originalText;

            // Check if TextboxElement should be auto-deleted (originalText was empty)
            bool shouldAutoDelete = elementToEdit.ShapeType == DrawingShapeType.Textbox 
                && string.IsNullOrWhiteSpace(originalTextValue);

            if (shouldAutoDelete)
            {
                // Clear selection before removing element
                if (SelectedElement == elementToEdit)
                {
                    SelectedElement = null;
                }
                _selectedElements.Remove(elementToEdit);
                elementToEdit.IsSelected = false;
                
                // End text editing first
                EndTextEditing(committed: false);
                
                // Remove from canvas
                CurrentSteps.Remove(elementToEdit);
                return;
            }

            // Restore original text and height
            elementToEdit.Text = originalTextValue;
            elementToEdit.Height = originalHeight;

            EndTextEditing(committed: false);
        }

        #endregion

        #region Text Editing Private Methods

        private void EndTextEditing(bool committed)
        {
            var element = _textEditingElement;
            var bounds = TextBoxBounds;

            // Restore text opacity before clearing element reference
            if (element != null)
            {
                element.TextOpacity = _originalTextOpacity;
            }

            _textEditingElement = null;
            _originalText = null;
            IsTextEditing = false;
            EditModeState = SelectedElement != null ? EditModeState.Selected : EditModeState.None;

            // Notify View
            TextEditingEnded?.Invoke(this, new TextEditingEventArgs(element, bounds, null, 0, System.Windows.TextAlignment.Center));
        }

        /// <summary>
        /// Converts Models.TextAlignment to System.Windows.TextAlignment.
        /// </summary>
        private static System.Windows.TextAlignment ConvertTextAlignment(Models.TextAlignment modelAlignment)
        {
            return modelAlignment switch
            {
                Models.TextAlignment.Left => System.Windows.TextAlignment.Left,
                Models.TextAlignment.Center => System.Windows.TextAlignment.Center,
                Models.TextAlignment.Right => System.Windows.TextAlignment.Right,
                _ => System.Windows.TextAlignment.Center
            };
        }

        #endregion
    }

    /// <summary>
    /// Event arguments for text editing events.
    /// </summary>
    public class TextEditingEventArgs : EventArgs
    {
        public DrawingElement Element { get; }
        public Rect TextBoxBounds { get; }
        public string InitialText { get; }
        public double FontSize { get; }
        public System.Windows.TextAlignment TextAlignment { get; }

        public TextEditingEventArgs(
            DrawingElement element,
            Rect textBoxBounds,
            string initialText,
            double fontSize,
            System.Windows.TextAlignment textAlignment)
        {
            Element = element;
            TextBoxBounds = textBoxBounds;
            InitialText = initialText;
            FontSize = fontSize;
            TextAlignment = textAlignment;
        }
    }
}