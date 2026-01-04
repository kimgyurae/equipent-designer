using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using EquipmentDesigner.Models.Dtos;

namespace EquipmentDesigner.Views.HardwareDefineWorkflow
{
    /// <summary>
    /// ViewModel for a command with parameters.
    /// </summary>
    public class CommandViewModel : ViewModelBase
    {
        private string _name = string.Empty;
        private string _description = string.Empty;

        public CommandViewModel()
        {
            Parameters = new ObservableCollection<ParameterViewModel>();
            Parameters.CollectionChanged += (s, e) => OnPropertyChanged(nameof(IsValid));

            AddParameterCommand = new RelayCommand(ExecuteAddParameter);
            RemoveParameterCommand = new RelayCommand<ParameterViewModel>(ExecuteRemoveParameter);
        }

        /// <summary>
        /// Command name (required).
        /// </summary>
        public string Name
        {
            get => _name;
            set
            {
                if (SetProperty(ref _name, value))
                    OnPropertyChanged(nameof(IsValid));
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
                    OnPropertyChanged(nameof(IsValid));
            }
        }

        /// <summary>
        /// Collection of parameters for this command.
        /// </summary>
        public ObservableCollection<ParameterViewModel> Parameters { get; }

        /// <summary>
        /// Returns true if all required properties and at least one parameter are provided.
        /// </summary>
        public bool IsValid =>
            !string.IsNullOrWhiteSpace(Name) &&
            !string.IsNullOrWhiteSpace(Description) &&
            Parameters.Count > 0;

        /// <summary>
        /// Command to add a new parameter.
        /// </summary>
        public ICommand AddParameterCommand { get; }

        /// <summary>
        /// Command to remove a parameter.
        /// </summary>
        public ICommand RemoveParameterCommand { get; }

        private void ExecuteAddParameter()
        {
            Parameters.Add(new ParameterViewModel());
        }

        private void ExecuteRemoveParameter(ParameterViewModel parameter)
        {
            if (parameter != null)
                Parameters.Remove(parameter);
        }

        /// <summary>
        /// Converts this ViewModel to a DTO.
        /// </summary>
        public CommandDto ToDto()
        {
            return new CommandDto
            {
                Name = Name,
                Description = Description,
                Parameters = Parameters.Select(p => p.ToDto()).ToList()
            };
        }

        /// <summary>
        /// Creates a ViewModel from a DTO.
        /// </summary>
        public static CommandViewModel FromDto(CommandDto dto)
        {
            var viewModel = new CommandViewModel
            {
                Name = dto.Name ?? string.Empty,
                Description = dto.Description ?? string.Empty
            };

            if (dto.Parameters != null)
            {
                foreach (var paramDto in dto.Parameters)
                    viewModel.Parameters.Add(ParameterViewModel.FromDto(paramDto));
            }

            return viewModel;
        }
    }
}
