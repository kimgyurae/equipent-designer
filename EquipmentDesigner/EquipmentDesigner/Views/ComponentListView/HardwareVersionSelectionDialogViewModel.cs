using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using EquipmentDesigner.Models;
using EquipmentDesigner.Resources;
using EquipmentDesigner.Services;

namespace EquipmentDesigner.ViewModels
{
    /// <summary>
    /// ViewModel for HardwareVersionSelectionDialog.
    /// Handles version list loading and selection logic.
    /// </summary>
    public class HardwareVersionSelectionDialogViewModel : ViewModelBase
    {
        private static readonly Brush SuccessBackground;
        private static readonly Brush SuccessForeground;
        private static readonly Brush InfoBackground;
        private static readonly Brush InfoForeground;
        private static readonly Brush NeutralBackground;
        private static readonly Brush NeutralForeground;

        static HardwareVersionSelectionDialogViewModel()
        {
            SuccessBackground = (Brush)Application.Current.FindResource("Brush.Status.Success.Background");
            SuccessForeground = (Brush)Application.Current.FindResource("Brush.Status.Success");
            InfoBackground = (Brush)Application.Current.FindResource("Brush.Status.Info.Background");
            InfoForeground = (Brush)Application.Current.FindResource("Brush.Status.Info");
            NeutralBackground = (Brush)Application.Current.FindResource("Brush.Status.Neutral.Background");
            NeutralForeground = (Brush)Application.Current.FindResource("Brush.Text.Secondary");
        }

        private HardwareType _hardwareType;
        private string _displayName;
        private bool _isLoading;
        private bool _isEmpty;

        /// <summary>
        /// Event raised when the dialog should close.
        /// The string parameter contains the selected WorkflowId, or null if cancelled.
        /// </summary>
        public event EventHandler<string> RequestClose;

        /// <summary>
        /// Gets or sets the hardware type for title display.
        /// </summary>
        public HardwareType HardwareType
        {
            get => _hardwareType;
            set
            {
                if (SetProperty(ref _hardwareType, value))
                {
                    OnPropertyChanged(nameof(DialogTitle));
                }
            }
        }

        /// <summary>
        /// Gets or sets the display name for the hardware.
        /// </summary>
        public string DisplayName
        {
            get => _displayName;
            set => SetProperty(ref _displayName, value);
        }

        /// <summary>
        /// Gets or sets whether data is currently loading.
        /// </summary>
        public bool IsLoading
        {
            get => _isLoading;
            private set
            {
                if (SetProperty(ref _isLoading, value))
                {
                    OnPropertyChanged(nameof(LoadingVisibility));
                    OnPropertyChanged(nameof(EmptyVisibility));
                    OnPropertyChanged(nameof(ListVisibility));
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the version list is empty.
        /// </summary>
        public bool IsEmpty
        {
            get => _isEmpty;
            private set
            {
                if (SetProperty(ref _isEmpty, value))
                {
                    OnPropertyChanged(nameof(LoadingVisibility));
                    OnPropertyChanged(nameof(EmptyVisibility));
                    OnPropertyChanged(nameof(ListVisibility));
                }
            }
        }

        /// <summary>
        /// Gets the dialog title based on hardware type.
        /// </summary>
        public string DialogTitle => _hardwareType switch
        {
            HardwareType.Equipment => Strings.VersionDialog_Equipment_Title,
            HardwareType.System => Strings.VersionDialog_System_Title,
            HardwareType.Unit => Strings.VersionDialog_Unit_Title,
            HardwareType.Device => Strings.VersionDialog_Device_Title,
            _ => "Versions"
        };

        /// <summary>
        /// Gets the visibility for the loading panel.
        /// </summary>
        public Visibility LoadingVisibility => _isLoading ? Visibility.Visible : Visibility.Collapsed;

        /// <summary>
        /// Gets the visibility for the empty state panel.
        /// </summary>
        public Visibility EmptyVisibility => !_isLoading && _isEmpty ? Visibility.Visible : Visibility.Collapsed;

        /// <summary>
        /// Gets the visibility for the version list.
        /// </summary>
        public Visibility ListVisibility => !_isLoading && !_isEmpty ? Visibility.Visible : Visibility.Collapsed;

        /// <summary>
        /// Collection of version items to display.
        /// </summary>
        public ObservableCollection<VersionItem> Versions { get; } = new ObservableCollection<VersionItem>();

        /// <summary>
        /// Command to select a version and close the dialog.
        /// </summary>
        public ICommand SelectVersionCommand { get; }

        /// <summary>
        /// Command to cancel and close the dialog.
        /// </summary>
        public ICommand CancelCommand { get; }

        public HardwareVersionSelectionDialogViewModel()
        {
            SelectVersionCommand = new RelayCommand<VersionItem>(ExecuteSelectVersion);
            CancelCommand = new RelayCommand(_ => RequestClose?.Invoke(this, null));
        }

        /// <summary>
        /// Initializes the ViewModel and starts loading versions.
        /// Call this after setting HardwareKey.
        /// </summary>
        public void Initialize()
        {
            LoadVersionsAsync();
        }

        private void ExecuteSelectVersion(VersionItem item)
        {
            if (item != null)
            {
                RequestClose?.Invoke(this, item.WorkflowId);
            }
        }

        private async void LoadVersionsAsync()
        {
            if (string.IsNullOrEmpty(HardwareKey))
            {
                IsEmpty = true;
                return;
            }

            IsLoading = true;
            Versions.Clear();

            try
            {
                var apiService = ServiceLocator.GetService<IHardwareApiService>();
                var response = await apiService.GetHardwareByHardwareKeyAsync(HardwareKey, 1, 100);

                if (response.Success && response.Data != null && response.Data.Count > 0)
                {
                    foreach (var version in response.Data)
                    {
                        var item = new VersionItem
                        {
                            WorkflowId = version.WorkflowId,
                            Version = version.Version,
                            Name = _displayName ?? "Unknown",
                            Description = version.Description ?? "",
                            State = version.State,
                            StateText = version.State.ToString(),
                            StateBackground = GetStateBackground(version.State),
                            StateForeground = GetStateForeground(version.State)
                        };
                        Versions.Add(item);
                    }
                    IsEmpty = false;
                }
                else
                {
                    IsEmpty = true;
                }
            }
            catch
            {
                IsEmpty = true;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private static Brush GetStateBackground(ComponentState state)
        {
            return state switch
            {
                ComponentState.Validated => SuccessBackground,
                ComponentState.Uploaded => InfoBackground,
                _ => NeutralBackground
            };
        }

        private static Brush GetStateForeground(ComponentState state)
        {
            return state switch
            {
                ComponentState.Validated => SuccessForeground,
                ComponentState.Uploaded => InfoForeground,
                _ => NeutralForeground
            };
        }
    }

    /// <summary>
    /// View model item for version display in the dialog.
    /// </summary>
    public class VersionItem
    {
        public string WorkflowId { get; set; }
        public string Version { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ComponentState State { get; set; }
        public string StateText { get; set; }
        public Brush StateBackground { get; set; }
        public Brush StateForeground { get; set; }
    }
}
