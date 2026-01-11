using System.Windows;
using FluentAssertions;
using EquipmentDesigner.Views.Drawboard.UMLEngine;
using EquipmentDesigner.Views.Drawboard.UMLEngine.Contexts;
using Xunit;

namespace EquipmentDesigner.Tests.Views.Drawboard.UMLEngine
{
    /// <summary>
    /// TDD tests for ZoomControlEngine - stateless calculation engine
    /// for zoom operations.
    /// </summary>
    public class ZoomControlEngineTests
    {
        private const double Tolerance = 0.001;

        #region Constants Tests

        [Fact]
        public void MinZoom_ShouldBe10()
        {
            ZoomControlEngine.MinZoom.Should().Be(10);
        }

        [Fact]
        public void MaxZoom_ShouldBe3000()
        {
            ZoomControlEngine.MaxZoom.Should().Be(3000);
        }

        [Fact]
        public void LinearZoomStep_ShouldBe10()
        {
            ZoomControlEngine.LinearZoomStep.Should().Be(10);
        }

        [Fact]
        public void LinearZoomThreshold_ShouldBe100()
        {
            ZoomControlEngine.LinearZoomThreshold.Should().Be(100);
        }

        [Fact]
        public void ScrollZoomFactor_ShouldBe1Point1()
        {
            ZoomControlEngine.ScrollZoomFactor.Should().BeApproximately(1.1, Tolerance);
        }

        #endregion

        #region ClampZoomLevel Tests

        [Theory]
        [InlineData(50)]
        [InlineData(100)]
        [InlineData(500)]
        [InlineData(1500)]
        public void ClampZoomLevel_WithValueWithinRange_ReturnsOriginalValue(int value)
        {
            var result = ZoomControlEngine.ClampZoomLevel(value);
            result.Should().Be(value);
        }

        [Theory]
        [InlineData(5)]
        [InlineData(0)]
        [InlineData(-100)]
        public void ClampZoomLevel_WithValueBelowMin_ReturnsMinZoom(int value)
        {
            var result = ZoomControlEngine.ClampZoomLevel(value);
            result.Should().Be(ZoomControlEngine.MinZoom);
        }

        [Theory]
        [InlineData(3001)]
        [InlineData(5000)]
        [InlineData(10000)]
        public void ClampZoomLevel_WithValueAboveMax_ReturnsMaxZoom(int value)
        {
            var result = ZoomControlEngine.ClampZoomLevel(value);
            result.Should().Be(ZoomControlEngine.MaxZoom);
        }

        [Fact]
        public void ClampZoomLevel_WithExactMinValue_ReturnsMinZoom()
        {
            var result = ZoomControlEngine.ClampZoomLevel(10);
            result.Should().Be(10);
        }

        [Fact]
        public void ClampZoomLevel_WithExactMaxValue_ReturnsMaxZoom()
        {
            var result = ZoomControlEngine.ClampZoomLevel(3000);
            result.Should().Be(3000);
        }

        #endregion

        #region CalculateZoomScale Tests

        [Fact]
        public void CalculateZoomScale_At100Percent_Returns1Point0()
        {
            var result = ZoomControlEngine.CalculateZoomScale(100);
            result.Should().BeApproximately(1.0, Tolerance);
        }

        [Fact]
        public void CalculateZoomScale_At50Percent_Returns0Point5()
        {
            var result = ZoomControlEngine.CalculateZoomScale(50);
            result.Should().BeApproximately(0.5, Tolerance);
        }

        [Fact]
        public void CalculateZoomScale_At200Percent_Returns2Point0()
        {
            var result = ZoomControlEngine.CalculateZoomScale(200);
            result.Should().BeApproximately(2.0, Tolerance);
        }

        [Fact]
        public void CalculateZoomScale_At10Percent_Returns0Point1()
        {
            var result = ZoomControlEngine.CalculateZoomScale(10);
            result.Should().BeApproximately(0.1, Tolerance);
        }

