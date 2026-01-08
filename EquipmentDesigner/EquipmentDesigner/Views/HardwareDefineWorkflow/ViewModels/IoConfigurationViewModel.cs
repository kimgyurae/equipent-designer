using EquipmentDesigner.Models;

namespace EquipmentDesigner.ViewModels
{
    /// <summary>
    /// ViewModel for an IO configuration.
    /// </summary>
    public class IoConfigurationViewModel : ViewModelBase
    {
        private string _name = string.Empty;
        private string _ioType;
        private string _address = string.Empty;
        private string _description = string.Empty;

        /// <summary>
        /// IO name (required).
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
        /// IO type (required): Input, Output, AnalogInput, AnalogOutput.
        /// </summary>
        public string IoType
        {
            get => _ioType;
            set
            {
                if (SetProperty(ref _ioType, value))
                    OnPropertyChanged(nameof(IsValid));
            }
        }

        /// <summary>
        /// IO address (optional).
        /// </summary>
        public string Address
        {
            get => _address;
            set => SetProperty(ref _address, value);
        }

        /// <summary>
        /// IO description (optional).
        /// </summary>
        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        /// <summary>
        /// Returns true if required properties (Name and IoType) are provided.
        /// </summary>
        public bool IsValid =>
            !string.IsNullOrWhiteSpace(Name) &&
            !string.IsNullOrEmpty(IoType);

        /// <summary>
        /// Converts this ViewModel to a DTO.
        /// </summary>
        public IoInfoDto ToDto()
        {
            return new IoInfoDto
            {
                Name = Name,
                IoType = IoType,
                Address = Address,
                Description = Description
            };
        }

        /// <summary>
        /// Creates a ViewModel from a DTO.
        /// </summary>
        public static IoConfigurationViewModel FromDto(IoInfoDto dto)
        {
            return new IoConfigurationViewModel
            {
                Name = dto.Name ?? string.Empty,
                IoType = dto.IoType,
                Address = dto.Address ?? string.Empty,
                Description = dto.Description ?? string.Empty
            };
        }
    }
}
