using System;
using System.Drawing;
using System.Linq;

namespace LaserBurrMachine.Presentation.Components.Flowchart
{
    /// <summary>
    /// Calculates optimal connection points between nodes based on their relative positions.
    /// Uses weighted angle-based calculation to determine the best exit and entry directions.
    /// </summary>
    public class ConnectionPointCalculator
    {
        #region Weight Configuration - SINGLE LOCATION FOR ADJUSTMENT

        /// <summary>
        /// Default weights used when no weights are specified.
        /// Change this to modify the default behavior for all new instances.
        /// </summary>
        private static readonly ConnectionDirectionWeights _defaultWeights =
            ConnectionDirectionWeights.PreferVerticalFlow;

        #endregion

        private readonly ConnectionDirectionWeights _weights;
        private readonly double[] _boundaries; // Pre-calculated [RightEnd, BottomEnd, LeftEnd, TopEnd]

        /// <summary>
        /// Creates a calculator with default weighted configuration (prefers vertical flow).
        /// </summary>
        public ConnectionPointCalculator() : this(_defaultWeights)
        {
        }

        /// <summary>
        /// Creates a calculator with custom direction weights.
        /// </summary>
        /// <param name="weights">The weights to use for direction selection.</param>
        public ConnectionPointCalculator(ConnectionDirectionWeights weights)
        {
            _weights = weights ?? _defaultWeights;
            _boundaries = CalculateWeightedBoundaries();
        }

        /// <summary>
        /// Creates a calculator with equal weights (original behavior, backward compatible).
        /// </summary>
        /// <returns>A calculator that produces the same results as the original algorithm.</returns>
        public static ConnectionPointCalculator CreateWithEqualWeights()
            => new ConnectionPointCalculator(ConnectionDirectionWeights.Default);

        /// <summary>
        /// Calculates the weighted boundaries based on direction weights.
        /// </summary>
        /// <remarks>
        /// The algorithm divides 360 degrees into four sectors based on weight ratios.
        /// Each direction is centered on its natural angle: Right=0, Bottom=90, Left=180, Top=270.
        /// Higher weights result in larger angular ranges for that direction.
        /// </remarks>
        private double[] CalculateWeightedBoundaries()
        {
            // Order: Right(centered on 0 deg), Bottom(centered on 90 deg),
            //        Left(centered on 180 deg), Top(centered on 270 deg)
            double[] weights =
            {
                _weights.RightToLeft,
                _weights.BottomToTop,
                _weights.LeftToRight,
                _weights.TopToBottom
            };

            double totalWeight = weights.Sum();
            double[] spans = weights.Select(w => 360.0 * w / totalWeight).ToArray();

            // Start from 0 deg minus half of Right's span (so Right is centered on 0 deg)
            double startAngle = NormalizeAngle(-spans[0] / 2.0);

            double[] boundaries = new double[4];
            double cumulative = startAngle;
            for (int i = 0; i < 4; i++)
            {
                cumulative += spans[i];
                boundaries[i] = NormalizeAngle(cumulative);
            }

            return boundaries;
        }

        /// <summary>
        /// Normalizes an angle to the range [0, 360).
        /// </summary>
        private static double NormalizeAngle(double angle)
        {
            while (angle < 0) angle += 360;
            while (angle >= 360) angle -= 360;
            return angle;
        }

        /// <summary>
        /// Checks if an angle is within a range, handling wrap-around at 360/0 degrees.
        /// </summary>
        private static bool IsInRange(double angle, double start, double end)
        {
            // Handle wrap-around case (e.g., range 326.25 to 33.75 crossing 0 deg)
            if (start > end)
            {
                return angle >= start || angle < end;
            }
            return angle >= start && angle < end;
        }

        /// <summary>
        /// Calculates the optimal connection directions between source and target nodes.
        /// </summary>
        /// <param name="source">The source node rectangle.</param>
        /// <param name="target">The target node rectangle.</param>
        /// <returns>ConnectionPointInfo with optimal source and target directions.</returns>
        public ConnectionPointInfo CalculateConnectionPoints(NodeRect source, NodeRect target)
        {
            // Calculate the angle from source center to target center
            double deltaX = target.CenterX - source.CenterX;
            double deltaY = target.CenterY - source.CenterY;

            // Handle overlapping nodes - default to bottom-to-top (preferred direction)
            if (Math.Abs(deltaX) < 1 && Math.Abs(deltaY) < 1)
            {
                return new ConnectionPointInfo(ConnectionDirection.Bottom, ConnectionDirection.Top);
            }

            // Calculate angle in degrees (0 = right, 90 = down, 180 = left, 270 = up)
            double angleRadians = Math.Atan2(deltaY, deltaX);
            double angleDegrees = angleRadians * 180.0 / Math.PI;

            // Normalize to 0-360 range
            if (angleDegrees < 0)
            {
                angleDegrees += 360;
            }

            // Determine optimal directions based on weighted boundaries
            // _boundaries[0] = end of Right, start of BottomToTop
            // _boundaries[1] = end of BottomToTop, start of Left
            // _boundaries[2] = end of Left, start of TopToBottom
            // _boundaries[3] = end of TopToBottom, start of Right
            ConnectionDirection sourceDir;
            ConnectionDirection targetDir;

            if (IsInRange(angleDegrees, _boundaries[3], _boundaries[0]))
            {
                // Target is to the right
                sourceDir = ConnectionDirection.Right;
                targetDir = ConnectionDirection.Left;
            }
            else if (IsInRange(angleDegrees, _boundaries[0], _boundaries[1]))
            {
                // Target is below (PREFERRED direction with higher weight)
                sourceDir = ConnectionDirection.Bottom;
                targetDir = ConnectionDirection.Top;
            }
            else if (IsInRange(angleDegrees, _boundaries[1], _boundaries[2]))
            {
                // Target is to the left
                sourceDir = ConnectionDirection.Left;
                targetDir = ConnectionDirection.Right;
            }
            else
            {
                // Target is above
                sourceDir = ConnectionDirection.Top;
                targetDir = ConnectionDirection.Bottom;
            }

            return new ConnectionPointInfo(sourceDir, targetDir);
        }

        /// <summary>
        /// Gets the actual coordinate point for a connection in the specified direction.
        /// </summary>
        /// <param name="rect">The node rectangle.</param>
        /// <param name="direction">The connection direction.</param>
        /// <returns>The Point coordinate for the connection.</returns>
        public Point GetConnectionPoint(NodeRect rect, ConnectionDirection direction)
        {
            return direction switch
            {
                ConnectionDirection.Top => new Point(rect.CenterX, rect.Y),
                ConnectionDirection.Right => new Point(rect.X + rect.Width, rect.CenterY),
                ConnectionDirection.Bottom => new Point(rect.CenterX, rect.Y + rect.Height),
                ConnectionDirection.Left => new Point(rect.X, rect.CenterY),
                _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, "Invalid connection direction")
            };
        }
    }
}