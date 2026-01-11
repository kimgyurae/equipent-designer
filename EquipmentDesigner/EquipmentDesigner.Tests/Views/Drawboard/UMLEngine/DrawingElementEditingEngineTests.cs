using System.Collections.Generic;
using System.Linq;
using System.Windows;
using FluentAssertions;
using EquipmentDesigner.Models;
using EquipmentDesigner.Views.Drawboard.UMLEngine;
using EquipmentDesigner.Views.Drawboard.UMLEngine.Contexts;
using Xunit;

namespace EquipmentDesigner.Tests.Views.Drawboard.UMLEngine
{
    /// <summary>
    /// TDD tests for DrawingElementEditingEngine - stateless calculation engine
    /// for element move, resize, and drawing operations.
    /// </summary>
    public class DrawingElementEditingEngineTests
    {
        private const double Tolerance = 0.001;

        #region Utility Methods Tests

        [Fact]
        public void GetFlippedHandle_WithNoFlip_ReturnsSameHandle()
        {
            // Arrange
            var handle = ResizeHandleType.TopLeft;

            // Act
            var result = DrawingElementEditingEngine.GetFlippedHandle(handle, false, false);

            // Assert
            result.Should().Be(ResizeHandleType.TopLeft);
        }

        [Fact]
        public void GetFlippedHandle_WithHorizontalFlip_SwapsLeftAndRightHandles()
        {
            // Act & Assert
            DrawingElementEditingEngine.GetFlippedHandle(ResizeHandleType.Left, true, false)
                .Should().Be(ResizeHandleType.Right);
            DrawingElementEditingEngine.GetFlippedHandle(ResizeHandleType.Right, true, false)
                .Should().Be(ResizeHandleType.Left);
        }

        [Fact]
        public void GetFlippedHandle_WithVerticalFlip_SwapsTopAndBottomHandles()
        {
            // Act & Assert
            DrawingElementEditingEngine.GetFlippedHandle(ResizeHandleType.Top, false, true)
                .Should().Be(ResizeHandleType.Bottom);
            DrawingElementEditingEngine.GetFlippedHandle(ResizeHandleType.Bottom, false, true)
                .Should().Be(ResizeHandleType.Top);
        }

        [Fact]
        public void GetFlippedHandle_WithBothFlips_DiagonallySwapsCornerHandles()
        {
            // Act & Assert - TopLeft becomes BottomRight with both flips
            DrawingElementEditingEngine.GetFlippedHandle(ResizeHandleType.TopLeft, true, true)
                .Should().Be(ResizeHandleType.BottomRight);
            DrawingElementEditingEngine.GetFlippedHandle(ResizeHandleType.BottomRight, true, true)
                .Should().Be(ResizeHandleType.TopLeft);
        }

        [Fact]
        public void GetFlippedHandle_WithHorizontalFlip_TopLeftBecomesTopRight()
        {
            var result = DrawingElementEditingEngine.GetFlippedHandle(ResizeHandleType.TopLeft, true, false);
            result.Should().Be(ResizeHandleType.TopRight);
        }

        [Fact]
        public void GetFlippedHandle_WithHorizontalFlip_BottomLeftBecomesBottomRight()
        {
            var result = DrawingElementEditingEngine.GetFlippedHandle(ResizeHandleType.BottomLeft, true, false);
            result.Should().Be(ResizeHandleType.BottomRight);
        }

        [Fact]
        public void GetFlippedHandle_WithVerticalFlip_TopLeftBecomesBottomLeft()
        {
            var result = DrawingElementEditingEngine.GetFlippedHandle(ResizeHandleType.TopLeft, false, true);
            result.Should().Be(ResizeHandleType.BottomLeft);
        }

        [Fact]
        public void GetFlippedHandle_WithVerticalFlip_TopRightBecomesBottomRight()
        {
            var result = DrawingElementEditingEngine.GetFlippedHandle(ResizeHandleType.TopRight, false, true);
            result.Should().Be(ResizeHandleType.BottomRight);
        }

        [Theory]
        [InlineData(ResizeHandleType.TopLeft, true)]
        [InlineData(ResizeHandleType.TopRight, true)]
        [InlineData(ResizeHandleType.BottomLeft, true)]
        [InlineData(ResizeHandleType.BottomRight, true)]
        [InlineData(ResizeHandleType.Top, false)]
        [InlineData(ResizeHandleType.Bottom, false)]
        [InlineData(ResizeHandleType.Left, false)]
        [InlineData(ResizeHandleType.Right, false)]
        [InlineData(ResizeHandleType.None, false)]
        public void IsCornerHandle_ReturnsCorrectValue(ResizeHandleType handle, bool expected)
        {
            DrawingElementEditingEngine.IsCornerHandle(handle).Should().Be(expected);
        }