        [Fact]
        public void CalculateZoomScale_At3000Percent_Returns30Point0()
        {
            var result = ZoomControlEngine.CalculateZoomScale(3000);
            result.Should().BeApproximately(30.0, Tolerance);
        }

        #endregion

        #region Linear Zoom Calculation Tests

        [Fact]
        public void CalculateLinearZoomIn_From50_Returns60()
        {
            var result = ZoomControlEngine.CalculateLinearZoomIn(50);
            result.Should().Be(60);
        }

        [Fact]
        public void CalculateLinearZoomIn_From90_Returns100()
        {
            var result = ZoomControlEngine.CalculateLinearZoomIn(90);
            result.Should().Be(100);
        }

        [Fact]
        public void CalculateLinearZoomIn_From100_Returns110()
        {
            var result = ZoomControlEngine.CalculateLinearZoomIn(100);
            result.Should().Be(110);
        }

        [Fact]
        public void CalculateLinearZoomIn_AtMaxZoom_ReturnsMaxZoom()
        {
            var result = ZoomControlEngine.CalculateLinearZoomIn(3000);
            result.Should().Be(3000);
        }

        [Fact]
        public void CalculateLinearZoomIn_NearMaxZoom_ClampsToMaxZoom()
        {
            var result = ZoomControlEngine.CalculateLinearZoomIn(2995);
            result.Should().Be(3000);
        }

        [Fact]
        public void CalculateLinearZoomOut_From50_Returns40()
        {
            var result = ZoomControlEngine.CalculateLinearZoomOut(50);
            result.Should().Be(40);
        }

        [Fact]
        public void CalculateLinearZoomOut_From20_Returns10()
        {
            var result = ZoomControlEngine.CalculateLinearZoomOut(20);
            result.Should().Be(10);
        }

        [Fact]
        public void CalculateLinearZoomOut_AtMinZoom_ReturnsMinZoom()
        {
            var result = ZoomControlEngine.CalculateLinearZoomOut(10);
            result.Should().Be(10);
        }

        [Fact]
        public void CalculateLinearZoomOut_NearMinZoom_ClampsToMinZoom()
        {
            var result = ZoomControlEngine.CalculateLinearZoomOut(15);
            result.Should().Be(10);
        }

        #endregion

        #region Exponential Zoom Calculation Tests

        [Fact]
        public void CalculateExponentialZoomIn_From100_ReturnsApprox110()
        {
            var result = ZoomControlEngine.CalculateExponentialZoomIn(100);
            result.Should().Be(110); // 100 * 1.1 = 110
        }

        [Fact]
        public void CalculateExponentialZoomIn_From200_ReturnsApprox220()
        {
            var result = ZoomControlEngine.CalculateExponentialZoomIn(200);
            result.Should().Be(220); // 200 * 1.1 = 220
        }

        [Fact]
        public void CalculateExponentialZoomIn_From1000_ReturnsApprox1100()
        {
            var result = ZoomControlEngine.CalculateExponentialZoomIn(1000);
            result.Should().Be(1100); // 1000 * 1.1 = 1100
        }

        [Fact]
        public void CalculateExponentialZoomIn_AtMaxZoom_ReturnsMaxZoom()
        {
            var result = ZoomControlEngine.CalculateExponentialZoomIn(3000);
            result.Should().Be(3000);
        }

        [Fact]
        public void CalculateExponentialZoomIn_NearMaxZoom_ClampsToMaxZoom()
        {
            var result = ZoomControlEngine.CalculateExponentialZoomIn(2800);
            result.Should().Be(3000); // 2800 * 1.1 = 3080, clamped to 3000
        }

        [Fact]
        public void CalculateExponentialZoomOut_From200_ReturnsApprox182()
        {
            var result = ZoomControlEngine.CalculateExponentialZoomOut(200);
            result.Should().Be(182); // 200 / 1.1 ≈ 181.8, rounded to 182
        }

        [Fact]
        public void CalculateExponentialZoomOut_From1000_ReturnsApprox909()
        {
            var result = ZoomControlEngine.CalculateExponentialZoomOut(1000);
            result.Should().Be(909); // 1000 / 1.1 ≈ 909.09, rounded to 909
        }

