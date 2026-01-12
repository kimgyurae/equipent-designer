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
        /// </summary>
        public bool TryStartTextEditing(DrawingElement element)
        {
            if (element == null || element.IsLocked || IsMultiSelectionMode)
                return false;

            // Can't edit text while in other edit modes
            if (EditModeState != EditModeState.Selected && EditModeState != EditModeState.None)
                return false;

            _textEditingElement = element;
            _originalText = element.Text;

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
        /// </summary>
        public void CommitTextEditing(string finalText)
        {
            if (!IsTextEditing || _textEditingElement == null) return;

            _textEditingElement.Text = finalText;

            // Final size calculation using Engine
            var result = TextEditingEngine.CalculateRequiredSize(_textEditContext, finalText);
            if (result.NeedsResize)
            {
                _textEditingElement.Height = result.RequiredHeight;
            }

            EndTextEditing(committed: true);
        }

        /// <summary>
        /// Cancels text editing and restores original text.
        /// </summary>
        public void CancelTextEditing()
        {
            if (!IsTextEditing || _textEditingElement == null) return;

            // Restore original text and height
            _textEditingElement.Text = _originalText;
            _textEditingElement.Height = _textEditContext.OriginalHeight;

            EndTextEditing(committed: false);
        }

        #endregion

        #region Text Editing Private Methods

        private void EndTextEditing(bool committed)
        {
            var element = _textEditingElement;
            var bounds = TextBoxBounds;

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