        [Theory]
        [InlineData(ResizeHandleType.Left, true)]
        [InlineData(ResizeHandleType.TopLeft, true)]
        [InlineData(ResizeHandleType.BottomLeft, true)]
        [InlineData(ResizeHandleType.Right, false)]
        [InlineData(ResizeHandleType.TopRight, false)]
        [InlineData(ResizeHandleType.BottomRight, false)]
        [InlineData(ResizeHandleType.Top, false)]
        [InlineData(ResizeHandleType.Bottom, false)]
        public void IsLeftHandle_ReturnsCorrectValue(ResizeHandleType handle, bool expected)
        {
            DrawingElementEditingEngine.IsLeftHandle(handle).Should().Be(expected);
        }

        [Theory]
        [InlineData(ResizeHandleType.Top, true)]
        [InlineData(ResizeHandleType.TopLeft, true)]
        [InlineData(ResizeHandleType.TopRight, true)]
        [InlineData(ResizeHandleType.Bottom, false)]
        [InlineData(ResizeHandleType.BottomLeft, false)]
        [InlineData(ResizeHandleType.BottomRight, false)]
        [InlineData(ResizeHandleType.Left, false)]
        [InlineData(ResizeHandleType.Right, false)]
        public void IsTopHandle_ReturnsCorrectValue(ResizeHandleType handle, bool expected)
        {
            DrawingElementEditingEngine.IsTopHandle(handle).Should().Be(expected);
        }

        #endregion

        #region Move Operations Tests

        [Fact]
        public void CreateMoveContext_WithValidBoundsAndStartPoint_CapturesOriginalState()
        {
            // Arrange
            var bounds = new Rect(100, 100, 200, 100);
            var startPoint = new Point(150, 150);

            // Act
            var context = DrawingElementEditingEngine.CreateMoveContext(bounds, startPoint);

            // Assert
            context.OriginalBounds.Should().Be(bounds);
            context.DragStartPoint.Should().Be(startPoint);
        }

        [Fact]
        public void CalculateMove_WithPositiveDelta_MovesElementDownRight()
        {
            // Arrange
            var context = new MoveContext(
                dragStartPoint: new Point(100, 100),
                originalBounds: new Rect(50, 50, 200, 100));

            // Act - move 50 right, 20 down
            var result = DrawingElementEditingEngine.CalculateMove(context, new Point(150, 120));

            // Assert
            result.NewX.Should().Be(100);  // 50 + (150-100) = 100
            result.NewY.Should().Be(70);   // 50 + (120-100) = 70
        }

        [Fact]
        public void CalculateMove_WithNegativeDelta_MovesElementUpLeft()
        {
            // Arrange
            var context = new MoveContext(
                dragStartPoint: new Point(100, 100),
                originalBounds: new Rect(50, 50, 200, 100));

            // Act - move 20 left, 30 up
            var result = DrawingElementEditingEngine.CalculateMove(context, new Point(80, 70));

            // Assert
            result.NewX.Should().Be(30);   // 50 + (80-100) = 30
            result.NewY.Should().Be(20);   // 50 + (70-100) = 20
        }

        [Fact]
        public void CalculateMove_WithZeroDelta_KeepsOriginalPosition()
        {
            // Arrange
            var context = new MoveContext(
                dragStartPoint: new Point(100, 100),
                originalBounds: new Rect(50, 50, 200, 100));

            // Act - no movement
            var result = DrawingElementEditingEngine.CalculateMove(context, new Point(100, 100));

            // Assert
            result.NewX.Should().Be(50);
            result.NewY.Should().Be(50);
        }

        [Fact]
        public void CalculateMove_WithLargeDelta_CalculatesCorrectPosition()
        {
            // Arrange
            var context = new MoveContext(
                dragStartPoint: new Point(0, 0),
                originalBounds: new Rect(100, 100, 200, 100));

            // Act - large movement
            var result = DrawingElementEditingEngine.CalculateMove(context, new Point(1000, 500));

            // Assert
            result.NewX.Should().Be(1100);  // 100 + 1000
            result.NewY.Should().Be(600);   // 100 + 500
        }

