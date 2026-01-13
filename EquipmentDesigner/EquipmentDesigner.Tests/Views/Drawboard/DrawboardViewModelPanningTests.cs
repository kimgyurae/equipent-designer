using System.Windows;
using EquipmentDesigner.Models;
using EquipmentDesigner.ViewModels;
using EquipmentDesigner.Views.Drawboard.UMLEngine;
using EquipmentDesigner.Views.Drawboard.UMLEngine.Contexts;
using FluentAssertions;
using Xunit;

namespace EquipmentDesigner.Tests.Views.Drawboard
{
    /// <summary>
    /// Tests for HandTool canvas panning functionality in DrawboardViewModel.
    /// </summary>
    public class DrawboardViewModelPanningTests
    {
        #region Tool State Detection

        [Fact]
        public void IsHandToolActive_WhenHandToolSelected_ReturnsTrue()
        {
            // Arrange
            var viewModel = new DrawboardViewModel("test-process-id", showBackButton: false);

            // Act
            viewModel.SelectToolById("Hand");

            // Assert
            viewModel.IsHandToolActive.Should().BeTrue();
        }

        [Fact]
        public void IsHandToolActive_WhenSelectionToolSelected_ReturnsFalse()
        {
            // Arrange
            var viewModel = new DrawboardViewModel("test-process-id", showBackButton: false);

            // Act
            viewModel.SelectToolById("Selection");

            // Assert
            viewModel.IsHandToolActive.Should().BeFalse();
        }

        [Fact]
        public void IsHandToolActive_WhenDrawingToolSelected_ReturnsFalse()
        {
            // Arrange
            var viewModel = new DrawboardViewModel("test-process-id", showBackButton: false);

            // Act
            viewModel.SelectToolById("ActionNode");

            // Assert
            viewModel.IsHandToolActive.Should().BeFalse();
        }

        [Fact]
        public void IsHandToolActive_UpdatesAfterSelectToolById()
        {
            // Arrange
            var viewModel = new DrawboardViewModel("test-process-id", showBackButton: false);
            viewModel.SelectToolById("Selection");
            viewModel.IsHandToolActive.Should().BeFalse();

            // Act
            viewModel.SelectToolById("Hand");

            // Assert
            viewModel.IsHandToolActive.Should().BeTrue();
        }

        #endregion

        #region Pan State Management

        [Fact]
        public void IsPanning_Initially_ReturnsFalse()
        {
            // Arrange & Act
            var viewModel = new DrawboardViewModel("test-process-id", showBackButton: false);

            // Assert
            viewModel.IsPanning.Should().BeFalse();
        }

        [Fact]
        public void StartPan_WhenHandToolActive_SetsIsPanningToTrue()
        {
            // Arrange
            var viewModel = new DrawboardViewModel("test-process-id", showBackButton: false);
            viewModel.SelectToolById("Hand");

            // Act
            viewModel.StartPan(new Point(100, 100));

            // Assert
            viewModel.IsPanning.Should().BeTrue();
        }

        [Fact]
        public void EndPan_SetsIsPanningToFalse()
        {
            // Arrange
            var viewModel = new DrawboardViewModel("test-process-id", showBackButton: false);
            viewModel.SelectToolById("Hand");
            viewModel.StartPan(new Point(100, 100));

            // Act
            viewModel.EndPan();

            // Assert
            viewModel.IsPanning.Should().BeFalse();
        }

        [Fact]
        public void StartPan_WhenNotHandTool_DoesNotStartPanning()
        {
            // Arrange
            var viewModel = new DrawboardViewModel("test-process-id", showBackButton: false);
            viewModel.SelectToolById("Selection");

            // Act
            viewModel.StartPan(new Point(100, 100));

            // Assert
            viewModel.IsPanning.Should().BeFalse();
        }

        #endregion

        #region Pan Calculation (Engine - Pure Functions)

        [Fact]
        public void CalculatePan_WhenNoMovement_ReturnsOriginalOffset()
        {
            // Arrange
            var context = new PanContext(
                dragStartPoint: new Point(100, 100),
                originalScrollOffset: new Point(50, 50),
                zoomScale: 1.0);

            // Act
            var result = CanvasPanEngine.CalculatePan(context, new Point(100, 100));

            // Assert
            result.NewScrollOffsetX.Should().Be(50);
            result.NewScrollOffsetY.Should().Be(50);
        }

        [Fact]
        public void CalculatePan_WhenDraggingRight_ScrollsLeft()
        {
            // Arrange
            var context = new PanContext(
                dragStartPoint: new Point(100, 100),
                originalScrollOffset: new Point(50, 50),
                zoomScale: 1.0);

            // Act - drag right by 30 pixels
            var result = CanvasPanEngine.CalculatePan(context, new Point(130, 100));

            // Assert - scroll offset decreases (scroll left)
            result.NewScrollOffsetX.Should().Be(20);
            result.NewScrollOffsetY.Should().Be(50);
        }

