using System;
using System.Collections.Specialized;
using System.Linq;
using EquipmentDesigner.Models;
using EquipmentDesigner.ViewModels;
using FluentAssertions;
using Xunit;

namespace EquipmentDesigner.Tests.Views.HardwareDefineWorkflow
{
    public class DeviceDefineViewModelTests
    {
        #region Property Initialization

        [Fact]
        public void Constructor_Default_InitializesWithEmptyName()
        {
            var viewModel = new DeviceDefineViewModel();
            viewModel.Name.Should().BeEmpty();
        }

        [Fact]
        public void Constructor_Default_InitializesWithNullParentUnitId()
        {
            var viewModel = new DeviceDefineViewModel();
            viewModel.ParentUnitId.Should().BeNull();
        }

        [Fact]
        public void Constructor_Default_InitializesWithEmptyDisplayName()
        {
            var viewModel = new DeviceDefineViewModel();
            viewModel.DisplayName.Should().BeEmpty();
        }

        [Fact]
        public void Constructor_Default_InitializesWithEmptyDescription()
        {
            var viewModel = new DeviceDefineViewModel();
            viewModel.Description.Should().BeEmpty();
        }

        [Fact]
        public void Constructor_Default_InitializesWithEmptyImplementationGuidelines()
        {
            var viewModel = new DeviceDefineViewModel();
            viewModel.ImplementationGuidelines.Should().BeEmpty();
        }

        [Fact]
        public void Constructor_Default_InitializesWithEmptyCommandsCollection()
        {
            var viewModel = new DeviceDefineViewModel();
            viewModel.Commands.Should().NotBeNull().And.BeEmpty();
        }

        [Fact]
        public void Constructor_Default_InitializesWithEmptyIoConfigurationsCollection()
        {
            var viewModel = new DeviceDefineViewModel();
            viewModel.IoConfigurations.Should().NotBeNull().And.BeEmpty();
        }

        #endregion

        #region Property Change Notification

        [Fact]
        public void Name_WhenSet_RaisesPropertyChanged()
        {
            var viewModel = new DeviceDefineViewModel();
            var raised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(DeviceDefineViewModel.Name))
                    raised = true;
            };

