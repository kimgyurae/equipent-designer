using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace EquipmentDesigner.Models
{
    /// <summary>
    /// Textbox element for text annotations on the drawing canvas
    /// </summary>
    public class TextboxElement : DrawingElement
    {
        private string _text = string.Empty;
        private TextFontSize _fontSize = TextFontSize.Base;
        private TextAlignment _textAlignment = TextAlignment.Left;
        private Color _textColor = Colors.Black;
        private double _textOpacity = 1.0;

        /// <inheritdoc />
        public override DrawingShapeType ShapeType => DrawingShapeType.Textbox;

        /// <summary>
        /// The text content of the textbox
        /// </summary>
        public string Text
        {
            get => _text;
            set
            {
                if (_text != value)
                {
                    _text = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Font size of the text (XS=10, Base=14, XL=20)
        /// </summary>
        public TextFontSize FontSize
        {
            get => _fontSize;
            set
            {
                if (_fontSize != value)
                {
                    _fontSize = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Text alignment (Left, Center, Right)
        /// </summary>
        public TextAlignment TextAlign
        {
            get => _textAlignment;
            set
            {
                if (_textAlignment != value)
                {
                    _textAlignment = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Text color (currently fixed to Black, prepared for future customization)
        /// </summary>
        public Color TextColor
        {
            get => _textColor;
            set
            {
                if (_textColor != value)
                {
                    _textColor = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Text opacity (currently fixed to 1.0, prepared for future customization)
        /// </summary>
        public double TextOpacity
        {
            get => _textOpacity;
            set
            {
                if (_textOpacity != value)
                {
                    _textOpacity = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <inheritdoc />
        public override DrawingElement Clone()
        {
            var clone = (TextboxElement)base.Clone();
            clone._text = _text;
            clone._fontSize = _fontSize;
            clone._textAlignment = _textAlignment;
            clone._textColor = _textColor;
            clone._textOpacity = _textOpacity;
            return clone;
        }
    }
}
