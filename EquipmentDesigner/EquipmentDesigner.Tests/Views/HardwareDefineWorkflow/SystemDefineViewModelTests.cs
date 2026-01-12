using System.Collections.Specialized;
using System.Linq;
using EquipmentDesigner.Models;
using EquipmentDesigner.ViewModels;
using FluentAssertions;
using Xunit;

namespace EquipmentDesigner.Tests.Views.HardwareDefineWorkflow
{
    public class SystemDefineViewModelTests
    {
        #region Property Initialization

        [Fact]
        public void Constructor_Default_InitializesWithEmptyName()
        {
            var viewModel = new SystemDefineViewModel();
            viewModel.Name.Should().BeEmpty();
        }

        [Fact]
        public void Constructor_Default_InitializesWithNullParentEquipmentId()
        {
            var viewModel = new SystemDefineViewModel();
            viewModel.ParentEquipmentId.Should().BeNull();
        }

        [Fact]
        public void Constructor_Default_InitializesWithEmptyDisplayName()
        {
            var viewModel = new SystemDefineViewModel();
            viewModel.DisplayName.Should().BeEmpty();
        }

        [Fact]
        public void Constructor_Default_InitializesWithEmptyDescription()
        {
            var viewModel = new SystemDefineViewModel();
            viewModel.Description.Should().BeEmpty();
        }

        [Fact]
        public void Constructor_Default_InitializesWithEmptyImplementationGuidelines()
        {
            var viewModel = new SystemDefineViewModel();
            viewModel.ImplementationGuidelines.Should().BeEmpty();
        }

        [Fact]
        public void Constructor_Default_InitializesWithEmptyProcess()
        {
            var viewModel = new SystemDefineViewModel();
            viewModel.Process.Should().BeEmpty();
        }

        [Fact]
        public void Constructor_Default_InitializesWithEmptyCommandsCollection()
        {
            var viewModel = new SystemDefineViewModel();
            viewModel.Commands.Should().NotBeNull().And.BeEmpty();
        }

        #endregion

        #region Property Change Notification

        [Fact]
        public void Name_WhenSet_RaisesPropertyChanged()
        {
            var viewModel = new SystemDefineViewModel();
            var raised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(SystemDefineViewModel.Name))
                    raised = true;
            };

