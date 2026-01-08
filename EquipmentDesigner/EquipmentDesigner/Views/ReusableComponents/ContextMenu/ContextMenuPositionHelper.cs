using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace EquipmentDesigner.Views.ReusableComponents.ContextMenu
{
    /// <summary>
    /// Represents the direction in which a sub-menu should open.
    /// </summary>
    public enum SubMenuDirection
    {
        Right,
        Left
    }

    /// <summary>
    /// Result of position calculation including coordinates and direction.
    /// </summary>
    public class PositionResult
    {
        public double X { get; set; }
        public double Y { get; set; }
        public SubMenuDirection Direction { get; set; }
        public bool NeedsScrolling { get; set; }
        public double MaxHeight { get; set; }
    }

    /// <summary>
    /// Helper class for calculating context menu and sub-menu positions
    /// with monitor boundary awareness.
    /// </summary>
    public static class ContextMenuPositionHelper
    {
        /// <summary>
        /// Gap between parent menu and sub-menu in pixels.
        /// </summary>
        public const double SubMenuGap = 8;

        /// <summary>
        /// Maximum percentage of parent menu height that sub-menu can occupy.
        /// </summary>
        public const double MaxSubMenuHeightRatio = 0.9;

        /// <summary>
        /// Calculates the position for the root context menu.
        /// </summary>
        /// <param name="clickPoint">The point where user clicked (screen coordinates).</param>
        /// <param name="menuWidth">Expected width of the menu.</param>
        /// <param name="menuHeight">Expected height of the menu.</param>
        /// <returns>Position result with coordinates and scrolling info.</returns>
        public static PositionResult CalculateRootMenuPosition(Point clickPoint, double menuWidth, double menuHeight)
        {
            var screenBounds = GetScreenBounds(clickPoint);
            var result = new PositionResult
            {
                X = clickPoint.X,
                Y = clickPoint.Y,
                Direction = SubMenuDirection.Right,
                NeedsScrolling = false,
                MaxHeight = screenBounds.Height
            };

            // Adjust horizontal position if menu would go off-screen
            if (clickPoint.X + menuWidth > screenBounds.Right)
            {
                result.X = screenBounds.Right - menuWidth;
            }
            if (result.X < screenBounds.Left)
            {
                result.X = screenBounds.Left;
            }

            // Adjust vertical position if menu would go off-screen
            if (clickPoint.Y + menuHeight > screenBounds.Bottom)
            {
                // Try to position above the click point
                if (clickPoint.Y - menuHeight >= screenBounds.Top)
                {
                    result.Y = clickPoint.Y - menuHeight;
                }
                else
                {
                    // Need scrolling - position at bottom of screen
                    result.Y = screenBounds.Top;
                    result.NeedsScrolling = true;
                    result.MaxHeight = screenBounds.Height;
                }
            }

            return result;
        }

        /// <summary>
        /// Calculates the position for a sub-menu relative to its parent menu item.
        /// </summary>
        /// <param name="parentMenuItemBounds">Bounds of the parent menu item in screen coordinates.</param>
        /// <param name="parentMenuBounds">Bounds of the parent menu container in screen coordinates.</param>
        /// <param name="topMostMenuHeight">Height of the top-most (root) menu.</param>
        /// <param name="subMenuWidth">Expected width of the sub-menu.</param>
        /// <param name="subMenuHeight">Expected height of the sub-menu.</param>
        /// <param name="preferredDirection">Preferred direction to open.</param>
        /// <returns>Position result with coordinates, direction, and scrolling info.</returns>
        public static PositionResult CalculateSubMenuPosition(
            Rect parentMenuItemBounds,
            Rect parentMenuBounds,
            double topMostMenuHeight,
            double subMenuWidth,
            double subMenuHeight,
            SubMenuDirection preferredDirection = SubMenuDirection.Right)
        {
            var screenBounds = GetScreenBounds(new Point(parentMenuItemBounds.X, parentMenuItemBounds.Y));
            var maxAllowedHeight = topMostMenuHeight * MaxSubMenuHeightRatio;

            var result = new PositionResult
            {
                Direction = preferredDirection,
                NeedsScrolling = false,
                MaxHeight = maxAllowedHeight
            };

            // Calculate X position based on direction
            double rightX = parentMenuBounds.Right + SubMenuGap;
            double leftX = parentMenuBounds.Left - subMenuWidth - SubMenuGap;

            // Check if preferred direction has enough space
            if (preferredDirection == SubMenuDirection.Right)
            {
                if (rightX + subMenuWidth <= screenBounds.Right)
                {
                    result.X = rightX;
                    result.Direction = SubMenuDirection.Right;
                }
                else if (leftX >= screenBounds.Left)
                {
                    result.X = leftX;
                    result.Direction = SubMenuDirection.Left;
                }
                else
                {
                    // Not enough space on either side, use right and let it be clipped
                    result.X = Math.Min(rightX, screenBounds.Right - subMenuWidth);
                    result.Direction = SubMenuDirection.Right;
                }
            }
            else
            {
                if (leftX >= screenBounds.Left)
                {
                    result.X = leftX;
                    result.Direction = SubMenuDirection.Left;
                }
                else if (rightX + subMenuWidth <= screenBounds.Right)
                {
                    result.X = rightX;
                    result.Direction = SubMenuDirection.Right;
                }
                else
                {
                    result.X = Math.Max(leftX, screenBounds.Left);
                    result.Direction = SubMenuDirection.Left;
                }
            }

            // Calculate Y position
            // Start aligned with parent item's top
            result.Y = parentMenuItemBounds.Top;

            // Calculate actual height (limited by max allowed)
            double actualHeight = Math.Min(subMenuHeight, maxAllowedHeight);
            if (subMenuHeight > maxAllowedHeight)
            {
                result.NeedsScrolling = true;
            }

            // Check if sub-menu extends beyond screen bottom
            double bottomEdge = result.Y + actualHeight;
            if (bottomEdge > screenBounds.Bottom)
            {
                // Calculate how much we can grow upward
                double overflowBottom = bottomEdge - screenBounds.Bottom;

                // Try to shift up
                double shiftedY = result.Y - overflowBottom;

                // Ensure we don't go above screen top
                if (shiftedY < screenBounds.Top)
                {
                    shiftedY = screenBounds.Top;

                    // If still doesn't fit, enable scrolling
                    double availableHeight = screenBounds.Bottom - shiftedY;
                    if (availableHeight < actualHeight)
                    {
                        result.NeedsScrolling = true;
                        result.MaxHeight = Math.Min(availableHeight, maxAllowedHeight);
                    }
                }

                result.Y = shiftedY;
            }

            // Final check - ensure we don't exceed max allowed height
            if (subMenuHeight > maxAllowedHeight)
            {
                result.NeedsScrolling = true;
                result.MaxHeight = maxAllowedHeight;
            }

            return result;
        }

        /// <summary>
        /// Gets the working area bounds of the screen containing the specified point.
        /// </summary>
        /// <param name="point">A point in screen coordinates.</param>
        /// <returns>The screen's working area bounds.</returns>
        public static Rect GetScreenBounds(Point point)
        {
            // Use Windows Forms to get accurate screen info
            var screen = System.Windows.Forms.Screen.FromPoint(
                new System.Drawing.Point((int)point.X, (int)point.Y));

            var workingArea = screen.WorkingArea;
            return new Rect(
                workingArea.X,
                workingArea.Y,
                workingArea.Width,
                workingArea.Height);
        }

        /// <summary>
        /// Transforms a point from element coordinates to screen coordinates.
        /// </summary>
        /// <param name="element">The visual element.</param>
        /// <param name="point">Point in element coordinates.</param>
        /// <returns>Point in screen coordinates.</returns>
        public static Point ElementToScreen(Visual element, Point point)
        {
            try
            {
                var source = PresentationSource.FromVisual(element);
                if (source?.CompositionTarget != null)
                {
                    // Transform to root visual
                    var transformToRoot = element.TransformToAncestor(source.RootVisual);
                    var rootPoint = transformToRoot.Transform(point);

                    // Apply DPI scaling
                    var matrix = source.CompositionTarget.TransformToDevice;
                    var devicePoint = matrix.Transform(rootPoint);

                    // Get window position
                    var hwndSource = source as HwndSource;
                    if (hwndSource != null)
                    {
                        var windowPoint = new Point(0, 0);
                        try
                        {
                            windowPoint = hwndSource.RootVisual.PointToScreen(new Point(0, 0));
                        }
                        catch
                        {
                            // Fallback
                        }

                        return new Point(
                            windowPoint.X + devicePoint.X / matrix.M11,
                            windowPoint.Y + devicePoint.Y / matrix.M22);
                    }
                }

                // Fallback to simple transformation
                return element.PointToScreen(point);
            }
            catch
            {
                return point;
            }
        }

        /// <summary>
        /// Gets the bounds of a visual element in screen coordinates.
        /// </summary>
        /// <param name="element">The visual element.</param>
        /// <returns>Bounds in screen coordinates.</returns>
        public static Rect GetElementScreenBounds(FrameworkElement element)
        {
            if (element == null) return Rect.Empty;

            try
            {
                var topLeft = ElementToScreen(element, new Point(0, 0));
                var bottomRight = ElementToScreen(element, new Point(element.ActualWidth, element.ActualHeight));

                return new Rect(topLeft, bottomRight);
            }
            catch
            {
                return Rect.Empty;
            }
        }
    }
}
