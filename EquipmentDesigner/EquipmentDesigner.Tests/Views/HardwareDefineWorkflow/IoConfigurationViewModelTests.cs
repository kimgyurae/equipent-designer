using EquipmentDesigner.Models.Dtos;
using EquipmentDesigner.ViewModels;
using FluentAssertions;
using Xunit;

namespace EquipmentDesigner.Tests.Views.HardwareDefineWorkflow
{
    public class IoConfigurationViewModelTests
    {
        #region Property Initialization

        [Fact]
        public void Constructor_Default_InitializesWithEmptyName()
        {
            var viewModel = new IoConfigurationViewModel();
            viewModel.Name.Should().BeEmpty();
        }

        [Fact]
        public void Constructor_Default_InitializesWithNullIoType()
        {
            var viewModel = new IoConfigurationViewModel();
            viewModel.IoType.Should().BeNull();
        }

        [Fact]
        public void Constructor_Default_InitializesWithEmptyAddress()
        {
            var viewModel = new IoConfigurationViewModel();
            viewModel.Address.Should().BeEmpty();
        }

        [Fact]
        public void Constructor_Default_InitializesWithEmptyDescription()
        {
            var viewModel = new IoConfigurationViewModel();
            viewModel.Description.Should().BeEmpty();
        }

        #endregion

        #region Property Change Notification

        [Fact]
        public void Name_WhenSet_RaisesPropertyChanged()
        {
            var viewModel = new IoConfigurationViewModel();
            var raised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(IoConfigurationViewModel.Name))
                    raised = true;
            };

            viewModel.Name = "TestIO";
            raised.Should().BeTrue();
        }

        [Fact]
        public void IoType_WhenSet_RaisesPropertyChanged()
        {
            var viewModel = new IoConfigurationViewModel();
            var raised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(IoConfigurationViewModel.IoType))
                    raised = true;
            };

            viewModel.IoType = "Input";
            raised.Should().BeTrue();
        }

        [Fact]
        public void Address_WhenSet_RaisesPropertyChanged()
        {
            var viewModel = new IoConfigurationViewModel();
            var raised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(IoConfigurationViewModel.Address))
                    raised = true;
            };

            viewModel.Address = "0x0010";
            raised.Should().BeTrue();
        }

        [Fact]
        public void Description_WhenSet_RaisesPropertyChanged()
        {
            var viewModel = new IoConfigurationViewModel();
            var raised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(IoConfigurationViewModel.Description))
                    raised = true;
            };

            viewModel.Description = "Test description";
            raised.Should().BeTrue();
        }

        [Fact]
        public void Name_WhenChanged_RaisesPropertyChangedForIsValid()
        {
            var viewModel = new IoConfigurationViewModel();
            var raised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(IoConfigurationViewModel.IsValid))
                    raised = true;
            };

            viewModel.Name = "TestIO";
            raised.Should().BeTrue();
        }

        #endregion

        #region Validation

        [Fact]
        public void IsValid_WhenNameIsEmpty_ReturnsFalse()
        {
            var viewModel = new IoConfigurationViewModel
            {
                Name = "",
                IoType = "Input"
            };

            viewModel.IsValid.Should().BeFalse();
        }

        [Fact]
        public void IsValid_WhenIoTypeIsNull_ReturnsFalse()
        {
            var viewModel = new IoConfigurationViewModel
            {
                Name = "TestIO",
                IoType = null
            };

            viewModel.IsValid.Should().BeFalse();
        }

        [Fact]
        public void IsValid_WhenNameAndIoTypeAreProvided_ReturnsTrue()
        {
            var viewModel = new IoConfigurationViewModel
            {
                Name = "TestIO",
                IoType = "Input"
            };

            viewModel.IsValid.Should().BeTrue();
        }

        #endregion

        #region Data Conversion

        [Fact]
        public void ToDto_ReturnsIoInfoDtoWithAllPropertiesMapped()
        {
            var viewModel = new IoConfigurationViewModel
            {
                Name = "TestIO",
                IoType = "Output",
                Address = "0x0020",
                Description = "Test description"
            };

            var dto = viewModel.ToDto();

            dto.Name.Should().Be("TestIO");
            dto.IoType.Should().Be("Output");
            dto.Address.Should().Be("0x0020");
            dto.Description.Should().Be("Test description");
        }

        [Fact]
        public void FromDto_PopulatesAllPropertiesFromIoInfoDto()
        {
            var dto = new IoInfoDto
            {
                Name = "TestIO",
                IoType = "AnalogInput",
                Address = "0x0030",
                Description = "Test description"
            };

            var viewModel = IoConfigurationViewModel.FromDto(dto);

            viewModel.Name.Should().Be("TestIO");
            viewModel.IoType.Should().Be("AnalogInput");
            viewModel.Address.Should().Be("0x0030");
            viewModel.Description.Should().Be("Test description");
        }

        #endregion
    }
}
