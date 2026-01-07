using System;
using System.Globalization;
using EquipmentDesigner.Models.Dtos;
using EquipmentDesigner.Views.Dashboard;
using FluentAssertions;
using Xunit;

namespace EquipmentDesigner.Tests.Converters
{
    /// <summary>
    /// Tests for ComponentState converters and WorkflowItem model.
    /// Bug: ComponentStateToBackgroundConverter always returns default brush because
    /// WorkflowItem.ComponentState is string type but converter expects ComponentState enum.
    /// </summary>
    public class ComponentStateConverterTests
    {
        #region WorkflowItem Model Tests

        [Fact]
        public void WorkflowItem_ComponentState_ShouldBeOfTypeComponentStateEnum()
        {
            // Arrange & Act
            var workflowItem = new WorkflowItem();
            var propertyInfo = typeof(WorkflowItem).GetProperty(nameof(WorkflowItem.ComponentState));

            // Assert - Property type should be ComponentState enum, not string
            propertyInfo.Should().NotBeNull();
            propertyInfo.PropertyType.Should().Be(typeof(ComponentState),
                "WorkflowItem.ComponentState should be ComponentState enum type to work with converters");
        }

        [Fact]
        public void WorkflowItem_CanBeInstantiatedWith_DraftState()
        {
            // Arrange & Act
            var workflowItem = new WorkflowItem
            {
                ComponentState = ComponentState.Draft
            };

            // Assert
            workflowItem.ComponentState.Should().Be(ComponentState.Draft);
        }

        [Fact]
        public void WorkflowItem_CanBeInstantiatedWith_ReadyState()
        {
            // Arrange & Act
            var workflowItem = new WorkflowItem
            {
                ComponentState = ComponentState.Ready
            };

            // Assert
            workflowItem.ComponentState.Should().Be(ComponentState.Ready);
        }

        [Fact]
        public void WorkflowItem_CanBeInstantiatedWith_UploadedState()
        {
            // Arrange & Act
            var workflowItem = new WorkflowItem
            {
                ComponentState = ComponentState.Uploaded
            };

            // Assert
            workflowItem.ComponentState.Should().Be(ComponentState.Uploaded);
        }

        [Fact]
        public void WorkflowItem_CanBeInstantiatedWith_ValidatedState()
        {
            // Arrange & Act
            var workflowItem = new WorkflowItem
            {
                ComponentState = ComponentState.Validated
            };

            // Assert
            workflowItem.ComponentState.Should().Be(ComponentState.Validated);
        }

        #endregion

        #region WorkflowItem Display Text Tests

        [Fact]
        public void WorkflowItem_ComponentStateDisplayText_ShouldReturnDraftForDraftState()
        {
            // Arrange
            var workflowItem = new WorkflowItem
            {
                ComponentState = ComponentState.Draft
            };

            // Act
            var displayText = workflowItem.ComponentStateDisplayText;

            // Assert
            displayText.Should().Be("Draft");
        }

        [Fact]
        public void WorkflowItem_ComponentStateDisplayText_ShouldReturnReadyForReadyState()
        {
            // Arrange
            var workflowItem = new WorkflowItem
            {
                ComponentState = ComponentState.Ready
            };

            // Act
            var displayText = workflowItem.ComponentStateDisplayText;

            // Assert
            displayText.Should().Be("Ready");
        }

        [Fact]
        public void WorkflowItem_ComponentStateDisplayText_ShouldReturnUploadedForUploadedState()
        {
            // Arrange
            var workflowItem = new WorkflowItem
            {
                ComponentState = ComponentState.Uploaded
            };

            // Act
            var displayText = workflowItem.ComponentStateDisplayText;

            // Assert
            displayText.Should().Be("Uploaded");
        }

        [Fact]
        public void WorkflowItem_ComponentStateDisplayText_ShouldReturnValidatedForValidatedState()
        {
            // Arrange
            var workflowItem = new WorkflowItem
            {
                ComponentState = ComponentState.Validated
            };

            // Act
            var displayText = workflowItem.ComponentStateDisplayText;

            // Assert
            displayText.Should().Be("Validated");
        }

        #endregion
    }
}