        [Fact]
        public void CalculateMove_WithNegativeCoordinates_HandlesCorrectly()
        {
            // Arrange
            var context = new MoveContext(
                dragStartPoint: new Point(50, 50),
                originalBounds: new Rect(100, 100, 200, 100));

            // Act - move to negative coordinates
            var result = DrawingElementEditingEngine.CalculateMove(context, new Point(-50, -50));

            // Assert
            result.NewX.Should().Be(0);    // 100 + (-50-50) = 0
            result.NewY.Should().Be(0);    // 100 + (-50-50) = 0
        }

        #endregion

        #region Drawing Operations Tests

        [Fact]
        public void CreateDrawingContext_WithValidStartPointAndShapeType_CapturesState()
        {
            // Arrange
            var startPoint = new Point(100, 100);
            var shapeType = DrawingShapeType.Action;

            // Act
            var context = DrawingElementEditingEngine.CreateDrawingContext(startPoint, shapeType);

            // Assert
            context.StartPoint.Should().Be(startPoint);
            context.ShapeType.Should().Be(shapeType);
        }

        [Fact]
        public void CalculateDrawingBounds_TopLeftToBottomRight_ReturnsCorrectBounds()
        {
            // Arrange
            var context = new DrawingContext(new Point(100, 100), DrawingShapeType.Action);

            // Act
            var result = DrawingElementEditingEngine.CalculateDrawingBounds(context, new Point(200, 150));

            // Assert
            result.X.Should().Be(100);
            result.Y.Should().Be(100);
            result.Width.Should().Be(100);
            result.Height.Should().Be(50);
        }

        [Fact]
        public void CalculateDrawingBounds_BottomRightToTopLeft_NormalizesCoordinates()
        {
            // Arrange - start at bottom-right, drag to top-left
            var context = new DrawingContext(new Point(200, 150), DrawingShapeType.Action);

            // Act
            var result = DrawingElementEditingEngine.CalculateDrawingBounds(context, new Point(100, 100));

            // Assert - should normalize to correct position
            result.X.Should().Be(100);
            result.Y.Should().Be(100);
            result.Width.Should().Be(100);
            result.Height.Should().Be(50);
        }

        [Fact]
        public void CalculateDrawingBounds_WithSameStartAndEndPoint_ReturnsMinimumSize()
        {
            // Arrange
            var context = new DrawingContext(new Point(100, 100), DrawingShapeType.Action);

            // Act
            var result = DrawingElementEditingEngine.CalculateDrawingBounds(context, new Point(100, 100));

            // Assert
            result.Width.Should().BeGreaterOrEqualTo(1.0);
            result.Height.Should().BeGreaterOrEqualTo(1.0);
        }

        #endregion

        #region Basic Resize Tests

        [Fact]
        public void CreateResizeContext_WithValidInputs_CalculatesAspectRatio()
        {
            // Arrange
            var bounds = new Rect(100, 100, 200, 100);  // 2:1 aspect ratio

            // Act
            var context = DrawingElementEditingEngine.CreateResizeContext(
                bounds, ResizeHandleType.BottomRight, new Point(300, 200));

            // Assert
            context.OriginalAspectRatio.Should().BeApproximately(2.0, Tolerance);
            context.TrueOriginalBounds.Should().Be(bounds);
            context.InitialHandle.Should().Be(ResizeHandleType.BottomRight);
        }

        [Fact]
        public void CalculateResize_BottomRight_IncreasesWidthAndHeight()
        {
            // Arrange
            var context = DrawingElementEditingEngine.CreateResizeContext(
                new Rect(100, 100, 200, 100),
                ResizeHandleType.BottomRight,
                new Point(300, 200));

            // Act - drag 50 right, 50 down
            var result = DrawingElementEditingEngine.CalculateResize(
                context, new Point(350, 250), maintainAspectRatio: false);

            // Assert
            result.NewWidth.Should().Be(250);   // 200 + 50
            result.NewHeight.Should().Be(150);  // 100 + 50
            result.NewX.Should().Be(100);       // Unchanged
            result.NewY.Should().Be(100);       // Unchanged
        }

        [Fact]
        public void CalculateResize_TopLeft_IncreasesWidthAndHeightByMovingOrigin()
        {
            // Arrange
            var context = DrawingElementEditingEngine.CreateResizeContext(
                new Rect(100, 100, 200, 100),
                ResizeHandleType.TopLeft,
                new Point(100, 100));

            // Act - drag 50 left, 50 up (increases size)
            var result = DrawingElementEditingEngine.CalculateResize(
                context, new Point(50, 50), maintainAspectRatio: false);

            // Assert
            result.NewX.Should().Be(50);
            result.NewY.Should().Be(50);
            result.NewWidth.Should().Be(250);   // 200 + 50
            result.NewHeight.Should().Be(150);  // 100 + 50
        }

