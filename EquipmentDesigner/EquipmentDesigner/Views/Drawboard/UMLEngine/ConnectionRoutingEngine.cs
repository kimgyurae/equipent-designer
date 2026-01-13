using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using EquipmentDesigner.Models;
using EquipmentDesigner.Views.Drawboard.UMLEngine.Contexts;
using EquipmentDesigner.Views.Drawboard.UMLEngine.Results;

namespace EquipmentDesigner.Views.Drawboard.UMLEngine
{
    /// <summary>
    /// Stateless engine for calculating connection routes and port positions.
    /// All methods are pure functions with no side effects.
    /// </summary>
    public static class ConnectionRoutingEngine
    {
        /// <summary>
        /// Distance from the element edge to the port center (outside the element).
        /// </summary>
        public const double PortOffset = 12.0;

        /// <summary>
        /// Radius for snap detection (magnet effect).
        /// </summary>
        public const double SnapDistance = 30.0;

        /// <summary>
        /// Minimum segment length for orthogonal routing.
        /// </summary>
        public const double MinSegmentLength = 20.0;

        /// <summary>
        /// Creates a connection context for the given source element and port.
        /// </summary>
        /// <param name="element">The source drawing element.</param>
        /// <param name="port">The port position on the source element.</param>
        /// <returns>An immutable connection context.</returns>
        public static ConnectionContext CreateConnectionContext(DrawingElement element, PortPosition port)
        {
            var bounds = element.Bounds;
            var portPosition = CalculatePortPosition(bounds, port);

            return new ConnectionContext(
                element.Id,
                port,
                portPosition,
                bounds);
        }

        /// <summary>
        /// Calculates the absolute canvas position of a port on an element.
        /// The port is positioned at the center of the specified edge, offset outside the element.
        /// </summary>
        /// <param name="bounds">The bounding rectangle of the element.</param>
        /// <param name="port">The port position.</param>
        /// <returns>The absolute canvas coordinates of the port center.</returns>
        public static Point CalculatePortPosition(Rect bounds, PortPosition port)
        {
            return port switch
            {
                PortPosition.Top => new Point(
                    bounds.X + bounds.Width / 2,
                    bounds.Y - PortOffset),
                PortPosition.Right => new Point(
                    bounds.X + bounds.Width + PortOffset,
                    bounds.Y + bounds.Height / 2),
                PortPosition.Bottom => new Point(
                    bounds.X + bounds.Width / 2,
                    bounds.Y + bounds.Height + PortOffset),
                PortPosition.Left => new Point(
                    bounds.X - PortOffset,
                    bounds.Y + bounds.Height / 2),
                _ => throw new ArgumentOutOfRangeException(nameof(port))
            };
        }

        /// <summary>
        /// Calculates which port on an element is closest to the given point.
        /// </summary>
        /// <param name="bounds">The bounding rectangle of the element.</param>
        /// <param name="point">The point to measure from.</param>
        /// <returns>The closest port position.</returns>
        public static PortPosition CalculateClosestPort(Rect bounds, Point point)
        {
            var ports = new[]
            {
                (Port: PortPosition.Top, Position: CalculatePortPosition(bounds, PortPosition.Top)),
                (Port: PortPosition.Right, Position: CalculatePortPosition(bounds, PortPosition.Right)),
                (Port: PortPosition.Bottom, Position: CalculatePortPosition(bounds, PortPosition.Bottom)),
                (Port: PortPosition.Left, Position: CalculatePortPosition(bounds, PortPosition.Left))
            };

            return ports
                .OrderBy(p => Distance(p.Position, point))
                .First()
                .Port;
        }

