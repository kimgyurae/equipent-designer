using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EquipmentDesigner.ViewModels
{
    /// <summary>
    /// Base class for ViewModels implementing INotifyPropertyChanged.
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        private bool _isEditable = true;
        private string _version = "1.0.0";
        private string _hardwareKey;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets or sets whether the form is editable.
        /// When false, all input controls should be read-only.
        /// </summary>
        public virtual bool IsEditable
        {
            get => _isEditable;
            set => SetProperty(ref _isEditable, value);
        }

        /// <summary>
        /// Gets or sets the version information (e.g., v1.0.0).
        /// </summary>
        public virtual string Version
        {
            get => _version;
            set => SetProperty(ref _version, value);
        }

        /// <summary>
        /// Gets or sets the hardware unique identification key.
        /// All versions of the same hardware share this key.
        /// If null, Name is used as default (backward compatibility).
        /// </summary>
        public virtual string HardwareKey
        {
            get => _hardwareKey;
            set => SetProperty(ref _hardwareKey, value);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}