using System;
using System.Collections.Generic;
using EquipmentDesigner.Models;
using FluentAssertions;
using Xunit;

namespace EquipmentDesigner.Tests.Models.Dtos
{
    /// <summary>
    /// TDD Tests for Hardware Version Selection feature - DTO layer
    /// </summary>
    public class HardwareVersionDtoTests
    {
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
        public void HardwareVersionHistoryDto_ShouldHaveHardwareTypeProperty()
        {
            // Arrange & Act
            var dto = new HardwareVersionHistoryDto { HardwareType = HardwareType.Equipment };

            // Assert
            dto.HardwareType.Should().Be(HardwareType.Equipment);
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
