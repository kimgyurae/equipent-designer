using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using EquipmentDesigner.Utils;

namespace EquipmentDesigner.Views
{
    /// <summary>
    /// Edit mode selection options.
    /// </summary>
    public enum EditModeSelection
    {
        /// <summary>
        /// Edit the original data directly (현재 버전 변경하기).
        /// </summary>
        DirectEdit,

        /// <summary>
        /// Create a new version with the same hardware key (새로운 버전 만들기).
        /// </summary>
        CreateNewVersion,

        /// <summary>
        /// Create a completely new hardware with new GUID and ID (복사 후 새로운 하드웨어 정의하기).
        /// </summary>
        CreateNewHardware
    }

    /// <summary>
    /// Dialog for selecting the edit mode when entering edit mode from read-only state.
    /// </summary>
    public partial class EditModeSelectionDialog : Window, INotifyPropertyChanged
    {
        private static readonly Brush DefaultBorderBrush;
        private static readonly Brush SelectedBorderBrush;
        private static readonly Brush SelectedBackgroundBrush;
        private static readonly Brush TransparentBrush;

        static EditModeSelectionDialog()
        {
            DefaultBorderBrush = (Brush)Application.Current.FindResource("Brush.Border.Primary");
            SelectedBorderBrush = new SolidColorBrush(Color.FromRgb(0x2B, 0x7F, 0xFF)); // #2B7FFF
            SelectedBackgroundBrush = new SolidColorBrush(Color.FromRgb(0xEF, 0xF6, 0xFF)); // #EFF6FF
            TransparentBrush = Brushes.Transparent;
        }

        private EditModeSelection _selectedMode = EditModeSelection.CreateNewVersion;
        private string _currentVersion = "1.0.0";
        private string _majorVersion = "1";
        private string _minorVersion = "0";
        private string _patchVersion = "0";