        [Fact]
        public void CalculateResize_Right_OnlyChangesWidth()
        {
            // Arrange
            var context = DrawingElementEditingEngine.CreateResizeContext(
                new Rect(100, 100, 200, 100),
                ResizeHandleType.Right,
                new Point(300, 150));

            // Act
            var result = DrawingElementEditingEngine.CalculateResize(
                context, new Point(350, 150), maintainAspectRatio: false);

            // Assert
            result.NewWidth.Should().Be(250);
            result.NewHeight.Should().Be(100);  // Unchanged
            result.NewX.Should().Be(100);       // Unchanged
            result.NewY.Should().Be(100);       // Unchanged
        }

        [Fact]
        public void CalculateResize_WithMinimumSizeViolation_ClampsToMinimum()
        {
            // Arrange
            var context = DrawingElementEditingEngine.CreateResizeContext(
                new Rect(100, 100, 50, 50),
                ResizeHandleType.TopLeft,
                new Point(100, 100));

            // Act - try to shrink beyond minimum
            var result = DrawingElementEditingEngine.CalculateResize(
                context, new Point(200, 200), maintainAspectRatio: false);

            // Assert
            result.NewWidth.Should().BeGreaterOrEqualTo(1.0);
            result.NewHeight.Should().BeGreaterOrEqualTo(1.0);
        }

        #endregion

        #region Aspect Ratio Tests

        [Fact]
        public void CalculateResize_CornerHandle_WithAspectRatio_PreservesOriginalRatio()
        {
            // Arrange
            var context = DrawingElementEditingEngine.CreateResizeContext(
                new Rect(100, 100, 200, 100),  // 2:1
                ResizeHandleType.BottomRight,
                new Point(300, 200));

            // Act
            var result = DrawingElementEditingEngine.CalculateResize(
                context, new Point(400, 250), maintainAspectRatio: true);

            // Assert
            var newAspectRatio = result.NewWidth / result.NewHeight;
            newAspectRatio.Should().BeApproximately(2.0, Tolerance);
        }

        [Fact]
        public void CalculateResize_TopHandle_WithAspectRatio_CentersWidthHorizontally()
        {
            // Arrange
            var context = DrawingElementEditingEngine.CreateResizeContext(
                new Rect(100, 100, 200, 100),
                ResizeHandleType.Top,
                new Point(200, 100));
            var originalCenterX = 100 + 200.0 / 2;  // 200

            // Act
            var result = DrawingElementEditingEngine.CalculateResize(
                context, new Point(200, 50), maintainAspectRatio: true);

            // Assert
            var newCenterX = result.NewX + result.NewWidth / 2;
            newCenterX.Should().BeApproximately(originalCenterX, Tolerance);
        }

        #endregion

        #region Flip Detection Tests

        [Fact]
        public void CalculateResize_RightHandle_DragPastLeftEdge_DetectsHorizontalFlip()
        {
            // Arrange
            var context = DrawingElementEditingEngine.CreateResizeContext(
                new Rect(100, 100, 200, 100),
                ResizeHandleType.Right,
                new Point(300, 150));

            // Act - drag past left edge (100)
            var result = DrawingElementEditingEngine.CalculateResize(
                context, new Point(50, 150), maintainAspectRatio: false);

            // Assert - handle should flip to Left
            result.ActiveHandle.Should().Be(ResizeHandleType.Left);
        }

        [Fact]
        public void CalculateResize_OnFlip_UpdatesContextWasFlippedFlag()
        {
            // Arrange
            var context = DrawingElementEditingEngine.CreateResizeContext(
                new Rect(100, 100, 200, 100),
                ResizeHandleType.Right,
                new Point(300, 150));

            // Act - trigger horizontal flip
            var result = DrawingElementEditingEngine.CalculateResize(
                context, new Point(50, 150), maintainAspectRatio: false);

            // Assert
            result.UpdatedContext.WasHorizontallyFlipped.Should().BeTrue();
        }

        #endregion

        #region Flip with Aspect Ratio Tests

