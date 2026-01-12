using System.Collections.Specialized;
using System.Linq;
using EquipmentDesigner.Models;
using EquipmentDesigner.ViewModels;
using FluentAssertions;
using Xunit;

namespace EquipmentDesigner.Tests.Views.HardwareDefineWorkflow
{
    public class UnitDefineViewModelTests
    {
        #region Property Initialization

        [Fact]
        public void Constructor_Default_InitializesWithEmptyName()
        {
            var viewModel = new UnitDefineViewModel();
            viewModel.Name.Should().BeEmpty();
        }

        [Fact]
        public void Constructor_Default_InitializesWithNullParentSystemId()
        {
            var viewModel = new UnitDefineViewModel();
            viewModel.ParentSystemId.Should().BeNull();
        }

        [Fact]
        public void Constructor_Default_InitializesWithEmptyDisplayName()
        {
            var viewModel = new UnitDefineViewModel();
            viewModel.DisplayName.Should().BeEmpty();
        }

        [Fact]
        public void Constructor_Default_InitializesWithEmptyDescription()
        {
            var viewModel = new UnitDefineViewModel();
            viewModel.Description.Should().BeEmpty();
        }

        [Fact]
        public void Constructor_Default_InitializesWithEmptyImplementationGuidelines()
        {
            var viewModel = new UnitDefineViewModel();
            viewModel.ImplementationGuidelines.Should().BeEmpty();
        }

        [Fact]
        public void Constructor_Default_InitializesWithEmptyProcess()
        {
            var viewModel = new UnitDefineViewModel();
            viewModel.Process.Should().BeEmpty();
        }

        [Fact]
        public void Constructor_Default_InitializesWithEmptyCommandsCollection()
        {
            var viewModel = new UnitDefineViewModel();
            viewModel.Commands.Should().NotBeNull().And.BeEmpty();
        }

        #endregion

        #region Property Change Notification

        [Fact]
        public void Name_WhenSet_RaisesPropertyChanged()
        {
            var viewModel = new UnitDefineViewModel();
            var raised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(UnitDefineViewModel.Name))
                    raised = true;
            };