            viewModel.Name = "TestDevice";
            raised.Should().BeTrue();
        }

        [Fact]
        public void ParentUnitId_WhenSet_RaisesPropertyChanged()
        {
            var viewModel = new DeviceDefineViewModel();
            var raised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(DeviceDefineViewModel.ParentUnitId))
                    raised = true;
            };

            viewModel.ParentUnitId = "UNIT001";
            raised.Should().BeTrue();
        }

        [Fact]
        public void DisplayName_WhenSet_RaisesPropertyChanged()
        {
            var viewModel = new DeviceDefineViewModel();
            var raised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(DeviceDefineViewModel.DisplayName))
                    raised = true;
            };

            viewModel.DisplayName = "Display Name";
            raised.Should().BeTrue();
        }


        [Fact]
        public void Description_WhenSet_RaisesPropertyChanged()
        {
            var viewModel = new DeviceDefineViewModel();
            var raised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(DeviceDefineViewModel.Description))
                    raised = true;
            };

            viewModel.Description = "Test description";
            raised.Should().BeTrue();
        }

        [Fact]
        public void ImplementationGuidelines_WhenSet_RaisesPropertyChanged()
        {
            var viewModel = new DeviceDefineViewModel();
            var raised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(DeviceDefineViewModel.ImplementationGuidelines))
                    raised = true;
            };

            viewModel.ImplementationGuidelines = "Guidelines";
            raised.Should().BeTrue();
        }

        [Fact]
        public void Name_WhenChanged_RaisesPropertyChangedForCanProceedToNext()
        {
            var viewModel = new DeviceDefineViewModel();
            var raised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(DeviceDefineViewModel.CanProceedToNext))
                    raised = true;
            };

            viewModel.Name = "TestDevice";
            raised.Should().BeTrue();
        }

        [Fact]
        public void Name_WhenChanged_RaisesPropertyChangedForCanCompleteWorkflow()
        {
            var viewModel = new DeviceDefineViewModel();
            var raised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(DeviceDefineViewModel.CanCompleteWorkflow))
                    raised = true;
            };

            viewModel.Name = "TestDevice";
            raised.Should().BeTrue();
        }

        #endregion

        #region Validation

        [Fact]
        public void CanProceedToNext_WhenNameIsNull_ReturnsFalse()
        {
            var viewModel = new DeviceDefineViewModel { Name = null };
            viewModel.CanProceedToNext.Should().BeFalse();
        }

        [Fact]
        public void CanProceedToNext_WhenNameIsEmptyString_ReturnsFalse()
        {
            var viewModel = new DeviceDefineViewModel { Name = "" };
            viewModel.CanProceedToNext.Should().BeFalse();
        }

        [Fact]
        public void CanProceedToNext_WhenNameIsWhitespaceOnly_ReturnsFalse()
        {
            var viewModel = new DeviceDefineViewModel { Name = "   " };
            viewModel.CanProceedToNext.Should().BeFalse();
        }

        [Fact]
        public void CanProceedToNext_WhenNameHasValidValue_ReturnsTrue()
        {
            var viewModel = new DeviceDefineViewModel { Name = "ValidName" };
            viewModel.CanProceedToNext.Should().BeTrue();
        }

        [Fact]
        public void CanCompleteWorkflow_ReturnsSameValueAsCanProceedToNext()
        {
            var viewModel = new DeviceDefineViewModel { Name = "ValidName" };
            viewModel.CanCompleteWorkflow.Should().Be(viewModel.CanProceedToNext);

            viewModel.Name = "";
            viewModel.CanCompleteWorkflow.Should().Be(viewModel.CanProceedToNext);
        }

        #endregion

        #region Commands Collection Management

        [Fact]
        public void RemoveCommandCommand_WhenExecuted_RemovesCommandViewModelFromCollection()
        {
            var viewModel = new DeviceDefineViewModel();
            var command = new CommandViewModel { Name = "Test" };
            viewModel.Commands.Add(command);

            viewModel.RemoveCommandCommand.Execute(command);

            viewModel.Commands.Should().NotContain(command);
        }

        [Fact]
        public void Commands_WhenItemAdded_RaisesCollectionChanged()
        {
            var viewModel = new DeviceDefineViewModel();
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
            var viewModel = new DeviceDefineViewModel();
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

        #region IO Configuration Management

        [Fact]
        public void RemoveIoCommand_WhenExecuted_RemovesIoConfigurationViewModelFromCollection()
        {
            var viewModel = new DeviceDefineViewModel();
            var io = new IoConfigurationViewModel { Name = "IO1", IoType = "Input" };
            viewModel.IoConfigurations.Add(io);

            viewModel.RemoveIoCommand.Execute(io);

            viewModel.IoConfigurations.Should().NotContain(io);
        }

        [Fact]
        public void IoConfigurations_WhenItemAdded_RaisesCollectionChanged()
        {
            var viewModel = new DeviceDefineViewModel();
            var raised = false;
            viewModel.IoConfigurations.CollectionChanged += (s, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                    raised = true;
            };

            viewModel.IoConfigurations.Add(new IoConfigurationViewModel());

            raised.Should().BeTrue();
        }

        [Fact]
        public void IoConfigurations_WhenItemRemoved_RaisesCollectionChanged()
        {
            var viewModel = new DeviceDefineViewModel();
            var io = new IoConfigurationViewModel();
            viewModel.IoConfigurations.Add(io);
            var raised = false;
            viewModel.IoConfigurations.CollectionChanged += (s, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Remove)
                    raised = true;
            };

            viewModel.IoConfigurations.Remove(io);

            raised.Should().BeTrue();
        }

        #endregion

        #region Other Commands

        [Fact]
        public void LoadFromServerCommand_CanExecute_ReturnsTrue()
        {
            var viewModel = new DeviceDefineViewModel();
            viewModel.LoadFromServerCommand.CanExecute(null).Should().BeTrue();
        }

        [Fact]
        public void AddAnotherCommand_CanExecute_WhenCanProceedToNextIsTrue_ReturnsTrue()
        {
            var viewModel = new DeviceDefineViewModel { Name = "ValidName" };
            viewModel.AddAnotherCommand.CanExecute(null).Should().BeTrue();
        }

        [Fact]
        public void AddAnotherCommand_CanExecute_WhenCanProceedToNextIsFalse_ReturnsFalse()
        {
            var viewModel = new DeviceDefineViewModel { Name = "" };
            viewModel.AddAnotherCommand.CanExecute(null).Should().BeFalse();
        }

        [Fact]
        public void CompleteWorkflowCommand_CanExecute_WhenCanCompleteWorkflowIsTrue_ReturnsTrue()
        {
            var viewModel = new DeviceDefineViewModel { Name = "ValidName" };
            viewModel.CompleteWorkflowCommand.CanExecute(null).Should().BeTrue();
        }

        [Fact]
        public void CompleteWorkflowCommand_CanExecute_WhenCanCompleteWorkflowIsFalse_ReturnsFalse()
        {
            var viewModel = new DeviceDefineViewModel { Name = "" };
            viewModel.CompleteWorkflowCommand.CanExecute(null).Should().BeFalse();
        }

        #endregion

        #region Data Conversion

        [Fact]
        public void ToDto_ReturnsDeviceDtoWithAllPropertiesMapped()
        {
            var viewModel = new DeviceDefineViewModel
            {
                ParentUnitId = "UNIT001",
                Name = "Device1",
                DisplayName = "Device One",
                Description = "Description",
                ImplementationGuidelines = "Guidelines"
            };

            var dto = viewModel.ToDto();

            dto.UnitId.Should().Be("UNIT001");
            dto.Name.Should().Be("Device1");
            dto.DisplayName.Should().Be("Device One");
            dto.Description.Should().Be("Description");
        }

        [Fact]
        public void ToDto_IncludesAllCommandsInDeviceDtoCommands()
        {
            var viewModel = new DeviceDefineViewModel
            {
                Name = "Device1"
            };
            viewModel.Commands.Add(new CommandViewModel { Name = "Cmd1", Description = "Desc1" });
            viewModel.Commands.Add(new CommandViewModel { Name = "Cmd2", Description = "Desc2" });

            var dto = viewModel.ToDto();

            dto.Commands.Should().HaveCount(2);
            dto.Commands.Select(c => c.Name).Should().Contain(new[] { "Cmd1", "Cmd2" });
        }

        [Fact]
        public void ToDto_IncludesAllIoConfigurationsInDeviceDtoIoInfo()
        {
            var viewModel = new DeviceDefineViewModel
            {
                Name = "Device1"
            };
            viewModel.IoConfigurations.Add(new IoConfigurationViewModel { Name = "IO1", IoType = "Input" });
            viewModel.IoConfigurations.Add(new IoConfigurationViewModel { Name = "IO2", IoType = "Output" });

            var dto = viewModel.ToDto();

            dto.IoInfo.Should().HaveCount(2);
            dto.IoInfo.Select(io => io.Name).Should().Contain(new[] { "IO1", "IO2" });
        }

        [Fact]
        public void FromDto_PopulatesAllPropertiesFromDeviceDto()
        {
            var dto = new DeviceDto
            {
                UnitId = "UNIT002",
                Name = "Device2",
                DisplayName = "Device Two",
                Description = "Description B",
                ImplementationInstructions = new System.Collections.Generic.List<string> { "Guideline1", "Guideline2" }
            };

            var viewModel = DeviceDefineViewModel.FromDto(dto);

            viewModel.ParentUnitId.Should().Be("UNIT002");
            viewModel.Name.Should().Be("Device2");
            viewModel.DisplayName.Should().Be("Device Two");
            viewModel.Description.Should().Be("Description B");
            viewModel.ImplementationGuidelines.Should().Be("Guideline1\nGuideline2");
        }

        [Fact]
        public void FromDto_PopulatesCommandsCollectionFromDeviceDtoCommands()
        {
            var dto = new DeviceDto
            {
                Name = "Device1",
                Commands = new System.Collections.Generic.List<CommandDto>
                {
                    new CommandDto { Name = "Cmd1", Description = "Desc1" },
                    new CommandDto { Name = "Cmd2", Description = "Desc2" }
                }
            };

            var viewModel = DeviceDefineViewModel.FromDto(dto);

            viewModel.Commands.Should().HaveCount(2);
            viewModel.Commands.Select(c => c.Name).Should().Contain(new[] { "Cmd1", "Cmd2" });
        }

        [Fact]
        public void FromDto_PopulatesIoConfigurationsCollectionFromDeviceDtoIoInfo()
        {
            var dto = new DeviceDto
            {
                Name = "Device1",
                IoInfo = new System.Collections.Generic.List<IoInfoDto>
                {
                    new IoInfoDto { Name = "IO1", IoType = "Input" },
                    new IoInfoDto { Name = "IO2", IoType = "Output" }
                }
            };

            var viewModel = DeviceDefineViewModel.FromDto(dto);

            viewModel.IoConfigurations.Should().HaveCount(2);
            viewModel.IoConfigurations.Select(io => io.Name).Should().Contain(new[] { "IO1", "IO2" });
        }

        #endregion

        #region Dialog Integration

        [Fact]
        public void AddIoCommand_WhenExecuted_RaisesShowAddIoDialogRequestedEvent()
        {
            var viewModel = new DeviceDefineViewModel();
            var raised = false;
            viewModel.ShowAddIoDialogRequested += (s, e) => raised = true;

            viewModel.AddIoCommand.Execute(null);

            raised.Should().BeTrue();
        }

        [Fact]
        public void ProcessDialogResult_WithValidIoConfiguration_AddsToCollection()
        {
            var viewModel = new DeviceDefineViewModel();
            var io = new IoConfigurationViewModel
            {
                Name = "TestIO",
                Address = "0x0001",
                IoType = "Digital Input"
            };
            var initialCount = viewModel.IoConfigurations.Count;

            viewModel.ProcessDialogResult(io);

            viewModel.IoConfigurations.Count.Should().Be(initialCount + 1);
            viewModel.IoConfigurations.Should().Contain(io);
        }

        [Fact]
        public void ProcessDialogResult_WithNullResult_DoesNotModifyCollection()
        {
            var viewModel = new DeviceDefineViewModel();
            var initialCount = viewModel.IoConfigurations.Count;

            viewModel.ProcessDialogResult(null);

            viewModel.IoConfigurations.Count.Should().Be(initialCount);
        }

        #endregion

        #region Add Command Dialog Integration

        [Fact]
        public void ShowAddCommandDialogRequested_EventExists_CanBeSubscribed()
        {
            var viewModel = new DeviceDefineViewModel();
            var raised = false;
            viewModel.ShowAddCommandDialogRequested += (s, e) => raised = true;

            // Just verifying we can subscribe without error
            raised.Should().BeFalse();
        }

        [Fact]
        public void AddCommandCommand_WhenExecuted_RaisesShowAddCommandDialogRequestedEvent()
        {
            var viewModel = new DeviceDefineViewModel();
            var raised = false;
            viewModel.ShowAddCommandDialogRequested += (s, e) => raised = true;

            viewModel.AddCommandCommand.Execute(null);

            raised.Should().BeTrue();
        }

        [Fact]
        public void ProcessCommandDialogResult_WhenResultIsNotNull_AddsCommandToCollection()
        {
            var viewModel = new DeviceDefineViewModel();
            var command = new CommandViewModel { Name = "TestCmd", Description = "Test description" };

            viewModel.ProcessCommandDialogResult(command);

            viewModel.Commands.Should().Contain(command);
        }

        [Fact]
        public void ProcessCommandDialogResult_WhenResultIsNull_DoesNotModifyCollection()
        {
            var viewModel = new DeviceDefineViewModel();
            var initialCount = viewModel.Commands.Count;

            viewModel.ProcessCommandDialogResult(null);

            viewModel.Commands.Count.Should().Be(initialCount);
        }

        [Fact]
        public void HasNoCommands_ReturnsTrue_WhenCommandsCollectionIsEmpty()
        {
            var viewModel = new DeviceDefineViewModel();
            viewModel.HasNoCommands.Should().BeTrue();
        }

        [Fact]
        public void HasNoCommands_ReturnsFalse_AfterProcessCommandDialogResultAddsCommand()
        {
            var viewModel = new DeviceDefineViewModel();
            var command = new CommandViewModel { Name = "TestCmd", Description = "Test description" };

            viewModel.ProcessCommandDialogResult(command);

            viewModel.HasNoCommands.Should().BeFalse();
        }

        #endregion

        #region Workflow Completion Event Tests

        [Fact]
        public void ExecuteCompleteWorkflow_WhenInvoked_RaisesWorkflowCompletedRequestEvent()
        {
            var viewModel = new DeviceDefineViewModel { Name = "ValidDevice" };
            var raised = false;
            viewModel.WorkflowCompletedRequest += (s, e) => raised = true;

            viewModel.CompleteWorkflowCommand.Execute(null);

            raised.Should().BeTrue();
        }

        [Fact]
        public void ExecuteCompleteWorkflow_WhenInvoked_EventSenderIsDeviceDefineViewModel()
        {
            var viewModel = new DeviceDefineViewModel { Name = "ValidDevice" };
            object capturedSender = null;
            viewModel.WorkflowCompletedRequest += (s, e) => capturedSender = s;

            viewModel.CompleteWorkflowCommand.Execute(null);

            capturedSender.Should().BeSameAs(viewModel);
        }

        [Fact]
        public void ExecuteCompleteWorkflow_WhenNoEventHandlersSubscribed_DoesNotThrow()
        {
            var viewModel = new DeviceDefineViewModel { Name = "ValidDevice" };

            Action action = () => viewModel.CompleteWorkflowCommand.Execute(null);

            action.Should().NotThrow();
        }

        [Fact]
        public void CanCompleteWorkflow_WhenAllStepsCallbackReturnsTrue_ReturnsTrue()
        {
            var viewModel = new DeviceDefineViewModel { Name = "ValidDevice" };
            viewModel.SetAllStepsRequiredFieldsFilledCheck(() => true);

            viewModel.CanCompleteWorkflow.Should().BeTrue();
        }

        [Fact]
        public void CanCompleteWorkflow_WhenAllStepsCallbackReturnsFalse_ReturnsFalse()
        {
            var viewModel = new DeviceDefineViewModel { Name = "ValidDevice" };
            viewModel.SetAllStepsRequiredFieldsFilledCheck(() => false);

            viewModel.CanCompleteWorkflow.Should().BeFalse();
        }

        #endregion
    }
}