        [Fact]
        public void CalculateExponentialZoomOut_NearThreshold_ClampsToMin()
        {
            var result = ZoomControlEngine.CalculateExponentialZoomOut(11);
            result.Should().Be(10); // 11 / 1.1 = 10
        }

        [Fact]
        public void CalculateExponentialZoomOut_ResultBelowMin_ReturnsMinZoom()
        {
            var result = ZoomControlEngine.CalculateExponentialZoomOut(10);
            result.Should().Be(10); // Can't go below 10
        }

        #endregion

        #region Mouse Wheel Zoom Tests

        [Fact]
        public void CalculateMouseWheelZoom_ZoomIn_BelowThreshold_UsesLinearStep()
        {
            // Arrange
            var context = new ZoomContext(
                currentZoomLevel: 50,
                mousePosition: new Point(100, 100),
                scrollOffset: new Point(0, 0),
                viewportSize: new Size(800, 600));

            // Act
            var result = ZoomControlEngine.CalculateMouseWheelZoom(context, zoomIn: true);

            // Assert
            result.NewZoomLevel.Should().Be(60); // Linear: 50 + 10 = 60
        }

        [Fact]
        public void CalculateMouseWheelZoom_ZoomIn_AboveThreshold_UsesExponentialFactor()
        {
            // Arrange
            var context = new ZoomContext(
                currentZoomLevel: 200,
                mousePosition: new Point(100, 100),
                scrollOffset: new Point(0, 0),
                viewportSize: new Size(800, 600));

            // Act
            var result = ZoomControlEngine.CalculateMouseWheelZoom(context, zoomIn: true);

            // Assert
            result.NewZoomLevel.Should().Be(220); // Exponential: 200 * 1.1 = 220
        }

        [Fact]
        public void CalculateMouseWheelZoom_ZoomIn_AtThreshold_UsesExponentialFactor()
        {
            // Arrange
            var context = new ZoomContext(
                currentZoomLevel: 100,
                mousePosition: new Point(100, 100),
                scrollOffset: new Point(0, 0),
                viewportSize: new Size(800, 600));

            // Act
            var result = ZoomControlEngine.CalculateMouseWheelZoom(context, zoomIn: true);

            // Assert
            result.NewZoomLevel.Should().Be(110); // At threshold, uses exponential: 100 * 1.1 = 110
        }

        [Fact]
        public void CalculateMouseWheelZoom_ZoomOut_BelowThreshold_UsesLinearStep()
        {
            // Arrange
            var context = new ZoomContext(
                currentZoomLevel: 50,
                mousePosition: new Point(100, 100),
                scrollOffset: new Point(0, 0),
                viewportSize: new Size(800, 600));

            // Act
            var result = ZoomControlEngine.CalculateMouseWheelZoom(context, zoomIn: false);

            // Assert
            result.NewZoomLevel.Should().Be(40); // Linear: 50 - 10 = 40
        }

        [Fact]
        public void CalculateMouseWheelZoom_ZoomOut_AboveThreshold_UsesExponentialFactor()
        {
            // Arrange
            var context = new ZoomContext(
                currentZoomLevel: 200,
                mousePosition: new Point(100, 100),
                scrollOffset: new Point(0, 0),
                viewportSize: new Size(800, 600));

            // Act
            var result = ZoomControlEngine.CalculateMouseWheelZoom(context, zoomIn: false);

            // Assert
            result.NewZoomLevel.Should().Be(182); // Exponential: 200 / 1.1 ≈ 182
        }

        [Fact]
        public void CalculateMouseWheelZoom_ZoomOut_AtThreshold_UsesLinearStep()
        {
            // Arrange
            var context = new ZoomContext(
                currentZoomLevel: 100,
                mousePosition: new Point(100, 100),
                scrollOffset: new Point(0, 0),
                viewportSize: new Size(800, 600));

            // Act
            var result = ZoomControlEngine.CalculateMouseWheelZoom(context, zoomIn: false);

            // Assert
            result.NewZoomLevel.Should().Be(90); // At threshold, uses linear: 100 - 10 = 90
        }