            viewModel.Name = "TestUnit";
            raised.Should().BeTrue();
        }

        [Fact]
        public void ParentSystemId_WhenSet_RaisesPropertyChanged()
        {
            var viewModel = new UnitDefineViewModel();
            var raised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(UnitDefineViewModel.ParentSystemId))
                    raised = true;
            };

            viewModel.ParentSystemId = "SYS001";
            raised.Should().BeTrue();
        }

        [Fact]
        public void DisplayName_WhenSet_RaisesPropertyChanged()
        {
            var viewModel = new UnitDefineViewModel();
            var raised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(UnitDefineViewModel.DisplayName))
                    raised = true;
            };

            viewModel.DisplayName = "Display Name";
            raised.Should().BeTrue();
        }


        [Fact]
        public void Description_WhenSet_RaisesPropertyChanged()
        {
            var viewModel = new UnitDefineViewModel();
            var raised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(UnitDefineViewModel.Description))
                    raised = true;
            };

            viewModel.Description = "Test description";
            raised.Should().BeTrue();
        }

        [Fact]
        public void ImplementationGuidelines_WhenSet_RaisesPropertyChanged()
        {
            var viewModel = new UnitDefineViewModel();
            var raised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(UnitDefineViewModel.ImplementationGuidelines))
                    raised = true;
            };

            viewModel.ImplementationGuidelines = "Guidelines";
            raised.Should().BeTrue();
        }

        [Fact]
        public void Process_WhenSet_RaisesPropertyChanged()
        {
            var viewModel = new UnitDefineViewModel();
            var raised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(UnitDefineViewModel.Process))
                    raised = true;
            };

            viewModel.Process = "Process A";
            raised.Should().BeTrue();
        }

        [Fact]
        public void Name_WhenChanged_RaisesPropertyChangedForCanProceedToNext()
        {
            var viewModel = new UnitDefineViewModel();
            var raised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(UnitDefineViewModel.CanProceedToNext))
                    raised = true;
            };

            viewModel.Name = "TestUnit";
            raised.Should().BeTrue();
        }

        #endregion

        #region Validation

        [Fact]
        public void CanProceedToNext_WhenNameIsNull_ReturnsFalse()
        {
            var viewModel = new UnitDefineViewModel { Name = null };
            viewModel.CanProceedToNext.Should().BeFalse();
        }

        [Fact]
        public void CanProceedToNext_WhenNameIsEmptyString_ReturnsFalse()
        {
            var viewModel = new UnitDefineViewModel { Name = "" };
            viewModel.CanProceedToNext.Should().BeFalse();
        }

        [Fact]
        public void CanProceedToNext_WhenNameIsWhitespaceOnly_ReturnsFalse()
        {
            var viewModel = new UnitDefineViewModel { Name = "   " };
            viewModel.CanProceedToNext.Should().BeFalse();
        }

        [Fact]
        public void CanProceedToNext_WhenNameHasValidValue_ReturnsTrue()
        {
            var viewModel = new UnitDefineViewModel { Name = "ValidName" };
            viewModel.CanProceedToNext.Should().BeTrue();
        }

        #endregion

        #region Commands Collection Management

        [Fact]
        public void RemoveCommandCommand_WhenExecuted_RemovesCommandViewModelFromCollection()
        {
            var viewModel = new UnitDefineViewModel();
            var command = new CommandViewModel { Name = "Test" };
            viewModel.Commands.Add(command);

            viewModel.RemoveCommandCommand.Execute(command);

            viewModel.Commands.Should().NotContain(command);
        }

        [Fact]
        public void Commands_WhenItemAdded_RaisesCollectionChanged()
        {
            var viewModel = new UnitDefineViewModel();
            var raised = false;
            viewModel.Commands.CollectionChanged += (s, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                    raised = true;
            };

            viewModel.Commands.Add(new CommandViewModel());

            raised.Should().BeTrue();
        }

        [Fact]
        public void Commands_WhenItemRemoved_RaisesCollectionChanged()
        {
            var viewModel = new UnitDefineViewModel();
            var command = new CommandViewModel();
            viewModel.Commands.Add(command);
            var raised = false;
            viewModel.Commands.CollectionChanged += (s, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Remove)
                    raised = true;
            };

            viewModel.Commands.Remove(command);

            raised.Should().BeTrue();
        }

        #endregion

        #region Other Commands

        [Fact]
        public void LoadFromServerCommand_CanExecute_ReturnsTrue()
        {
            var viewModel = new UnitDefineViewModel();
            viewModel.LoadFromServerCommand.CanExecute(null).Should().BeTrue();
        }

        [Fact]
        public void AddAnotherCommand_CanExecute_WhenCanProceedToNextIsTrue_ReturnsTrue()
        {
            var viewModel = new UnitDefineViewModel { Name = "ValidName" };
            viewModel.AddAnotherCommand.CanExecute(null).Should().BeTrue();
        }

        [Fact]
        public void AddAnotherCommand_CanExecute_WhenCanProceedToNextIsFalse_ReturnsFalse()
        {
            var viewModel = new UnitDefineViewModel { Name = "" };
            viewModel.AddAnotherCommand.CanExecute(null).Should().BeFalse();
        }

        #endregion

        #region Add Command Dialog Integration

        [Fact]
        public void ShowAddCommandDialogRequested_EventExists_CanBeSubscribed()
        {
            var viewModel = new UnitDefineViewModel();
            var raised = false;
            viewModel.ShowAddCommandDialogRequested += (s, e) => raised = true;

            // Just verifying we can subscribe without error
            raised.Should().BeFalse();
        }

        [Fact]
        public void AddCommandCommand_WhenExecuted_RaisesShowAddCommandDialogRequestedEvent()
        {
            var viewModel = new UnitDefineViewModel();
            var raised = false;
            viewModel.ShowAddCommandDialogRequested += (s, e) => raised = true;

            viewModel.AddCommandCommand.Execute(null);

            raised.Should().BeTrue();
        }

        [Fact]
        public void ProcessCommandDialogResult_WhenResultIsNotNull_AddsCommandToCollection()
        {
            var viewModel = new UnitDefineViewModel();
            var command = new CommandViewModel { Name = "TestCmd", Description = "Test description" };

            viewModel.ProcessCommandDialogResult(command);

            viewModel.Commands.Should().Contain(command);
        }

        [Fact]
        public void ProcessCommandDialogResult_WhenResultIsNull_DoesNotModifyCollection()
        {
            var viewModel = new UnitDefineViewModel();
            var initialCount = viewModel.Commands.Count;

            viewModel.ProcessCommandDialogResult(null);

            viewModel.Commands.Count.Should().Be(initialCount);
        }

        [Fact]
        public void HasNoCommands_ReturnsTrue_WhenCommandsCollectionIsEmpty()
        {
            var viewModel = new UnitDefineViewModel();
            viewModel.HasNoCommands.Should().BeTrue();
        }

        [Fact]
        public void HasNoCommands_ReturnsFalse_AfterProcessCommandDialogResultAddsCommand()
        {
            var viewModel = new UnitDefineViewModel();
            var command = new CommandViewModel { Name = "TestCmd", Description = "Test description" };

            viewModel.ProcessCommandDialogResult(command);

            viewModel.HasNoCommands.Should().BeFalse();
        }

        #endregion

        #region HardwareDefinition Conversion

        [Fact]
        public void ToHardwareDefinition_SetsHardwareTypeToUnit()
        {
            var viewModel = new UnitDefineViewModel { Name = "TestUnit" };

            var hw = viewModel.ToHardwareDefinition();

            hw.HardwareType.Should().Be(HardwareType.Unit);
        }

        [Fact]
        public void ToHardwareDefinition_MapsNamePropertyCorrectly()
        {
            var viewModel = new UnitDefineViewModel { Name = "TestUnit" };

            var hw = viewModel.ToHardwareDefinition();

            hw.Name.Should().Be("TestUnit");
        }

        [Fact]
        public void ToHardwareDefinition_MapsDisplayNamePropertyCorrectly()
        {
            var viewModel = new UnitDefineViewModel
            {
                Name = "TestUnit",
                DisplayName = "Test Unit Display"
            };

            var hw = viewModel.ToHardwareDefinition();

            hw.DisplayName.Should().Be("Test Unit Display");
        }

        [Fact]
        public void ToHardwareDefinition_MapsDescriptionPropertyCorrectly()
        {
            var viewModel = new UnitDefineViewModel
            {
                Name = "TestUnit",
                Description = "Test Unit Description"
            };

            var hw = viewModel.ToHardwareDefinition();

            hw.Description.Should().Be("Test Unit Description");
        }

        [Fact]
        public void ToHardwareDefinition_MapsProcessToProcessInfoCorrectly()
        {
            var viewModel = new UnitDefineViewModel
            {
                Name = "TestUnit",
                Process = "TestProcess"
            };

            var hw = viewModel.ToHardwareDefinition();

            hw.ProcessInfo.Should().Be("TestProcess");
        }

        [Fact]
        public void ToHardwareDefinition_MapsVersionPropertyCorrectly()
        {
            var viewModel = new UnitDefineViewModel
            {
                Name = "TestUnit",
                Version = "2.0.0"
            };

            var hw = viewModel.ToHardwareDefinition();

            hw.Version.Should().Be("2.0.0");
        }

        [Fact]
        public void ToHardwareDefinition_MapsHardwareKeyPropertyCorrectly()
        {
            var viewModel = new UnitDefineViewModel
            {
                Name = "TestUnit",
                HardwareKey = "unit-key-123"
            };

            var hw = viewModel.ToHardwareDefinition();

            hw.HardwareKey.Should().Be("unit-key-123");
        }

        [Fact]
        public void ToHardwareDefinition_SplitsImplementationGuidelinesIntoList()
        {
            var viewModel = new UnitDefineViewModel
            {
                Name = "TestUnit",
                ImplementationGuidelines = "Line1\nLine2\nLine3"
            };

            var hw = viewModel.ToHardwareDefinition();

            hw.ImplementationInstructions.Should().HaveCount(3);
            hw.ImplementationInstructions.Should().ContainInOrder("Line1", "Line2", "Line3");
        }

        [Fact]
        public void ToHardwareDefinition_HandlesEmptyImplementationGuidelinesWithEmptyList()
        {
            var viewModel = new UnitDefineViewModel
            {
                Name = "TestUnit",
                ImplementationGuidelines = string.Empty
            };

            var hw = viewModel.ToHardwareDefinition();

            hw.ImplementationInstructions.Should().BeEmpty();
        }

        [Fact]
        public void ToHardwareDefinition_ConvertsCommandsCollectionUsingCommandViewModelToDto()
        {
            var viewModel = new UnitDefineViewModel { Name = "TestUnit" };
            viewModel.Commands.Add(new CommandViewModel { Name = "Cmd1", Description = "Desc1" });
            viewModel.Commands.Add(new CommandViewModel { Name = "Cmd2", Description = "Desc2" });

            var hw = viewModel.ToHardwareDefinition();

            hw.Commands.Should().HaveCount(2);
            hw.Commands.Select(c => c.Name).Should().Contain(new[] { "Cmd1", "Cmd2" });
        }

        [Fact]
        public void ToHardwareDefinition_ReturnsEmptyCommandsListWhenCollectionIsEmpty()
        {
            var viewModel = new UnitDefineViewModel { Name = "TestUnit" };

            var hw = viewModel.ToHardwareDefinition();

            hw.Commands.Should().NotBeNull();
            hw.Commands.Should().BeEmpty();
        }

        [Fact]
        public void FromHardwareDefinition_CreatesViewModelWithCorrectName()
        {
            var hw = new HardwareDefinition
            {
                HardwareType = HardwareType.Unit,
                Name = "TestUnit"
            };

            var viewModel = UnitDefineViewModel.FromHardwareDefinition(hw);

            viewModel.Name.Should().Be("TestUnit");
        }

        [Fact]
        public void FromHardwareDefinition_CreatesViewModelWithCorrectDisplayName()
        {
            var hw = new HardwareDefinition
            {
                HardwareType = HardwareType.Unit,
                Name = "TestUnit",
                DisplayName = "Test Unit Display"
            };

            var viewModel = UnitDefineViewModel.FromHardwareDefinition(hw);

            viewModel.DisplayName.Should().Be("Test Unit Display");
        }

        [Fact]
        public void FromHardwareDefinition_CreatesViewModelWithCorrectDescription()
        {
            var hw = new HardwareDefinition
            {
                HardwareType = HardwareType.Unit,
                Name = "TestUnit",
                Description = "Test Description"
            };

            var viewModel = UnitDefineViewModel.FromHardwareDefinition(hw);

            viewModel.Description.Should().Be("Test Description");
        }

        [Fact]
        public void FromHardwareDefinition_MapsProcessInfoToProcessCorrectly()
        {
            var hw = new HardwareDefinition
            {
                HardwareType = HardwareType.Unit,
                Name = "TestUnit",
                ProcessInfo = "TestProcess"
            };

            var viewModel = UnitDefineViewModel.FromHardwareDefinition(hw);

            viewModel.Process.Should().Be("TestProcess");
        }

        [Fact]
        public void FromHardwareDefinition_CreatesViewModelWithCorrectVersion()
        {
            var hw = new HardwareDefinition
            {
                HardwareType = HardwareType.Unit,
                Name = "TestUnit",
                Version = "3.0.0"
            };

            var viewModel = UnitDefineViewModel.FromHardwareDefinition(hw);

            viewModel.Version.Should().Be("3.0.0");
        }

        [Fact]
        public void FromHardwareDefinition_CreatesViewModelWithCorrectHardwareKey()
        {
            var hw = new HardwareDefinition
            {
                HardwareType = HardwareType.Unit,
                Name = "TestUnit",
                HardwareKey = "unit-key-456"
            };

            var viewModel = UnitDefineViewModel.FromHardwareDefinition(hw);

            viewModel.HardwareKey.Should().Be("unit-key-456");
        }

        [Fact]
        public void FromHardwareDefinition_JoinsImplementationInstructionsIntoGuidelinesWithNewlines()
        {
            var hw = new HardwareDefinition
            {
                HardwareType = HardwareType.Unit,
                Name = "TestUnit",
                ImplementationInstructions = new System.Collections.Generic.List<string>
                {
                    "Instruction1",
                    "Instruction2",
                    "Instruction3"
                }
            };

            var viewModel = UnitDefineViewModel.FromHardwareDefinition(hw);

            viewModel.ImplementationGuidelines.Should().Be("Instruction1\nInstruction2\nInstruction3");
        }

        [Fact]
        public void FromHardwareDefinition_HandlesNullImplementationInstructionsWithEmptyString()
        {
            var hw = new HardwareDefinition
            {
                HardwareType = HardwareType.Unit,
                Name = "TestUnit",
                ImplementationInstructions = null
            };

            var viewModel = UnitDefineViewModel.FromHardwareDefinition(hw);

            viewModel.ImplementationGuidelines.Should().BeEmpty();
        }

        [Fact]
        public void FromHardwareDefinition_CreatesCommandViewModelsFromCommandsList()
        {
            var hw = new HardwareDefinition
            {
                HardwareType = HardwareType.Unit,
                Name = "TestUnit",
                Commands = new System.Collections.Generic.List<CommandDto>
                {
                    new CommandDto { Name = "Cmd1", Description = "Desc1" },
                    new CommandDto { Name = "Cmd2", Description = "Desc2" }
                }
            };

            var viewModel = UnitDefineViewModel.FromHardwareDefinition(hw);

            viewModel.Commands.Should().HaveCount(2);
            viewModel.Commands.Select(c => c.Name).Should().Contain(new[] { "Cmd1", "Cmd2" });
        }

        [Fact]
        public void FromHardwareDefinition_HandlesNullCommandsWithEmptyCollection()
        {
            var hw = new HardwareDefinition
            {
                HardwareType = HardwareType.Unit,
                Name = "TestUnit",
                Commands = null
            };

            var viewModel = UnitDefineViewModel.FromHardwareDefinition(hw);

            viewModel.Commands.Should().NotBeNull();
            viewModel.Commands.Should().BeEmpty();
        }

        [Fact]
        public void FromHardwareDefinition_ThrowsArgumentNullException_WhenHardwareDefinitionIsNull()
        {
            System.Action act = () => UnitDefineViewModel.FromHardwareDefinition(null);

            act.Should().Throw<System.ArgumentNullException>()
                .WithParameterName("hw");
        }

        [Fact]
        public void FromHardwareDefinition_HandlesNullPropertiesWithEmptyStringDefaults()
        {
            var hw = new HardwareDefinition
            {
                HardwareType = HardwareType.Unit,
                Name = null,
                DisplayName = null,
                Description = null,
                ProcessInfo = null
            };

            var viewModel = UnitDefineViewModel.FromHardwareDefinition(hw);

            viewModel.Name.Should().BeEmpty();
            viewModel.DisplayName.Should().BeEmpty();
            viewModel.Description.Should().BeEmpty();
            viewModel.Process.Should().BeEmpty();
        }

        [Fact]
        public void RoundTripConversion_PreservesAllUnitPropertiesIncludingCommands()
        {
            var original = new UnitDefineViewModel
            {
                Name = "TestUnit",
                DisplayName = "Test Unit Display",
                Description = "Test Description",
                ImplementationGuidelines = "Line1\nLine2",
                Process = "TestProcess",
                Version = "1.5.0",
                HardwareKey = "unit-key-789"
            };
            original.Commands.Add(new CommandViewModel { Name = "Cmd1", Description = "Desc1" });
            original.Commands.Add(new CommandViewModel { Name = "Cmd2", Description = "Desc2" });

            var hw = original.ToHardwareDefinition();
            var restored = UnitDefineViewModel.FromHardwareDefinition(hw);

            restored.Name.Should().Be(original.Name);
            restored.DisplayName.Should().Be(original.DisplayName);
            restored.Description.Should().Be(original.Description);
            restored.ImplementationGuidelines.Should().Be(original.ImplementationGuidelines);
            restored.Process.Should().Be(original.Process);
            restored.Version.Should().Be(original.Version);
            restored.HardwareKey.Should().Be(original.HardwareKey);
            restored.Commands.Should().HaveCount(2);
            restored.Commands.Select(c => c.Name).Should().Contain(new[] { "Cmd1", "Cmd2" });
        }

        #endregion
    }
}