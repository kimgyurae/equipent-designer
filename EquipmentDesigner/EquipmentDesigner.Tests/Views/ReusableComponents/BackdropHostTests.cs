using System.ComponentModel;
using EquipmentDesigner.Controls;
using FluentAssertions;
using Xunit;

namespace EquipmentDesigner.Tests.Views.ReusableComponents
{
    public class BackdropHostTests
    {
        #region IBackdropHost Interface Tests

        [Fact]
        public void IBackdropHost_HasIsBackdropVisibleProperty_WithGetterAndSetter()
        {
            // Arrange & Act
            var hasProperty = typeof(IBackdropHost).GetProperty(nameof(IBackdropHost.IsBackdropVisible));

            // Assert
            hasProperty.Should().NotBeNull();
            hasProperty.CanRead.Should().BeTrue();
            hasProperty.CanWrite.Should().BeTrue();
            hasProperty.PropertyType.Should().Be(typeof(bool));
        }

        [Fact]
        public void IBackdropHost_HasShowBackdropMethod_ReturnsVoid()
        {
            // Arrange & Act
            var method = typeof(IBackdropHost).GetMethod(nameof(IBackdropHost.ShowBackdrop));

            // Assert
            method.Should().NotBeNull();
            method.ReturnType.Should().Be(typeof(void));
            method.GetParameters().Should().BeEmpty();
        }

        [Fact]
        public void IBackdropHost_HasHideBackdropMethod_ReturnsVoid()
        {
            // Arrange & Act
            var method = typeof(IBackdropHost).GetMethod(nameof(IBackdropHost.HideBackdrop));

            // Assert
            method.Should().NotBeNull();
            method.ReturnType.Should().Be(typeof(void));
            method.GetParameters().Should().BeEmpty();
        }

        [Fact]
        public void IBackdropHost_InheritsFromINotifyPropertyChanged()
        {
            // Assert
            typeof(IBackdropHost).Should().Implement<INotifyPropertyChanged>();
        }

        #endregion

        #region BackdropHostMixin Initialization Tests

        [Fact]
        public void Constructor_WithRaisePropertyChangedCallback_CreatesInstance()
        {
            // Arrange & Act
            var mixin = new BackdropHostMixin(propertyName => { });

            // Assert
            mixin.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_WithNullCallback_CreatesInstanceWithoutError()
        {
            // Arrange & Act
            var mixin = new BackdropHostMixin(null);

            // Assert
            mixin.Should().NotBeNull();
        }

        [Fact]
        public void IsBackdropVisible_DefaultValue_IsFalse()
        {
            // Arrange
            var mixin = new BackdropHostMixin(propertyName => { });

            // Assert
            mixin.IsBackdropVisible.Should().BeFalse();
        }

        #endregion

        #region BackdropHostMixin ShowBackdrop Tests

        [Fact]
        public void ShowBackdrop_WhenCalled_SetsIsBackdropVisibleToTrue()
        {
            // Arrange
            var mixin = new BackdropHostMixin(propertyName => { });

            // Act
            mixin.ShowBackdrop();

            // Assert
            mixin.IsBackdropVisible.Should().BeTrue();
        }

        [Fact]
        public void ShowBackdrop_WhenCalledMultipleTimes_KeepsIsBackdropVisibleTrue()
        {
            // Arrange
            var mixin = new BackdropHostMixin(propertyName => { });

            // Act
            mixin.ShowBackdrop();
            mixin.ShowBackdrop();
            mixin.ShowBackdrop();

            // Assert
            mixin.IsBackdropVisible.Should().BeTrue();
        }

        [Fact]
        public void ShowBackdrop_WhenIsBackdropVisibleAlreadyTrue_DoesNotRaisePropertyChanged()
        {
            // Arrange
            var callCount = 0;
            var mixin = new BackdropHostMixin(propertyName => callCount++);
            mixin.ShowBackdrop(); // First call - should raise
            callCount = 0; // Reset counter

            // Act
            mixin.ShowBackdrop(); // Second call - should not raise

            // Assert
            callCount.Should().Be(0);
        }

        #endregion

        #region BackdropHostMixin HideBackdrop Tests

        [Fact]
        public void HideBackdrop_WhenCalled_SetsIsBackdropVisibleToFalse()
        {
            // Arrange
            var mixin = new BackdropHostMixin(propertyName => { });
            mixin.ShowBackdrop(); // Set to true first

            // Act
            mixin.HideBackdrop();

            // Assert
            mixin.IsBackdropVisible.Should().BeFalse();
        }

        [Fact]
        public void HideBackdrop_WhenCalledMultipleTimes_KeepsIsBackdropVisibleFalse()
        {
            // Arrange
            var mixin = new BackdropHostMixin(propertyName => { });
            mixin.ShowBackdrop(); // Set to true first

            // Act
            mixin.HideBackdrop();
            mixin.HideBackdrop();
            mixin.HideBackdrop();

            // Assert
            mixin.IsBackdropVisible.Should().BeFalse();
        }

        [Fact]
        public void HideBackdrop_WhenIsBackdropVisibleAlreadyFalse_DoesNotRaisePropertyChanged()
        {
            // Arrange
            var callCount = 0;
            var mixin = new BackdropHostMixin(propertyName => callCount++);

            // Act
            mixin.HideBackdrop(); // Already false - should not raise

            // Assert
            callCount.Should().Be(0);
        }

        #endregion

        #region BackdropHostMixin PropertyChanged Tests

        [Fact]
        public void IsBackdropVisible_WhenChangedFromFalseToTrue_InvokesCallback()
        {
            // Arrange
            string raisedPropertyName = null;
            var mixin = new BackdropHostMixin(propertyName => raisedPropertyName = propertyName);

            // Act
            mixin.IsBackdropVisible = true;

            // Assert
            raisedPropertyName.Should().Be(nameof(BackdropHostMixin.IsBackdropVisible));
        }

        [Fact]
        public void IsBackdropVisible_WhenChangedFromTrueToFalse_InvokesCallback()
        {
            // Arrange
            string raisedPropertyName = null;
            var mixin = new BackdropHostMixin(propertyName => raisedPropertyName = propertyName);
            mixin.IsBackdropVisible = true;
            raisedPropertyName = null; // Reset

            // Act
            mixin.IsBackdropVisible = false;

            // Assert
            raisedPropertyName.Should().Be(nameof(BackdropHostMixin.IsBackdropVisible));
        }

        [Fact]
        public void IsBackdropVisible_WhenSetToSameValue_DoesNotInvokeCallback()
        {
            // Arrange
            var callCount = 0;
            var mixin = new BackdropHostMixin(propertyName => callCount++);

            // Act
            mixin.IsBackdropVisible = false; // Same as default

            // Assert
            callCount.Should().Be(0);
        }

        #endregion
    }
}
