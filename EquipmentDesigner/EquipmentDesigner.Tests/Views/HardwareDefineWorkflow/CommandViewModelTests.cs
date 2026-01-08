using System.Collections.Specialized;
using System.Linq;
using EquipmentDesigner.Models.Dtos;
using EquipmentDesigner.ViewModels;
using FluentAssertions;
using Xunit;

namespace EquipmentDesigner.Tests.Views.HardwareDefineWorkflow
{
    public class CommandViewModelTests
    {
        #region Property Initialization

        [Fact]
        public void Constructor_Default_InitializesWithEmptyName()
        {
            var viewModel = new CommandViewModel();
            viewModel.Name.Should().BeEmpty();
        }

        [Fact]
        public void Constructor_Default_InitializesWithEmptyDescription()
        {
            var viewModel = new CommandViewModel();
            viewModel.Description.Should().BeEmpty();
        }

        [Fact]
        public void Constructor_Default_InitializesWithEmptyParametersCollection()
        {
            var viewModel = new CommandViewModel();
            viewModel.Parameters.Should().NotBeNull().And.BeEmpty();
        }

        #endregion

        #region Property Change Notification

        [Fact]
        public void Name_WhenSet_RaisesPropertyChanged()
        {
            var viewModel = new CommandViewModel();
            var raised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(CommandViewModel.Name))
                    raised = true;
            };

