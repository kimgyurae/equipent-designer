using System.ComponentModel;
using EquipmentDesigner.Views.HardwareDefineWorkflow;
using FluentAssertions;
using Xunit;

namespace EquipmentDesigner.Tests.Views.HardwareDefineWorkflow
{
    public class WorkflowStepViewModelTests
    {
        #region Property Initialization

        [Fact]
        public void Constructor_WithStepNumberAndName_InitializesWithCorrectStepNumber()
        {
            // Arrange & Act
            var viewModel = new WorkflowStepViewModel(1, "Equipment");

            // Assert
            viewModel.StepNumber.Should().Be(1);
        }

        [Fact]
        public void Constructor_WithStepNumberAndName_InitializesWithCorrectStepName()
        {
            // Arrange & Act
            var viewModel = new WorkflowStepViewModel(2, "System");

            // Assert
            viewModel.StepName.Should().Be("System");
        }

        [Fact]
        public void Constructor_WithStepNumberAndName_IsActiveIsFalseByDefault()
        {
            // Arrange & Act
            var viewModel = new WorkflowStepViewModel(1, "Equipment");

            // Assert
            viewModel.IsActive.Should().BeFalse();
        }

        [Fact]
        public void Constructor_WithStepNumberAndName_IsCompletedIsFalseByDefault()
        {
            // Arrange & Act
            var viewModel = new WorkflowStepViewModel(1, "Equipment");

            // Assert
            viewModel.IsCompleted.Should().BeFalse();
        }

        #endregion

        #region Property Setting

        [Fact]
        public void IsActive_WhenSetToTrue_ReturnsTrue()
        {
            // Arrange
            var viewModel = new WorkflowStepViewModel(1, "Equipment");

            // Act
            viewModel.IsActive = true;

            // Assert
            viewModel.IsActive.Should().BeTrue();
        }

        [Fact]
        public void IsCompleted_WhenSetToTrue_ReturnsTrue()
        {
            // Arrange
            var viewModel = new WorkflowStepViewModel(1, "Equipment");

            // Act
            viewModel.IsCompleted = true;

            // Assert
            viewModel.IsCompleted.Should().BeTrue();
        }

        #endregion

        #region Property Change Notification

        [Fact]
        public void IsActive_WhenChanged_RaisesPropertyChanged()
        {
            // Arrange
            var viewModel = new WorkflowStepViewModel(1, "Equipment");
            var propertyChangedRaised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(WorkflowStepViewModel.IsActive))
                    propertyChangedRaised = true;
            };

            // Act
            viewModel.IsActive = true;

            // Assert
            propertyChangedRaised.Should().BeTrue();
        }

        [Fact]
        public void IsCompleted_WhenChanged_RaisesPropertyChanged()
        {
            // Arrange
            var viewModel = new WorkflowStepViewModel(1, "Equipment");
            var propertyChangedRaised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(WorkflowStepViewModel.IsCompleted))
                    propertyChangedRaised = true;
            };

            // Act
            viewModel.IsCompleted = true;

            // Assert
            propertyChangedRaised.Should().BeTrue();
        }

        #endregion
    }
}
