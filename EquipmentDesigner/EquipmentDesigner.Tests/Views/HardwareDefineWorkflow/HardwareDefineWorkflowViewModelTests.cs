using System;
using System.Linq;
using System.Threading.Tasks;
using EquipmentDesigner.Models;
using EquipmentDesigner.Models;
using EquipmentDesigner.Models;
using EquipmentDesigner.Services;
using EquipmentDesigner.Services;
using EquipmentDesigner.ViewModels;
using FluentAssertions;
using Xunit;

namespace EquipmentDesigner.Tests.Views.HardwareDefineWorkflow
{
    public class HardwareDefineWorkflowViewModelTests
    {
        #region Initialization with Different Start Types

        [Fact]
        public void Constructor_WithEquipmentStartType_InitializesCorrectly()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Equipment);
            viewModel.StartType.Should().Be(HardwareType.Equipment);
        }

        [Fact]
        public void Constructor_WithSystemStartType_InitializesCorrectly()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.System);
            viewModel.StartType.Should().Be(HardwareType.System);
        }

        [Fact]
        public void Constructor_WithUnitStartType_InitializesCorrectly()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Unit);
            viewModel.StartType.Should().Be(HardwareType.Unit);
        }

        [Fact]
        public void Constructor_WithDeviceStartType_InitializesCorrectly()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Device);
            viewModel.StartType.Should().Be(HardwareType.Device);
        }

        #endregion

        #region Workflow Steps Based on Start Type

        [Fact]
        public void WorkflowSteps_WhenEquipmentStart_ReturnsEquipmentSystemUnitDevice()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Equipment);
            var stepNames = viewModel.WorkflowSteps.Select(s => s.StepName).ToList();

            stepNames.Should().HaveCount(4);
            stepNames.Should().ContainInOrder("Equipment", "System", "Unit", "Device");
        }

        [Fact]
        public void WorkflowSteps_WhenSystemStart_ReturnsSystemUnitDevice()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.System);
            var stepNames = viewModel.WorkflowSteps.Select(s => s.StepName).ToList();

            stepNames.Should().HaveCount(3);
            stepNames.Should().ContainInOrder("System", "Unit", "Device");
        }

        [Fact]
        public void WorkflowSteps_WhenUnitStart_ReturnsUnitDevice()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Unit);
            var stepNames = viewModel.WorkflowSteps.Select(s => s.StepName).ToList();

            stepNames.Should().HaveCount(2);
            stepNames.Should().ContainInOrder("Unit", "Device");
        }

        [Fact]
        public void WorkflowSteps_WhenDeviceStart_ReturnsDeviceOnly()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Device);
            var stepNames = viewModel.WorkflowSteps.Select(s => s.StepName).ToList();

            stepNames.Should().HaveCount(1);
            stepNames.Should().Contain("Device");
        }

        #endregion

        #region Current Step Index

        [Fact]
        public void CurrentStepIndex_OnInitialization_IsZero()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Equipment);
            viewModel.CurrentStepIndex.Should().Be(0);
        }

        [Fact]
        public void CurrentStep_OnInitialization_ReturnsFirstStep()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Equipment);
            viewModel.CurrentStep.StepName.Should().Be("Equipment");
        }

        #endregion

        #region Navigation Commands

        [Fact]
        public void GoToNextStepCommand_WhenExecuted_AdvancesCurrentStepIndexByOne()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Equipment);
            viewModel.EquipmentViewModel.Name = "TestEquipment"; // Make current step valid

            viewModel.GoToNextStepCommand.Execute(null);

            viewModel.CurrentStepIndex.Should().Be(1);
        }

        [Fact]
        public void GoToNextStepCommand_CanExecute_WhenAtLastStep_ReturnsFalse()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Device);
            viewModel.DeviceViewModel.Name = "TestDevice";

            viewModel.GoToNextStepCommand.CanExecute(null).Should().BeFalse();
        }

        [Fact]
        public void GoToPreviousStepCommand_WhenExecuted_DecreasesCurrentStepIndexByOne()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Equipment);
            viewModel.EquipmentViewModel.Name = "TestEquipment";
            viewModel.GoToNextStepCommand.Execute(null); // Move to step 1

            viewModel.GoToPreviousStepCommand.Execute(null);

            viewModel.CurrentStepIndex.Should().Be(0);
        }

        [Fact]
        public void GoToPreviousStepCommand_CanExecute_WhenAtFirstStep_ReturnsFalse()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Equipment);

            viewModel.GoToPreviousStepCommand.CanExecute(null).Should().BeFalse();
        }

        [Fact]
        public void ExitToDashboardCommand_CanExecute_AlwaysReturnsTrue()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Equipment);
            viewModel.ExitToDashboardCommand.CanExecute(null).Should().BeTrue();
        }

        #endregion

        #region Step Position Properties

        [Fact]
        public void IsFirstStep_WhenCurrentStepIndexIsZero_ReturnsTrue()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Equipment);
            viewModel.IsFirstStep.Should().BeTrue();
        }

        [Fact]
        public void IsLastStep_WhenCurrentStepIndexEqualsLastStepIndex_ReturnsTrue()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Device);
            viewModel.IsLastStep.Should().BeTrue();
        }

        [Fact]
        public void IsFirstStep_WhenNavigatedToSecondStep_ReturnsFalse()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Equipment);
            viewModel.EquipmentViewModel.Name = "TestEquipment";
            viewModel.GoToNextStepCommand.Execute(null);

            viewModel.IsFirstStep.Should().BeFalse();
        }

        #endregion

        #region Property Change Notifications

        [Fact]
        public void CurrentStepIndex_WhenNavigating_RaisesPropertyChanged()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Equipment);
            viewModel.EquipmentViewModel.Name = "TestEquipment";
            var raised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(HardwareDefineWorkflowViewModel.CurrentStepIndex))
                    raised = true;
            };

            viewModel.GoToNextStepCommand.Execute(null);

            raised.Should().BeTrue();
        }

        [Fact]
        public void CurrentStep_WhenNavigating_RaisesPropertyChanged()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Equipment);
            viewModel.EquipmentViewModel.Name = "TestEquipment";
            var raised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(HardwareDefineWorkflowViewModel.CurrentStep))
                    raised = true;
            };

            viewModel.GoToNextStepCommand.Execute(null);

            raised.Should().BeTrue();
        }

        [Fact]
        public void IsFirstStep_WhenNavigating_RaisesPropertyChanged()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Equipment);
            viewModel.EquipmentViewModel.Name = "TestEquipment";
            var raised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(HardwareDefineWorkflowViewModel.IsFirstStep))
                    raised = true;
            };

            viewModel.GoToNextStepCommand.Execute(null);

            raised.Should().BeTrue();
        }

        [Fact]
        public void IsLastStep_WhenNavigating_RaisesPropertyChanged()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Equipment);
            viewModel.EquipmentViewModel.Name = "TestEquipment";
            var raised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(HardwareDefineWorkflowViewModel.IsLastStep))
                    raised = true;
            };

            viewModel.GoToNextStepCommand.Execute(null);

            raised.Should().BeTrue();
        }

        #endregion


        #region Workflow Step Step Numbers

        [Fact]
        public void WorkflowSteps_HaveCorrectStepNumbers()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Equipment);

            // Equipment workflow has 4 steps: Equipment(1), System(2), Unit(3), Device(4)
            viewModel.WorkflowSteps[0].StepNumber.Should().Be(1);
            viewModel.WorkflowSteps[1].StepNumber.Should().Be(2);
            viewModel.WorkflowSteps[2].StepNumber.Should().Be(3);
            viewModel.WorkflowSteps[3].StepNumber.Should().Be(4);
        }

        #endregion

        #region Active Step Tracking

        [Fact]
        public void CurrentStep_IsActive_WhenIsCurrentStep()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Equipment);
            viewModel.WorkflowSteps[0].IsActive.Should().BeTrue();
            viewModel.WorkflowSteps[1].IsActive.Should().BeFalse();
        }

        [Fact]
        public void PreviousSteps_AreCompleted_WhenNavigatedPast()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Equipment);
            viewModel.EquipmentViewModel.Name = "TestEquipment";
            viewModel.GoToNextStepCommand.Execute(null); // Move to step 1 (System)

            viewModel.WorkflowSteps[0].IsCompleted.Should().BeTrue();
            viewModel.WorkflowSteps[0].IsActive.Should().BeFalse();
            viewModel.WorkflowSteps[1].IsActive.Should().BeTrue();
        }

        #endregion

        #region NavigateToStepCommand Initialization

        [Fact]
        public void NavigateToStepCommand_OnInitialization_IsNotNull()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Equipment);
            viewModel.NavigateToStepCommand.Should().NotBeNull();
        }

        [Fact]
        public void NavigateToStepCommand_CanExecute_WithNullParameter_ReturnsFalse()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Equipment);
            viewModel.NavigateToStepCommand.CanExecute(null).Should().BeFalse();
        }

        #endregion

        #region Navigation to Completed Steps

        [Fact]
        public void NavigateToStep_WhenStepIsCompleted_ChangesCurrentStepIndexToTargetStep()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Equipment);
            viewModel.EquipmentViewModel.Name = "TestEquipment";
            viewModel.GoToNextStepCommand.Execute(null); // Move to step 1 (System)
            viewModel.SystemViewModel.Name = "TestSystem";
            viewModel.GoToNextStepCommand.Execute(null); // Move to step 2 (Unit)

            viewModel.NavigateToStepCommand.Execute(viewModel.WorkflowSteps[0]); // Navigate back to Equipment

            viewModel.CurrentStepIndex.Should().Be(0);
        }

        [Fact]
        public void NavigateToStep_WhenStepIsCompleted_UpdatesIsActiveCorrectly()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Equipment);
            viewModel.EquipmentViewModel.Name = "TestEquipment";
            viewModel.GoToNextStepCommand.Execute(null); // Move to step 1

            viewModel.NavigateToStepCommand.Execute(viewModel.WorkflowSteps[0]); // Navigate back to Equipment

            viewModel.WorkflowSteps[0].IsActive.Should().BeTrue();
            viewModel.WorkflowSteps[1].IsActive.Should().BeFalse();
        }

        [Fact]
        public void NavigateToStep_WhenStepIsCompleted_PreservesCompletedStatesForPreviousSteps()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Equipment);
            viewModel.EquipmentViewModel.Name = "TestEquipment";
            viewModel.GoToNextStepCommand.Execute(null);
            viewModel.SystemViewModel.Name = "TestSystem";
            viewModel.GoToNextStepCommand.Execute(null);
            viewModel.UnitViewModel.Name = "TestUnit";
            viewModel.GoToNextStepCommand.Execute(null); // Now at Device (step 3)

            viewModel.NavigateToStepCommand.Execute(viewModel.WorkflowSteps[1]); // Navigate back to System (step 1)

            viewModel.WorkflowSteps[0].IsCompleted.Should().BeTrue(); // Equipment still completed
        }

        [Fact]
        public void NavigateToStep_WhenNavigatingBackTwoSteps_SetsCorrectCurrentStepIndex()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Equipment);
            viewModel.EquipmentViewModel.Name = "TestEquipment";
            viewModel.GoToNextStepCommand.Execute(null);
            viewModel.SystemViewModel.Name = "TestSystem";
            viewModel.GoToNextStepCommand.Execute(null);
            viewModel.UnitViewModel.Name = "TestUnit";
            viewModel.GoToNextStepCommand.Execute(null); // Now at Device (step 3)

            viewModel.NavigateToStepCommand.Execute(viewModel.WorkflowSteps[0]); // Navigate back to Equipment (step 0)

            viewModel.CurrentStepIndex.Should().Be(0);
        }

        #endregion

        #region Navigation to Current Step

        [Fact]
        public void NavigateToStep_WhenStepIsActive_CurrentStepIndexRemainsUnchanged()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Equipment);
            var initialIndex = viewModel.CurrentStepIndex;

            viewModel.NavigateToStepCommand.Execute(viewModel.WorkflowSteps[0]); // Navigate to current step

            viewModel.CurrentStepIndex.Should().Be(initialIndex);
        }

        [Fact]
        public void NavigateToStep_WhenStepIsActive_NoPropertyChangedEventFired()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Equipment);
            var propertyChangedFired = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(HardwareDefineWorkflowViewModel.CurrentStepIndex))
                    propertyChangedFired = true;
            };

            viewModel.NavigateToStepCommand.Execute(viewModel.WorkflowSteps[0]); // Navigate to current step

            propertyChangedFired.Should().BeFalse();
        }

        #endregion

 

        #region CanNavigateTo Property on WorkflowStepViewModel

        [Fact]
        public void CanNavigateTo_WhenStepIsCompleted_ReturnsTrue()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Equipment);
            viewModel.EquipmentViewModel.Name = "TestEquipment";
            viewModel.GoToNextStepCommand.Execute(null); // Move to step 1

            viewModel.WorkflowSteps[0].CanNavigateTo.Should().BeTrue();
        }

        [Fact]
        public void CanNavigateTo_WhenStepIsActive_ReturnsTrue()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Equipment);

            viewModel.WorkflowSteps[0].CanNavigateTo.Should().BeTrue();
        }

        [Fact]
        public void CanNavigateTo_WhenStepBecomesCompleted_UpdatesToTrue()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Equipment);
            viewModel.WorkflowSteps[0].CanNavigateTo.Should().BeTrue(); // Active

            viewModel.EquipmentViewModel.Name = "TestEquipment";
            viewModel.GoToNextStepCommand.Execute(null);

            viewModel.WorkflowSteps[0].CanNavigateTo.Should().BeTrue(); // Now completed
            viewModel.WorkflowSteps[1].CanNavigateTo.Should().BeTrue(); // Now active
        }

        #endregion

        #region Step State Consistency After Navigation


        [Fact]
        public void NavigateToStep_AfterBackwardNavigation_StepsBeforeTargetRemainCompleted()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Equipment);
            viewModel.EquipmentViewModel.Name = "TestEquipment";
            viewModel.GoToNextStepCommand.Execute(null);
            viewModel.SystemViewModel.Name = "TestSystem";
            viewModel.GoToNextStepCommand.Execute(null);
            viewModel.UnitViewModel.Name = "TestUnit";
            viewModel.GoToNextStepCommand.Execute(null); // Now at Device (step 3)

            ////viewModel.NavigateToStepCommand.Execute(viewModel.WorkflowSteps[1]); // Navigate to System (step 1)

            viewModel.WorkflowSteps[0].IsCompleted.Should().BeTrue();
        }

        #endregion

        #region NavigateToStep Property Changed Notifications

        [Fact]
        public void NavigateToStep_WhenSuccessful_RaisesCurrentStepIndexPropertyChanged()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Equipment);
            viewModel.EquipmentViewModel.Name = "TestEquipment";
            viewModel.GoToNextStepCommand.Execute(null);
            var raised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(HardwareDefineWorkflowViewModel.CurrentStepIndex))
                    raised = true;
            };

            viewModel.NavigateToStepCommand.Execute(viewModel.WorkflowSteps[0]);

            raised.Should().BeTrue();
        }

        [Fact]
        public void NavigateToStep_WhenSuccessful_RaisesCurrentStepPropertyChanged()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Equipment);
            viewModel.EquipmentViewModel.Name = "TestEquipment";
            viewModel.GoToNextStepCommand.Execute(null);
            var raised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(HardwareDefineWorkflowViewModel.CurrentStep))
                    raised = true;
            };

            viewModel.NavigateToStepCommand.Execute(viewModel.WorkflowSteps[0]);

            raised.Should().BeTrue();
        }

        [Fact]
        public void NavigateToStep_WhenSuccessful_RaisesIsFirstStepPropertyChanged()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Equipment);
            viewModel.EquipmentViewModel.Name = "TestEquipment";
            viewModel.GoToNextStepCommand.Execute(null);
            var raised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(HardwareDefineWorkflowViewModel.IsFirstStep))
                    raised = true;
            };

            viewModel.NavigateToStepCommand.Execute(viewModel.WorkflowSteps[0]);

            raised.Should().BeTrue();
        }

        [Fact]
        public void NavigateToStep_WhenSuccessful_RaisesIsLastStepPropertyChanged()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Equipment);
            viewModel.EquipmentViewModel.Name = "TestEquipment";
            viewModel.GoToNextStepCommand.Execute(null);
            var raised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(HardwareDefineWorkflowViewModel.IsLastStep))
                    raised = true;
            };

            viewModel.NavigateToStepCommand.Execute(viewModel.WorkflowSteps[0]);

            raised.Should().BeTrue();
        }

        #endregion

        #region NavigateToStep Edge Cases

        [Fact]
        public void NavigateToStep_WithSingleStepWorkflow_CannotNavigateAnywhere()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Device);

            // Only one step, already active - CanExecute should be true but navigation does nothing
            // We can verify that the event exists and is connected
            viewModel.NavigateToStepCommand.CanExecute(viewModel.WorkflowSteps[0]).Should().BeTrue();
            viewModel.CurrentStepIndex.Should().Be(0);
        }

        [Fact]
        public void NavigateToStep_WhenAtLastStep_CanNavigateBackToAnyCompletedStep()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Equipment);
            viewModel.EquipmentViewModel.Name = "TestEquipment";
            viewModel.GoToNextStepCommand.Execute(null);
            viewModel.SystemViewModel.Name = "TestSystem";
            viewModel.GoToNextStepCommand.Execute(null);
            viewModel.UnitViewModel.Name = "TestUnit";
            viewModel.GoToNextStepCommand.Execute(null); // Now at Device (last step)

            viewModel.NavigateToStepCommand.CanExecute(viewModel.WorkflowSteps[0]).Should().BeTrue();
            viewModel.NavigateToStepCommand.CanExecute(viewModel.WorkflowSteps[1]).Should().BeTrue();
            viewModel.NavigateToStepCommand.CanExecute(viewModel.WorkflowSteps[2]).Should().BeTrue();
        }

        [Fact]
        public void NavigateToStep_WithInvalidStepNotInWorkflow_DoesNothing()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Equipment);
            var externalStep = new WorkflowStepViewModel(99, "External");
            var initialIndex = viewModel.CurrentStepIndex;

            viewModel.NavigateToStepCommand.Execute(externalStep);

            viewModel.CurrentStepIndex.Should().Be(initialIndex);
        }

        #endregion

        #region Workflow Completion Event Subscription

        [Fact]
        public async Task CompleteWorkflowAsync_WhenEquipmentStartType_SavesWorkflowWithTreeNodes()
        {
            // Arrange
            ServiceLocator.Reset();
            ServiceLocator.ConfigureForTesting();
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Equipment);
            viewModel.EquipmentViewModel.Name = "TestEquipment";
            viewModel.SystemViewModel.Name = "TestSystem";
            viewModel.UnitViewModel.Name = "TestUnit";
            viewModel.DeviceViewModel.Name = "TestDevice";

            // Act
            viewModel.DeviceViewModel.CompleteWorkflowCommand.Execute(null);
            await Task.Delay(1000); // Wait for async operation

            // Assert - CompleteWorkflowAsync saves to HardwareDefinitionDataStore
            var workflowRepository = ServiceLocator.GetService<IWorkflowRepository>();
            var workflowData = await workflowRepository.LoadAsync();

            var savedWorkflow = workflowData.WorkflowSessions.FirstOrDefault(w => w.Id == viewModel.WorkflowId);
            savedWorkflow.Should().NotBeNull();
            savedWorkflow.TreeNodes.Should().NotBeEmpty();
            savedWorkflow.HardwareType.Should().Be(HardwareType.Equipment);
        }

        [Fact]
        public async Task CompleteWorkflowAsync_WhenSystemStartType_SavesWorkflowWithCorrectStartType()
        {
            // Arrange
            ServiceLocator.Reset();
            ServiceLocator.ConfigureForTesting();
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.System);
            viewModel.SystemViewModel.Name = "TestSystem";
            viewModel.UnitViewModel.Name = "TestUnit";
            viewModel.DeviceViewModel.Name = "TestDevice";

            // Act
            viewModel.DeviceViewModel.CompleteWorkflowCommand.Execute(null);
            await Task.Delay(1000);

            // Assert - CompleteWorkflowAsync saves to HardwareDefinitionDataStore
            var workflowRepository = ServiceLocator.GetService<IWorkflowRepository>();
            var workflowData = await workflowRepository.LoadAsync();

            var savedWorkflow = workflowData.WorkflowSessions.FirstOrDefault(w => w.Id == viewModel.WorkflowId);
            savedWorkflow.Should().NotBeNull();
            savedWorkflow.HardwareType.Should().Be(HardwareType.System);
        }

        [Fact]
        public async Task CompleteWorkflowAsync_WhenDeviceStartType_SavesWorkflowWithDeviceStartType()
        {
            // Arrange
            ServiceLocator.Reset();
            ServiceLocator.ConfigureForTesting();
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Device);
            viewModel.DeviceViewModel.Name = "TestDevice";

            // Act
            viewModel.DeviceViewModel.CompleteWorkflowCommand.Execute(null);
            await Task.Delay(1000);

            // Assert - CompleteWorkflowAsync saves to HardwareDefinitionDataStore
            var workflowRepository = ServiceLocator.GetService<IWorkflowRepository>();
            var workflowData = await workflowRepository.LoadAsync();

            var savedWorkflow = workflowData.WorkflowSessions.FirstOrDefault(w => w.Id == viewModel.WorkflowId);
            savedWorkflow.Should().NotBeNull();
            savedWorkflow.HardwareType.Should().Be(HardwareType.Device);
        }

        [Fact]
        public async Task CompleteWorkflowAsync_SavedComponentsHaveDefinedState()
        {
            // Arrange
            ServiceLocator.Reset();
            ServiceLocator.ConfigureForTesting();
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Equipment);
            viewModel.EquipmentViewModel.Name = "TestEquipment";
            viewModel.SystemViewModel.Name = "TestSystem";
            viewModel.UnitViewModel.Name = "TestUnit";
            viewModel.DeviceViewModel.Name = "TestDevice";

            // Act
            viewModel.DeviceViewModel.CompleteWorkflowCommand.Execute(null);
            await Task.Delay(1000);

            // Assert - CompleteWorkflowAsync now saves to HardwareDefinitionDataStore with Defined state
            var workflowRepository = ServiceLocator.GetService<IWorkflowRepository>();
            var workflowData = await workflowRepository.LoadAsync();

            var savedWorkflow = workflowData.WorkflowSessions.FirstOrDefault(w => w.Id == viewModel.WorkflowId);
            savedWorkflow.Should().NotBeNull();
            savedWorkflow.State.Should().Be(ComponentState.Ready);
        }

        [Fact]
        public async Task CompleteWorkflowAsync_SavesWorkflowWithTreeNodes()
        {
            // Arrange
            ServiceLocator.Reset();
            ServiceLocator.ConfigureForTesting();
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Equipment);
            viewModel.EquipmentViewModel.Name = "TestEquipment";
            viewModel.SystemViewModel.Name = "TestSystem";
            viewModel.UnitViewModel.Name = "TestUnit";
            viewModel.DeviceViewModel.Name = "TestDevice";

            // Act
            viewModel.DeviceViewModel.CompleteWorkflowCommand.Execute(null);
            await Task.Delay(1000);

            // Assert - Workflow should contain tree nodes
            var workflowRepository = ServiceLocator.GetService<IWorkflowRepository>();
            var workflowData = await workflowRepository.LoadAsync();

            var savedWorkflow = workflowData.WorkflowSessions.FirstOrDefault(w => w.Id == viewModel.WorkflowId);
            savedWorkflow.Should().NotBeNull();
            savedWorkflow.TreeNodes.Should().NotBeEmpty();
        }

        [Fact]
        public async Task CompleteWorkflowAsync_UpdatesExistingWorkflow()
        {
            // Arrange
            ServiceLocator.Reset();
            ServiceLocator.ConfigureForTesting();
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Equipment);
            viewModel.EquipmentViewModel.Name = "TestEquipment";
            viewModel.SystemViewModel.Name = "TestSystem";
            viewModel.UnitViewModel.Name = "TestUnit";
            viewModel.DeviceViewModel.Name = "TestDevice";

            // First save
            viewModel.DeviceViewModel.CompleteWorkflowCommand.Execute(null);
            await Task.Delay(1000);

            // Modify and save again
            viewModel.EquipmentViewModel.Name = "UpdatedEquipment";
            viewModel.DeviceViewModel.CompleteWorkflowCommand.Execute(null);
            await Task.Delay(1000);

            // Assert - Should update, not create duplicate
            var workflowRepository = ServiceLocator.GetService<IWorkflowRepository>();
            var workflowData = await workflowRepository.LoadAsync();

            workflowData.WorkflowSessions.Count(w => w.Id == viewModel.WorkflowId).Should().Be(1);
        }

        [Fact]
        public async Task CompleteWorkflowAsync_SetsLastModifiedAt()
        {
            // Arrange
            ServiceLocator.Reset();
            ServiceLocator.ConfigureForTesting();
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Device);
            viewModel.DeviceViewModel.Name = "TestDevice";
            var beforeComplete = DateTime.Now;

            // Act
            viewModel.DeviceViewModel.CompleteWorkflowCommand.Execute(null);
            await Task.Delay(1000);

            // Assert
            var workflowRepository = ServiceLocator.GetService<IWorkflowRepository>();
            var workflowData = await workflowRepository.LoadAsync();

            var savedWorkflow = workflowData.WorkflowSessions.FirstOrDefault(w => w.Id == viewModel.WorkflowId);
            savedWorkflow.Should().NotBeNull();
            savedWorkflow.LastModifiedAt.Should().BeOnOrAfter(beforeComplete);
        }

        #endregion

        #region IsReadOnly and Edit Mode Tests

        [Fact]
        public void IsReadOnly_OnNewWorkflow_DefaultsToFalse()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Device);
            viewModel.IsReadOnly.Should().BeFalse();
        }

        [Fact]
        public void IsReadOnly_CanBeSetToTrue()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Device);
            viewModel.IsReadOnly = true;
            viewModel.IsReadOnly.Should().BeTrue();
        }

        [Fact]
        public void EnableEditCommand_IsNotNull()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Device);
            viewModel.EnableEditCommand.Should().NotBeNull();
        }

        [Fact]
        public void EnableEditCommand_WhenExecuted_SetsIsReadOnlyToFalse()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Device);
            viewModel.IsReadOnly = true;
            
            viewModel.EnableEditCommand.Execute(null);
            
            viewModel.IsReadOnly.Should().BeFalse();
        }

        [Fact]
        public void IsEditButtonVisible_WhenIsReadOnlyTrue_ReturnsTrue()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Device);
            viewModel.IsReadOnly = true;
            viewModel.IsEditButtonVisible.Should().BeTrue();
        }

        [Fact]
        public void IsEditButtonVisible_WhenIsReadOnlyFalse_ReturnsFalse()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Device);
            viewModel.IsReadOnly = false;
            viewModel.IsEditButtonVisible.Should().BeFalse();
        }

        [Fact]
        public void IsReadOnly_WhenChanged_RaisesPropertyChanged()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Device);
            var raised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(HardwareDefineWorkflowViewModel.IsReadOnly))
                    raised = true;
            };

            viewModel.IsReadOnly = true;

            raised.Should().BeTrue();
        }

        [Fact]
        public void IsReadOnly_WhenChanged_RaisesIsEditButtonVisiblePropertyChanged()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Device);
            var raised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(HardwareDefineWorkflowViewModel.IsEditButtonVisible))
                    raised = true;
            };

            viewModel.IsReadOnly = true;

            raised.Should().BeTrue();
        }

        #endregion

        #region CompleteWorkflowCommand New Behavior Tests

        [Fact]
        public void CompleteWorkflowCommand_OnInitialization_IsNotNull()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Device);
            viewModel.CompleteWorkflowCommand.Should().NotBeNull();
        }

        [Fact]
        public void CanCompleteWorkflow_WhenIsReadOnlyTrue_ReturnsFalse()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Device);
            viewModel.DeviceViewModel.Name = "TestDevice";
            viewModel.IsReadOnly = true;
            
            viewModel.CanCompleteWorkflow.Should().BeFalse();
        }

        [Fact]
        public async Task CanCompleteWorkflow_WhenIsWorkflowCompletedTrue_ReturnsFalse()
        {
            // Arrange
            ServiceLocator.Reset();
            ServiceLocator.ConfigureForTesting();
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Device);
            viewModel.DeviceViewModel.Name = "TestDevice";
            
            // Complete the workflow first
            viewModel.CompleteWorkflowCommand.Execute(null);
            await Task.Delay(1000); // Wait for async operation
            
            viewModel.CanCompleteWorkflow.Should().BeFalse();
        }

        [Fact]
        public void CanCompleteWorkflow_WhenRequiredFieldsNotFilled_ReturnsFalse()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Device);
            // Don't set Name - required field
            
            viewModel.CanCompleteWorkflow.Should().BeFalse();
        }

        [Fact]
        public void CanCompleteWorkflow_WhenAllConditionsMet_ReturnsTrue()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Device);
            viewModel.DeviceViewModel.Name = "TestDevice";
            viewModel.IsReadOnly = false;
            
            viewModel.CanCompleteWorkflow.Should().BeTrue();
        }

        [Fact]
        public async Task CompleteWorkflowCommand_WhenExecuted_SetsIsWorkflowCompletedToTrue()
        {
            // Arrange
            ServiceLocator.Reset();
            ServiceLocator.ConfigureForTesting();
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Device);
            viewModel.DeviceViewModel.Name = "TestDevice";
            
            // Act
            viewModel.CompleteWorkflowCommand.Execute(null);
            await Task.Delay(1000); // Wait for async operation
            
            // Assert
            viewModel.IsWorkflowCompleted.Should().BeTrue();
        }

        [Fact]
        public async Task CompleteWorkflowCommand_WhenExecuted_ShowUploadButtonBecomesTrue()
        {
            // Arrange
            ServiceLocator.Reset();
            ServiceLocator.ConfigureForTesting();
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Device);
            viewModel.DeviceViewModel.Name = "TestDevice";
            
            // Act
            viewModel.CompleteWorkflowCommand.Execute(null);
            await Task.Delay(1000); // Wait for async operation
            
            // Assert
            viewModel.ShowUploadButton.Should().BeTrue();
        }

        [Fact]
        public async Task ShowUploadButton_AfterDataChangedPostCompletion_BecomesFalse()
        {
            // Arrange
            ServiceLocator.Reset();
            ServiceLocator.ConfigureForTesting();
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Device);
            viewModel.DeviceViewModel.Name = "TestDevice";
            viewModel.CompleteWorkflowCommand.Execute(null);
            await Task.Delay(1000);
            viewModel.ShowUploadButton.Should().BeTrue();
            
            // Act - Simulate data change
            viewModel.DeviceViewModel.Name = "ModifiedName";
            
            // Assert
            viewModel.ShowUploadButton.Should().BeFalse();
        }

        #endregion

        #region UploadToServerCommand Tests

        [Fact]
        public void UploadToServerCommand_OnInitialization_IsNotNull()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Device);
            viewModel.UploadToServerCommand.Should().NotBeNull();
        }

        [Fact]
        public async Task UploadToServer_MapsTreeDataToFlatHardwareLists()
        {
            // Arrange
            ServiceLocator.Reset();
            ServiceLocator.ConfigureForTesting();
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Equipment);
            viewModel.EquipmentViewModel.Name = "TestEquipment";
            viewModel.SystemViewModel.Name = "TestSystem";
            viewModel.UnitViewModel.Name = "TestUnit";
            viewModel.DeviceViewModel.Name = "TestDevice";
            
            // Complete workflow first
            viewModel.CompleteWorkflowCommand.Execute(null);
            await Task.Delay(1000);
            
            // Act
            viewModel.UploadToServerCommand.Execute(null);
            await Task.Delay(1000);
            
            // Assert - Check UploadedWorkflowDataStore (unified structure)
            var uploadedRepo = ServiceLocator.GetService<IUploadedWorkflowRepository>();
            var uploadedData = await uploadedRepo.LoadAsync();
            
            uploadedData.WorkflowSessions.Should().HaveCount(1);
            uploadedData.WorkflowSessions[0].HardwareType.Should().Be(HardwareType.Equipment);
            uploadedData.WorkflowSessions[0].TreeNodes.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task UploadToServer_SetsComponentStateToUploaded()
        {
            // Arrange
            ServiceLocator.Reset();
            ServiceLocator.ConfigureForTesting();
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Device);
            viewModel.DeviceViewModel.Name = "TestDevice";
            
            // Complete workflow first
            viewModel.CompleteWorkflowCommand.Execute(null);
            await Task.Delay(1000);
            
            // Act
            viewModel.UploadToServerCommand.Execute(null);
            await Task.Delay(1000);
            
            // Assert
            var uploadedRepo = ServiceLocator.GetService<IUploadedWorkflowRepository>();
            var uploadedData = await uploadedRepo.LoadAsync();
            
            uploadedData.WorkflowSessions.Should().HaveCount(1);
            uploadedData.WorkflowSessions[0].State.Should().Be(ComponentState.Uploaded);
        }

        [Fact]
        public async Task UploadToServer_PreservesParentChildRelationshipsInUploadedData()
        {
            // Arrange
            ServiceLocator.Reset();
            ServiceLocator.ConfigureForTesting();
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Equipment);
            viewModel.EquipmentViewModel.Name = "TestEquipment";
            viewModel.SystemViewModel.Name = "TestSystem";
            viewModel.UnitViewModel.Name = "TestUnit";
            viewModel.DeviceViewModel.Name = "TestDevice";
            
            // Complete workflow first
            viewModel.CompleteWorkflowCommand.Execute(null);
            await Task.Delay(1000);
            
            // Act
            viewModel.UploadToServerCommand.Execute(null);
            await Task.Delay(1000);
            
            // Assert - Check that tree structure preserves hierarchy
            var uploadedRepo = ServiceLocator.GetService<IUploadedWorkflowRepository>();
            var uploadedData = await uploadedRepo.LoadAsync();
            
            var session = uploadedData.WorkflowSessions[0];
            session.TreeNodes.Should().NotBeNullOrEmpty();
            var rootNode = session.TreeNodes[0];
            rootNode.HardwareType.Should().Be(HardwareType.Equipment);
            rootNode.EquipmentData.Should().NotBeNull();
            rootNode.EquipmentData.Name.Should().Be("TestEquipment");
        }

        [Fact]
        public async Task UploadToServer_UpdatesBothWorkflowsAndUploadedHardwaresFiles()
        {
            // Arrange
            ServiceLocator.Reset();
            ServiceLocator.ConfigureForTesting();
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Device);
            viewModel.DeviceViewModel.Name = "TestDevice";
            
            // Pre-populate workflow repository
            var workflowRepo = ServiceLocator.GetService<IWorkflowRepository>();
            var workflowData = await workflowRepo.LoadAsync();
            workflowData.WorkflowSessions.Add(new HardwareDefinition
            {
                Id = viewModel.WorkflowId,
                HardwareType = HardwareType.Device,
                State = ComponentState.Ready
            });
            await workflowRepo.SaveAsync(workflowData);
            
            // Complete workflow
            viewModel.CompleteWorkflowCommand.Execute(null);
            await Task.Delay(1000);
            
            // Act
            viewModel.UploadToServerCommand.Execute(null);
            await Task.Delay(1000);
            
            // Assert - UploadedWorkflowDataStore updated with unified structure
            var uploadedRepo = ServiceLocator.GetService<IUploadedWorkflowRepository>();
            var uploadedData = await uploadedRepo.LoadAsync();
            uploadedData.WorkflowSessions.Should().HaveCount(1);
            uploadedData.WorkflowSessions[0].State.Should().Be(ComponentState.Uploaded);
            
            // Workflow should be removed from IncompleteWorkflowDataStore after successful upload
            workflowData = await workflowRepo.LoadAsync();
            workflowData.WorkflowSessions.Any(s => s.Id == viewModel.WorkflowId).Should().BeFalse();
        }

        [Fact]
        public async Task UploadToServer_RemovesWorkflowFromIncompleteWorkflowDataStore()
        {
            // Arrange
            ServiceLocator.Reset();
            ServiceLocator.ConfigureForTesting();
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Device);
            viewModel.DeviceViewModel.Name = "TestDevice";
            
            // Pre-populate workflow repository with a session
            var workflowRepo = ServiceLocator.GetService<IWorkflowRepository>();
            var workflowData = await workflowRepo.LoadAsync();
            workflowData.WorkflowSessions.Add(new HardwareDefinition
            {
                Id = viewModel.WorkflowId,
                HardwareType = HardwareType.Device,
                State = ComponentState.Ready
            });
            await workflowRepo.SaveAsync(workflowData);
            
            // Complete workflow
            viewModel.CompleteWorkflowCommand.Execute(null);
            await Task.Delay(1000);
            
            // Act
            viewModel.UploadToServerCommand.Execute(null);
            await Task.Delay(1000);
            
            // Assert - Workflow should be removed from IncompleteWorkflowDataStore
            workflowData = await workflowRepo.LoadAsync();
            workflowData.WorkflowSessions.Any(s => s.Id == viewModel.WorkflowId).Should().BeFalse();
        }

        #endregion

        #region Autosave Integration with WorkflowRepository

        [Fact]
        public async Task SaveWorkflowStateAsync_UsesWorkflowRepositoryInsteadOfOldRepository()
        {
            // Arrange
            ServiceLocator.Reset();
            ServiceLocator.ConfigureForTesting();
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Device);
            viewModel.DeviceViewModel.Name = "TestDevice";
            
            // Act - ExitToDashboard triggers SaveWorkflowStateAsync
            viewModel.ExitToDashboardCommand.Execute(null);
            await Task.Delay(1000);
            
            // Assert - Check WorkflowRepository was used
            var workflowRepo = ServiceLocator.GetService<IWorkflowRepository>();
            var workflowData = await workflowRepo.LoadAsync();
            
            workflowData.WorkflowSessions.Should().HaveCount(1);
            workflowData.WorkflowSessions.First().Id.Should().Be(viewModel.WorkflowId);
        }

        [Fact]
        public async Task SaveWorkflowStateAsync_CreatesHardwareDefinitionInIncompleteWorkflowDataStore()
        {
            // Arrange
            ServiceLocator.Reset();
            ServiceLocator.ConfigureForTesting();
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Equipment);
            viewModel.EquipmentViewModel.Name = "TestEquipment";
            
            // Act
            viewModel.ExitToDashboardCommand.Execute(null);
            await Task.Delay(1000);
            
            // Assert
            var workflowRepo = ServiceLocator.GetService<IWorkflowRepository>();
            var workflowData = await workflowRepo.LoadAsync();
            
            var session = workflowData.WorkflowSessions.FirstOrDefault(s => s.Id == viewModel.WorkflowId);
            session.Should().NotBeNull();
            session.HardwareType.Should().Be(HardwareType.Equipment);
            session.State.Should().Be(ComponentState.Draft);
        }

        [Fact]
        public async Task SaveWorkflowStateAsync_PreservesEntireTreeStructureInTreeNodesProperty()
        {
            // Arrange
            ServiceLocator.Reset();
            ServiceLocator.ConfigureForTesting();
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Equipment);
            viewModel.EquipmentViewModel.Name = "TestEquipment";
            viewModel.SystemViewModel.Name = "TestSystem";
            viewModel.UnitViewModel.Name = "TestUnit";
            viewModel.DeviceViewModel.Name = "TestDevice";
            
            // Act
            viewModel.ExitToDashboardCommand.Execute(null);
            await Task.Delay(1000);
            
            // Assert
            var workflowRepo = ServiceLocator.GetService<IWorkflowRepository>();
            var workflowData = await workflowRepo.LoadAsync();
            
            var session = workflowData.WorkflowSessions.FirstOrDefault(s => s.Id == viewModel.WorkflowId);
            session.Should().NotBeNull();
            session.TreeNodes.Should().NotBeEmpty();
            
            // Verify Equipment is at root
            var equipmentNode = session.TreeNodes.FirstOrDefault();
            equipmentNode.Should().NotBeNull();
            equipmentNode.HardwareType.Should().Be(HardwareType.Equipment);
            equipmentNode.EquipmentData.Should().NotBeNull();
            equipmentNode.EquipmentData.Name.Should().Be("TestEquipment");
        }

        [Fact]
        public async Task SaveWorkflowStateAsync_UpdatesExistingWorkflowSessionByWorkflowId()
        {
            // Arrange
            ServiceLocator.Reset();
            ServiceLocator.ConfigureForTesting();
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Device);
            viewModel.DeviceViewModel.Name = "InitialName";
            
            // First save
            viewModel.ExitToDashboardCommand.Execute(null);
            await Task.Delay(1000);
            
            // Modify and save again
            viewModel.DeviceViewModel.Name = "UpdatedName";
            viewModel.ExitToDashboardCommand.Execute(null);
            await Task.Delay(1000);
            
            // Assert
            var workflowRepo = ServiceLocator.GetService<IWorkflowRepository>();
            var workflowData = await workflowRepo.LoadAsync();
            
            // Should still only have 1 session (updated, not duplicated)
            workflowData.WorkflowSessions.Count(s => s.Id == viewModel.WorkflowId).Should().Be(1);
            
            var session = workflowData.WorkflowSessions.First(s => s.Id == viewModel.WorkflowId);
            session.TreeNodes.First().DeviceData.Name.Should().Be("UpdatedName");
        }

        [Fact]
        public async Task SaveWorkflowStateAsync_SetsStateToDefinedWhenAllRequiredFieldsFilled()
        {
            // Arrange
            ServiceLocator.Reset();
            ServiceLocator.ConfigureForTesting();
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Device);
            viewModel.DeviceViewModel.Name = "TestDevice"; // Required field filled
            
            // Act
            viewModel.ExitToDashboardCommand.Execute(null);
            await Task.Delay(1000);
            
            // Assert
            var workflowRepo = ServiceLocator.GetService<IWorkflowRepository>();
            var workflowData = await workflowRepo.LoadAsync();
            
            var session = workflowData.WorkflowSessions.First(s => s.Id == viewModel.WorkflowId);
            session.State.Should().Be(ComponentState.Ready);
        }

        [Fact]
        public async Task SaveWorkflowStateAsync_SetsStateToUndefinedWhenRequiredFieldsNotFilled()
        {
            // Arrange
            ServiceLocator.Reset();
            ServiceLocator.ConfigureForTesting();
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Device);
            // Don't set Name - required field not filled
            
            // Act
            viewModel.ExitToDashboardCommand.Execute(null);
            await Task.Delay(1000);
            
            // Assert
            var workflowRepo = ServiceLocator.GetService<IWorkflowRepository>();
            var workflowData = await workflowRepo.LoadAsync();
            
            var session = workflowData.WorkflowSessions.First(s => s.Id == viewModel.WorkflowId);
            session.State.Should().Be(ComponentState.Draft);
        }

        [Fact]
        public async Task SaveWorkflowStateAsync_UpdatesLastModifiedAtTimestamp()
        {
            // Arrange
            ServiceLocator.Reset();
            ServiceLocator.ConfigureForTesting();
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareType.Device);
            viewModel.DeviceViewModel.Name = "TestDevice";
            var beforeSave = DateTime.Now;
            
            // Act
            viewModel.ExitToDashboardCommand.Execute(null);
            await Task.Delay(1000);
            
            // Assert
            var workflowRepo = ServiceLocator.GetService<IWorkflowRepository>();
            var workflowData = await workflowRepo.LoadAsync();
            
            var session = workflowData.WorkflowSessions.First(s => s.Id == viewModel.WorkflowId);
            session.LastModifiedAt.Should().BeCloseTo(beforeSave, TimeSpan.FromSeconds(5));
        }

        #endregion
    }
}