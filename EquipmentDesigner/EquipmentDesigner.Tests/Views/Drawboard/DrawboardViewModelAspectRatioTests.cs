using System;
using System.Windows;
using FluentAssertions;
using EquipmentDesigner.Models;
using EquipmentDesigner.ViewModels;
using Xunit;

namespace EquipmentDesigner.Tests.Views.Drawboard
{
    /// <summary>
    /// TDD tests for aspect ratio constraint with all 8 resize thumbs.
    /// Tests validate that Shift+resize maintains aspect ratio for side handles (Top, Bottom, Left, Right),
    /// not just corner handles.
    /// </summary>
    public class DrawboardViewModelAspectRatioTests
    {
        private const double Tolerance = 0.001;

        #region Test Helper

        private static DrawboardViewModel CreateViewModelWithSelectedElement(
            double x, double y, double width, double height)
        {
            var viewModel = new DrawboardViewModel(showBackButton: false);
            var element = new AspectRatioTestElement
            {
                X = x,
                Y = y,
                Width = width,
                Height = height
            };
            viewModel.Elements.Add(element);
            viewModel.SelectElement(element);
            return viewModel;
        }

        private static double GetAspectRatio(DrawingElement element)
        {
            return element.Width / element.Height;
        }

        #endregion

        #region Core Aspect Ratio Logic Tests

        [Fact]
        public void ApplyAspectRatioConstraint_PreservesOriginalAspectRatio_ForTopHandle()
        {
            // Arrange
            var viewModel = CreateViewModelWithSelectedElement(100, 100, 200, 100);
            var element = viewModel.SelectedElement;
            var originalAspectRatio = GetAspectRatio(element); // 2.0

            // Start resize from Top handle
            viewModel.StartResize(ResizeHandleType.Top, new Point(200, 100));

            // Act - drag upward (increase height by 50)
            viewModel.UpdateResize(new Point(200, 50), maintainAspectRatio: true);

            // Assert - aspect ratio should be preserved
            var newAspectRatio = GetAspectRatio(element);
            newAspectRatio.Should().BeApproximately(originalAspectRatio, Tolerance,
                "Top handle with Shift should maintain aspect ratio");
        }

        [Fact]
        public void ApplyAspectRatioConstraint_PreservesOriginalAspectRatio_ForBottomHandle()
        {
            // Arrange
            var viewModel = CreateViewModelWithSelectedElement(100, 100, 200, 100);
            var element = viewModel.SelectedElement;
            var originalAspectRatio = GetAspectRatio(element); // 2.0

            // Start resize from Bottom handle
            viewModel.StartResize(ResizeHandleType.Bottom, new Point(200, 200));

            // Act - drag downward (increase height by 50)
            viewModel.UpdateResize(new Point(200, 250), maintainAspectRatio: true);

            // Assert - aspect ratio should be preserved
            var newAspectRatio = GetAspectRatio(element);
            newAspectRatio.Should().BeApproximately(originalAspectRatio, Tolerance,
                "Bottom handle with Shift should maintain aspect ratio");
        }

        [Fact]
        public void ApplyAspectRatioConstraint_PreservesOriginalAspectRatio_ForLeftHandle()
        {
            // Arrange
            var viewModel = CreateViewModelWithSelectedElement(100, 100, 200, 100);
            var element = viewModel.SelectedElement;
            var originalAspectRatio = GetAspectRatio(element); // 2.0

            // Start resize from Left handle
            viewModel.StartResize(ResizeHandleType.Left, new Point(100, 150));

            // Act - drag leftward (increase width by 50)
            viewModel.UpdateResize(new Point(50, 150), maintainAspectRatio: true);

            // Assert - aspect ratio should be preserved
            var newAspectRatio = GetAspectRatio(element);
            newAspectRatio.Should().BeApproximately(originalAspectRatio, Tolerance,
                "Left handle with Shift should maintain aspect ratio");
        }

        [Fact]
        public void ApplyAspectRatioConstraint_PreservesOriginalAspectRatio_ForRightHandle()
        {
            // Arrange
            var viewModel = CreateViewModelWithSelectedElement(100, 100, 200, 100);
            var element = viewModel.SelectedElement;
            var originalAspectRatio = GetAspectRatio(element); // 2.0

            // Start resize from Right handle
            viewModel.StartResize(ResizeHandleType.Right, new Point(300, 150));

            // Act - drag rightward (increase width by 50)
            viewModel.UpdateResize(new Point(350, 150), maintainAspectRatio: true);

            // Assert - aspect ratio should be preserved
            var newAspectRatio = GetAspectRatio(element);
            newAspectRatio.Should().BeApproximately(originalAspectRatio, Tolerance,
                "Right handle with Shift should maintain aspect ratio");
        }

