using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using EquipmentDesigner.Models;
using EquipmentDesigner.Models.Rules;

namespace EquipmentDesigner.ViewModels
{
    /// <summary>
    /// ViewModel for the ViolationPopup control.
    /// Manages the display of rule violations for a selected element.
    /// </summary>
    public class ViolationPopupViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private DrawingElement _element;
        private bool _isVisible;
        private double _x;
        private double _y;

        /// <summary>
        /// The element whose violations are being displayed.
        /// </summary>
        public DrawingElement Element
        {
            get => _element;
            set
            {
                if (_element != value)
                {
                    _element = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Violations));
                    OnPropertyChanged(nameof(HasViolations));
                }
            }
        }

        /// <summary>
        /// Whether the popup is visible.
        /// </summary>
        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                if (_isVisible != value)
                {
                    _isVisible = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// X position for the popup in screen coordinates.
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
        /// Y position for the popup in screen coordinates.
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
        /// Gets the violations for the current element.
        /// </summary>
        public IReadOnlyList<RuleViolation> Violations =>
            _element?.Violations ?? (IReadOnlyList<RuleViolation>)System.Array.Empty<RuleViolation>();

        /// <summary>
        /// Gets whether the current element has violations.
        /// </summary>
        public bool HasViolations => _element?.HasViolations == true;

        /// <summary>
        /// Updates the popup state based on the selected element and position.
        /// </summary>
        /// <param name="element">The selected element (null if none selected or multi-selection).</param>
        /// <param name="x">Screen X position for the popup.</param>
        /// <param name="y">Screen Y position for the popup.</param>
        public void Update(DrawingElement element, double x, double y)
        {
            Element = element;
            X = x;
            Y = y;
            IsVisible = element != null && element.HasViolations;
        }

        /// <summary>
        /// Hides the popup.
        /// </summary>
        public void Hide()
        {
            IsVisible = false;
            Element = null;
        }

        /// <summary>
        /// Refreshes the violations display (call when element's connections change).
        /// </summary>
        public void RefreshViolations()
        {
            OnPropertyChanged(nameof(Violations));
            OnPropertyChanged(nameof(HasViolations));

            // Update visibility based on current violation state
            if (_element != null)
            {
                IsVisible = _element.HasViolations;
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