        [Fact]
        public void CalculateResize_RightHandle_AfterHorizontalFlip_PreservesAspectRatio()
        {
            // Arrange
            var context = DrawingElementEditingEngine.CreateResizeContext(
                new Rect(100, 100, 200, 100),  // 2:1
                ResizeHandleType.Right,
                new Point(300, 150));

            // Act - flip and continue
            var result1 = DrawingElementEditingEngine.CalculateResize(
                context, new Point(50, 150), maintainAspectRatio: true);
            var result2 = DrawingElementEditingEngine.CalculateResize(
                result1.UpdatedContext, new Point(-50, 150), maintainAspectRatio: true);

            // Assert
            var aspectRatio = result2.NewWidth / result2.NewHeight;
            aspectRatio.Should().BeApproximately(2.0, Tolerance);
        }

        #endregion
        #region Group Operations Tests

        [Fact]
        public void ComputeGroupBounds_WithMultipleElements_ReturnsMinimumBoundingBox()
        {
            // Arrange
            var elements = new List<DrawingElement>
            {
                new ActionElement { X = 10, Y = 10, Width = 50, Height = 30 },
                new ActionElement { X = 100, Y = 80, Width = 40, Height = 20 }
            };

            // Act
            var result = DrawingElementEditingEngine.ComputeGroupBounds(elements);

            // Assert
            result.X.Should().BeApproximately(10, Tolerance);
            result.Y.Should().BeApproximately(10, Tolerance);
            result.Width.Should().BeApproximately(130, Tolerance); // 140 - 10
            result.Height.Should().BeApproximately(90, Tolerance); // 100 - 10
        }

        [Fact]
        public void ComputeGroupBounds_WithEmptyCollection_ReturnsEmptyRect()
        {
            // Arrange
            var elements = new List<DrawingElement>();

            // Act
            var result = DrawingElementEditingEngine.ComputeGroupBounds(elements);

            // Assert
            result.Should().Be(Rect.Empty);
        }

        [Fact]
        public void CalculateGroupMove_AppliesSameDeltaToAllElements()
        {
            // Arrange
            var originalBounds = new List<Rect>
            {
                new Rect(10, 20, 50, 30),
                new Rect(100, 80, 40, 20)
            };
            var dragStart = new Point(50, 50);
            var currentPoint = new Point(70, 80); // Delta: +20, +30

            // Act
            var results = DrawingElementEditingEngine.CalculateGroupMove(originalBounds, dragStart, currentPoint);

            // Assert
            results.Count.Should().Be(2);
            results[0].NewX.Should().BeApproximately(30, Tolerance); // 10 + 20
            results[0].NewY.Should().BeApproximately(50, Tolerance); // 20 + 30
            results[1].NewX.Should().BeApproximately(120, Tolerance); // 100 + 20
            results[1].NewY.Should().BeApproximately(110, Tolerance); // 80 + 30
        }

        [Fact]
        public void CreateGroupResizeContext_CapturesAllElementSnapshots()
        {
            // Arrange
            var elements = new List<DrawingElement>
            {
                new ActionElement { X = 10, Y = 10, Width = 50, Height = 30 },
                new ActionElement { X = 100, Y = 80, Width = 40, Height = 20 }
            };
            var startPoint = new Point(140, 100);

            // Act
            var context = DrawingElementEditingEngine.CreateGroupResizeContext(
                elements,
                ResizeHandleType.BottomRight,
                startPoint);

            // Assert
            context.ElementSnapshots.Count.Should().Be(2);
            context.OriginalGroupBounds.X.Should().BeApproximately(10, Tolerance);
            context.OriginalGroupBounds.Y.Should().BeApproximately(10, Tolerance);
            context.InitialHandle.Should().Be(ResizeHandleType.BottomRight);
            context.OriginalStartPoint.Should().Be(startPoint);
        }

        [Fact]
        public void CalculateGroupResize_ScalesProportionally()
        {
            // Arrange
            var elements = new List<DrawingElement>
            {
                new ActionElement { X = 0, Y = 0, Width = 100, Height = 100 },
                new ActionElement { X = 100, Y = 0, Width = 100, Height = 100 }
            };
            // Group bounds: (0, 0, 200, 100)
            var context = DrawingElementEditingEngine.CreateGroupResizeContext(
                elements,
                ResizeHandleType.BottomRight,
                new Point(200, 100));

            // Act - Scale to 400x200 (2x both axes)
            var result = DrawingElementEditingEngine.CalculateGroupResize(
                context,
                new Point(400, 200),
                maintainAspectRatio: false);

            // Assert
            result.Transforms.Count.Should().Be(2);

            // First element: (0,0,100,100) -> (0,0,200,200)
            result.Transforms[0].NewX.Should().BeApproximately(0, Tolerance);
            result.Transforms[0].NewY.Should().BeApproximately(0, Tolerance);
            result.Transforms[0].NewWidth.Should().BeApproximately(200, Tolerance);
            result.Transforms[0].NewHeight.Should().BeApproximately(200, Tolerance);

            // Second element: (100,0,100,100) -> (200,0,200,200)
            result.Transforms[1].NewX.Should().BeApproximately(200, Tolerance);
            result.Transforms[1].NewY.Should().BeApproximately(0, Tolerance);
            result.Transforms[1].NewWidth.Should().BeApproximately(200, Tolerance);
            result.Transforms[1].NewHeight.Should().BeApproximately(200, Tolerance);
        }