        #endregion

        #region Top Handle with Shift Tests

        [Fact]
        public void TopHandle_WithShift_IncreasesHeightAndProportionallyIncreasesWidth()
        {
            // Arrange
            var viewModel = CreateViewModelWithSelectedElement(100, 100, 200, 100);
            var element = viewModel.SelectedElement;
            var originalWidth = element.Width;
            var originalHeight = element.Height;

            viewModel.StartResize(ResizeHandleType.Top, new Point(200, 100));

            // Act - drag upward (increase height by 50)
            viewModel.UpdateResize(new Point(200, 50), maintainAspectRatio: true);

            // Assert
            element.Height.Should().BeGreaterThan(originalHeight,
                "Height should increase when dragging Top handle upward");
            element.Width.Should().BeGreaterThan(originalWidth,
                "Width should also increase to maintain aspect ratio");
        }

        [Fact]
        public void TopHandle_WithShift_DecreasesHeightAndProportionallyDecreasesWidth()
        {
            // Arrange
            var viewModel = CreateViewModelWithSelectedElement(100, 100, 200, 100);
            var element = viewModel.SelectedElement;
            var originalWidth = element.Width;
            var originalHeight = element.Height;

            viewModel.StartResize(ResizeHandleType.Top, new Point(200, 100));

            // Act - drag downward (decrease height by 30)
            viewModel.UpdateResize(new Point(200, 130), maintainAspectRatio: true);

            // Assert
            element.Height.Should().BeLessThan(originalHeight,
                "Height should decrease when dragging Top handle downward");
            element.Width.Should().BeLessThan(originalWidth,
                "Width should also decrease to maintain aspect ratio");
        }

        [Fact]
        public void TopHandle_WithShift_AnchorsBottomEdge()
        {
            // Arrange
            var viewModel = CreateViewModelWithSelectedElement(100, 100, 200, 100);
            var element = viewModel.SelectedElement;
            var originalBottom = element.Y + element.Height; // 200

            viewModel.StartResize(ResizeHandleType.Top, new Point(200, 100));

            // Act - drag upward
            viewModel.UpdateResize(new Point(200, 50), maintainAspectRatio: true);

            // Assert - bottom edge should remain anchored
            var newBottom = element.Y + element.Height;
            newBottom.Should().BeApproximately(originalBottom, Tolerance,
                "Bottom edge (Y + Height) should remain anchored when resizing from Top handle");
        }

        [Fact]
        public void TopHandle_WithShift_CentersWidthAdjustmentHorizontally()
        {
            // Arrange
            var viewModel = CreateViewModelWithSelectedElement(100, 100, 200, 100);
            var element = viewModel.SelectedElement;
            var originalCenterX = element.X + element.Width / 2; // 200

            viewModel.StartResize(ResizeHandleType.Top, new Point(200, 100));

            // Act - drag upward
            viewModel.UpdateResize(new Point(200, 50), maintainAspectRatio: true);

            // Assert - horizontal center should remain the same
            var newCenterX = element.X + element.Width / 2;
            newCenterX.Should().BeApproximately(originalCenterX, Tolerance,
                "Horizontal center should remain the same when resizing from Top handle with Shift");
        }

        #endregion

        #region Bottom Handle with Shift Tests

        [Fact]
        public void BottomHandle_WithShift_IncreasesHeightAndProportionallyIncreasesWidth()
        {
            // Arrange
            var viewModel = CreateViewModelWithSelectedElement(100, 100, 200, 100);
            var element = viewModel.SelectedElement;
            var originalWidth = element.Width;
            var originalHeight = element.Height;

            viewModel.StartResize(ResizeHandleType.Bottom, new Point(200, 200));

            // Act - drag downward (increase height by 50)
            viewModel.UpdateResize(new Point(200, 250), maintainAspectRatio: true);

            // Assert
            element.Height.Should().BeGreaterThan(originalHeight,
                "Height should increase when dragging Bottom handle downward");
            element.Width.Should().BeGreaterThan(originalWidth,
                "Width should also increase to maintain aspect ratio");
        }