            viewModel.Name = "TestCommand";
            raised.Should().BeTrue();
        }

        [Fact]
        public void Description_WhenSet_RaisesPropertyChanged()
        {
            var viewModel = new CommandViewModel();
            var raised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(CommandViewModel.Description))
                    raised = true;
            };

            viewModel.Description = "Test description";
            raised.Should().BeTrue();
        }

        [Fact]
        public void Name_WhenChanged_RaisesPropertyChangedForIsValid()
        {
            var viewModel = new CommandViewModel();
            var raised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(CommandViewModel.IsValid))
                    raised = true;
            };

            viewModel.Name = "TestCommand";
            raised.Should().BeTrue();
        }

        [Fact]
        public void Description_WhenChanged_RaisesPropertyChangedForIsValid()
        {
            var viewModel = new CommandViewModel();
            var raised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(CommandViewModel.IsValid))
                    raised = true;
            };

            viewModel.Description = "Test description";
            raised.Should().BeTrue();
        }

        #endregion

        #region Validation

        [Fact]
        public void IsValid_WhenNameIsEmpty_ReturnsFalse()
        {
            var viewModel = new CommandViewModel
            {
                Name = "",
                Description = "Test"
            };
            viewModel.Parameters.Add(new ParameterViewModel { Name = "P1", Type = "String", Description = "D1" });

            viewModel.IsValid.Should().BeFalse();
        }

        [Fact]
        public void IsValid_WhenDescriptionIsEmpty_ReturnsFalse()
        {
            var viewModel = new CommandViewModel
            {
                Name = "Test",
                Description = ""
            };
            viewModel.Parameters.Add(new ParameterViewModel { Name = "P1", Type = "String", Description = "D1" });

            viewModel.IsValid.Should().BeFalse();
        }

        [Fact]
        public void IsValid_WhenParametersIsEmpty_ReturnsFalse()
        {
            var viewModel = new CommandViewModel
            {
                Name = "Test",
                Description = "Test description"
            };

            viewModel.IsValid.Should().BeFalse();
        }

        [Fact]
        public void IsValid_WhenAllPropertiesAndAtLeastOneParameterProvided_ReturnsTrue()
        {
            var viewModel = new CommandViewModel
            {
                Name = "TestCommand",
                Description = "Test description"
            };
            viewModel.Parameters.Add(new ParameterViewModel { Name = "P1", Type = "String", Description = "D1" });

            viewModel.IsValid.Should().BeTrue();
        }

        #endregion

        #region Parameters Collection Management

        [Fact]
        public void AddParameterCommand_WhenExecuted_AddsNewParameterToCollection()
        {
            var viewModel = new CommandViewModel();
            var initialCount = viewModel.Parameters.Count;

            viewModel.AddParameterCommand.Execute(null);

            viewModel.Parameters.Count.Should().Be(initialCount + 1);
        }

        [Fact]
        public void RemoveParameterCommand_WhenExecuted_RemovesParameterFromCollection()
        {
            var viewModel = new CommandViewModel();
            var parameter = new ParameterViewModel { Name = "Test" };
            viewModel.Parameters.Add(parameter);

            viewModel.RemoveParameterCommand.Execute(parameter);

            viewModel.Parameters.Should().NotContain(parameter);
        }

        [Fact]
        public void Parameters_WhenItemAdded_RaisesCollectionChanged()
        {
            var viewModel = new CommandViewModel();
            var raised = false;
            viewModel.Parameters.CollectionChanged += (s, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                    raised = true;
            };

            viewModel.Parameters.Add(new ParameterViewModel());

            raised.Should().BeTrue();
        }

        [Fact]
        public void Parameters_WhenItemRemoved_RaisesCollectionChanged()
        {
            var viewModel = new CommandViewModel();
            var parameter = new ParameterViewModel();
            viewModel.Parameters.Add(parameter);
            var raised = false;
            viewModel.Parameters.CollectionChanged += (s, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Remove)
                    raised = true;
            };

            viewModel.Parameters.Remove(parameter);

            raised.Should().BeTrue();
        }

        #endregion

        #region ParametersDisplayText

        [Fact]
        public void ParametersDisplayText_WhenParametersCollectionIsEmpty_ReturnsEmptyString()
        {
            var viewModel = new CommandViewModel();
            viewModel.ParametersDisplayText.Should().BeEmpty();
        }

        [Fact]
        public void ParametersDisplayText_WithSingleParameter_ReturnsTypeNameFormat()
        {
            var viewModel = new CommandViewModel();
            viewModel.Parameters.Add(new ParameterViewModel { Name = "speed", Type = "double", Description = "Speed value" });

            viewModel.ParametersDisplayText.Should().Be("double speed");
        }

        [Fact]
        public void ParametersDisplayText_WithMultipleParameters_ReturnsCommaSeparatedFormat()
        {
            var viewModel = new CommandViewModel();
            viewModel.Parameters.Add(new ParameterViewModel { Name = "grip", Type = "bool", Description = "Grip state" });
            viewModel.Parameters.Add(new ParameterViewModel { Name = "speed", Type = "double", Description = "Speed value" });

            viewModel.ParametersDisplayText.Should().Be("bool grip, double speed");
        }

        [Fact]
        public void ParametersDisplayText_UsesLowercaseTypeAndName()
        {
            var viewModel = new CommandViewModel();
            viewModel.Parameters.Add(new ParameterViewModel { Name = "MyParam", Type = "String", Description = "Test" });

            viewModel.ParametersDisplayText.Should().Be("string myparam");
        }

        [Fact]
        public void ParametersDisplayText_UpdatesWhenParametersCollectionChanges()
        {
            var viewModel = new CommandViewModel();
            var raised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(CommandViewModel.ParametersDisplayText))
                    raised = true;
            };

            viewModel.Parameters.Add(new ParameterViewModel { Name = "test", Type = "int", Description = "Test" });

            raised.Should().BeTrue();
        }

        #endregion

        #region Data Conversion

        [Fact]
        public void ToDto_ReturnsCommandDtoWithAllPropertiesMapped()
        {
            var viewModel = new CommandViewModel
            {
                Name = "TestCommand",
                Description = "Test description"
            };
            viewModel.Parameters.Add(new ParameterViewModel { Name = "P1", Type = "String", Description = "D1" });

            var dto = viewModel.ToDto();

            dto.Name.Should().Be("TestCommand");
            dto.Description.Should().Be("Test description");
        }

        [Fact]
        public void ToDto_IncludesAllParametersInCommandDtoParameters()
        {
            var viewModel = new CommandViewModel
            {
                Name = "TestCommand",
                Description = "Test description"
            };
            viewModel.Parameters.Add(new ParameterViewModel { Name = "P1", Type = "String", Description = "D1" });
            viewModel.Parameters.Add(new ParameterViewModel { Name = "P2", Type = "Int", Description = "D2" });

            var dto = viewModel.ToDto();

            dto.Parameters.Should().HaveCount(2);
            dto.Parameters.Select(p => p.Name).Should().Contain(new[] { "P1", "P2" });
        }

        [Fact]
        public void FromDto_PopulatesAllPropertiesFromCommandDto()
        {
            var dto = new CommandDto
            {
                Name = "TestCommand",
                Description = "Test description"
            };

            var viewModel = CommandViewModel.FromDto(dto);

            viewModel.Name.Should().Be("TestCommand");
            viewModel.Description.Should().Be("Test description");
        }

        [Fact]
        public void FromDto_PopulatesParametersCollectionFromCommandDtoParameters()
        {
            var dto = new CommandDto
            {
                Name = "TestCommand",
                Description = "Test description",
                Parameters = new System.Collections.Generic.List<ParameterDto>
                {
                    new ParameterDto { Name = "P1", Type = "String", Description = "D1" },
                    new ParameterDto { Name = "P2", Type = "Int", Description = "D2" }
                }
            };

            var viewModel = CommandViewModel.FromDto(dto);

            viewModel.Parameters.Should().HaveCount(2);
            viewModel.Parameters.Select(p => p.Name).Should().Contain(new[] { "P1", "P2" });
        }

        #endregion
    }
}