        /// <summary>
        /// Finds the closest snap target among the given elements.
        /// </summary>
        /// <param name="mousePosition">The current mouse position.</param>
        /// <param name="elements">The collection of elements to check.</param>
        /// <param name="excludeId">Element ID to exclude (typically the source element).</param>
        /// <param name="snapDistance">The maximum distance for snapping.</param>
        /// <returns>A tuple of (elementId, port, position) if a snap target is found, null otherwise.</returns>
        public static (string ElementId, PortPosition Port, Point Position)? FindSnapTarget(
            Point mousePosition,
            IEnumerable<DrawingElement> elements,
            string excludeId,
            double snapDistance = SnapDistance)
        {
            (string ElementId, PortPosition Port, Point Position, double Distance)? closest = null;

            foreach (var element in elements)
            {
                if (element.Id == excludeId) continue;

                var bounds = element.Bounds;

                foreach (PortPosition port in Enum.GetValues(typeof(PortPosition)))
                {
                    var portPos = CalculatePortPosition(bounds, port);
                    var distance = Distance(portPos, mousePosition);

                    if (distance <= snapDistance)
                    {
                        if (closest == null || distance < closest.Value.Distance)
                        {
                            closest = (element.Id, port, portPos, distance);
                        }
                    }
                }
            }

            if (closest != null)
            {
                return (closest.Value.ElementId, closest.Value.Port, closest.Value.Position);
            }

            return null;
        }

        /// <summary>
        /// Calculates an orthogonal route from the source port to the target point.
        /// The route uses only horizontal and vertical segments (90-degree turns).
        /// </summary>
        /// <param name="context">The connection context with source information.</param>
        /// <param name="targetPoint">The target point (mouse position or snap target).</param>
        /// <returns>A connection route result with the calculated path.</returns>
        public static ConnectionRouteResult CalculateOrthogonalRoute(
            ConnectionContext context,
            Point targetPoint)
        {
            var routePoints = CalculateOrthogonalPath(
                context.SourcePortPosition,
                context.SourcePort,
                targetPoint,
                null);

            return new ConnectionRouteResult(routePoints);
        }

        /// <summary>
        /// Calculates an orthogonal route from the source port to a snap target.
        /// </summary>
        /// <param name="context">The connection context with source information.</param>
        /// <param name="targetElementId">The ID of the target element.</param>
        /// <param name="targetPort">The target port position.</param>
        /// <param name="targetPosition">The target port position coordinates.</param>
        /// <returns>A connection route result with snap information.</returns>
        public static ConnectionRouteResult CalculateOrthogonalRouteToTarget(
            ConnectionContext context,
            string targetElementId,
            PortPosition targetPort,
            Point targetPosition)
        {
            var routePoints = CalculateOrthogonalPath(
                context.SourcePortPosition,
                context.SourcePort,
                targetPosition,
                targetPort);

            return new ConnectionRouteResult(
                routePoints,
                targetPort,
                targetElementId,
                targetPosition);
        }

        /// <summary>
        /// Calculates the orthogonal path between two points.
        /// </summary>
        private static List<Point> CalculateOrthogonalPath(
            Point start,
            PortPosition startPort,
            Point end,
            PortPosition? endPort)
        {
            var points = new List<Point> { start };

            // Determine the initial direction based on the source port
            var startDirection = GetPortDirection(startPort);

            // Determine the final direction based on the target port (if specified)
            var endDirection = endPort.HasValue
                ? GetOppositeDirection(GetPortDirection(endPort.Value))
                : CalculateApproachDirection(start, startPort, end);

            // Calculate intermediate waypoints for orthogonal routing
            var waypoints = CalculateWaypoints(start, startDirection, end, endDirection);
            points.AddRange(waypoints);

            points.Add(end);

            // Remove duplicate consecutive points
            return RemoveDuplicatePoints(points);
        }

        /// <summary>
        /// Gets the outward direction for a port.
        /// </summary>
        private static Direction GetPortDirection(PortPosition port)
        {
            return port switch
            {
                PortPosition.Top => Direction.Up,
                PortPosition.Right => Direction.Right,
                PortPosition.Bottom => Direction.Down,
                PortPosition.Left => Direction.Left,
                _ => Direction.Right
            };
        }