        [Fact]
        public void BottomHandle_WithShift_AnchorsTopEdge()
        {
            // Arrange
            var viewModel = CreateViewModelWithSelectedElement(100, 100, 200, 100);
            var element = viewModel.SelectedElement;
            var originalTop = element.Y; // 100

            viewModel.StartResize(ResizeHandleType.Bottom, new Point(200, 200));

            // Act - drag downward
            viewModel.UpdateResize(new Point(200, 250), maintainAspectRatio: true);

            // Assert - top edge should remain anchored
            element.Y.Should().BeApproximately(originalTop, Tolerance,
                "Top edge (Y) should remain anchored when resizing from Bottom handle");
        }

        [Fact]
        public void BottomHandle_WithShift_CentersWidthAdjustmentHorizontally()
        {
            // Arrange
            var viewModel = CreateViewModelWithSelectedElement(100, 100, 200, 100);
            var element = viewModel.SelectedElement;
            var originalCenterX = element.X + element.Width / 2; // 200

            viewModel.StartResize(ResizeHandleType.Bottom, new Point(200, 200));

            // Act - drag downward
            viewModel.UpdateResize(new Point(200, 250), maintainAspectRatio: true);

            // Assert - horizontal center should remain the same
            var newCenterX = element.X + element.Width / 2;
            newCenterX.Should().BeApproximately(originalCenterX, Tolerance,
                "Horizontal center should remain the same when resizing from Bottom handle with Shift");
        }

        #endregion

        #region Left Handle with Shift Tests

        [Fact]
        public void LeftHandle_WithShift_IncreasesWidthAndProportionallyIncreasesHeight()
        {
            // Arrange
            var viewModel = CreateViewModelWithSelectedElement(100, 100, 200, 100);
            var element = viewModel.SelectedElement;
            var originalWidth = element.Width;
            var originalHeight = element.Height;

            viewModel.StartResize(ResizeHandleType.Left, new Point(100, 150));

            // Act - drag leftward (increase width by 50)
            viewModel.UpdateResize(new Point(50, 150), maintainAspectRatio: true);

            // Assert
            element.Width.Should().BeGreaterThan(originalWidth,
                "Width should increase when dragging Left handle leftward");
            element.Height.Should().BeGreaterThan(originalHeight,
                "Height should also increase to maintain aspect ratio");
        }

        [Fact]
        public void LeftHandle_WithShift_AnchorsRightEdge()
        {
            // Arrange
            var viewModel = CreateViewModelWithSelectedElement(100, 100, 200, 100);
            var element = viewModel.SelectedElement;
            var originalRight = element.X + element.Width; // 300

            viewModel.StartResize(ResizeHandleType.Left, new Point(100, 150));

            // Act - drag leftward
            viewModel.UpdateResize(new Point(50, 150), maintainAspectRatio: true);

            // Assert - right edge should remain anchored
            var newRight = element.X + element.Width;
            newRight.Should().BeApproximately(originalRight, Tolerance,
                "Right edge (X + Width) should remain anchored when resizing from Left handle");
        }

        [Fact]
        public void LeftHandle_WithShift_CentersHeightAdjustmentVertically()
        {
            // Arrange
            var viewModel = CreateViewModelWithSelectedElement(100, 100, 200, 100);
            var element = viewModel.SelectedElement;
            var originalCenterY = element.Y + element.Height / 2; // 150

            viewModel.StartResize(ResizeHandleType.Left, new Point(100, 150));

            // Act - drag leftward
            viewModel.UpdateResize(new Point(50, 150), maintainAspectRatio: true);

            // Assert - vertical center should remain the same
            var newCenterY = element.Y + element.Height / 2;
            newCenterY.Should().BeApproximately(originalCenterY, Tolerance,
                "Vertical center should remain the same when resizing from Left handle with Shift");
        }

        #endregion

        #region Right Handle with Shift Tests

        [Fact]
        public void RightHandle_WithShift_IncreasesWidthAndProportionallyIncreasesHeight()
        {
            // Arrange
            var viewModel = CreateViewModelWithSelectedElement(100, 100, 200, 100);
            var element = viewModel.SelectedElement;
            var originalWidth = element.Width;
            var originalHeight = element.Height;

            viewModel.StartResize(ResizeHandleType.Right, new Point(300, 150));

            // Act - drag rightward (increase width by 50)
            viewModel.UpdateResize(new Point(350, 150), maintainAspectRatio: true);

            // Assert
            element.Width.Should().BeGreaterThan(originalWidth,
                "Width should increase when dragging Right handle rightward");
            element.Height.Should().BeGreaterThan(originalHeight,
                "Height should also increase to maintain aspect ratio");
        }

