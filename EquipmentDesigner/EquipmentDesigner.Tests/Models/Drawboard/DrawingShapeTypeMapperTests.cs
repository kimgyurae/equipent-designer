using EquipmentDesigner.Models;
using FluentAssertions;
using Xunit;

namespace EquipmentDesigner.Tests.Models.Drawboard
{
    public class DrawingShapeTypeMapperTests
    {
        [Fact]
        public void ToProcessNodeType_Initial_ReturnsProcessNodeTypeInitial()
        {
            // Arrange & Act
            var result = DrawingShapeTypeMapper.ToProcessNodeType(DrawingShapeType.Initial);

            // Assert
            result.Should().Be(UMLNodeType.Initial);
        }

        [Fact]
        public void ToProcessNodeType_Action_ReturnsProcessNodeTypeAction()
        {
            // Arrange & Act
            var result = DrawingShapeTypeMapper.ToProcessNodeType(DrawingShapeType.Action);

            // Assert
            result.Should().Be(UMLNodeType.Action);
        }

        [Fact]
        public void ToProcessNodeType_Decision_ReturnsProcessNodeTypeDecision()
        {
            // Arrange & Act
            var result = DrawingShapeTypeMapper.ToProcessNodeType(DrawingShapeType.Decision);

            // Assert
            result.Should().Be(UMLNodeType.Decision);
        }

        [Fact]
        public void ToProcessNodeType_Terminal_ReturnsProcessNodeTypeTerminal()
        {
            // Arrange & Act
            var result = DrawingShapeTypeMapper.ToProcessNodeType(DrawingShapeType.Terminal);

            // Assert
            result.Should().Be(UMLNodeType.Terminal);
        }

        [Fact]
        public void ToProcessNodeType_PredefinedAction_ReturnsProcessNodeTypePredefinedProcess()
        {
            // Arrange & Act
            var result = DrawingShapeTypeMapper.ToProcessNodeType(DrawingShapeType.PredefinedAction);

            // Assert
            result.Should().Be(UMLNodeType.PredefinedAction);
        }

        [Fact]
        public void ToProcessNodeType_Textbox_ReturnsNull()
        {
            // Arrange & Act
            var result = DrawingShapeTypeMapper.ToProcessNodeType(DrawingShapeType.Textbox);

            // Assert
            result.Should().BeNull();
        }
    }
}
