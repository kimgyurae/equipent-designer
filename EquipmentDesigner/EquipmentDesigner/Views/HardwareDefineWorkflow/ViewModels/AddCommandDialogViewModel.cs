using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace EquipmentDesigner.ViewModels
{
    /// <summary>
    /// ViewModel for the Add Command dialog window.
    /// </summary>
    public class AddCommandDialogViewModel : ViewModelBase
    {
        private string _commandName = string.Empty;
        private string _description = string.Empty;

        /// <summary>
        /// Event raised when the dialog should close.
        /// </summary>
        public event EventHandler<CommandViewModel> RequestClose;

        public AddCommandDialogViewModel()
        {
            Parameters = new ObservableCollection<ParameterViewModel>();
            Parameters.CollectionChanged += OnParametersCollectionChanged;

            AddParameterCommand = new RelayCommand(ExecuteAddParameter);
            RemoveParameterCommand = new RelayCommand<ParameterViewModel>(ExecuteRemoveParameter);
            AddCommandCommand = new RelayCommand(ExecuteAddCommand, CanExecuteAddCommand);
            CancelCommand = new RelayCommand(ExecuteCancel);
        }

        /// <summary>
        /// Command name (required).
        /// </summary>
        public string CommandName
        {
            get => _commandName;
            set
            {
                if (SetProperty(ref _commandName, value))
                {
                    OnPropertyChanged(nameof(CanAddCommand));
                }
            }
        }

        /// <summary>
        /// Command description (required).
        /// </summary>
        public string Description
        {
            get => _description;
            set
            {
                if (SetProperty(ref _description, value))
                {
                    OnPropertyChanged(nameof(CanAddCommand));
                }
            }
        }

        /// <summary>
        /// Collection of parameters for this command.
        /// </summary>
        public ObservableCollection<ParameterViewModel> Parameters { get; }

        /// <summary>
        /// Returns true if all required fields are valid and all parameters are valid.
        /// </summary>
        public bool CanAddCommand =>
            !string.IsNullOrWhiteSpace(CommandName) &&
            !string.IsNullOrWhiteSpace(Description) &&
            AllParametersValid;

        /// <summary>
        /// Returns true if the parameters collection is empty.
        /// </summary>
        public bool HasNoParameters => Parameters.Count == 0;

        /// <summary>
        /// Returns true if all parameters have valid fields, or if there are no parameters.
        /// </summary>
        public bool AllParametersValid =>
            Parameters.Count == 0 || Parameters.All(p => p.IsValid);

        /// <summary>
        /// Command to add a new parameter.
        /// </summary>
        public ICommand AddParameterCommand { get; }

        /// <summary>
        /// Command to remove a parameter.
        /// </summary>
        public ICommand RemoveParameterCommand { get; }

        /// <summary>
        /// Command to add the command and close the dialog.
        /// </summary>
        public ICommand AddCommandCommand { get; }

        /// <summary>
        /// Command to cancel and close the dialog.
        /// </summary>
        public ICommand CancelCommand { get; }

        private void OnParametersCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Subscribe to property changes for new parameters
            if (e.NewItems != null)
            {
                foreach (ParameterViewModel param in e.NewItems)
                {
                    param.PropertyChanged += OnParameterPropertyChanged;
                }
            }

            // Unsubscribe from property changes for removed parameters
            if (e.OldItems != null)
            {
                foreach (ParameterViewModel param in e.OldItems)
                {
                    param.PropertyChanged -= OnParameterPropertyChanged;
                }
            }

            OnPropertyChanged(nameof(HasNoParameters));
            OnPropertyChanged(nameof(AllParametersValid));
            OnPropertyChanged(nameof(CanAddCommand));
        }

        private void OnParameterPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(AllParametersValid));
            OnPropertyChanged(nameof(CanAddCommand));
        }

        private void ExecuteAddParameter()
        {
            Parameters.Add(new ParameterViewModel());
        }

        private void ExecuteRemoveParameter(ParameterViewModel parameter)
        {
            if (parameter != null && Parameters.Contains(parameter))
            {
                Parameters.Remove(parameter);
            }
        }

        private void ExecuteAddCommand()
        {
            var commandViewModel = new CommandViewModel
            {
                Name = CommandName,
                Description = Description
            };

            foreach (var param in Parameters)
            {
                commandViewModel.Parameters.Add(new ParameterViewModel
                {
                    Name = param.Name,
                    Type = param.Type,
                    Description = param.Description
                });
            }

            RequestClose?.Invoke(this, commandViewModel);
        }

        private bool CanExecuteAddCommand()
        {
            return CanAddCommand;
        }

        private void ExecuteCancel()
        {
            RequestClose?.Invoke(this, null);
        }
    }
}
