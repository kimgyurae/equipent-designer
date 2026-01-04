using System.Collections.Generic;
using System.Linq;
using EquipmentDesigner.Views.HardwareDefineWorkflow;
using FluentAssertions;
using Xunit;

namespace EquipmentDesigner.Tests.Views.HardwareDefineWorkflow
{
    public class AddIoDialogViewModelTests
    {
        #region Property Initialization

        [Fact]
        public void Constructor_Default_InitializesIoNameAsEmptyString()
        {
            var viewModel = new AddIoDialogViewModel();
            viewModel.IoName.Should().BeEmpty();
        }

        [Fact]
        public void Constructor_Default_InitializesAddressAsEmptyString()
        {
            var viewModel = new AddIoDialogViewModel();
            viewModel.Address.Should().BeEmpty();
        }

        [Fact]
        public void Constructor_Default_InitializesIoTypeAsNull()
        {
            var viewModel = new AddIoDialogViewModel();
            viewModel.IoType.Should().BeNull();
        }

        [Fact]
        public void Constructor_Default_InitializesCanAddIoAsFalse()
        {
            var viewModel = new AddIoDialogViewModel();
            viewModel.CanAddIo.Should().BeFalse();
        }

        [Fact]
        public void AvailableIoTypes_ContainsExactlyFourOptions()
        {
            var viewModel = new AddIoDialogViewModel();
            viewModel.AvailableIoTypes.Should().HaveCount(4);
        }

        [Fact]
        public void AvailableIoTypes_ContainsDigitalInput()
        {
            var viewModel = new AddIoDialogViewModel();
            viewModel.AvailableIoTypes.Should().Contain("Digital Input");
        }

        [Fact]
        public void AvailableIoTypes_ContainsDigitalOutput()
        {
            var viewModel = new AddIoDialogViewModel();
            viewModel.AvailableIoTypes.Should().Contain("Digital Output");
        }

        [Fact]
        public void AvailableIoTypes_ContainsAnalogInput()
        {
            var viewModel = new AddIoDialogViewModel();
            viewModel.AvailableIoTypes.Should().Contain("Analog Input");
        }

        [Fact]
        public void AvailableIoTypes_ContainsAnalogOutput()
        {
            var viewModel = new AddIoDialogViewModel();
            viewModel.AvailableIoTypes.Should().Contain("Analog Output");
        }

        #endregion

        #region Property Change Notification

        [Fact]
        public void IoName_WhenSet_RaisesPropertyChanged()
        {
            var viewModel = new AddIoDialogViewModel();
            var raised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(AddIoDialogViewModel.IoName))
                    raised = true;
            };