        [Fact]
        public void CalculateMouseWheelZoom_MaintainsMousePositionOnZoomIn()
        {
            // Arrange: Mouse at viewport position (100, 100), scroll at (50, 50), zoom 100%
            var context = new ZoomContext(
                currentZoomLevel: 100,
                mousePosition: new Point(100, 100),
                scrollOffset: new Point(50, 50),
                viewportSize: new Size(800, 600));

            // Act: Zoom in from 100% to 110%
            var result = ZoomControlEngine.CalculateMouseWheelZoom(context, zoomIn: true);

            // Assert: Content coordinate under mouse should remain same after zoom
            // Old scale = 1.0, New scale = 1.1
            // Content X under mouse = (50 + 100) / 1.0 = 150
            // New scroll X = 150 * 1.1 - 100 = 165 - 100 = 65
            // Content Y under mouse = (50 + 100) / 1.0 = 150
            // New scroll Y = 150 * 1.1 - 100 = 165 - 100 = 65
            result.NewScrollOffset.X.Should().BeApproximately(65, Tolerance);
            result.NewScrollOffset.Y.Should().BeApproximately(65, Tolerance);
        }

        [Fact]
        public void CalculateMouseWheelZoom_MaintainsMousePositionOnZoomOut()
        {
            // Arrange: Mouse at viewport position (100, 100), scroll at (100, 100), zoom 200%
            var context = new ZoomContext(
                currentZoomLevel: 200,
                mousePosition: new Point(100, 100),
                scrollOffset: new Point(100, 100),
                viewportSize: new Size(800, 600));

            // Act: Zoom out from 200% to ~182%
            var result = ZoomControlEngine.CalculateMouseWheelZoom(context, zoomIn: false);

            // Assert: Content coordinate under mouse should remain same after zoom
            // Old scale = 2.0, New scale = 1.82
            // Content X under mouse = (100 + 100) / 2.0 = 100
            // New scroll X = 100 * 1.82 - 100 = 182 - 100 = 82
            double newScale = result.NewZoomLevel / 100.0;
            double expectedX = 100 * newScale - 100;
            result.NewScrollOffset.X.Should().BeApproximately(expectedX, Tolerance);
        }

        [Fact]
        public void CalculateMouseWheelZoom_AtMaxZoom_ZoomChanged_ReturnsFalse()
        {
            // Arrange
            var context = new ZoomContext(
                currentZoomLevel: 3000,
                mousePosition: new Point(100, 100),
                scrollOffset: new Point(0, 0),
                viewportSize: new Size(800, 600));

            // Act
            var result = ZoomControlEngine.CalculateMouseWheelZoom(context, zoomIn: true);

            // Assert
            result.ZoomChanged.Should().BeFalse();
            result.NewZoomLevel.Should().Be(3000);
        }

        [Fact]
        public void CalculateMouseWheelZoom_AtMinZoom_ZoomChanged_ReturnsFalse()
        {
            // Arrange
            var context = new ZoomContext(
                currentZoomLevel: 10,
                mousePosition: new Point(100, 100),
                scrollOffset: new Point(0, 0),
                viewportSize: new Size(800, 600));

            // Act
            var result = ZoomControlEngine.CalculateMouseWheelZoom(context, zoomIn: false);

            // Assert
            result.ZoomChanged.Should().BeFalse();
            result.NewZoomLevel.Should().Be(10);
        }

        #endregion

        #region Viewport Center Zoom Tests

        [Fact]
        public void CalculateViewportCenterZoom_ZoomIn_UsesLinearStepAlways()
        {
            // Arrange: Even above threshold, button zoom uses linear
            var context = new ZoomContext(
                currentZoomLevel: 200,
                mousePosition: new Point(0, 0), // Not used for center zoom
                scrollOffset: new Point(100, 100),
                viewportSize: new Size(800, 600));

            // Act
            var result = ZoomControlEngine.CalculateViewportCenterZoom(context, zoomIn: true);

            // Assert
            result.NewZoomLevel.Should().Be(210); // Linear: 200 + 10 = 210
        }