        [Fact]
        public void RightHandle_WithShift_AnchorsLeftEdge()
        {
            // Arrange
            var viewModel = CreateViewModelWithSelectedElement(100, 100, 200, 100);
            var element = viewModel.SelectedElement;
            var originalLeft = element.X; // 100

            viewModel.StartResize(ResizeHandleType.Right, new Point(300, 150));

            // Act - drag rightward
            viewModel.UpdateResize(new Point(350, 150), maintainAspectRatio: true);

            // Assert - left edge should remain anchored
            element.X.Should().BeApproximately(originalLeft, Tolerance,
                "Left edge (X) should remain anchored when resizing from Right handle");
        }

        [Fact]
        public void RightHandle_WithShift_CentersHeightAdjustmentVertically()
        {
            // Arrange
            var viewModel = CreateViewModelWithSelectedElement(100, 100, 200, 100);
            var element = viewModel.SelectedElement;
            var originalCenterY = element.Y + element.Height / 2; // 150

            viewModel.StartResize(ResizeHandleType.Right, new Point(300, 150));

            // Act - drag rightward
            viewModel.UpdateResize(new Point(350, 150), maintainAspectRatio: true);

            // Assert - vertical center should remain the same
            var newCenterY = element.Y + element.Height / 2;
            newCenterY.Should().BeApproximately(originalCenterY, Tolerance,
                "Vertical center should remain the same when resizing from Right handle with Shift");
        }

        #endregion

        #region Edge Case Tests

        [Theory]
        [InlineData(200, 100)]  // 2:1 aspect ratio
        [InlineData(100, 300)]  // 1:3 aspect ratio
        [InlineData(150, 150)]  // 1:1 (square)
        public void SideHandle_WithShift_HandlesNonSquareAspectRatios(double width, double height)
        {
            // Arrange
            var viewModel = CreateViewModelWithSelectedElement(100, 100, width, height);
            var element = viewModel.SelectedElement;
            var originalAspectRatio = GetAspectRatio(element);

            viewModel.StartResize(ResizeHandleType.Top, new Point(100 + width / 2, 100));

            // Act - drag upward
            viewModel.UpdateResize(new Point(100 + width / 2, 50), maintainAspectRatio: true);

            // Assert
            var newAspectRatio = GetAspectRatio(element);
            newAspectRatio.Should().BeApproximately(originalAspectRatio, Tolerance,
                $"Aspect ratio {originalAspectRatio:F2} should be preserved");
        }

        [Fact]
        public void SideHandle_WithShift_RespectsMinimumSizeConstraint()
        {
            // Arrange - start with small element
            var viewModel = CreateViewModelWithSelectedElement(100, 100, 20, 10);
            var element = viewModel.SelectedElement;

            viewModel.StartResize(ResizeHandleType.Top, new Point(110, 100));

            // Act - try to shrink below minimum by dragging down significantly
            viewModel.UpdateResize(new Point(110, 108), maintainAspectRatio: true);

            // Assert - dimensions should not go below minimum (1.0)
            element.Width.Should().BeGreaterOrEqualTo(1.0);
            element.Height.Should().BeGreaterOrEqualTo(1.0);
        }

        #endregion

        #region Flip with Aspect Ratio Bug Tests (Pattern C: Bug Fix TDD)

        /// <summary>
        /// API-Level Test: Reproduces user-visible symptom.
        /// After horizontal flip with Shift held, both dimensions should grow proportionally.
        /// BUG: Currently, after flip, height grows at an extreme rate while width grows normally.
        /// </summary>
        [Fact]
        public void RightHandle_WithShift_AfterHorizontalFlip_BothDimensionsGrowProportionally()
        {
            // Arrange - Element at (100, 100) with size 200x100, aspect ratio 2:1
            var viewModel = CreateViewModelWithSelectedElement(100, 100, 200, 100);
            var element = viewModel.SelectedElement;
            var originalAspectRatio = GetAspectRatio(element); // 2.0

            // Start resize from Right handle at right edge (300, 150)
            viewModel.StartResize(ResizeHandleType.Right, new Point(300, 150));

            // Act - Step 1: Drag left past the left edge to trigger horizontal flip
            // Element left edge is at X=100, so drag to X=50 (past left edge by 50)
            viewModel.UpdateResize(new Point(50, 150), maintainAspectRatio: true);

            // After flip, element should be small (flipped)
            // Now continue dragging further left (same direction) to grow the element
            viewModel.UpdateResize(new Point(-50, 150), maintainAspectRatio: true);

            // Assert - Aspect ratio should still match original 2:1
            var newAspectRatio = GetAspectRatio(element);
            newAspectRatio.Should().BeApproximately(originalAspectRatio, Tolerance,
                "After horizontal flip, aspect ratio should still be preserved");
        }

