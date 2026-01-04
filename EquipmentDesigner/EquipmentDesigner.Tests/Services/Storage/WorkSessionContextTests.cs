using FluentAssertions;
using Xunit;
using EquipmentDesigner.Models.Storage;

namespace EquipmentDesigner.Tests.Services.Storage
{
    public class WorkSessionContextTests
    {
        [Fact]
        public void Constructor_WhenCreated_LastWorkflowTypeIsNull()
        {
            // Arrange & Act
            var context = new WorkSessionContext();

            // Assert
            context.LastWorkflowType.Should().BeNull();
        }

        [Fact]
        public void Constructor_WhenCreated_LastEditingComponentIdIsNull()
        {
            // Arrange & Act
            var context = new WorkSessionContext();

            // Assert
            context.LastEditingComponentId.Should().BeNull();
        }

        [Fact]
        public void Constructor_WhenCreated_LastEditingComponentTypeIsNull()
        {
            // Arrange & Act
            var context = new WorkSessionContext();

            // Assert
            context.LastEditingComponentType.Should().BeNull();
        }

        [Fact]
        public void Constructor_WhenCreated_CurrentWorkflowStepIsZero()
        {
            // Arrange & Act
            var context = new WorkSessionContext();

            // Assert
            context.CurrentWorkflowStep.Should().Be(0);
        }

        [Fact]
        public void Constructor_WhenCreated_IncompleteWorkflowsIsEmptyList()
        {
            // Arrange & Act
            var context = new WorkSessionContext();

            // Assert
            context.IncompleteWorkflows.Should().NotBeNull();
            context.IncompleteWorkflows.Should().BeEmpty();
        }
    }

    public class ComponentTypeEnumTests
    {
        [Fact]
        public void ComponentType_ContainsEquipmentValue()
        {
            // Assert
            ComponentType.Equipment.Should().BeDefined();
        }

        [Fact]
        public void ComponentType_ContainsSystemValue()
        {
            // Assert
            ComponentType.System.Should().BeDefined();
        }

        [Fact]
        public void ComponentType_ContainsUnitValue()
        {
            // Assert
            ComponentType.Unit.Should().BeDefined();
        }

        [Fact]
        public void ComponentType_ContainsDeviceValue()
        {
            // Assert
            ComponentType.Device.Should().BeDefined();
        }
    }

    public class IncompleteWorkflowInfoTests
    {
        [Fact]
        public void IncompleteWorkflowInfo_CanStoreComponentProgress()
        {
            // Arrange
            var info = new IncompleteWorkflowInfo
            {
                ComponentId = "eq-001",
                ComponentType = ComponentType.Equipment,
                State = EquipmentDesigner.Models.Dtos.ComponentState.Undefined,
                LastModifiedAt = new System.DateTime(2026, 1, 4, 10, 0, 0),
                CompletedFields = 3,
                TotalFields = 8
            };

            // Assert
            info.ComponentId.Should().Be("eq-001");
            info.ComponentType.Should().Be(ComponentType.Equipment);
            info.State.Should().Be(EquipmentDesigner.Models.Dtos.ComponentState.Undefined);
            info.CompletedFields.Should().Be(3);
            info.TotalFields.Should().Be(8);
        }
    }
}
