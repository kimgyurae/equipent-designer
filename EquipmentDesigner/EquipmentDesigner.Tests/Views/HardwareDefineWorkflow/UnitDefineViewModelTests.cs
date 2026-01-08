using System.Collections.Specialized;
using System.Linq;
using EquipmentDesigner.Models.Dtos;
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
        public void Constructor_Default_InitializesWithEmptySubname()
        {
            var viewModel = new UnitDefineViewModel();
            viewModel.Subname.Should().BeEmpty();
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
        public void Subname_WhenSet_RaisesPropertyChanged()
        {
            var viewModel = new UnitDefineViewModel();
            var raised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(UnitDefineViewModel.Subname))
                    raised = true;
            };

            viewModel.Subname = "Subname";
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

        #region Data Conversion

        [Fact]
        public void ToDto_ReturnsUnitDtoWithAllPropertiesMapped()
        {
            var viewModel = new UnitDefineViewModel
            {
                ParentSystemId = "SYS001",
                Name = "Unit1",
                DisplayName = "Unit One",
                Subname = "Sub",
                Description = "Description",
                ImplementationGuidelines = "Guidelines",
                Process = "Process A"
            };

            var dto = viewModel.ToDto();

            dto.SystemId.Should().Be("SYS001");
            dto.Name.Should().Be("Unit1");
            dto.DisplayName.Should().Be("Unit One");
            dto.Subname.Should().Be("Sub");
            dto.Description.Should().Be("Description");
            dto.ProcessInfo.Should().Be("Process A");
        }

        [Fact]
        public void ToDto_IncludesAllCommandsInUnitDtoCommands()
        {
            var viewModel = new UnitDefineViewModel
            {
                Name = "Unit1"
            };
            viewModel.Commands.Add(new CommandViewModel { Name = "Cmd1", Description = "Desc1" });
            viewModel.Commands.Add(new CommandViewModel { Name = "Cmd2", Description = "Desc2" });

            var dto = viewModel.ToDto();

            dto.Commands.Should().HaveCount(2);
            dto.Commands.Select(c => c.Name).Should().Contain(new[] { "Cmd1", "Cmd2" });
        }

        [Fact]
        public void FromDto_PopulatesAllPropertiesFromUnitDto()
        {
            var dto = new UnitDto
            {
                SystemId = "SYS002",
                Name = "Unit2",
                DisplayName = "Unit Two",
                Subname = "SubB",
                Description = "Description B",
                ImplementationInstructions = new System.Collections.Generic.List<string> { "Guideline1", "Guideline2" },
                ProcessInfo = "Process B"
            };

            var viewModel = UnitDefineViewModel.FromDto(dto);

            viewModel.ParentSystemId.Should().Be("SYS002");
            viewModel.Name.Should().Be("Unit2");
            viewModel.DisplayName.Should().Be("Unit Two");
            viewModel.Subname.Should().Be("SubB");
            viewModel.Description.Should().Be("Description B");
            viewModel.ImplementationGuidelines.Should().Be("Guideline1\nGuideline2");
            viewModel.Process.Should().Be("Process B");
        }

        [Fact]
        public void FromDto_PopulatesCommandsCollectionFromUnitDtoCommands()
        {
            var dto = new UnitDto
            {
                Name = "Unit1",
                Commands = new System.Collections.Generic.List<CommandDto>
                {
                    new CommandDto { Name = "Cmd1", Description = "Desc1" },
                    new CommandDto { Name = "Cmd2", Description = "Desc2" }
                }
            };

            var viewModel = UnitDefineViewModel.FromDto(dto);

            viewModel.Commands.Should().HaveCount(2);
            viewModel.Commands.Select(c => c.Name).Should().Contain(new[] { "Cmd1", "Cmd2" });
        }

        #endregion
    }
}