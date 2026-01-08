using System.Collections.Specialized;
using System.Linq;
using EquipmentDesigner.ViewModels;
using FluentAssertions;
using Xunit;

namespace EquipmentDesigner.Tests.Views.HardwareDefineWorkflow
{
    public class AddCommandDialogViewModelTests
    {
        #region Property Initialization

        [Fact]
        public void Constructor_Default_InitializesCommandNameAsEmptyString()
        {
            var viewModel = new AddCommandDialogViewModel();
            viewModel.CommandName.Should().BeEmpty();
        }

        [Fact]
        public void Constructor_Default_InitializesDescriptionAsEmptyString()
        {
            var viewModel = new AddCommandDialogViewModel();
            viewModel.Description.Should().BeEmpty();
        }

        [Fact]
        public void Constructor_Default_InitializesParametersAsEmptyCollection()
        {
            var viewModel = new AddCommandDialogViewModel();
            viewModel.Parameters.Should().NotBeNull().And.BeEmpty();
        }

        [Fact]
        public void Constructor_Default_InitializesCanAddCommandAsFalse()
        {
            var viewModel = new AddCommandDialogViewModel();
            viewModel.CanAddCommand.Should().BeFalse();
        }

        #endregion

        #region Core Validation - CanAddCommand

        [Fact]
        public void CanAddCommand_WhenCommandNameIsEmpty_ReturnsFalse()
        {
            var viewModel = new AddCommandDialogViewModel
            {
                CommandName = "",
                Description = "Valid description"
            };
            viewModel.CanAddCommand.Should().BeFalse();
        }

        [Fact]
        public void CanAddCommand_WhenCommandNameIsWhitespaceOnly_ReturnsFalse()
        {
            var viewModel = new AddCommandDialogViewModel
            {
                CommandName = "   ",
                Description = "Valid description"
            };
            viewModel.CanAddCommand.Should().BeFalse();
        }

        [Fact]
        public void CanAddCommand_WhenDescriptionIsEmpty_ReturnsFalse()
        {
            var viewModel = new AddCommandDialogViewModel
            {
                CommandName = "ValidName",
                Description = ""
            };
            viewModel.CanAddCommand.Should().BeFalse();
        }

        [Fact]
        public void CanAddCommand_WhenDescriptionIsWhitespaceOnly_ReturnsFalse()
        {
            var viewModel = new AddCommandDialogViewModel
            {
                CommandName = "ValidName",
                Description = "   "
            };
            viewModel.CanAddCommand.Should().BeFalse();
        }

        [Fact]
        public void CanAddCommand_WhenBothCommandNameAndDescriptionAreValid_ReturnsTrue()
        {
            var viewModel = new AddCommandDialogViewModel
            {
                CommandName = "ValidName",
                Description = "Valid description"
            };
            viewModel.CanAddCommand.Should().BeTrue();
        }

        [Fact]
        public void CanAddCommand_UpdatesWhenCommandNameChanges()
        {
            var viewModel = new AddCommandDialogViewModel { Description = "Valid description" };
            var raised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(AddCommandDialogViewModel.CanAddCommand))
                    raised = true;
            };

