using System;
using System.Collections.Generic;
using EquipmentDesigner.Models;
using FluentAssertions;
using Xunit;

namespace EquipmentDesigner.Tests.Models.Dtos
{
    /// <summary>
    /// TDD Tests for Hardware Version Selection feature - DTO layer
    /// RED Phase: These tests should fail initially until implementation is complete
    /// </summary>
    public class HardwareVersionDtoTests
    {
        #region DTO HardwareKey Tests

        [Fact]
        public void EquipmentDto_ShouldHaveHardwareKeyProperty()
        {
            // Arrange & Act
            var dto = new EquipmentDto();

            // Assert
            dto.HardwareKey.Should().BeNull("HardwareKey should default to null");
        }

        [Fact]
        public void EquipmentDto_HardwareKeyCanBeSet()
        {
            // Arrange
            var dto = new EquipmentDto();

            // Act
            dto.HardwareKey = "auto-assembler";

            // Assert
            dto.HardwareKey.Should().Be("auto-assembler");
        }

        [Fact]
        public void SystemDto_ShouldHaveHardwareKeyProperty()
        {
            // Arrange & Act
            var dto = new SystemDto();

            // Assert
            dto.HardwareKey.Should().BeNull("HardwareKey should default to null");
        }

        [Fact]
        public void SystemDto_HardwareKeyCanBeSet()
        {
            // Arrange
            var dto = new SystemDto();

            // Act
            dto.HardwareKey = "conveyor-system";

            // Assert
            dto.HardwareKey.Should().Be("conveyor-system");
        }

        [Fact]
        public void UnitDto_ShouldHaveHardwareKeyProperty()
        {
            // Arrange & Act
            var dto = new UnitDto();

            // Assert
            dto.HardwareKey.Should().BeNull("HardwareKey should default to null");
        }

        [Fact]
        public void UnitDto_HardwareKeyCanBeSet()
        {
            // Arrange
            var dto = new UnitDto();

            // Act
            dto.HardwareKey = "motor-unit";

            // Assert
            dto.HardwareKey.Should().Be("motor-unit");
        }

        [Fact]
        public void DeviceDto_ShouldHaveHardwareKeyProperty()
        {
            // Arrange & Act
            var dto = new DeviceDto();

            // Assert
            dto.HardwareKey.Should().BeNull("HardwareKey should default to null");
        }

        [Fact]
        public void DeviceDto_HardwareKeyCanBeSet()
        {
            // Arrange
            var dto = new DeviceDto();

            // Act
            dto.HardwareKey = "sensor-device";

            // Assert
            dto.HardwareKey.Should().Be("sensor-device");
        }

        #endregion

        #region HardwareVersionSummaryDto Tests

        [Fact]
        public void HardwareVersionSummaryDto_ShouldHaveWorkflowIdProperty()
        {
            // Arrange & Act
            var dto = new HardwareVersionSummaryDto { WorkflowId = "wf-123" };

            // Assert
            dto.WorkflowId.Should().Be("wf-123");
        }

        [Fact]
        public void HardwareVersionSummaryDto_ShouldHaveVersionProperty()
        {
            // Arrange & Act
            var dto = new HardwareVersionSummaryDto { Version = "v2.7.1" };

            // Assert
            dto.Version.Should().Be("v2.7.1");
        }

        [Fact]
        public void HardwareVersionSummaryDto_ShouldHaveStateProperty()
        {
            // Arrange & Act
            var dto = new HardwareVersionSummaryDto { State = ComponentState.Validated };

            // Assert
            dto.State.Should().Be(ComponentState.Validated);
        }

        [Fact]
        public void HardwareVersionSummaryDto_ShouldHaveLastModifiedAtProperty()
        {
            // Arrange
            var now = DateTime.Now;

            // Act
            var dto = new HardwareVersionSummaryDto { LastModifiedAt = now };

            // Assert
            dto.LastModifiedAt.Should().Be(now);
        }

        [Fact]
        public void HardwareVersionSummaryDto_IsLatestShouldDefaultToFalse()
        {
            // Arrange & Act
            var dto = new HardwareVersionSummaryDto();

            // Assert
            dto.IsLatest.Should().BeFalse();
        }

        [Fact]
        public void HardwareVersionSummaryDto_ShouldHaveDescriptionProperty()
        {
            // Arrange & Act
            var dto = new HardwareVersionSummaryDto { Description = "Test description" };

            // Assert
            dto.Description.Should().Be("Test description");
        }

        #endregion

        #region HardwareVersionHistoryDto Tests

        [Fact]
        public void HardwareVersionHistoryDto_ShouldHaveHardwareKeyProperty()
        {
            // Arrange & Act
            var dto = new HardwareVersionHistoryDto { HardwareKey = "auto-assembler" };

            // Assert
            dto.HardwareKey.Should().Be("auto-assembler");
        }

        [Fact]
        public void HardwareVersionHistoryDto_ShouldHaveHardwareLayerProperty()
        {
            // Arrange & Act
            var dto = new HardwareVersionHistoryDto { HardwareLayer = HardwareLayer.Equipment };

            // Assert
            dto.HardwareLayer.Should().Be(HardwareLayer.Equipment);
        }

        [Fact]
        public void HardwareVersionHistoryDto_ShouldHaveDisplayNameProperty()
        {
            // Arrange & Act
            var dto = new HardwareVersionHistoryDto { DisplayName = "Auto Assembler" };

            // Assert
            dto.DisplayName.Should().Be("Auto Assembler");
        }

        [Fact]
        public void HardwareVersionHistoryDto_ShouldHaveTotalVersionCountProperty()
        {
            // Arrange & Act
            var dto = new HardwareVersionHistoryDto { TotalVersionCount = 5 };

            // Assert
            dto.TotalVersionCount.Should().Be(5);
        }

        [Fact]
        public void HardwareVersionHistoryDto_VersionsShouldInitializeAsEmptyList()
        {
            // Arrange & Act
            var dto = new HardwareVersionHistoryDto();

            // Assert
            dto.Versions.Should().NotBeNull();
            dto.Versions.Should().BeEmpty();
        }

        [Fact]
        public void HardwareVersionHistoryDto_ShouldHaveVersionsAsList()
        {
            // Arrange
            var versions = new List<HardwareVersionSummaryDto>
            {
                new HardwareVersionSummaryDto { Version = "v2.0.0", IsLatest = true },
                new HardwareVersionSummaryDto { Version = "1.0.0", IsLatest = false }
            };

            // Act
            var dto = new HardwareVersionHistoryDto { Versions = versions };

            // Assert
            dto.Versions.Should().HaveCount(2);
            dto.Versions[0].Version.Should().Be("v2.0.0");
            dto.Versions[0].IsLatest.Should().BeTrue();
        }

        #endregion
    }
}
