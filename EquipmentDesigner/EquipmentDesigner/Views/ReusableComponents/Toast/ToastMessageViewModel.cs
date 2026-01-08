using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;
using EquipmentDesigner.ViewModels;

namespace EquipmentDesigner.Controls
{
    /// <summary>
    /// ViewModel for an individual toast message.
    /// </summary>
    public class ToastMessageViewModel : INotifyPropertyChanged
    {
        private string _title;
        private string _description;
        private ToastType _toastType;
        private bool _isVisible = true;

        /// <summary>
        /// Unique identifier for this toast.
        /// </summary>
        public Guid Id { get; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets the toast title (required).
        /// </summary>
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        /// <summary>
        /// Gets or sets the toast description (optional).
        /// </summary>
        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        /// <summary>
        /// Gets or sets the type of toast (determines color scheme).
        /// </summary>
        public ToastType ToastType
        {
            get => _toastType;
            set
            {
                if (SetProperty(ref _toastType, value))
                {
                    OnPropertyChanged(nameof(BackgroundBrushKey));
                    OnPropertyChanged(nameof(BorderBrushKey));
                    OnPropertyChanged(nameof(IconPathData));
                    OnPropertyChanged(nameof(IconBrushKey));
                }
            }
        }

        /// <summary>
        /// Gets whether the toast is currently visible.
        /// </summary>
        public bool IsVisible
        {
            get => _isVisible;
            set => SetProperty(ref _isVisible, value);
        }

        /// <summary>
        /// Gets whether the description should be displayed.
        /// </summary>
        public bool HasDescription => !string.IsNullOrEmpty(Description);

        /// <summary>
        /// Resource key for the background brush.
        /// </summary>
        public string BackgroundBrushKey => ToastType switch
        {
            ToastType.Success => "Brush.Status.Success.Background",
            ToastType.Warning => "Brush.Status.Warning.Background",
            ToastType.Error => "Brush.Status.Danger.Background",
            _ => "Brush.Status.Info.Background"
        };

        /// <summary>
        /// Resource key for the border brush.
        /// </summary>
        public string BorderBrushKey => ToastType switch
        {
            ToastType.Success => "Brush.Status.Success.Border",
            ToastType.Warning => "Brush.Status.Warning.Border",
            ToastType.Error => "Brush.Status.Danger.Border",
            _ => "Brush.Status.Info.Border"
        };

        /// <summary>
        /// Resource key for the icon brush.
        /// </summary>
        public string IconBrushKey => ToastType switch
        {
            ToastType.Success => "Brush.Icon.Success",
            ToastType.Warning => "Brush.Icon.Warning",
            ToastType.Error => "Brush.Icon.Danger",
            _ => "Brush.Icon.Primary"
        };

        /// <summary>
        /// SVG path data for the toast type icon.
        /// </summary>
        public string IconPathData => ToastType switch
        {
            ToastType.Success => "M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z", // Checkmark circle
            ToastType.Warning => "M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z", // Warning triangle
            ToastType.Error => "M10 14l2-2m0 0l2-2m-2 2l-2-2m2 2l2 2m7-2a9 9 0 11-18 0 9 9 0 0118 0z", // X circle
            _ => "M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" // Info circle
        };

        /// <summary>
        /// Command to dismiss this toast.
        /// </summary>
        public ICommand DismissCommand { get; }

        /// <summary>
        /// Event raised when the toast requests dismissal.
        /// </summary>
        public event EventHandler DismissRequested;

        /// <summary>
        /// Creates a new toast message view model.
        /// </summary>
        public ToastMessageViewModel()
        {
            DismissCommand = new RelayCommand(_ => ExecuteDismiss());
        }

        /// <summary>
        /// Creates a new toast message view model with specified properties.
        /// </summary>
        public ToastMessageViewModel(string title, string description = null, ToastType type = ToastType.Info)
            : this()
        {
            Title = title;
            Description = description;
            ToastType = type;
        }

        private void ExecuteDismiss()
        {
            DismissRequested?.Invoke(this, EventArgs.Empty);
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

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

        #endregion
    }
}