        [Fact]
        public void CalculatePan_WhenDraggingLeft_ScrollsRight()
        {
            // Arrange
            var context = new PanContext(
                dragStartPoint: new Point(100, 100),
                originalScrollOffset: new Point(50, 50),
                zoomScale: 1.0);

            // Act - drag left by 30 pixels
            var result = CanvasPanEngine.CalculatePan(context, new Point(70, 100));

            // Assert - scroll offset increases (scroll right)
            result.NewScrollOffsetX.Should().Be(80);
            result.NewScrollOffsetY.Should().Be(50);
        }

        [Fact]
        public void CalculatePan_WhenDraggingDown_ScrollsUp()
        {
            // Arrange
            var context = new PanContext(
                dragStartPoint: new Point(100, 100),
                originalScrollOffset: new Point(50, 50),
                zoomScale: 1.0);

            // Act - drag down by 30 pixels
            var result = CanvasPanEngine.CalculatePan(context, new Point(100, 130));

            // Assert - scroll offset decreases (scroll up)
            result.NewScrollOffsetX.Should().Be(50);
            result.NewScrollOffsetY.Should().Be(20);
        }

        [Fact]
        public void CalculatePan_WhenDraggingUp_ScrollsDown()
        {
            // Arrange
            var context = new PanContext(
                dragStartPoint: new Point(100, 100),
                originalScrollOffset: new Point(50, 50),
                zoomScale: 1.0);

            // Act - drag up by 30 pixels
            var result = CanvasPanEngine.CalculatePan(context, new Point(100, 70));

            // Assert - scroll offset increases (scroll down)
            result.NewScrollOffsetX.Should().Be(50);
            result.NewScrollOffsetY.Should().Be(80);
        }

        [Fact]
        public void CalculatePan_DiagonalMovement_WorksCorrectly()
        {
            // Arrange
            var context = new PanContext(
                dragStartPoint: new Point(100, 100),
                originalScrollOffset: new Point(50, 50),
                zoomScale: 1.0);

            // Act - drag diagonally (right 20, down 30)
            var result = CanvasPanEngine.CalculatePan(context, new Point(120, 130));

            // Assert
            result.NewScrollOffsetX.Should().Be(30); // 50 - 20
            result.NewScrollOffsetY.Should().Be(20); // 50 - 30
        }

        [Fact]
        public void CalculatePan_ClampsToZero_WhenNegative()
        {
            // Arrange
            var context = new PanContext(
                dragStartPoint: new Point(100, 100),
                originalScrollOffset: new Point(10, 10),
                zoomScale: 1.0);

            // Act - drag right/down by large amount that would make offset negative
            var result = CanvasPanEngine.CalculatePan(context, new Point(200, 200));

            // Assert - clamped to 0
            result.NewScrollOffsetX.Should().Be(0);
            result.NewScrollOffsetY.Should().Be(0);
        }

        #endregion

        #region Integration with Edit Mode State

        [Fact]
        public void StartPan_SetsEditModeStateToPanning()
        {
            // Arrange
            var viewModel = new DrawboardViewModel("test-process-id", showBackButton: false);
            viewModel.SelectToolById("Hand");

            // Act
            viewModel.StartPan(new Point(100, 100));

            // Assert
            viewModel.EditModeState.Should().Be(EditModeState.Panning);
        }

        [Fact]
        public void EndPan_SetsEditModeStateToNone()
        {
            // Arrange
            var viewModel = new DrawboardViewModel("test-process-id", showBackButton: false);
            viewModel.SelectToolById("Hand");
            viewModel.StartPan(new Point(100, 100));

            // Act
            viewModel.EndPan();

            // Assert
            viewModel.EditModeState.Should().Be(EditModeState.None);
        }

        [Fact]
        public void EndPan_WhenNotPanning_DoesNothing()
        {
            // Arrange
            var viewModel = new DrawboardViewModel("test-process-id", showBackButton: false);
            viewModel.EditModeState.Should().Be(EditModeState.None);

            // Act
            viewModel.EndPan();

            // Assert - should not throw, state remains None
            viewModel.EditModeState.Should().Be(EditModeState.None);
        }

        #endregion

        #region Pan Update Logic

        [Fact]
        public void UpdatePan_WhenNotPanning_DoesNothing()
        {
            // Arrange
            var viewModel = new DrawboardViewModel("test-process-id", showBackButton: false);
            double? receivedX = null;
            double? receivedY = null;
            viewModel.ApplyScrollOffset = (x, y) => { receivedX = x; receivedY = y; };

            // Act
            viewModel.UpdatePan(new Point(150, 150));

            // Assert
            receivedX.Should().BeNull();
            receivedY.Should().BeNull();
        }

        [Fact]
        public void UpdatePan_WhenPanning_InvokesApplyScrollOffset()
        {
            // Arrange
            var viewModel = new DrawboardViewModel("test-process-id", showBackButton: false);
            viewModel.SelectToolById("Hand");
            double? receivedX = null;
            double? receivedY = null;
            viewModel.ApplyScrollOffset = (x, y) => { receivedX = x; receivedY = y; };
            viewModel.StartPan(new Point(100, 100));

            // Act
            viewModel.UpdatePan(new Point(130, 120));

            // Assert
            receivedX.Should().NotBeNull();
            receivedY.Should().NotBeNull();
        }

        #endregion
    }
}