        [Fact]
        public void CalculateViewportCenterZoom_ZoomOut_UsesLinearStepAlways()
        {
            // Arrange
            var context = new ZoomContext(
                currentZoomLevel: 200,
                mousePosition: new Point(0, 0),
                scrollOffset: new Point(100, 100),
                viewportSize: new Size(800, 600));

            // Act
            var result = ZoomControlEngine.CalculateViewportCenterZoom(context, zoomIn: false);

            // Assert
            result.NewZoomLevel.Should().Be(190); // Linear: 200 - 10 = 190
        }

        [Fact]
        public void CalculateViewportCenterZoom_MaintainsViewportCenterOnZoomIn()
        {
            // Arrange: Viewport 800x600, scroll at (100, 100), zoom 100%
            var context = new ZoomContext(
                currentZoomLevel: 100,
                mousePosition: new Point(0, 0),
                scrollOffset: new Point(100, 100),
                viewportSize: new Size(800, 600));

            // Act: Zoom in from 100% to 110%
            var result = ZoomControlEngine.CalculateViewportCenterZoom(context, zoomIn: true);

            // Assert: Viewport center should remain same content coordinate
            // Old scale = 1.0, New scale = 1.1
            // Viewport center X = 800 / 2 = 400
            // Content center X = (100 + 400) / 1.0 = 500
            // New scroll X = 500 * 1.1 - 400 = 550 - 400 = 150
            // Content center Y = (100 + 300) / 1.0 = 400
            // New scroll Y = 400 * 1.1 - 300 = 440 - 300 = 140
            result.NewScrollOffset.X.Should().BeApproximately(150, Tolerance);
            result.NewScrollOffset.Y.Should().BeApproximately(140, Tolerance);
        }

        [Fact]
        public void CalculateViewportCenterZoom_MaintainsViewportCenterOnZoomOut()
        {
            // Arrange
            var context = new ZoomContext(
                currentZoomLevel: 200,
                mousePosition: new Point(0, 0),
                scrollOffset: new Point(200, 200),
                viewportSize: new Size(800, 600));

            // Act: Zoom out from 200% to 190%
            var result = ZoomControlEngine.CalculateViewportCenterZoom(context, zoomIn: false);

            // Assert
            // Old scale = 2.0, New scale = 1.9
            // Content center X = (200 + 400) / 2.0 = 300
            // New scroll X = 300 * 1.9 - 400 = 570 - 400 = 170
            // Content center Y = (200 + 300) / 2.0 = 250
            // New scroll Y = 250 * 1.9 - 300 = 475 - 300 = 175
            result.NewScrollOffset.X.Should().BeApproximately(170, Tolerance);
            result.NewScrollOffset.Y.Should().BeApproximately(175, Tolerance);
        }

        [Fact]
        public void CalculateViewportCenterZoom_AtMaxZoom_ZoomChanged_ReturnsFalse()
        {
            // Arrange
            var context = new ZoomContext(
                currentZoomLevel: 3000,
                mousePosition: new Point(0, 0),
                scrollOffset: new Point(0, 0),
                viewportSize: new Size(800, 600));

            // Act
            var result = ZoomControlEngine.CalculateViewportCenterZoom(context, zoomIn: true);

            // Assert
            result.ZoomChanged.Should().BeFalse();
            result.NewZoomLevel.Should().Be(3000);
        }

        [Fact]
        public void CalculateViewportCenterZoom_AtMinZoom_ZoomChanged_ReturnsFalse()
        {
            // Arrange
            var context = new ZoomContext(
                currentZoomLevel: 10,
                mousePosition: new Point(0, 0),
                scrollOffset: new Point(0, 0),
                viewportSize: new Size(800, 600));

            // Act
            var result = ZoomControlEngine.CalculateViewportCenterZoom(context, zoomIn: false);

            // Assert
            result.ZoomChanged.Should().BeFalse();
            result.NewZoomLevel.Should().Be(10);
        }

