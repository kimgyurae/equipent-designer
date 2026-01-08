using System;

namespace EquipmentDesigner.Models
{
    /// <summary>
    /// Abstract base class for all drawing elements in the process editor workspace
    /// </summary>
    public abstract class DrawingElement
    {
        private double _width = 1.0;
        private double _height = 1.0;

        /// <summary>
        /// Unique identifier for the drawing element
        /// </summary>
        public string Id { get; protected set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// X coordinate position
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// Y coordinate position
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// Width of the element (minimum 1.0)
        /// </summary>
        public double Width
        {
            get => _width;
            set => _width = value < 1.0 ? 1.0 : value;
        }

        /// <summary>
        /// Height of the element (minimum 1.0)
        /// </summary>
        public double Height
        {
            get => _height;
            set => _height = value < 1.0 ? 1.0 : value;
        }

        /// <summary>
        /// Z-index for layering order (higher values are on top)
        /// </summary>
        public int ZIndex { get; set; }

        /// <summary>
        /// Whether the element is currently selected for editing
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        /// Whether the element is locked (prevents editing)
        /// </summary>
        public bool IsLocked { get; set; }

        /// <summary>
        /// Shape type determined by derived class
        /// </summary>
        public abstract DrawingShapeType ShapeType { get; }

        /// <summary>
        /// Creates a deep copy of the element with a new Id
        /// </summary>
        public virtual DrawingElement Clone()
        {
            var clone = (DrawingElement)MemberwiseClone();
            clone.Id = Guid.NewGuid().ToString();
            return clone;
        }
    }
}
