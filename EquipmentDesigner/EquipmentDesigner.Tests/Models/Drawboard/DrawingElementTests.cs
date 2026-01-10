using FluentAssertions;
using Xunit;

namespace EquipmentDesigner.Tests.Models.Drawboard
{
    public class DrawingElementTests
    {
        #region Position Tests

        [Fact]
        public void DrawingElement_StoresXCoordinate_AsDoubleValue()
        {
            // Arrange & Act
            var element = new TestDrawingElement { X = 100.5 };

            // Assert
            element.X.Should().Be(100.5);
        }

        [Fact]
        public void DrawingElement_StoresYCoordinate_AsDoubleValue()
        {
            // Arrange & Act
            var element = new TestDrawingElement { Y = 200.75 };

            // Assert
            element.Y.Should().Be(200.75);
        }

        #endregion

        #region Size Tests

        [Fact]
        public void DrawingElement_StoresWidth_AsDoubleValue()
        {
            // Arrange & Act
            var element = new TestDrawingElement { Width = 150.25 };

            // Assert
            element.Width.Should().Be(150.25);
        }

        [Fact]
        public void DrawingElement_StoresHeight_AsDoubleValue()
        {
            // Arrange & Act
            var element = new TestDrawingElement { Height = 80.5 };

            // Assert
            element.Height.Should().Be(80.5);
        }

        [Fact]
        public void DrawingElement_ClampsNegativeWidth_ToMinimumOf1()
        {
            // Arrange & Act
            var element = new TestDrawingElement { Width = -50 };

            // Assert
            element.Width.Should().Be(1.0);
        }

        [Fact]
        public void DrawingElement_ClampsNegativeHeight_ToMinimumOf1()
        {
            // Arrange & Act
            var element = new TestDrawingElement { Height = -30 };

            // Assert
            element.Height.Should().Be(1.0);
        }

        #endregion

        #region Id Tests

        [Fact]
        public void DrawingElement_GeneratesUniqueId_OnCreation()
        {
            // Arrange & Act
            var element = new TestDrawingElement();

            // Assert
            element.Id.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void DrawingElement_GeneratesDifferentIds_ForDifferentInstances()
        {
            // Arrange & Act
            var element1 = new TestDrawingElement();
            var element2 = new TestDrawingElement();

            // Assert
            element1.Id.Should().NotBe(element2.Id);
        }

        #endregion

        #region ZIndex Tests

        [Fact]
        public void DrawingElement_StoresZIndex_AsInteger()
        {
            // Arrange & Act
            var element = new TestDrawingElement { ZIndex = 5 };

            // Assert
            element.ZIndex.Should().Be(5);
        }

        #endregion

        #region Selection State Tests

        [Fact]
        public void DrawingElement_IsSelected_DefaultsToFalse()
        {
            // Arrange & Act
            var element = new TestDrawingElement();

            // Assert
            element.IsSelected.Should().BeFalse();
        }

        [Fact]
        public void DrawingElement_StoresIsSelected_AsBooleanValue()
        {
            // Arrange & Act
            var element = new TestDrawingElement { IsSelected = true };

            // Assert
            element.IsSelected.Should().BeTrue();
        }

        #endregion

        #region Lock State Tests

        [Fact]
        public void DrawingElement_IsLocked_DefaultsToFalse()
        {
            // Arrange & Act
            var element = new TestDrawingElement();

            // Assert
            element.IsLocked.Should().BeFalse();
        }

        [Fact]
        public void DrawingElement_StoresIsLocked_AsBooleanValue()
        {
            // Arrange & Act
            var element = new TestDrawingElement { IsLocked = true };

            // Assert
            element.IsLocked.Should().BeTrue();
        }

        #endregion

        #region Clone Tests

        [Fact]
        public void DrawingElement_Clone_CreatesDeepCopyWithNewId()
        {
            // Arrange
            var original = new TestDrawingElement
            {
                X = 100,
                Y = 200,
                Width = 150,
                Height = 80,
                ZIndex = 3,
                IsSelected = true,
                IsLocked = true,
            };

            // Act
            var clone = original.Clone() as TestDrawingElement;

            // Assert
            clone.Should().NotBeNull();
            clone.Id.Should().NotBe(original.Id);
            clone.X.Should().Be(original.X);
            clone.Y.Should().Be(original.Y);
            clone.Width.Should().Be(original.Width);
            clone.Height.Should().Be(original.Height);
            clone.ZIndex.Should().Be(original.ZIndex);
            clone.IsSelected.Should().Be(original.IsSelected);
            clone.IsLocked.Should().Be(original.IsLocked);
        }

        #endregion
    }

    /// <summary>
    /// Concrete implementation for testing abstract DrawingElement
    /// </summary>
    internal class TestDrawingElement : EquipmentDesigner.Models.DrawingElement
    {
        public override EquipmentDesigner.Models.DrawingShapeType ShapeType
            => EquipmentDesigner.Models.DrawingShapeType.Action;
    }
}