        #endregion

        #region ZoomContext Tests

        [Fact]
        public void CreateZoomContext_WithValidInputs_ReturnsCorrectContext()
        {
            // Act
            var context = ZoomControlEngine.CreateZoomContext(
                currentZoomLevel: 150,
                mousePosition: new Point(100, 200),
                scrollOffset: new Point(50, 75),
                viewportSize: new Size(800, 600));

            // Assert
            context.CurrentZoomLevel.Should().Be(150);
            context.MousePosition.Should().Be(new Point(100, 200));
            context.ScrollOffset.Should().Be(new Point(50, 75));
            context.ViewportSize.Should().Be(new Size(800, 600));
        }

        [Fact]
        public void ZoomContext_CalculatesCurrentZoomScale_Correctly()
        {
            // Arrange
            var context = new ZoomContext(
                currentZoomLevel: 150,
                mousePosition: new Point(0, 0),
                scrollOffset: new Point(0, 0),
                viewportSize: new Size(800, 600));

            // Assert
            context.CurrentZoomScale.Should().BeApproximately(1.5, Tolerance);
        }

        #endregion

        #region ZoomResult Tests

        [Fact]
        public void ZoomResult_WithZoomChange_ZoomChanged_ReturnsTrue()
        {
            // Arrange
            var context = new ZoomContext(
                currentZoomLevel: 100,
                mousePosition: new Point(100, 100),
                scrollOffset: new Point(0, 0),
                viewportSize: new Size(800, 600));

            // Act
            var result = ZoomControlEngine.CalculateMouseWheelZoom(context, zoomIn: true);

            // Assert
            result.ZoomChanged.Should().BeTrue();
        }

        [Fact]
        public void ZoomResult_NewZoomScale_MatchesNewZoomLevel()
        {
            // Arrange
            var context = new ZoomContext(
                currentZoomLevel: 100,
                mousePosition: new Point(100, 100),
                scrollOffset: new Point(0, 0),
                viewportSize: new Size(800, 600));

            // Act
            var result = ZoomControlEngine.CalculateMouseWheelZoom(context, zoomIn: true);

            // Assert
            double expectedScale = result.NewZoomLevel / 100.0;
            result.NewZoomScale.Should().BeApproximately(expectedScale, Tolerance);
        }

        #endregion

        #region Edge Cases and Boundary Tests

        [Fact]
        public void CalculateMouseWheelZoom_WithNegativeScrollOffset_ClampsToZero()
        {
            // Arrange: Zoom out from origin, would result in negative scroll
            var context = new ZoomContext(
                currentZoomLevel: 200,
                mousePosition: new Point(10, 10),
                scrollOffset: new Point(5, 5),
                viewportSize: new Size(800, 600));

            // Act
            var result = ZoomControlEngine.CalculateMouseWheelZoom(context, zoomIn: false);

            // Assert: Scroll offset should not be negative
            result.NewScrollOffset.X.Should().BeGreaterOrEqualTo(0);
            result.NewScrollOffset.Y.Should().BeGreaterOrEqualTo(0);
        }

        [Fact]
        public void CalculateMouseWheelZoom_MouseAtOrigin_CalculatesCorrectOffset()
        {
            // Arrange: Mouse at viewport origin (0, 0)
            var context = new ZoomContext(
                currentZoomLevel: 100,
                mousePosition: new Point(0, 0),
                scrollOffset: new Point(100, 100),
                viewportSize: new Size(800, 600));

            // Act
            var result = ZoomControlEngine.CalculateMouseWheelZoom(context, zoomIn: true);

            // Assert: Content at (0,0) of viewport = scroll offset / scale = 100 / 1 = 100
            // New scroll = 100 * 1.1 - 0 = 110
            result.NewScrollOffset.X.Should().BeApproximately(110, Tolerance);
            result.NewScrollOffset.Y.Should().BeApproximately(110, Tolerance);
        }

        #endregion
    }
}
