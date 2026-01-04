using EquipmentDesigner.Models.Dtos;

namespace EquipmentDesigner.Views.HardwareDefineWorkflow
{
    /// <summary>
    /// ViewModel for a command parameter.
    /// </summary>
    public class ParameterViewModel : ViewModelBase
    {
        private string _name = string.Empty;
        private string _type;
        private string _description = string.Empty;

        /// <summary>
        /// Parameter name (required).
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
        /// Parameter type (required): String, Int, Float, Bool, etc.
        /// </summary>
        public string Type
        {
            get => _type;
            set
            {
                if (SetProperty(ref _type, value))
                    OnPropertyChanged(nameof(IsValid));
            }
        }

        /// <summary>
        /// Parameter description (required).
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
        /// Returns true if all required properties are provided.
        /// </summary>
        public bool IsValid =>
            !string.IsNullOrWhiteSpace(Name) &&
            !string.IsNullOrEmpty(Type) &&
            !string.IsNullOrWhiteSpace(Description);

        /// <summary>
        /// Converts this ViewModel to a DTO.
        /// </summary>
        public ParameterDto ToDto()
        {
            return new ParameterDto
            {
                Name = Name,
                Type = Type,
                Description = Description
            };
        }

        /// <summary>
        /// Creates a ViewModel from a DTO.
        /// </summary>
        public static ParameterViewModel FromDto(ParameterDto dto)
        {
            return new ParameterViewModel
            {
                Name = dto.Name ?? string.Empty,
                Type = dto.Type,
                Description = dto.Description ?? string.Empty
            };
        }
    }
}