            viewModel.IoName = "TestIO";
            raised.Should().BeTrue();
        }

        [Fact]
        public void Address_WhenSet_RaisesPropertyChanged()
        {
            var viewModel = new AddIoDialogViewModel();
            var raised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(AddIoDialogViewModel.Address))
                    raised = true;
            };

            viewModel.Address = "0x0001";
            raised.Should().BeTrue();
        }

        [Fact]
        public void IoType_WhenSet_RaisesPropertyChanged()
        {
            var viewModel = new AddIoDialogViewModel();
            var raised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(AddIoDialogViewModel.IoType))
                    raised = true;
            };

            viewModel.IoType = "Digital Input";
            raised.Should().BeTrue();
        }

        [Fact]
        public void IoName_WhenChanged_RaisesPropertyChangedForCanAddIo()
        {
            var viewModel = new AddIoDialogViewModel();
            var raised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(AddIoDialogViewModel.CanAddIo))
                    raised = true;
            };

            viewModel.IoName = "TestIO";
            raised.Should().BeTrue();
        }

        [Fact]
        public void Address_WhenChanged_RaisesPropertyChangedForCanAddIo()
        {
            var viewModel = new AddIoDialogViewModel();
            var raised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(AddIoDialogViewModel.CanAddIo))
                    raised = true;
            };

            viewModel.Address = "0x0001";
            raised.Should().BeTrue();
        }

        [Fact]
        public void IoType_WhenChanged_RaisesPropertyChangedForCanAddIo()
        {
            var viewModel = new AddIoDialogViewModel();
            var raised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(AddIoDialogViewModel.CanAddIo))
                    raised = true;
            };

            viewModel.IoType = "Digital Input";
            raised.Should().BeTrue();
        }

        #endregion

        #region Validation

        [Fact]
        public void CanAddIo_WhenIoNameIsNull_ReturnsFalse()
        {
            var viewModel = new AddIoDialogViewModel
            {
                IoName = null,
                Address = "0x0001",
                IoType = "Digital Input"
            };
            viewModel.CanAddIo.Should().BeFalse();
        }

        [Fact]
        public void CanAddIo_WhenIoNameIsEmptyString_ReturnsFalse()
        {
            var viewModel = new AddIoDialogViewModel
            {
                IoName = "",
                Address = "0x0001",
                IoType = "Digital Input"
            };
            viewModel.CanAddIo.Should().BeFalse();
        }

        [Fact]
        public void CanAddIo_WhenIoNameIsWhitespaceOnly_ReturnsFalse()
        {
            var viewModel = new AddIoDialogViewModel
            {
                IoName = "   ",
                Address = "0x0001",
                IoType = "Digital Input"
            };
            viewModel.CanAddIo.Should().BeFalse();
        }

        [Fact]
        public void CanAddIo_WhenAddressIsNull_ReturnsFalse()
        {
            var viewModel = new AddIoDialogViewModel
            {
                IoName = "TestIO",
                Address = null,
                IoType = "Digital Input"
            };
            viewModel.CanAddIo.Should().BeFalse();
        }

        [Fact]
        public void CanAddIo_WhenAddressIsEmptyString_ReturnsFalse()
        {
            var viewModel = new AddIoDialogViewModel
            {
                IoName = "TestIO",
                Address = "",
                IoType = "Digital Input"
            };
            viewModel.CanAddIo.Should().BeFalse();
        }

        [Fact]
        public void CanAddIo_WhenAddressIsWhitespaceOnly_ReturnsFalse()
        {
            var viewModel = new AddIoDialogViewModel
            {
                IoName = "TestIO",
                Address = "   ",
                IoType = "Digital Input"
            };
            viewModel.CanAddIo.Should().BeFalse();
        }

        [Fact]
        public void CanAddIo_WhenIoTypeIsNull_ReturnsFalse()
        {
            var viewModel = new AddIoDialogViewModel
            {
                IoName = "TestIO",
                Address = "0x0001",
                IoType = null
            };
            viewModel.CanAddIo.Should().BeFalse();
        }

        [Fact]
        public void CanAddIo_WhenIoTypeIsEmptyString_ReturnsFalse()
        {
            var viewModel = new AddIoDialogViewModel
            {
                IoName = "TestIO",
                Address = "0x0001",
                IoType = ""
            };
            viewModel.CanAddIo.Should().BeFalse();
        }

        [Fact]
        public void CanAddIo_WhenAllFieldsAreValid_ReturnsTrue()
        {
            var viewModel = new AddIoDialogViewModel
            {
                IoName = "TestIO",
                Address = "0x0001",
                IoType = "Digital Input"
            };
            viewModel.CanAddIo.Should().BeTrue();
        }

        #endregion

        #region Command Behavior

        [Fact]
        public void AddIoCommand_CanExecute_WhenCanAddIoIsFalse_ReturnsFalse()
        {
            var viewModel = new AddIoDialogViewModel();
            viewModel.AddIoCommand.CanExecute(null).Should().BeFalse();
        }

        [Fact]
        public void AddIoCommand_CanExecute_WhenCanAddIoIsTrue_ReturnsTrue()
        {
            var viewModel = new AddIoDialogViewModel
            {
                IoName = "TestIO",
                Address = "0x0001",
                IoType = "Digital Input"
            };
            viewModel.AddIoCommand.CanExecute(null).Should().BeTrue();
        }

        [Fact]
        public void AddIoCommand_Execute_CreatesIoConfigurationViewModelWithCorrectName()
        {
            var viewModel = new AddIoDialogViewModel
            {
                IoName = "TestIO",
                Address = "0x0001",
                IoType = "Digital Input"
            };

            IoConfigurationViewModel result = null;
            viewModel.RequestClose += (s, e) => result = e;

            viewModel.AddIoCommand.Execute(null);

            result.Should().NotBeNull();
            result.Name.Should().Be("TestIO");
        }

        [Fact]
        public void AddIoCommand_Execute_CreatesIoConfigurationViewModelWithCorrectAddress()
        {
            var viewModel = new AddIoDialogViewModel
            {
                IoName = "TestIO",
                Address = "0x0001",
                IoType = "Digital Input"
            };

            IoConfigurationViewModel result = null;
            viewModel.RequestClose += (s, e) => result = e;

            viewModel.AddIoCommand.Execute(null);

            result.Should().NotBeNull();
            result.Address.Should().Be("0x0001");
        }

        [Fact]
        public void AddIoCommand_Execute_CreatesIoConfigurationViewModelWithCorrectIoType()
        {
            var viewModel = new AddIoDialogViewModel
            {
                IoName = "TestIO",
                Address = "0x0001",
                IoType = "Digital Input"
            };

            IoConfigurationViewModel result = null;
            viewModel.RequestClose += (s, e) => result = e;

            viewModel.AddIoCommand.Execute(null);

            result.Should().NotBeNull();
            result.IoType.Should().Be("Digital Input");
        }

        [Fact]
        public void AddIoCommand_Execute_RaisesRequestCloseEvent()
        {
            var viewModel = new AddIoDialogViewModel
            {
                IoName = "TestIO",
                Address = "0x0001",
                IoType = "Digital Input"
            };

            var raised = false;
            viewModel.RequestClose += (s, e) => raised = true;

            viewModel.AddIoCommand.Execute(null);

            raised.Should().BeTrue();
        }

        [Fact]
        public void CancelCommand_CanExecute_AlwaysReturnsTrue()
        {
            var viewModel = new AddIoDialogViewModel();
            viewModel.CancelCommand.CanExecute(null).Should().BeTrue();
        }

        [Fact]
        public void CancelCommand_Execute_RaisesRequestCloseEventWithNull()
        {
            var viewModel = new AddIoDialogViewModel();

            var raised = false;
            IoConfigurationViewModel result = new IoConfigurationViewModel(); // Initialize to non-null to verify it becomes null
            viewModel.RequestClose += (s, e) =>
            {
                raised = true;
                result = e;
            };

            viewModel.CancelCommand.Execute(null);

            raised.Should().BeTrue();
            result.Should().BeNull();
        }

        #endregion
    }
}
