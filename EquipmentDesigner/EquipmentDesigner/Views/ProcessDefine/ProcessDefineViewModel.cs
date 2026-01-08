using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using EquipmentDesigner.Services;

namespace EquipmentDesigner.ViewModels
{
    /// <summary>
    /// ViewModel for Process definition view.
    /// </summary>
    public class ProcessDefineViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool _showBackButton;

        public ProcessDefineViewModel(bool showBackButton = true)
        {
            _showBackButton = showBackButton;
            BackToHardwareDefineCommand = new RelayCommand(ExecuteBackToHardwareDefine);
        }

        /// <summary>
        /// Whether to show the back button (hidden when opened in a new window).
        /// </summary>
        public bool ShowBackButton
        {
            get => _showBackButton;
            set
            {
                if (_showBackButton != value)
                {
                    _showBackButton = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Command to navigate back to hardware define view.
        /// </summary>
        public ICommand BackToHardwareDefineCommand { get; }

        private void ExecuteBackToHardwareDefine()
        {
            NavigationService.Instance.NavigateBackFromProcessDefine();
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
