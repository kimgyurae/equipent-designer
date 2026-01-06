using FluentAssertions;
using Xunit;
using EquipmentDesigner.Models.Storage;
using EquipmentDesigner.Models;

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
        public void Constructor_WhenCreated_LastEditingHardwareLayerIsNull()
        {
            // Arrange & Act
            var context = new WorkSessionContext();

            // Assert
            context.LastEditingHardwareLayer.Should().BeNull();
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

    public class HardwareLayerEnumTests
    {
        [Fact]
        public void HardwareLayer_ContainsEquipmentValue()
        {
            // Assert
            HardwareLayer.Equipment.Should().BeDefined();
        }

        [Fact]
        public void HardwareLayer_ContainsSystemValue()
        {
            // Assert
            HardwareLayer.System.Should().BeDefined();
        }

        [Fact]
        public void HardwareLayer_ContainsUnitValue()
        {
            // Assert
            HardwareLayer.Unit.Should().BeDefined();
        }

        [Fact]
        public void HardwareLayer_ContainsDeviceValue()
        {
            // Assert
            HardwareLayer.Device.Should().BeDefined();
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
                HardwareLayer = HardwareLayer.Equipment,
                State = EquipmentDesigner.Models.Dtos.ComponentState.Undefined,
                LastModifiedAt = new System.DateTime(2026, 1, 4, 10, 0, 0),
                CompletedFields = 3,
                TotalFields = 8
            };

            // Assert
            info.ComponentId.Should().Be("eq-001");
            info.HardwareLayer.Should().Be(HardwareLayer.Equipment);
            info.State.Should().Be(EquipmentDesigner.Models.Dtos.ComponentState.Undefined);
            info.CompletedFields.Should().Be(3);
            info.TotalFields.Should().Be(8);
        }
    }
}