            viewModel.Name = "TestSystem";
            raised.Should().BeTrue();
        }

        [Fact]
        public void ParentEquipmentId_WhenSet_RaisesPropertyChanged()
        {
            var viewModel = new SystemDefineViewModel();
            var raised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(SystemDefineViewModel.ParentEquipmentId))
                    raised = true;
            };

            viewModel.ParentEquipmentId = "EQ001";
            raised.Should().BeTrue();
        }

        [Fact]
        public void DisplayName_WhenSet_RaisesPropertyChanged()
        {
            var viewModel = new SystemDefineViewModel();
            var raised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(SystemDefineViewModel.DisplayName))
                    raised = true;
            };

            viewModel.DisplayName = "Display Name";
            raised.Should().BeTrue();
        }


        [Fact]
        public void Description_WhenSet_RaisesPropertyChanged()
        {
            var viewModel = new SystemDefineViewModel();
            var raised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(SystemDefineViewModel.Description))
                    raised = true;
            };

            viewModel.Description = "Test description";
            raised.Should().BeTrue();
        }

        [Fact]
        public void ImplementationGuidelines_WhenSet_RaisesPropertyChanged()
        {
            var viewModel = new SystemDefineViewModel();
            var raised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(SystemDefineViewModel.ImplementationGuidelines))
                    raised = true;
            };

            viewModel.ImplementationGuidelines = "Guidelines";
            raised.Should().BeTrue();
        }

        [Fact]
        public void Process_WhenSet_RaisesPropertyChanged()
        {
            var viewModel = new SystemDefineViewModel();
            var raised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(SystemDefineViewModel.Process))
                    raised = true;
            };

            viewModel.Process = "Process A";
            raised.Should().BeTrue();
        }

        [Fact]
        public void Name_WhenChanged_RaisesPropertyChangedForCanProceedToNext()
        {
            var viewModel = new SystemDefineViewModel();
            var raised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(SystemDefineViewModel.CanProceedToNext))
                    raised = true;
            };

            viewModel.Name = "TestSystem";
            raised.Should().BeTrue();
        }

        #endregion

        #region Validation

        [Fact]
        public void CanProceedToNext_WhenNameIsNull_ReturnsFalse()
        {
            var viewModel = new SystemDefineViewModel { Name = null };
            viewModel.CanProceedToNext.Should().BeFalse();
        }

        [Fact]
        public void CanProceedToNext_WhenNameIsEmptyString_ReturnsFalse()
        {
            var viewModel = new SystemDefineViewModel { Name = "" };
            viewModel.CanProceedToNext.Should().BeFalse();
        }

        [Fact]
        public void CanProceedToNext_WhenNameIsWhitespaceOnly_ReturnsFalse()
        {
            var viewModel = new SystemDefineViewModel { Name = "   " };
            viewModel.CanProceedToNext.Should().BeFalse();
        }

        [Fact]
        public void CanProceedToNext_WhenNameHasValidValue_ReturnsTrue()
        {
            var viewModel = new SystemDefineViewModel { Name = "ValidName" };
            viewModel.CanProceedToNext.Should().BeTrue();
        }

        #endregion

        #region Commands Collection Management

        [Fact]
        public void RemoveCommandCommand_WhenExecuted_RemovesCommandViewModelFromCollection()
        {
            var viewModel = new SystemDefineViewModel();
            var command = new CommandViewModel { Name = "Test" };
            viewModel.Commands.Add(command);

            viewModel.RemoveCommandCommand.Execute(command);

            viewModel.Commands.Should().NotContain(command);
        }

        [Fact]
        public void Commands_WhenItemAdded_RaisesCollectionChanged()
        {
            var viewModel = new SystemDefineViewModel();
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
            var viewModel = new SystemDefineViewModel();
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
            var viewModel = new SystemDefineViewModel();
            viewModel.LoadFromServerCommand.CanExecute(null).Should().BeTrue();
        }

        [Fact]
        public void AddAnotherCommand_CanExecute_WhenCanProceedToNextIsTrue_ReturnsTrue()
        {
            var viewModel = new SystemDefineViewModel { Name = "ValidName" };
            viewModel.AddAnotherCommand.CanExecute(null).Should().BeTrue();
        }

        [Fact]
        public void AddAnotherCommand_CanExecute_WhenCanProceedToNextIsFalse_ReturnsFalse()
        {
            var viewModel = new SystemDefineViewModel { Name = "" };
            viewModel.AddAnotherCommand.CanExecute(null).Should().BeFalse();
        }

        #endregion

        #region Add Command Dialog Integration

        [Fact]
        public void ShowAddCommandDialogRequested_EventExists_CanBeSubscribed()
        {
            var viewModel = new SystemDefineViewModel();
            var raised = false;
            viewModel.ShowAddCommandDialogRequested += (s, e) => raised = true;

            // Just verifying we can subscribe without error
            raised.Should().BeFalse();
        }

        [Fact]
        public void AddCommandCommand_WhenExecuted_RaisesShowAddCommandDialogRequestedEvent()
        {
            var viewModel = new SystemDefineViewModel();
            var raised = false;
            viewModel.ShowAddCommandDialogRequested += (s, e) => raised = true;

            viewModel.AddCommandCommand.Execute(null);

            raised.Should().BeTrue();
        }

        [Fact]
        public void ProcessCommandDialogResult_WhenResultIsNotNull_AddsCommandToCollection()
        {
            var viewModel = new SystemDefineViewModel();
            var command = new CommandViewModel { Name = "TestCmd", Description = "Test description" };

            viewModel.ProcessCommandDialogResult(command);

            viewModel.Commands.Should().Contain(command);
        }

        [Fact]
        public void ProcessCommandDialogResult_WhenResultIsNull_DoesNotModifyCollection()
        {
            var viewModel = new SystemDefineViewModel();
            var initialCount = viewModel.Commands.Count;

            viewModel.ProcessCommandDialogResult(null);

            viewModel.Commands.Count.Should().Be(initialCount);
        }

        [Fact]
        public void HasNoCommands_ReturnsTrue_WhenCommandsCollectionIsEmpty()
        {
            var viewModel = new SystemDefineViewModel();
            viewModel.HasNoCommands.Should().BeTrue();
        }

        [Fact]
        public void HasNoCommands_ReturnsFalse_AfterProcessCommandDialogResultAddsCommand()
        {
            var viewModel = new SystemDefineViewModel();
            var command = new CommandViewModel { Name = "TestCmd", Description = "Test description" };

            viewModel.ProcessCommandDialogResult(command);

            viewModel.HasNoCommands.Should().BeFalse();
        }

        #endregion

        #region HardwareDefinition Conversion

        [Fact]
        public void ToHardwareDefinition_SetsHardwareTypeToSystem()
        {
            var viewModel = new SystemDefineViewModel { Name = "TestSystem" };

            var hw = viewModel.ToHardwareDefinition();

            hw.HardwareType.Should().Be(HardwareType.System);
        }

        [Fact]
        public void ToHardwareDefinition_MapsNamePropertyCorrectly()
        {
            var viewModel = new SystemDefineViewModel { Name = "TestSystem" };

            var hw = viewModel.ToHardwareDefinition();

            hw.Name.Should().Be("TestSystem");
        }

        [Fact]
        public void ToHardwareDefinition_MapsDisplayNamePropertyCorrectly()
        {
            var viewModel = new SystemDefineViewModel
            {
                Name = "TestSystem",
                DisplayName = "Test System Display"
            };

            var hw = viewModel.ToHardwareDefinition();

            hw.DisplayName.Should().Be("Test System Display");
        }

        [Fact]
        public void ToHardwareDefinition_MapsDescriptionPropertyCorrectly()
        {
            var viewModel = new SystemDefineViewModel
            {
                Name = "TestSystem",
                Description = "Test System Description"
            };

            var hw = viewModel.ToHardwareDefinition();

            hw.Description.Should().Be("Test System Description");
        }

        [Fact]
        public void ToHardwareDefinition_MapsProcessToProcessInfoCorrectly()
        {
            var viewModel = new SystemDefineViewModel
            {
                Name = "TestSystem",
                Process = "TestProcess"
            };

            var hw = viewModel.ToHardwareDefinition();

            hw.ProcessInfo.Should().Be("TestProcess");
        }

        [Fact]
        public void ToHardwareDefinition_MapsVersionPropertyCorrectly()
        {
            var viewModel = new SystemDefineViewModel
            {
                Name = "TestSystem",
                Version = "2.0.0"
            };

            var hw = viewModel.ToHardwareDefinition();

            hw.Version.Should().Be("2.0.0");
        }

        [Fact]
        public void ToHardwareDefinition_MapsHardwareKeyPropertyCorrectly()
        {
            var viewModel = new SystemDefineViewModel
            {
                Name = "TestSystem",
                HardwareKey = "sys-key-123"
            };

            var hw = viewModel.ToHardwareDefinition();

            hw.HardwareKey.Should().Be("sys-key-123");
        }

        [Fact]
        public void ToHardwareDefinition_SplitsImplementationGuidelinesIntoList()
        {
            var viewModel = new SystemDefineViewModel
            {
                Name = "TestSystem",
                ImplementationGuidelines = "Line1\nLine2\nLine3"
            };

            var hw = viewModel.ToHardwareDefinition();

            hw.ImplementationInstructions.Should().HaveCount(3);
            hw.ImplementationInstructions.Should().ContainInOrder("Line1", "Line2", "Line3");
        }

        [Fact]
        public void ToHardwareDefinition_HandlesEmptyImplementationGuidelinesWithEmptyList()
        {
            var viewModel = new SystemDefineViewModel
            {
                Name = "TestSystem",
                ImplementationGuidelines = string.Empty
            };

            var hw = viewModel.ToHardwareDefinition();

            hw.ImplementationInstructions.Should().BeEmpty();
        }

        [Fact]
        public void ToHardwareDefinition_SplitsImplementationGuidelinesOnBothCrLfAndLf()
        {
            var viewModel = new SystemDefineViewModel
            {
                Name = "TestSystem",
                ImplementationGuidelines = "Line1\r\nLine2\nLine3"
            };

            var hw = viewModel.ToHardwareDefinition();

            hw.ImplementationInstructions.Should().HaveCount(3);
            hw.ImplementationInstructions.Should().ContainInOrder("Line1", "Line2", "Line3");
        }

        [Fact]
        public void ToHardwareDefinition_ConvertsCommandsCollectionUsingCommandViewModelToDto()
        {
            var viewModel = new SystemDefineViewModel { Name = "TestSystem" };
            viewModel.Commands.Add(new CommandViewModel { Name = "Cmd1", Description = "Desc1" });
            viewModel.Commands.Add(new CommandViewModel { Name = "Cmd2", Description = "Desc2" });

            var hw = viewModel.ToHardwareDefinition();

            hw.Commands.Should().HaveCount(2);
            hw.Commands.Select(c => c.Name).Should().Contain(new[] { "Cmd1", "Cmd2" });
        }

        [Fact]
        public void ToHardwareDefinition_ReturnsEmptyCommandsListWhenCollectionIsEmpty()
        {
            var viewModel = new SystemDefineViewModel { Name = "TestSystem" };

            var hw = viewModel.ToHardwareDefinition();

            hw.Commands.Should().NotBeNull();
            hw.Commands.Should().BeEmpty();
        }

        [Fact]
        public void FromHardwareDefinition_CreatesViewModelWithCorrectName()
        {
            var hw = new HardwareDefinition
            {
                HardwareType = HardwareType.System,
                Name = "TestSystem"
            };

            var viewModel = SystemDefineViewModel.FromHardwareDefinition(hw);

            viewModel.Name.Should().Be("TestSystem");
        }

        [Fact]
        public void FromHardwareDefinition_CreatesViewModelWithCorrectDisplayName()
        {
            var hw = new HardwareDefinition
            {
                HardwareType = HardwareType.System,
                Name = "TestSystem",
                DisplayName = "Test System Display"
            };

            var viewModel = SystemDefineViewModel.FromHardwareDefinition(hw);

            viewModel.DisplayName.Should().Be("Test System Display");
        }

        [Fact]
        public void FromHardwareDefinition_CreatesViewModelWithCorrectDescription()
        {
            var hw = new HardwareDefinition
            {
                HardwareType = HardwareType.System,
                Name = "TestSystem",
                Description = "Test Description"
            };

            var viewModel = SystemDefineViewModel.FromHardwareDefinition(hw);

            viewModel.Description.Should().Be("Test Description");
        }

        [Fact]
        public void FromHardwareDefinition_MapsProcessInfoToProcessCorrectly()
        {
            var hw = new HardwareDefinition
            {
                HardwareType = HardwareType.System,
                Name = "TestSystem",
                ProcessInfo = "TestProcess"
            };

            var viewModel = SystemDefineViewModel.FromHardwareDefinition(hw);

            viewModel.Process.Should().Be("TestProcess");
        }

        [Fact]
        public void FromHardwareDefinition_CreatesViewModelWithCorrectVersion()
        {
            var hw = new HardwareDefinition
            {
                HardwareType = HardwareType.System,
                Name = "TestSystem",
                Version = "3.0.0"
            };

            var viewModel = SystemDefineViewModel.FromHardwareDefinition(hw);

            viewModel.Version.Should().Be("3.0.0");
        }

        [Fact]
        public void FromHardwareDefinition_CreatesViewModelWithCorrectHardwareKey()
        {
            var hw = new HardwareDefinition
            {
                HardwareType = HardwareType.System,
                Name = "TestSystem",
                HardwareKey = "sys-key-456"
            };

            var viewModel = SystemDefineViewModel.FromHardwareDefinition(hw);

            viewModel.HardwareKey.Should().Be("sys-key-456");
        }

        [Fact]
        public void FromHardwareDefinition_JoinsImplementationInstructionsIntoGuidelinesWithNewlines()
        {
            var hw = new HardwareDefinition
            {
                HardwareType = HardwareType.System,
                Name = "TestSystem",
                ImplementationInstructions = new System.Collections.Generic.List<string>
                {
                    "Instruction1",
                    "Instruction2",
                    "Instruction3"
                }
            };

            var viewModel = SystemDefineViewModel.FromHardwareDefinition(hw);

            viewModel.ImplementationGuidelines.Should().Be("Instruction1\nInstruction2\nInstruction3");
        }

        [Fact]
        public void FromHardwareDefinition_HandlesNullImplementationInstructionsWithEmptyString()
        {
            var hw = new HardwareDefinition
            {
                HardwareType = HardwareType.System,
                Name = "TestSystem",
                ImplementationInstructions = null
            };

            var viewModel = SystemDefineViewModel.FromHardwareDefinition(hw);

            viewModel.ImplementationGuidelines.Should().BeEmpty();
        }

        [Fact]
        public void FromHardwareDefinition_CreatesCommandViewModelsFromCommandsList()
        {
            var hw = new HardwareDefinition
            {
                HardwareType = HardwareType.System,
                Name = "TestSystem",
                Commands = new System.Collections.Generic.List<CommandDto>
                {
                    new CommandDto { Name = "Cmd1", Description = "Desc1" },
                    new CommandDto { Name = "Cmd2", Description = "Desc2" }
                }
            };

            var viewModel = SystemDefineViewModel.FromHardwareDefinition(hw);

            viewModel.Commands.Should().HaveCount(2);
            viewModel.Commands.Select(c => c.Name).Should().Contain(new[] { "Cmd1", "Cmd2" });
        }

        [Fact]
        public void FromHardwareDefinition_HandlesNullCommandsWithEmptyCollection()
        {
            var hw = new HardwareDefinition
            {
                HardwareType = HardwareType.System,
                Name = "TestSystem",
                Commands = null
            };

            var viewModel = SystemDefineViewModel.FromHardwareDefinition(hw);

            viewModel.Commands.Should().NotBeNull();
            viewModel.Commands.Should().BeEmpty();
        }

        [Fact]
        public void FromHardwareDefinition_ThrowsArgumentNullException_WhenHardwareDefinitionIsNull()
        {
            System.Action act = () => SystemDefineViewModel.FromHardwareDefinition(null);

            act.Should().Throw<System.ArgumentNullException>()
                .WithParameterName("hw");
        }

        [Fact]
        public void FromHardwareDefinition_HandlesNullPropertiesWithEmptyStringDefaults()
        {
            var hw = new HardwareDefinition
            {
                HardwareType = HardwareType.System,
                Name = null,
                DisplayName = null,
                Description = null,
                ProcessInfo = null
            };

            var viewModel = SystemDefineViewModel.FromHardwareDefinition(hw);

            viewModel.Name.Should().BeEmpty();
            viewModel.DisplayName.Should().BeEmpty();
            viewModel.Description.Should().BeEmpty();
            viewModel.Process.Should().BeEmpty();
        }

        [Fact]
        public void RoundTripConversion_PreservesAllSystemPropertiesIncludingCommands()
        {
            var original = new SystemDefineViewModel
            {
                Name = "TestSystem",
                DisplayName = "Test System Display",
                Description = "Test Description",
                ImplementationGuidelines = "Line1\nLine2",
                Process = "TestProcess",
                Version = "1.5.0",
                HardwareKey = "sys-key-789"
            };
            original.Commands.Add(new CommandViewModel { Name = "Cmd1", Description = "Desc1" });
            original.Commands.Add(new CommandViewModel { Name = "Cmd2", Description = "Desc2" });

            var hw = original.ToHardwareDefinition();
            var restored = SystemDefineViewModel.FromHardwareDefinition(hw);

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