        /// <summary>
        /// Gets the opposite direction.
        /// </summary>
        private static Direction GetOppositeDirection(Direction direction)
        {
            return direction switch
            {
                Direction.Up => Direction.Down,
                Direction.Down => Direction.Up,
                Direction.Left => Direction.Right,
                Direction.Right => Direction.Left,
                _ => direction
            };
        }

        /// <summary>
        /// Calculates the best approach direction to reach the target.
        /// </summary>
        private static Direction CalculateApproachDirection(Point start, PortPosition startPort, Point end)
        {
            var dx = end.X - start.X;
            var dy = end.Y - start.Y;

            // Prefer the direction that aligns with the largest displacement
            if (Math.Abs(dx) > Math.Abs(dy))
            {
                return dx > 0 ? Direction.Right : Direction.Left;
            }
            else
            {
                return dy > 0 ? Direction.Down : Direction.Up;
            }
        }

        /// <summary>
        /// Calculates the waypoints for orthogonal routing.
        /// </summary>
        private static List<Point> CalculateWaypoints(
            Point start,
            Direction startDirection,
            Point end,
            Direction endDirection)
        {
            var waypoints = new List<Point>();

            // Simple L-shaped or Z-shaped routing
            var dx = end.X - start.X;
            var dy = end.Y - start.Y;

            // First, extend from start in the start direction
            Point firstWaypoint;
            switch (startDirection)
            {
                case Direction.Up:
                case Direction.Down:
                    // Vertical start - extend vertically first, then horizontal
                    if (Math.Abs(dy) > MinSegmentLength)
                    {
                        // If significant vertical distance, use midpoint
                        var midY = start.Y + dy / 2;
                        firstWaypoint = new Point(start.X, midY);
                        waypoints.Add(firstWaypoint);
                        waypoints.Add(new Point(end.X, midY));
                    }
                    else
                    {
                        // Small vertical distance, extend past element first
                        var extendY = startDirection == Direction.Up
                            ? Math.Min(start.Y - MinSegmentLength, end.Y - MinSegmentLength)
                            : Math.Max(start.Y + MinSegmentLength, end.Y + MinSegmentLength);
                        firstWaypoint = new Point(start.X, extendY);
                        waypoints.Add(firstWaypoint);
                        waypoints.Add(new Point(end.X, extendY));
                    }
                    break;

                case Direction.Left:
                case Direction.Right:
                    // Horizontal start - extend horizontally first, then vertical
                    if (Math.Abs(dx) > MinSegmentLength)
                    {
                        // If significant horizontal distance, use midpoint
                        var midX = start.X + dx / 2;
                        firstWaypoint = new Point(midX, start.Y);
                        waypoints.Add(firstWaypoint);
                        waypoints.Add(new Point(midX, end.Y));
                    }
                    else
                    {
                        // Small horizontal distance, extend past element first
                        var extendX = startDirection == Direction.Left
                            ? Math.Min(start.X - MinSegmentLength, end.X - MinSegmentLength)
                            : Math.Max(start.X + MinSegmentLength, end.X + MinSegmentLength);
                        firstWaypoint = new Point(extendX, start.Y);
                        waypoints.Add(firstWaypoint);
                        waypoints.Add(new Point(extendX, end.Y));
                    }
                    break;
            }

            return waypoints;
        }

        /// <summary>
        /// Removes consecutive duplicate points from the list.
        /// </summary>
        private static List<Point> RemoveDuplicatePoints(List<Point> points)
        {
            if (points.Count <= 1) return points;

            var result = new List<Point> { points[0] };

            for (int i = 1; i < points.Count; i++)
            {
                if (Distance(points[i], result[result.Count - 1]) > 0.5)
                {
                    result.Add(points[i]);
                }
            }

            return result;
        }

        /// <summary>
        /// Calculates the Euclidean distance between two points.
        /// </summary>
        private static double Distance(Point a, Point b)
        {
            var dx = a.X - b.X;
            var dy = a.Y - b.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>
        /// Direction enumeration for routing calculations.
        /// </summary>
        private enum Direction
        {
            Up,
            Down,
            Left,
            Right
        }
    }
}
