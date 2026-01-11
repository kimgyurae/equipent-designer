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

        #region Multi-Selection Edge Resize with Aspect Ratio Tests (Bug Fix TDD)

        /// <summary>
        /// API-Level Test: Top edge handle with Shift should scale both width and height proportionally.
        /// BUG: Currently only height changes, width remains unchanged.
        /// </summary>
        [Fact]
        public void CalculateGroupResize_TopHandle_WithShift_ScalesBothDimensions()
        {
            // Arrange - Group with 2:1 aspect ratio
            var elements = new List<DrawingElement>
            {
                new ActionElement { X = 0, Y = 0, Width = 100, Height = 100 }
            };
            // Group bounds: (0, 0, 100, 100) - 1:1 aspect ratio
            var context = DrawingElementEditingEngine.CreateGroupResizeContext(
                elements,
                ResizeHandleType.Top,
                new Point(50, 0)); // Top edge center

            // Act - Drag top edge upward by 50 (increase height from 100 to 150)
            // With Shift, width should also increase proportionally
            var result = DrawingElementEditingEngine.CalculateGroupResize(
                context,
                new Point(50, -50),
                maintainAspectRatio: true);

            // Assert - Both dimensions should scale by same factor (1.5x)
            result.Transforms[0].NewWidth.Should().BeApproximately(150, Tolerance,
                "Width should scale proportionally when using Top handle with Shift");
            result.Transforms[0].NewHeight.Should().BeApproximately(150, Tolerance,
                "Height should increase when dragging Top handle upward");
        }

        /// <summary>
        /// API-Level Test: Bottom edge handle with Shift should scale both dimensions.
        /// </summary>
        [Fact]
        public void CalculateGroupResize_BottomHandle_WithShift_ScalesBothDimensions()
        {
            // Arrange
            var elements = new List<DrawingElement>
            {
                new ActionElement { X = 0, Y = 0, Width = 100, Height = 100 }
            };
            var context = DrawingElementEditingEngine.CreateGroupResizeContext(
                elements,
                ResizeHandleType.Bottom,
                new Point(50, 100)); // Bottom edge center

            // Act - Drag bottom edge downward by 50
            var result = DrawingElementEditingEngine.CalculateGroupResize(
                context,
                new Point(50, 150),
                maintainAspectRatio: true);

            // Assert
            result.Transforms[0].NewWidth.Should().BeApproximately(150, Tolerance,
                "Width should scale proportionally when using Bottom handle with Shift");
            result.Transforms[0].NewHeight.Should().BeApproximately(150, Tolerance,
                "Height should increase when dragging Bottom handle downward");
        }

        /// <summary>
        /// API-Level Test: Left edge handle with Shift should scale both dimensions.
        /// </summary>
        [Fact]
        public void CalculateGroupResize_LeftHandle_WithShift_ScalesBothDimensions()
        {
            // Arrange
            var elements = new List<DrawingElement>
            {
                new ActionElement { X = 0, Y = 0, Width = 100, Height = 100 }
            };
            var context = DrawingElementEditingEngine.CreateGroupResizeContext(
                elements,
                ResizeHandleType.Left,
                new Point(0, 50)); // Left edge center

            // Act - Drag left edge leftward by 50 (increase width from 100 to 150)
            var result = DrawingElementEditingEngine.CalculateGroupResize(
                context,
                new Point(-50, 50),
                maintainAspectRatio: true);

            // Assert
            result.Transforms[0].NewWidth.Should().BeApproximately(150, Tolerance,
                "Width should increase when dragging Left handle leftward");
            result.Transforms[0].NewHeight.Should().BeApproximately(150, Tolerance,
                "Height should scale proportionally when using Left handle with Shift");
        }

        /// <summary>
        /// API-Level Test: Right edge handle with Shift should scale both dimensions.
        /// </summary>
        [Fact]
        public void CalculateGroupResize_RightHandle_WithShift_ScalesBothDimensions()
        {
            // Arrange
            var elements = new List<DrawingElement>
            {
                new ActionElement { X = 0, Y = 0, Width = 100, Height = 100 }
            };
            var context = DrawingElementEditingEngine.CreateGroupResizeContext(
                elements,
                ResizeHandleType.Right,
                new Point(100, 50)); // Right edge center

            // Act - Drag right edge rightward by 50
            var result = DrawingElementEditingEngine.CalculateGroupResize(
                context,
                new Point(150, 50),
                maintainAspectRatio: true);

            // Assert
            result.Transforms[0].NewWidth.Should().BeApproximately(150, Tolerance,
                "Width should increase when dragging Right handle rightward");
            result.Transforms[0].NewHeight.Should().BeApproximately(150, Tolerance,
                "Height should scale proportionally when using Right handle with Shift");
        }

        /// <summary>
        /// Replication Test: Verifies scale factors are equal for edge handles with maintainAspectRatio.
        /// This directly tests the root cause - scaleX and scaleY should be equal.
        /// </summary>
        [Theory]
        [InlineData(ResizeHandleType.Top)]
        [InlineData(ResizeHandleType.Bottom)]
        [InlineData(ResizeHandleType.Left)]
        [InlineData(ResizeHandleType.Right)]
        public void CalculateGroupResize_EdgeHandle_WithShift_AppliesUniformScale(ResizeHandleType handle)
        {
            // Arrange - Non-square group to clearly see aspect ratio changes
            var elements = new List<DrawingElement>
            {
                new ActionElement { X = 0, Y = 0, Width = 200, Height = 100 }
            };
            // Group bounds: (0, 0, 200, 100) - 2:1 aspect ratio
            var startPoint = handle switch
            {
                ResizeHandleType.Top => new Point(100, 0),
                ResizeHandleType.Bottom => new Point(100, 100),
                ResizeHandleType.Left => new Point(0, 50),
                ResizeHandleType.Right => new Point(200, 50),
                _ => new Point(0, 0)
            };
            var context = DrawingElementEditingEngine.CreateGroupResizeContext(
                elements, handle, startPoint);

            // Act - Move the handle to scale by 1.5x in primary dimension
            var currentPoint = handle switch
            {
                ResizeHandleType.Top => new Point(100, -50),    // Height 100->150
                ResizeHandleType.Bottom => new Point(100, 150), // Height 100->150
                ResizeHandleType.Left => new Point(-100, 50),   // Width 200->300
                ResizeHandleType.Right => new Point(300, 50),   // Width 200->300
                _ => new Point(0, 0)
            };
            var result = DrawingElementEditingEngine.CalculateGroupResize(
                context, currentPoint, maintainAspectRatio: true);

            // Assert - Aspect ratio should be preserved (2:1)
            var originalAspectRatio = 200.0 / 100.0;
            var newAspectRatio = result.Transforms[0].NewWidth / result.Transforms[0].NewHeight;
            newAspectRatio.Should().BeApproximately(originalAspectRatio, Tolerance,
                $"Aspect ratio should be preserved when using {handle} handle with Shift");
        }

        /// <summary>
        /// Replication Test: Multiple elements should all scale proportionally with edge handle + Shift.
        /// </summary>
        [Fact]
        public void CalculateGroupResize_TopHandle_WithShift_ScalesAllElementsProportionally()
        {
            // Arrange - Two elements
            var elements = new List<DrawingElement>
            {
                new ActionElement { X = 0, Y = 0, Width = 100, Height = 50 },
                new ActionElement { X = 100, Y = 50, Width = 100, Height = 50 }
            };
            // Group bounds: (0, 0, 200, 100)
            var context = DrawingElementEditingEngine.CreateGroupResizeContext(
                elements,
                ResizeHandleType.Top,
                new Point(100, 0));

            // Act - Scale height from 100 to 150 (1.5x)
            var result = DrawingElementEditingEngine.CalculateGroupResize(
                context,
                new Point(100, -50),
                maintainAspectRatio: true);

            // Assert - Both elements should scale by 1.5x in both dimensions
            result.Transforms.Count.Should().Be(2);

            // First element: (0, 0, 100, 50) -> should become (x, y, 150, 75)
            result.Transforms[0].NewWidth.Should().BeApproximately(150, Tolerance);
            result.Transforms[0].NewHeight.Should().BeApproximately(75, Tolerance);

            // Second element: (100, 50, 100, 50) -> should become (x, y, 150, 75)
            result.Transforms[1].NewWidth.Should().BeApproximately(150, Tolerance);
            result.Transforms[1].NewHeight.Should().BeApproximately(75, Tolerance);
        }

        /// <summary>
        /// Regression Test: Corner handles should continue working correctly with Shift.
        /// </summary>
        [Theory]
        [InlineData(ResizeHandleType.TopLeft)]
        [InlineData(ResizeHandleType.TopRight)]
        [InlineData(ResizeHandleType.BottomLeft)]
        [InlineData(ResizeHandleType.BottomRight)]
        public void CalculateGroupResize_CornerHandle_WithShift_ContinuesToWork(ResizeHandleType handle)
        {
            // Arrange
            var elements = new List<DrawingElement>
            {
                new ActionElement { X = 0, Y = 0, Width = 200, Height = 100 }
            };
            var startPoint = handle switch
            {
                ResizeHandleType.TopLeft => new Point(0, 0),
                ResizeHandleType.TopRight => new Point(200, 0),
                ResizeHandleType.BottomLeft => new Point(0, 100),
                ResizeHandleType.BottomRight => new Point(200, 100),
                _ => new Point(0, 0)
            };
            var context = DrawingElementEditingEngine.CreateGroupResizeContext(
                elements, handle, startPoint);

            // Act - Scale by dragging the corner
            var currentPoint = handle switch
            {
                ResizeHandleType.TopLeft => new Point(-100, -50),
                ResizeHandleType.TopRight => new Point(300, -50),
                ResizeHandleType.BottomLeft => new Point(-100, 150),
                ResizeHandleType.BottomRight => new Point(300, 150),
                _ => new Point(0, 0)
            };
            var result = DrawingElementEditingEngine.CalculateGroupResize(
                context, currentPoint, maintainAspectRatio: true);

            // Assert - Aspect ratio should be preserved
            var originalAspectRatio = 200.0 / 100.0;
            var newAspectRatio = result.Transforms[0].NewWidth / result.Transforms[0].NewHeight;
            newAspectRatio.Should().BeApproximately(originalAspectRatio, Tolerance,
                $"Aspect ratio should be preserved when using {handle} handle with Shift");
        }

        /// <summary>
        /// Position Test: Top handle with Shift should anchor bottom edge and center width.
        /// </summary>
        [Fact]
        public void CalculateGroupResize_TopHandle_WithShift_AnchorsBottomAndCentersWidth()
        {
            // Arrange
            var elements = new List<DrawingElement>
            {
                new ActionElement { X = 0, Y = 0, Width = 100, Height = 100 }
            };
            var context = DrawingElementEditingEngine.CreateGroupResizeContext(
                elements,
                ResizeHandleType.Top,
                new Point(50, 0));
            var originalBottom = 100.0;
            var originalCenterX = 50.0;

            // Act - Drag top up by 50
            var result = DrawingElementEditingEngine.CalculateGroupResize(
                context,
                new Point(50, -50),
                maintainAspectRatio: true);

            // Assert
            var newBottom = result.NewGroupBounds.Bottom;
            var newCenterX = result.NewGroupBounds.Left + result.NewGroupBounds.Width / 2;

            newBottom.Should().BeApproximately(originalBottom, Tolerance,
                "Bottom edge should remain anchored when resizing from Top handle");
            newCenterX.Should().BeApproximately(originalCenterX, Tolerance,
                "Horizontal center should remain the same when resizing from Top handle with Shift");
        }

        #endregion
    }
}