using System.Linq;
using EquipmentDesigner.Models;
using EquipmentDesigner.Views.HardwareDefineWorkflow;
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
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareLayer.Equipment);
            viewModel.StartType.Should().Be(HardwareLayer.Equipment);
        }

        [Fact]
        public void Constructor_WithSystemStartType_InitializesCorrectly()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareLayer.System);
            viewModel.StartType.Should().Be(HardwareLayer.System);
        }

        [Fact]
        public void Constructor_WithUnitStartType_InitializesCorrectly()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareLayer.Unit);
            viewModel.StartType.Should().Be(HardwareLayer.Unit);
        }

        [Fact]
        public void Constructor_WithDeviceStartType_InitializesCorrectly()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareLayer.Device);
            viewModel.StartType.Should().Be(HardwareLayer.Device);
        }

        #endregion

        #region Workflow Steps Based on Start Type

        [Fact]
        public void WorkflowSteps_WhenEquipmentStart_ReturnsEquipmentSystemUnitDevice()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareLayer.Equipment);
            var stepNames = viewModel.WorkflowSteps.Select(s => s.StepName).ToList();

            stepNames.Should().HaveCount(4);
            stepNames.Should().ContainInOrder("Equipment", "System", "Unit", "Device");
        }

        [Fact]
        public void WorkflowSteps_WhenSystemStart_ReturnsSystemUnitDevice()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareLayer.System);
            var stepNames = viewModel.WorkflowSteps.Select(s => s.StepName).ToList();

            stepNames.Should().HaveCount(3);
            stepNames.Should().ContainInOrder("System", "Unit", "Device");
        }

        [Fact]
        public void WorkflowSteps_WhenUnitStart_ReturnsUnitDevice()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareLayer.Unit);
            var stepNames = viewModel.WorkflowSteps.Select(s => s.StepName).ToList();

            stepNames.Should().HaveCount(2);
            stepNames.Should().ContainInOrder("Unit", "Device");
        }

        [Fact]
        public void WorkflowSteps_WhenDeviceStart_ReturnsDeviceOnly()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareLayer.Device);
            var stepNames = viewModel.WorkflowSteps.Select(s => s.StepName).ToList();

            stepNames.Should().HaveCount(1);
            stepNames.Should().Contain("Device");
        }

        #endregion

        #region Current Step Index

        [Fact]
        public void CurrentStepIndex_OnInitialization_IsZero()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareLayer.Equipment);
            viewModel.CurrentStepIndex.Should().Be(0);
        }

        [Fact]
        public void CurrentStep_OnInitialization_ReturnsFirstStep()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareLayer.Equipment);
            viewModel.CurrentStep.StepName.Should().Be("Equipment");
        }

        #endregion

        #region Navigation Commands

        [Fact]
        public void GoToNextStepCommand_WhenExecuted_AdvancesCurrentStepIndexByOne()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareLayer.Equipment);
            viewModel.EquipmentViewModel.Name = "TestEquipment"; // Make current step valid

            viewModel.GoToNextStepCommand.Execute(null);

            viewModel.CurrentStepIndex.Should().Be(1);
        }

        [Fact]
        public void GoToNextStepCommand_CanExecute_WhenAtLastStep_ReturnsFalse()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareLayer.Device);
            viewModel.DeviceViewModel.Name = "TestDevice";

            viewModel.GoToNextStepCommand.CanExecute(null).Should().BeFalse();
        }

        [Fact]
        public void GoToPreviousStepCommand_WhenExecuted_DecreasesCurrentStepIndexByOne()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareLayer.Equipment);
            viewModel.EquipmentViewModel.Name = "TestEquipment";
            viewModel.GoToNextStepCommand.Execute(null); // Move to step 1

            viewModel.GoToPreviousStepCommand.Execute(null);

            viewModel.CurrentStepIndex.Should().Be(0);
        }

        [Fact]
        public void GoToPreviousStepCommand_CanExecute_WhenAtFirstStep_ReturnsFalse()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareLayer.Equipment);

            viewModel.GoToPreviousStepCommand.CanExecute(null).Should().BeFalse();
        }

        [Fact]
        public void ExitToDashboardCommand_CanExecute_AlwaysReturnsTrue()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareLayer.Equipment);
            viewModel.ExitToDashboardCommand.CanExecute(null).Should().BeTrue();
        }

        #endregion

        #region Step Position Properties

        [Fact]
        public void IsFirstStep_WhenCurrentStepIndexIsZero_ReturnsTrue()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareLayer.Equipment);
            viewModel.IsFirstStep.Should().BeTrue();
        }

        [Fact]
        public void IsLastStep_WhenCurrentStepIndexEqualsLastStepIndex_ReturnsTrue()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareLayer.Device);
            viewModel.IsLastStep.Should().BeTrue();
        }

        [Fact]
        public void IsFirstStep_WhenNavigatedToSecondStep_ReturnsFalse()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareLayer.Equipment);
            viewModel.EquipmentViewModel.Name = "TestEquipment";
            viewModel.GoToNextStepCommand.Execute(null);

            viewModel.IsFirstStep.Should().BeFalse();
        }

        #endregion

        #region Property Change Notifications

        [Fact]
        public void CurrentStepIndex_WhenNavigating_RaisesPropertyChanged()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareLayer.Equipment);
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
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareLayer.Equipment);
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
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareLayer.Equipment);
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
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareLayer.Equipment);
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
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareLayer.Equipment);

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
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareLayer.Equipment);
            viewModel.WorkflowSteps[0].IsActive.Should().BeTrue();
            viewModel.WorkflowSteps[1].IsActive.Should().BeFalse();
        }

        [Fact]
        public void PreviousSteps_AreCompleted_WhenNavigatedPast()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareLayer.Equipment);
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
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareLayer.Equipment);
            viewModel.NavigateToStepCommand.Should().NotBeNull();
        }

        [Fact]
        public void NavigateToStepCommand_CanExecute_WithNullParameter_ReturnsFalse()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareLayer.Equipment);
            viewModel.NavigateToStepCommand.CanExecute(null).Should().BeFalse();
        }

        #endregion

        #region Navigation to Completed Steps

        [Fact]
        public void NavigateToStep_WhenStepIsCompleted_ChangesCurrentStepIndexToTargetStep()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareLayer.Equipment);
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
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareLayer.Equipment);
            viewModel.EquipmentViewModel.Name = "TestEquipment";
            viewModel.GoToNextStepCommand.Execute(null); // Move to step 1

            viewModel.NavigateToStepCommand.Execute(viewModel.WorkflowSteps[0]); // Navigate back to Equipment

            viewModel.WorkflowSteps[0].IsActive.Should().BeTrue();
            viewModel.WorkflowSteps[1].IsActive.Should().BeFalse();
        }

        [Fact]
        public void NavigateToStep_WhenStepIsCompleted_PreservesCompletedStatesForPreviousSteps()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareLayer.Equipment);
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
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareLayer.Equipment);
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
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareLayer.Equipment);
            var initialIndex = viewModel.CurrentStepIndex;

            viewModel.NavigateToStepCommand.Execute(viewModel.WorkflowSteps[0]); // Navigate to current step

            viewModel.CurrentStepIndex.Should().Be(initialIndex);
        }

        [Fact]
        public void NavigateToStep_WhenStepIsActive_NoPropertyChangedEventFired()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareLayer.Equipment);
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
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareLayer.Equipment);
            viewModel.EquipmentViewModel.Name = "TestEquipment";
            viewModel.GoToNextStepCommand.Execute(null); // Move to step 1

            viewModel.WorkflowSteps[0].CanNavigateTo.Should().BeTrue();
        }

        [Fact]
        public void CanNavigateTo_WhenStepIsActive_ReturnsTrue()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareLayer.Equipment);

            viewModel.WorkflowSteps[0].CanNavigateTo.Should().BeTrue();
        }

        [Fact]
        public void CanNavigateTo_WhenStepBecomesCompleted_UpdatesToTrue()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareLayer.Equipment);
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
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareLayer.Equipment);
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
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareLayer.Equipment);
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
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareLayer.Equipment);
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
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareLayer.Equipment);
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
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareLayer.Equipment);
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
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareLayer.Device);

            // Only one step, already active - CanExecute should be true but navigation does nothing
            viewModel.NavigateToStepCommand.CanExecute(viewModel.WorkflowSteps[0]).Should().BeTrue();
            viewModel.CurrentStepIndex.Should().Be(0);
        }

        [Fact]
        public void NavigateToStep_WhenAtLastStep_CanNavigateBackToAnyCompletedStep()
        {
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareLayer.Equipment);
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
            var viewModel = new HardwareDefineWorkflowViewModel(HardwareLayer.Equipment);
            var externalStep = new WorkflowStepViewModel(99, "External");
            var initialIndex = viewModel.CurrentStepIndex;

            viewModel.NavigateToStepCommand.Execute(externalStep);

            viewModel.CurrentStepIndex.Should().Be(initialIndex);
        }

        #endregion
    }
}