        /// <summary>
        /// API-Level Test: Left handle flip scenario.
        /// </summary>
        [Fact]
        public void LeftHandle_WithShift_AfterHorizontalFlip_BothDimensionsGrowProportionally()
        {
            // Arrange - Element at (100, 100) with size 200x100
            var viewModel = CreateViewModelWithSelectedElement(100, 100, 200, 100);
            var element = viewModel.SelectedElement;
            var originalAspectRatio = GetAspectRatio(element); // 2.0

            // Start resize from Left handle at left edge (100, 150)
            viewModel.StartResize(ResizeHandleType.Left, new Point(100, 150));

            // Act - Drag right past the right edge to trigger flip, then continue
            // Element right edge is at X=300
            viewModel.UpdateResize(new Point(350, 150), maintainAspectRatio: true);
            viewModel.UpdateResize(new Point(450, 150), maintainAspectRatio: true);

            // Assert
            var newAspectRatio = GetAspectRatio(element);
            newAspectRatio.Should().BeApproximately(originalAspectRatio, Tolerance,
                "After horizontal flip from Left handle, aspect ratio should be preserved");
        }

        /// <summary>
        /// API-Level Test: Vertical flip with Top handle.
        /// BUG: After vertical flip, width grows at extreme rate while height grows normally.
        /// </summary>
        [Fact]
        public void TopHandle_WithShift_AfterVerticalFlip_BothDimensionsGrowProportionally()
        {
            // Arrange - Element at (100, 100) with size 200x100
            var viewModel = CreateViewModelWithSelectedElement(100, 100, 200, 100);
            var element = viewModel.SelectedElement;
            var originalAspectRatio = GetAspectRatio(element); // 2.0

            // Start resize from Top handle at top edge (200, 100)
            viewModel.StartResize(ResizeHandleType.Top, new Point(200, 100));

            // Act - Drag down past the bottom edge to trigger flip, then continue
            // Element bottom edge is at Y=200
            viewModel.UpdateResize(new Point(200, 250), maintainAspectRatio: true);
            viewModel.UpdateResize(new Point(200, 350), maintainAspectRatio: true);

            // Assert
            var newAspectRatio = GetAspectRatio(element);
            newAspectRatio.Should().BeApproximately(originalAspectRatio, Tolerance,
                "After vertical flip from Top handle, aspect ratio should be preserved");
        }

        /// <summary>
        /// API-Level Test: Vertical flip with Bottom handle.
        /// </summary>
        [Fact]
        public void BottomHandle_WithShift_AfterVerticalFlip_BothDimensionsGrowProportionally()
        {
            // Arrange - Element at (100, 100) with size 200x100
            var viewModel = CreateViewModelWithSelectedElement(100, 100, 200, 100);
            var element = viewModel.SelectedElement;
            var originalAspectRatio = GetAspectRatio(element); // 2.0

            // Start resize from Bottom handle at bottom edge (200, 200)
            viewModel.StartResize(ResizeHandleType.Bottom, new Point(200, 200));

            // Act - Drag up past the top edge to trigger flip, then continue
            // Element top edge is at Y=100
            viewModel.UpdateResize(new Point(200, 50), maintainAspectRatio: true);
            viewModel.UpdateResize(new Point(200, -50), maintainAspectRatio: true);

            // Assert
            var newAspectRatio = GetAspectRatio(element);
            newAspectRatio.Should().BeApproximately(originalAspectRatio, Tolerance,
                "After vertical flip from Bottom handle, aspect ratio should be preserved");
        }

