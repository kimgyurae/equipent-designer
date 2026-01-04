using System;

namespace EquipmentDesigner.Views.ReusableComponents
{
    /// <summary>
    /// Helper class to reduce boilerplate for IBackdropHost implementation.
    /// Provides backdrop visibility state management with property change notification.
    /// </summary>
    public class BackdropHostMixin
    {
        private bool _isBackdropVisible;
        private readonly Action<string> _raisePropertyChanged;

        /// <summary>
        /// Creates a new BackdropHostMixin instance.
        /// </summary>
        /// <param name="raisePropertyChanged">Callback to invoke when IsBackdropVisible changes. Can be null.</param>
        public BackdropHostMixin(Action<string> raisePropertyChanged)
        {
            _raisePropertyChanged = raisePropertyChanged;
        }

        /// <summary>
        /// Gets or sets whether the backdrop should be visible.
        /// Invokes the property changed callback when value changes.
        /// </summary>
        public bool IsBackdropVisible
        {
            get => _isBackdropVisible;
            set
            {
                if (_isBackdropVisible != value)
                {
                    _isBackdropVisible = value;
                    _raisePropertyChanged?.Invoke(nameof(IsBackdropVisible));
                }
            }
        }

        /// <summary>
        /// Shows the backdrop by setting IsBackdropVisible to true.
        /// </summary>
        public void ShowBackdrop() => IsBackdropVisible = true;

        /// <summary>
        /// Hides the backdrop by setting IsBackdropVisible to false.
        /// </summary>
        public void HideBackdrop() => IsBackdropVisible = false;
    }
}
