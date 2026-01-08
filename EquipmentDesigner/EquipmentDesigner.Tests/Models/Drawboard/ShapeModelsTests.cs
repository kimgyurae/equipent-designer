using EquipmentDesigner.Models.Drawboard;
using FluentAssertions;
using Xunit;

namespace EquipmentDesigner.Tests.Models.Drawboard
{
    public class ShapeModelsTests
    {
        #region InitialShape Tests

        [Fact]
        public void InitialShape_ShapeType_ReturnsInitial()
        {
            // Arrange & Act
            var shape = new InitialShape();

            // Assert
            shape.ShapeType.Should().Be(DrawingShapeType.Initial);
        }

        [Fact]
        public void InitialShape_DefaultBackgroundColor_IsGray200()
        {
            // Arrange & Act
            var shape = new InitialShape();

            // Assert
            shape.BackgroundColor.Should().Be("#E2E8F0");
        }

        #endregion

        #region ActionShape Tests

        [Fact]
        public void ActionShape_ShapeType_ReturnsAction()
        {
            // Arrange & Act
            var shape = new ActionShape();

            // Assert
            shape.ShapeType.Should().Be(DrawingShapeType.Action);
        }

        [Fact]
        public void ActionShape_DefaultBackgroundColor_IsGray200()
        {
            // Arrange & Act
            var shape = new ActionShape();

            // Assert
            shape.BackgroundColor.Should().Be("#E2E8F0");
        }

        #endregion

        #region DecisionShape Tests

        [Fact]
        public void DecisionShape_ShapeType_ReturnsDecision()
        {
            // Arrange & Act
            var shape = new DecisionShape();

            // Assert
            shape.ShapeType.Should().Be(DrawingShapeType.Decision);
        }

        [Fact]
        public void DecisionShape_DefaultBackgroundColor_IsGray200()
        {
            // Arrange & Act
            var shape = new DecisionShape();

            // Assert
            shape.BackgroundColor.Should().Be("#E2E8F0");
        }

        #endregion

        #region TerminalShape Tests

        [Fact]
        public void TerminalShape_ShapeType_ReturnsTerminal()
        {
            // Arrange & Act
            var shape = new TerminalShape();

            // Assert
            shape.ShapeType.Should().Be(DrawingShapeType.Terminal);
        }

        [Fact]
        public void TerminalShape_DefaultBackgroundColor_IsGray200()
        {
            // Arrange & Act
            var shape = new TerminalShape();

            // Assert
            shape.BackgroundColor.Should().Be("#E2E8F0");
        }

        #endregion

        #region PredefinedActionShape Tests

        [Fact]
        public void PredefinedActionShape_ShapeType_ReturnsPredefinedAction()
        {
            // Arrange & Act
            var shape = new PredefinedActionShape();

            // Assert
            shape.ShapeType.Should().Be(DrawingShapeType.PredefinedAction);
        }

        [Fact]
        public void PredefinedActionShape_DefaultBackgroundColor_IsGray200()
        {
            // Arrange & Act
            var shape = new PredefinedActionShape();

            // Assert
            shape.BackgroundColor.Should().Be("#E2E8F0");
        }

        #endregion

        #region TextboxShape Tests

        [Fact]
        public void TextboxShape_ShapeType_ReturnsTextbox()
        {
            // Arrange & Act
            var shape = new TextboxShape();

            // Assert
            shape.ShapeType.Should().Be(DrawingShapeType.Textbox);
        }

        [Fact]
        public void TextboxShape_DefaultBackgroundColor_IsTransparent()
        {
            // Arrange & Act
            var shape = new TextboxShape();

            // Assert
            shape.BackgroundColor.Should().Be("Transparent");
        }

        [Fact]
        public void TextboxShape_StoresTextProperty()
        {
            // Arrange & Act
            var shape = new TextboxShape { Text = "Hello World" };

            // Assert
            shape.Text.Should().Be("Hello World");
        }

        #endregion
    }
}
