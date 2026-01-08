using EquipmentDesigner.ViewModels;
using FluentAssertions;
using Xunit;

namespace EquipmentDesigner.Tests.Views.HardwareDefineWorkflow
{
    public class ParameterViewModelTests
    {
        #region Property Initialization

        [Fact]
        public void Constructor_Default_InitializesWithEmptyName()
        {
            // Arrange & Act
            var viewModel = new ParameterViewModel();

            // Assert
            viewModel.Name.Should().BeEmpty();
        }

        [Fact]
        public void Constructor_Default_InitializesWithNullType()
        {
            // Arrange & Act
            var viewModel = new ParameterViewModel();

            // Assert
            viewModel.Type.Should().BeNull();
        }

        [Fact]
        public void Constructor_Default_InitializesWithEmptyDescription()
        {
            // Arrange & Act
            var viewModel = new ParameterViewModel();

            // Assert
            viewModel.Description.Should().BeEmpty();
        }

        #endregion

        #region Property Change Notification

        [Fact]
        public void Name_WhenSet_RaisesPropertyChanged()
        {
            // Arrange
            var viewModel = new ParameterViewModel();
            var propertyChangedRaised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ParameterViewModel.Name))
                    propertyChangedRaised = true;
            };

            // Act
            viewModel.Name = "TestParam";

            // Assert
            propertyChangedRaised.Should().BeTrue();
        }

        [Fact]
        public void Type_WhenSet_RaisesPropertyChanged()
        {
            // Arrange
            var viewModel = new ParameterViewModel();
            var propertyChangedRaised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ParameterViewModel.Type))
                    propertyChangedRaised = true;
            };

            // Act
            viewModel.Type = "String";

            // Assert
            propertyChangedRaised.Should().BeTrue();
        }

        [Fact]
        public void Description_WhenSet_RaisesPropertyChanged()
        {
            // Arrange
            var viewModel = new ParameterViewModel();
            var propertyChangedRaised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ParameterViewModel.Description))
                    propertyChangedRaised = true;
            };

            // Act
            viewModel.Description = "Test description";

            // Assert
            propertyChangedRaised.Should().BeTrue();
        }

        [Fact]
        public void Name_WhenChanged_RaisesPropertyChangedForIsValid()
        {
            // Arrange
            var viewModel = new ParameterViewModel();
            var propertyChangedRaised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ParameterViewModel.IsValid))
                    propertyChangedRaised = true;
            };

            // Act
            viewModel.Name = "TestParam";

            // Assert
            propertyChangedRaised.Should().BeTrue();
        }

        [Fact]
        public void Type_WhenChanged_RaisesPropertyChangedForIsValid()
        {
            // Arrange
            var viewModel = new ParameterViewModel();
            var propertyChangedRaised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ParameterViewModel.IsValid))
                    propertyChangedRaised = true;
            };

            // Act
            viewModel.Type = "String";

            // Assert
            propertyChangedRaised.Should().BeTrue();
        }

        [Fact]
        public void Description_WhenChanged_RaisesPropertyChangedForIsValid()
        {
            // Arrange
            var viewModel = new ParameterViewModel();
            var propertyChangedRaised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ParameterViewModel.IsValid))
                    propertyChangedRaised = true;
            };

            // Act
            viewModel.Description = "Test description";

            // Assert
            propertyChangedRaised.Should().BeTrue();
        }

        #endregion

        #region Validation

        [Fact]
        public void IsValid_WhenNameIsEmpty_ReturnsFalse()
        {
            // Arrange
            var viewModel = new ParameterViewModel
            {
                Name = "",
                Type = "String",
                Description = "Test"
            };

            // Act & Assert
            viewModel.IsValid.Should().BeFalse();
        }

        [Fact]
        public void IsValid_WhenTypeIsNull_ReturnsFalse()
        {
            // Arrange
            var viewModel = new ParameterViewModel
            {
                Name = "TestParam",
                Type = null,
                Description = "Test"
            };

            // Act & Assert
            viewModel.IsValid.Should().BeFalse();
        }

        [Fact]
        public void IsValid_WhenDescriptionIsEmpty_ReturnsFalse()
        {
            // Arrange
            var viewModel = new ParameterViewModel
            {
                Name = "TestParam",
                Type = "String",
                Description = ""
            };

            // Act & Assert
            viewModel.IsValid.Should().BeFalse();
        }

        [Fact]
        public void IsValid_WhenAllPropertiesProvided_ReturnsTrue()
        {
            // Arrange
            var viewModel = new ParameterViewModel
            {
                Name = "TestParam",
                Type = "String",
                Description = "Test description"
            };

            // Act & Assert
            viewModel.IsValid.Should().BeTrue();
        }

        #endregion

        #region Data Conversion

        [Fact]
        public void ToDto_ReturnsParameterDtoWithAllPropertiesMapped()
        {
            // Arrange
            var viewModel = new ParameterViewModel
            {
                Name = "TestParam",
                Type = "Int",
                Description = "Test description"
            };

            // Act
            var dto = viewModel.ToDto();

            // Assert
            dto.Name.Should().Be("TestParam");
            dto.Type.Should().Be("Int");
            dto.Description.Should().Be("Test description");
        }

        [Fact]
        public void FromDto_PopulatesAllPropertiesFromParameterDto()
        {
            // Arrange
            var dto = new Models.ParameterDto
            {
                Name = "TestParam",
                Type = "Float",
                Description = "Test description"
            };

            // Act
            var viewModel = ParameterViewModel.FromDto(dto);

            // Assert
            viewModel.Name.Should().Be("TestParam");
            viewModel.Type.Should().Be("Float");
            viewModel.Description.Should().Be("Test description");
        }

        #endregion
    }
}
