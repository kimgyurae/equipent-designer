using EquipmentDesigner.Services;
using FluentAssertions;
using Xunit;

namespace EquipmentDesigner.Tests.Services
{
    public class NavigationServiceTests
    {
        #region NavigateToCreateNewComponent Tests

        [Fact]
        public void NavigateToCreateNewComponent_RaisesNavigationRequestedEvent()
        {
            // Arrange
            NavigationTarget capturedTarget = null;
            NavigationService.Instance.NavigationRequested += target => capturedTarget = target;

            // Act
            NavigationService.Instance.NavigateToCreateNewComponent();

            // Assert
            capturedTarget.Should().NotBeNull();
        }

        [Fact]
        public void NavigateToCreateNewComponent_SetsTargetTypeToCreateNewComponent()
        {
            // Arrange
            NavigationTarget capturedTarget = null;
            NavigationService.Instance.NavigationRequested += target => capturedTarget = target;

            // Act
            NavigationService.Instance.NavigateToCreateNewComponent();

            // Assert
            capturedTarget.TargetType.Should().Be(NavigationTargetType.CreateNewComponent);
        }

        #endregion

        #region NavigateToComponentList Tests

        [Fact]
        public void NavigateToComponentList_RaisesNavigationRequestedEvent()
        {
            // Arrange
            NavigationTarget capturedTarget = null;
            NavigationService.Instance.NavigationRequested += target => capturedTarget = target;

            // Act
            NavigationService.Instance.NavigateToComponentList();

            // Assert
            capturedTarget.Should().NotBeNull();
        }

        [Fact]
        public void NavigateToComponentList_SetsTargetTypeToComponentList()
        {
            // Arrange
            NavigationTarget capturedTarget = null;
            NavigationService.Instance.NavigationRequested += target => capturedTarget = target;

            // Act
            NavigationService.Instance.NavigateToComponentList();

            // Assert
            capturedTarget.TargetType.Should().Be(NavigationTargetType.ComponentList);
        }

        #endregion
    }
}