        /// <summary>
        /// Replication Test: Isolates the scale calculation issue after flip.
        /// The bug is that after flip, _trueOriginalBounds becomes very small, causing
        /// scale = newDimension / _trueOriginalBounds.Dimension to be extremely large.
        /// </summary>
        [Fact]
        public void ScaleCalculation_AfterFlip_DoesNotProduceExtremeValues()
        {
            // Arrange - Element at (100, 100) with size 200x100
            var viewModel = CreateViewModelWithSelectedElement(100, 100, 200, 100);
            var element = viewModel.SelectedElement;

            // Start resize from Right handle
            viewModel.StartResize(ResizeHandleType.Right, new Point(300, 150));

            // Act - Trigger flip by dragging past left edge
            viewModel.UpdateResize(new Point(50, 150), maintainAspectRatio: true);
            var widthAfterFlip = element.Width;
            var heightAfterFlip = element.Height;

            // Continue dragging in same direction (growing after flip)
            viewModel.UpdateResize(new Point(-50, 150), maintainAspectRatio: true);
            var widthAfterContinue = element.Width;
            var heightAfterContinue = element.Height;

            // Assert - The growth rate should be reasonable
            // BUG: Without fix, heightAfterContinue could be 10x+ heightAfterFlip
            // while widthAfterContinue is only ~2x widthAfterFlip
            var widthGrowthRatio = widthAfterContinue / Math.Max(1, widthAfterFlip);
            var heightGrowthRatio = heightAfterContinue / Math.Max(1, heightAfterFlip);

            // The ratio between growth rates should not exceed 2:1 
            // (some variation is acceptable due to aspect ratio, but not 10x)
            var growthRateImbalance = Math.Max(widthGrowthRatio, heightGrowthRatio) /
                                      Math.Max(0.1, Math.Min(widthGrowthRatio, heightGrowthRatio));
            growthRateImbalance.Should().BeLessThan(3.0,
                "Width and height growth rates should be proportional after flip, not extremely imbalanced");
        }

        /// <summary>
        /// Replication Test: Corner handle diagonal flip should preserve aspect ratio.
        /// </summary>
        [Fact]
        public void CornerHandle_WithShift_AfterDiagonalFlip_PreservesAspectRatio()
        {
            // Arrange
            var viewModel = CreateViewModelWithSelectedElement(100, 100, 200, 100);
            var element = viewModel.SelectedElement;
            var originalAspectRatio = GetAspectRatio(element); // 2.0

            // Start resize from BottomRight corner
            viewModel.StartResize(ResizeHandleType.BottomRight, new Point(300, 200));

            // Act - Drag to TopLeft quadrant (diagonal flip)
            viewModel.UpdateResize(new Point(50, 50), maintainAspectRatio: true);
            viewModel.UpdateResize(new Point(-50, -50), maintainAspectRatio: true);

            // Assert
            var newAspectRatio = GetAspectRatio(element);
            newAspectRatio.Should().BeApproximately(originalAspectRatio, Tolerance,
                "After diagonal flip from corner handle, aspect ratio should be preserved");
        }

        /// <summary>
        /// Replication Test: Non-square aspect ratio after flip.
        /// </summary>
        [Theory]
        [InlineData(300, 100)]  // 3:1 wide
        [InlineData(100, 300)]  // 1:3 tall
        public void SideHandle_WithShift_AfterFlip_PreservesNonSquareAspectRatio(double width, double height)
        {
            // Arrange
            var viewModel = CreateViewModelWithSelectedElement(100, 100, width, height);
            var element = viewModel.SelectedElement;
            var originalAspectRatio = GetAspectRatio(element);

            // Start resize from Right handle
            viewModel.StartResize(ResizeHandleType.Right, new Point(100 + width, 100 + height / 2));

            // Act - Trigger flip and continue
            viewModel.UpdateResize(new Point(50, 100 + height / 2), maintainAspectRatio: true);
            viewModel.UpdateResize(new Point(-100, 100 + height / 2), maintainAspectRatio: true);

            // Assert
            var newAspectRatio = GetAspectRatio(element);
            newAspectRatio.Should().BeApproximately(originalAspectRatio, Tolerance,
                $"Non-square aspect ratio {originalAspectRatio:F2} should be preserved after flip");
        }

        #endregion
    }

    /// <summary>
    /// Concrete DrawingElement implementation for aspect ratio testing.
    /// Named differently to avoid conflict with TestDrawingElement in DrawingElementTests.
    /// </summary>
    internal class AspectRatioTestElement : DrawingElement
    {
        public override DrawingShapeType ShapeType => DrawingShapeType.Action;
    }
}