        /// <summary>
        /// Gets or sets the selected edit mode.
        /// </summary>
        public EditModeSelection SelectedMode
        {
            get => _selectedMode;
            private set
            {
                if (_selectedMode != value)
                {
                    _selectedMode = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsConfirmEnabled));
                }
            }
        }

        /// <summary>
        /// Gets whether the user confirmed the selection.
        /// </summary>
        public bool IsConfirmed { get; private set; }

        /// <summary>
        /// Gets or sets the current version string.
        /// </summary>
        public string CurrentVersion
        {
            get => _currentVersion;
            set
            {
                if (_currentVersion != value && VersionHelper.IsValid(value))
                {
                    _currentVersion = value;
                    // Initialize version inputs with current version
                    _majorVersion = VersionHelper.GetMajor(value).ToString();
                    _minorVersion = VersionHelper.GetMinor(value).ToString();
                    _patchVersion = VersionHelper.GetPatch(value).ToString();
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(MajorVersion));
                    OnPropertyChanged(nameof(MinorVersion));
                    OnPropertyChanged(nameof(PatchVersion));
                    OnPropertyChanged(nameof(NewVersionDisplay));
                    OnPropertyChanged(nameof(IsVersionValid));
                    OnPropertyChanged(nameof(IsConfirmEnabled));
                }
            }
        }

        /// <summary>
        /// Gets or sets the major version input.
        /// </summary>
        public string MajorVersion
        {
            get => _majorVersion;
            set
            {
                if (_majorVersion != value)
                {
                    _majorVersion = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(NewVersionDisplay));
                    OnPropertyChanged(nameof(IsVersionValid));
                    OnPropertyChanged(nameof(IsMajorEmpty));
                    OnPropertyChanged(nameof(IsConfirmEnabled));
                }
            }
        }

        /// <summary>
        /// Gets or sets the minor version input.
        /// </summary>
        public string MinorVersion
        {
            get => _minorVersion;
            set
            {
                if (_minorVersion != value)
                {
                    _minorVersion = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(NewVersionDisplay));
                    OnPropertyChanged(nameof(IsVersionValid));
                    OnPropertyChanged(nameof(IsMinorEmpty));
                    OnPropertyChanged(nameof(IsConfirmEnabled));
                }
            }
        }

        /// <summary>
        /// Gets or sets the patch version input.
        /// </summary>
        public string PatchVersion
        {
            get => _patchVersion;
            set
            {
                if (_patchVersion != value)
                {
                    _patchVersion = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(NewVersionDisplay));
                    OnPropertyChanged(nameof(IsVersionValid));
                    OnPropertyChanged(nameof(IsPatchEmpty));
                    OnPropertyChanged(nameof(IsConfirmEnabled));
                }
            }
        }

        /// <summary>
        /// Gets the display string for the new version.
        /// </summary>
        public string NewVersionDisplay
        {
            get
            {
                var major = string.IsNullOrWhiteSpace(_majorVersion) ? "?" : _majorVersion;
                var minor = string.IsNullOrWhiteSpace(_minorVersion) ? "?" : _minorVersion;
                var patch = string.IsNullOrWhiteSpace(_patchVersion) ? "?" : _patchVersion;
                return $"{major}.{minor}.{patch}";
            }
        }

        /// <summary>
        /// Gets whether the new version is valid (higher than current version).
        /// </summary>
        public bool IsVersionValid
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_majorVersion) ||
                    string.IsNullOrWhiteSpace(_minorVersion) ||
                    string.IsNullOrWhiteSpace(_patchVersion))
                {
                    return false;
                }

                if (!int.TryParse(_majorVersion, out var major) || major < 0 ||
                    !int.TryParse(_minorVersion, out var minor) || minor < 0 ||
                    !int.TryParse(_patchVersion, out var patch) || patch < 0)
                {
                    return false;
                }

                try
                {
                    var newVersion = VersionHelper.CreateVersionString(major, minor, patch);
                    // New version must be higher than current version
                    return VersionHelper.IsHigherThan(newVersion, _currentVersion);
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Gets whether the major version input is empty.
        /// </summary>
        public bool IsMajorEmpty => string.IsNullOrWhiteSpace(_majorVersion);

        /// <summary>
        /// Gets whether the minor version input is empty.
        /// </summary>
        public bool IsMinorEmpty => string.IsNullOrWhiteSpace(_minorVersion);

        /// <summary>
        /// Gets whether the patch version input is empty.
        /// </summary>
        public bool IsPatchEmpty => string.IsNullOrWhiteSpace(_patchVersion);

        /// <summary>
        /// Gets whether the confirm button should be enabled.
        /// </summary>
        public bool IsConfirmEnabled
        {
            get
            {
                // CreateNewVersion mode requires valid version
                if (SelectedMode == EditModeSelection.CreateNewVersion)
                {
                    return IsVersionValid;
                }
                // Other modes are always enabled
                return true;
            }
        }

        /// <summary>
        /// Gets the new version string if valid, otherwise null.
        /// </summary>
        public string NewVersion
        {
            get
            {
                if (!IsVersionValid)
                    return null;

                return VersionHelper.CreateVersionString(
                    int.Parse(_majorVersion),
                    int.Parse(_minorVersion),
                    int.Parse(_patchVersion));
            }
        }

        public EditModeSelectionDialog()
        {
            InitializeComponent();
            DataContext = this;

            // Default selection: CreateNewVersion (recommended)
            UpdateOptionVisuals();
        }

        private void DirectEditOption_Click(object sender, MouseButtonEventArgs e)
        {
            SelectedMode = EditModeSelection.DirectEdit;
            UpdateOptionVisuals();
        }

        private void CreateNewVersionOption_Click(object sender, MouseButtonEventArgs e)
        {
            SelectedMode = EditModeSelection.CreateNewVersion;
            UpdateOptionVisuals();
        }

        private void CreateNewHardwareOption_Click(object sender, MouseButtonEventArgs e)
        {
            SelectedMode = EditModeSelection.CreateNewHardware;
            UpdateOptionVisuals();
        }

        private void UpdateOptionVisuals()
        {
            switch (SelectedMode)
            {
                case EditModeSelection.DirectEdit:
                    DirectEditOption.BorderBrush = SelectedBorderBrush;
                    DirectEditOption.Background = SelectedBackgroundBrush;
                    CreateNewVersionOption.BorderBrush = DefaultBorderBrush;
                    CreateNewVersionOption.Background = TransparentBrush;
                    CreateNewHardwareOption.BorderBrush = DefaultBorderBrush;
                    CreateNewHardwareOption.Background = TransparentBrush;
                    break;

                case EditModeSelection.CreateNewVersion:
                    DirectEditOption.BorderBrush = DefaultBorderBrush;
                    DirectEditOption.Background = TransparentBrush;
                    CreateNewVersionOption.BorderBrush = SelectedBorderBrush;
                    CreateNewVersionOption.Background = SelectedBackgroundBrush;
                    CreateNewHardwareOption.BorderBrush = DefaultBorderBrush;
                    CreateNewHardwareOption.Background = TransparentBrush;
                    break;

                case EditModeSelection.CreateNewHardware:
                    DirectEditOption.BorderBrush = DefaultBorderBrush;
                    DirectEditOption.Background = TransparentBrush;
                    CreateNewVersionOption.BorderBrush = DefaultBorderBrush;
                    CreateNewVersionOption.Background = TransparentBrush;
                    CreateNewHardwareOption.BorderBrush = SelectedBorderBrush;
                    CreateNewHardwareOption.Background = SelectedBackgroundBrush;
                    break;
            }
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            IsConfirmed = true;
            DialogResult = true;
            Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            IsConfirmed = false;
            DialogResult = false;
            Close();
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    /// <summary>
    /// Converter that returns "Error" tag when boolean is true (empty), otherwise null.
    /// </summary>
    public class BoolToErrorTagConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool isEmpty && isEmpty)
            {
                return "Error";
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}