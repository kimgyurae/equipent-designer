using EquipmentDesigner.Models;
using EquipmentDesigner.ViewModels;
using FluentAssertions;
using Xunit;

namespace EquipmentDesigner.Tests.Views.Dashboard
{
    /// <summary>
    /// TDD Tests for ComponentItem HardwareKey property
    /// </summary>
    public class ComponentItemHardwareKeyTests
    {
        [Fact]
        public void ComponentItem_ShouldHaveHardwareKeyProperty()
        {
            // Arrange & Act
            var item = new ComponentItem();

            // Assert
            item.HardwareKey.Should().BeNull("HardwareKey should default to null");
        }

        [Fact]
        public void ComponentItem_HardwareKeyCanBeSet()
        {
            // Arrange
            var item = new ComponentItem();

            // Act
            item.HardwareKey = "auto-assembler";

            // Assert
            item.HardwareKey.Should().Be("auto-assembler");
        }

        [Fact]
        public void ComponentItem_ShouldStoreHardwareKeyFromDto()
        {
            // Arrange & Act
            var item = new ComponentItem
            {
                Id = "wf-123",
                Name = "Auto Assembler",
                HardwareKey = "auto-assembler-key",
                HardwareType = HardwareType.Equipment,
                Version = "v2.0.0"
            };

            // Assert
            item.HardwareKey.Should().Be("auto-assembler-key");
            item.Name.Should().Be("Auto Assembler");
        }
    }
}