        [Fact]
        public void CalculateGroupResize_WithShift_MaintainsAspectRatio()
        {
            // Arrange
            var elements = new List<DrawingElement>
            {
                new ActionElement { X = 0, Y = 0, Width = 100, Height = 100 }
            };
            // Group bounds: (0, 0, 100, 100)
            var context = DrawingElementEditingEngine.CreateGroupResizeContext(
                elements,
                ResizeHandleType.BottomRight,
                new Point(100, 100));

            // Act - Drag more in X than Y, with aspect ratio lock
            var result = DrawingElementEditingEngine.CalculateGroupResize(
                context,
                new Point(200, 120), // Would be 2x in X, 1.2x in Y
                maintainAspectRatio: true);

            // Assert - Should use the larger scale (2x) for both
            result.Transforms[0].NewWidth.Should().BeApproximately(200, Tolerance);
            result.Transforms[0].NewHeight.Should().BeApproximately(200, Tolerance);
        }

        [Fact]
        public void CalculateGroupResize_EdgeHandle_ScalesSingleAxis()
        {
            // Arrange
            var elements = new List<DrawingElement>
            {
                new ActionElement { X = 0, Y = 0, Width = 100, Height = 100 }
            };
            var context = DrawingElementEditingEngine.CreateGroupResizeContext(
                elements,
                ResizeHandleType.Right, // Edge handle - only scales X
                new Point(100, 50));

            // Act - Drag right edge from 100 to 200
            var result = DrawingElementEditingEngine.CalculateGroupResize(
                context,
                new Point(200, 50),
                maintainAspectRatio: false);

            // Assert - Width doubled, height unchanged
            result.Transforms[0].NewWidth.Should().BeApproximately(200, Tolerance);
            result.Transforms[0].NewHeight.Should().BeApproximately(100, Tolerance);
        }

        [Fact]
        public void CalculateGroupResize_TopHandle_ScalesSingleAxis()
        {
            // Arrange
            var elements = new List<DrawingElement>
            {
                new ActionElement { X = 0, Y = 0, Width = 100, Height = 100 }
            };
            var context = DrawingElementEditingEngine.CreateGroupResizeContext(
                elements,
                ResizeHandleType.Bottom, // Edge handle - only scales Y
                new Point(50, 100));

            // Act - Drag bottom edge from 100 to 200
            var result = DrawingElementEditingEngine.CalculateGroupResize(
                context,
                new Point(50, 200),
                maintainAspectRatio: false);

            // Assert - Height doubled, width unchanged
            result.Transforms[0].NewWidth.Should().BeApproximately(100, Tolerance);
            result.Transforms[0].NewHeight.Should().BeApproximately(200, Tolerance);
        }

        [Fact]
        public void CalculateGroupResize_PreservesRelativePositions()
        {
            // Arrange - Two elements with 50px gap
            var elements = new List<DrawingElement>
            {
                new ActionElement { X = 0, Y = 0, Width = 50, Height = 50 },
                new ActionElement { X = 100, Y = 0, Width = 50, Height = 50 } // 50px gap
            };
            // Group bounds: (0, 0, 150, 50)
            var context = DrawingElementEditingEngine.CreateGroupResizeContext(
                elements,
                ResizeHandleType.BottomRight,
                new Point(150, 50));

            // Act - Double the size
            var result = DrawingElementEditingEngine.CalculateGroupResize(
                context,
                new Point(300, 100),
                maintainAspectRatio: false);

            // Assert - Gap should also double (from 50px to 100px)
            var gap = result.Transforms[1].NewX - (result.Transforms[0].NewX + result.Transforms[0].NewWidth);
            gap.Should().BeApproximately(100, Tolerance); // 50 * 2 = 100
        }

        #endregion
    }
}