            viewModel.CommandName = "ValidName";
            raised.Should().BeTrue();
        }

        [Fact]
        public void CanAddCommand_UpdatesWhenDescriptionChanges()
        {
            var viewModel = new AddCommandDialogViewModel { CommandName = "ValidName" };
            var raised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(AddCommandDialogViewModel.CanAddCommand))
                    raised = true;
            };

            viewModel.Description = "Valid description";
            raised.Should().BeTrue();
        }

        #endregion

        #region Parameter Management

        [Fact]
        public void AddParameterCommand_WhenExecuted_AddsNewParameterViewModelToCollection()
        {
            var viewModel = new AddCommandDialogViewModel();
            var initialCount = viewModel.Parameters.Count;

            viewModel.AddParameterCommand.Execute(null);

            viewModel.Parameters.Count.Should().Be(initialCount + 1);
        }

        [Fact]
        public void RemoveParameterCommand_WhenExecuted_RemovesParameterFromCollection()
        {
            var viewModel = new AddCommandDialogViewModel();
            viewModel.AddParameterCommand.Execute(null);
            var parameter = viewModel.Parameters.First();

            viewModel.RemoveParameterCommand.Execute(parameter);

            viewModel.Parameters.Should().NotContain(parameter);
        }

        [Fact]
        public void RemoveParameterCommand_WhenParameterIsNull_DoesNothing()
        {
            var viewModel = new AddCommandDialogViewModel();
            viewModel.AddParameterCommand.Execute(null);
            var countBefore = viewModel.Parameters.Count;

            viewModel.RemoveParameterCommand.Execute(null);

            viewModel.Parameters.Count.Should().Be(countBefore);
        }

        [Fact]
        public void RemoveParameterCommand_WhenParameterNotInCollection_DoesNothing()
        {
            var viewModel = new AddCommandDialogViewModel();
            viewModel.AddParameterCommand.Execute(null);
            var countBefore = viewModel.Parameters.Count;
            var foreignParameter = new ParameterViewModel();

            viewModel.RemoveParameterCommand.Execute(foreignParameter);

            viewModel.Parameters.Count.Should().Be(countBefore);
        }

        [Fact]
        public void HasNoParameters_WhenCollectionIsEmpty_ReturnsTrue()
        {
            var viewModel = new AddCommandDialogViewModel();
            viewModel.HasNoParameters.Should().BeTrue();
        }

        [Fact]
        public void HasNoParameters_WhenCollectionHasItems_ReturnsFalse()
        {
            var viewModel = new AddCommandDialogViewModel();
            viewModel.AddParameterCommand.Execute(null);
            viewModel.HasNoParameters.Should().BeFalse();
        }

        [Fact]
        public void HasNoParameters_UpdatesWhenParametersCollectionChanges()
        {
            var viewModel = new AddCommandDialogViewModel();
            var raised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(AddCommandDialogViewModel.HasNoParameters))
                    raised = true;
            };

            viewModel.AddParameterCommand.Execute(null);

            raised.Should().BeTrue();
        }

        #endregion

        #region Parameter Validation

        [Fact]
        public void CanAddCommand_WhenNoParametersAdded_ReturnsTrue()
        {
            var viewModel = new AddCommandDialogViewModel
            {
                CommandName = "ValidName",
                Description = "Valid description"
            };
            // No parameters added - should still be valid
            viewModel.CanAddCommand.Should().BeTrue();
        }

        [Fact]
        public void CanAddCommand_WhenParameterHasEmptyName_ReturnsFalse()
        {
            var viewModel = new AddCommandDialogViewModel
            {
                CommandName = "ValidName",
                Description = "Valid description"
            };
            viewModel.AddParameterCommand.Execute(null);
            var parameter = viewModel.Parameters.First();
            parameter.Name = "";
            parameter.Type = "string";
            parameter.Description = "Valid param description";

            viewModel.CanAddCommand.Should().BeFalse();
        }

        [Fact]
        public void CanAddCommand_WhenParameterHasEmptyType_ReturnsFalse()
        {
            var viewModel = new AddCommandDialogViewModel
            {
                CommandName = "ValidName",
                Description = "Valid description"
            };
            viewModel.AddParameterCommand.Execute(null);
            var parameter = viewModel.Parameters.First();
            parameter.Name = "validParam";
            parameter.Type = "";
            parameter.Description = "Valid param description";

            viewModel.CanAddCommand.Should().BeFalse();
        }

        [Fact]
        public void CanAddCommand_WhenParameterHasEmptyDescription_ReturnsFalse()
        {
            var viewModel = new AddCommandDialogViewModel
            {
                CommandName = "ValidName",
                Description = "Valid description"
            };
            viewModel.AddParameterCommand.Execute(null);
            var parameter = viewModel.Parameters.First();
            parameter.Name = "validParam";
            parameter.Type = "string";
            parameter.Description = "";

            viewModel.CanAddCommand.Should().BeFalse();
        }

        [Fact]
        public void CanAddCommand_WhenAllParametersAreValid_ReturnsTrue()
        {
            var viewModel = new AddCommandDialogViewModel
            {
                CommandName = "ValidName",
                Description = "Valid description"
            };
            viewModel.AddParameterCommand.Execute(null);
            var parameter = viewModel.Parameters.First();
            parameter.Name = "validParam";
            parameter.Type = "string";
            parameter.Description = "Valid param description";

            viewModel.CanAddCommand.Should().BeTrue();
        }

        [Fact]
        public void AllParametersValid_WhenCollectionIsEmpty_ReturnsTrue()
        {
            var viewModel = new AddCommandDialogViewModel();
            viewModel.AllParametersValid.Should().BeTrue();
        }

        [Fact]
        public void AllParametersValid_WhenAllParametersHaveValidFields_ReturnsTrue()
        {
            var viewModel = new AddCommandDialogViewModel();
            viewModel.AddParameterCommand.Execute(null);
            var parameter = viewModel.Parameters.First();
            parameter.Name = "validParam";
            parameter.Type = "string";
            parameter.Description = "Valid description";

            viewModel.AllParametersValid.Should().BeTrue();
        }

        [Fact]
        public void AllParametersValid_WhenAnyParameterIsInvalid_ReturnsFalse()
        {
            var viewModel = new AddCommandDialogViewModel();
            viewModel.AddParameterCommand.Execute(null);
            var parameter = viewModel.Parameters.First();
            parameter.Name = "";  // Invalid - empty name

            viewModel.AllParametersValid.Should().BeFalse();
        }

        [Fact]
        public void AllParametersValid_UpdatesWhenParameterPropertyChanges()
        {
            var viewModel = new AddCommandDialogViewModel
            {
                CommandName = "ValidName",
                Description = "Valid description"
            };
            viewModel.AddParameterCommand.Execute(null);
            var parameter = viewModel.Parameters.First();

            var raised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(AddCommandDialogViewModel.AllParametersValid))
                    raised = true;
            };

            parameter.Name = "validParam";

            raised.Should().BeTrue();
        }

        #endregion

        #region Dialog Actions

        [Fact]
        public void AddCommandCommand_CanExecute_WhenCanAddCommandIsTrue_ReturnsTrue()
        {
            var viewModel = new AddCommandDialogViewModel
            {
                CommandName = "ValidName",
                Description = "Valid description"
            };
            viewModel.AddCommandCommand.CanExecute(null).Should().BeTrue();
        }

        [Fact]
        public void AddCommandCommand_CanExecute_WhenCanAddCommandIsFalse_ReturnsFalse()
        {
            var viewModel = new AddCommandDialogViewModel();
            viewModel.AddCommandCommand.CanExecute(null).Should().BeFalse();
        }

        [Fact]
        public void AddCommandCommand_Execute_RaisesRequestCloseEventWithCommandViewModel()
        {
            var viewModel = new AddCommandDialogViewModel
            {
                CommandName = "TestCommand",
                Description = "Test description"
            };

            CommandViewModel result = null;
            viewModel.RequestClose += (s, e) => result = e;

            viewModel.AddCommandCommand.Execute(null);

            result.Should().NotBeNull();
        }

        [Fact]
        public void CancelCommand_Execute_RaisesRequestCloseEventWithNull()
        {
            var viewModel = new AddCommandDialogViewModel();

            var raised = false;
            CommandViewModel result = new CommandViewModel();
            viewModel.RequestClose += (s, e) =>
            {
                raised = true;
                result = e;
            };

            viewModel.CancelCommand.Execute(null);

            raised.Should().BeTrue();
            result.Should().BeNull();
        }

        [Fact]
        public void RequestClose_EventIncludesProperlyPopulatedCommandViewModel()
        {
            var viewModel = new AddCommandDialogViewModel
            {
                CommandName = "TestCommand",
                Description = "Test description"
            };
            viewModel.AddParameterCommand.Execute(null);
            var param = viewModel.Parameters.First();
            param.Name = "param1";
            param.Type = "string";
            param.Description = "Parameter description";

            CommandViewModel result = null;
            viewModel.RequestClose += (s, e) => result = e;

            viewModel.AddCommandCommand.Execute(null);

            result.Name.Should().Be("TestCommand");
            result.Description.Should().Be("Test description");
            result.Parameters.Should().HaveCount(1);
        }

        #endregion

        #region Result Mapping

        [Fact]
        public void CommandViewModelResult_ContainsCorrectName()
        {
            var viewModel = new AddCommandDialogViewModel
            {
                CommandName = "MyCommand",
                Description = "My description"
            };

            CommandViewModel result = null;
            viewModel.RequestClose += (s, e) => result = e;
            viewModel.AddCommandCommand.Execute(null);

            result.Name.Should().Be("MyCommand");
        }

        [Fact]
        public void CommandViewModelResult_ContainsCorrectDescription()
        {
            var viewModel = new AddCommandDialogViewModel
            {
                CommandName = "MyCommand",
                Description = "My description"
            };

            CommandViewModel result = null;
            viewModel.RequestClose += (s, e) => result = e;
            viewModel.AddCommandCommand.Execute(null);

            result.Description.Should().Be("My description");
        }

        [Fact]
        public void CommandViewModelResult_ContainsAllParameters()
        {
            var viewModel = new AddCommandDialogViewModel
            {
                CommandName = "MyCommand",
                Description = "My description"
            };
            viewModel.AddParameterCommand.Execute(null);
            viewModel.AddParameterCommand.Execute(null);
            var param1 = viewModel.Parameters[0];
            param1.Name = "param1";
            param1.Type = "string";
            param1.Description = "First param";
            var param2 = viewModel.Parameters[1];
            param2.Name = "param2";
            param2.Type = "int";
            param2.Description = "Second param";

            CommandViewModel result = null;
            viewModel.RequestClose += (s, e) => result = e;
            viewModel.AddCommandCommand.Execute(null);

            result.Parameters.Should().HaveCount(2);
        }

        [Fact]
        public void CommandViewModelResult_ParametersHaveCorrectValues()
        {
            var viewModel = new AddCommandDialogViewModel
            {
                CommandName = "MyCommand",
                Description = "My description"
            };
            viewModel.AddParameterCommand.Execute(null);
            var param = viewModel.Parameters.First();
            param.Name = "myParam";
            param.Type = "bool";
            param.Description = "My param description";

            CommandViewModel result = null;
            viewModel.RequestClose += (s, e) => result = e;
            viewModel.AddCommandCommand.Execute(null);

            var resultParam = result.Parameters.First();
            resultParam.Name.Should().Be("myParam");
            resultParam.Type.Should().Be("bool");
            resultParam.Description.Should().Be("My param description");
        }

        [Fact]
        public void CommandViewModelResult_ParametersCollectionIsEmptyWhenNoParametersAdded()
        {
            var viewModel = new AddCommandDialogViewModel
            {
                CommandName = "MyCommand",
                Description = "My description"
            };

            CommandViewModel result = null;
            viewModel.RequestClose += (s, e) => result = e;
            viewModel.AddCommandCommand.Execute(null);

            result.Parameters.Should().BeEmpty();
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void Dialog_HandlesSpecialCharactersInCommandName()
        {
            var viewModel = new AddCommandDialogViewModel
            {
                CommandName = "Test-Command_123!@#",
                Description = "Valid description"
            };

            CommandViewModel result = null;
            viewModel.RequestClose += (s, e) => result = e;
            viewModel.AddCommandCommand.Execute(null);

            result.Name.Should().Be("Test-Command_123!@#");
        }

        [Fact]
        public void Dialog_HandlesSpecialCharactersInDescription()
        {
            var viewModel = new AddCommandDialogViewModel
            {
                CommandName = "ValidName",
                Description = "Description with special chars: <>&\"'"
            };

            CommandViewModel result = null;
            viewModel.RequestClose += (s, e) => result = e;
            viewModel.AddCommandCommand.Execute(null);

            result.Description.Should().Be("Description with special chars: <>&\"'");
        }

        [Fact]
        public void Dialog_HandlesVeryLongTextInCommandName()
        {
            var longName = new string('A', 1000);
            var viewModel = new AddCommandDialogViewModel
            {
                CommandName = longName,
                Description = "Valid description"
            };

            CommandViewModel result = null;
            viewModel.RequestClose += (s, e) => result = e;
            viewModel.AddCommandCommand.Execute(null);

            result.Name.Should().Be(longName);
        }

        [Fact]
        public void Dialog_HandlesRapidAddRemoveParameterOperations()
        {
            var viewModel = new AddCommandDialogViewModel();

            // Add 5 parameters rapidly
            for (int i = 0; i < 5; i++)
            {
                viewModel.AddParameterCommand.Execute(null);
            }
            viewModel.Parameters.Should().HaveCount(5);

            // Remove all parameters rapidly
            while (viewModel.Parameters.Count > 0)
            {
                viewModel.RemoveParameterCommand.Execute(viewModel.Parameters.First());
            }
            viewModel.Parameters.Should().BeEmpty();
        }

        #endregion
    }
}
