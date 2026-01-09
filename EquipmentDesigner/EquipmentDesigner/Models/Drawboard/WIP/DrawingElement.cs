using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace EquipmentDesigner.Models
{
    /// <summary>
    /// Abstract base class for all drawing elements in the process editor workspace
    /// </summary>
    public abstract class DrawingElement : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private double _x;
        private double _y;
        private double _width = 1.0;
        private double _height = 1.0;
        private double _opacity = 1.0;
        private int _zIndex;
        private bool _isSelected;
        private bool _isLocked;

        /// <summary>
        /// Unique identifier for the drawing element
        /// </summary>
        public string Id { get; protected set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// X coordinate position
        /// </summary>
        public double X
        {
            get => _x;
            set
            {
                if (_x != value)
                {
                    _x = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Y coordinate position
        /// </summary>
        public double Y
        {
            get => _y;
            set
            {
                if (_y != value)
                {
                    _y = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Width of the element (minimum 1.0)
        /// </summary>
        public double Width
        {
            get => _width;
            set
            {
                var newValue = Math.Max(1.0, value);
                if (_width != newValue)
                {
                    _width = newValue;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Height of the element (minimum 1.0)
        /// </summary>
        public double Height
        {
            get => _height;
            set
            {
                var newValue = Math.Max(1.0, value);
                if (_height != newValue)
                {
                    _height = newValue;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Opacity of the element (0.0 to 1.0)
        /// </summary>
        public double Opacity
        {
            get => _opacity;
            set
            {
                if (_opacity != value)
                {
                    _opacity = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Z-index for layering order (higher values are on top)
        /// </summary>
        public int ZIndex
        {
            get => _zIndex;
            set
            {
                if (_zIndex != value)
                {
                    _zIndex = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Whether the element is currently selected for editing
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Whether the element is locked (prevents editing)
        /// </summary>
        public bool IsLocked
        {
            get => _isLocked;
            set
            {
                if (_isLocked != value)
                {
                    _isLocked = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Shape type determined by derived class
        /// </summary>
        public abstract DrawingShapeType ShapeType { get; }

        /// <summary>
        /// Gets the bounding rectangle of this element.
        /// </summary>
        public Rect Bounds => new Rect(X, Y, Width, Height);

        /// <summary>
        /// Creates a deep copy of the element with a new Id
        /// </summary>
        public virtual DrawingElement Clone()
        {
            var clone = (DrawingElement)MemberwiseClone();
            clone.Id = Guid.NewGuid().ToString();
            return clone;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}