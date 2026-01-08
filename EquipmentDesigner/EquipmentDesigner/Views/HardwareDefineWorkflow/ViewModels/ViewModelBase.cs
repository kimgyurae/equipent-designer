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