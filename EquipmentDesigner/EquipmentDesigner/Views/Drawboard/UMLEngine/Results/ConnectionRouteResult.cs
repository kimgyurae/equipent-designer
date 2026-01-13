using System.Collections.Generic;
using System.Windows;
using EquipmentDesigner.Models;

namespace EquipmentDesigner.Views.Drawboard.UMLEngine.Results
{
    /// <summary>
    /// Immutable result of connection route calculation.
    /// Contains the orthogonal path and snap information.
    /// </summary>
    public readonly struct ConnectionRouteResult
    {
        /// <summary>
        /// The orthogonal route points from source to target/mouse position.
        /// Always includes at least the start and end points.
        /// </summary>
        public IReadOnlyList<Point> RoutePoints { get; }

        /// <summary>
        /// The port being snapped to on the target element (if any).
        /// </summary>
        public PortPosition? SnapTargetPort { get; }

        /// <summary>
        /// The ID of the element being snapped to (if any).
        /// </summary>
        public string SnapTargetElementId { get; }

        /// <summary>
        /// Whether the connection is currently snapping to a target port.
        /// </summary>
        public bool IsSnapped => SnapTargetElementId != null;

        /// <summary>
        /// The position of the snap target port (if snapped).
        /// </summary>
        public Point? SnapTargetPosition { get; }

        /// <summary>
        /// Creates a new connection route result without snap.
        /// </summary>
        /// <param name="routePoints">The orthogonal route points.</param>
        public ConnectionRouteResult(IReadOnlyList<Point> routePoints)
        {
            RoutePoints = routePoints;
            SnapTargetPort = null;
            SnapTargetElementId = null;
            SnapTargetPosition = null;
        }

        /// <summary>
        /// Creates a new connection route result with snap information.
        /// </summary>
        /// <param name="routePoints">The orthogonal route points.</param>
        /// <param name="snapTargetPort">The port being snapped to.</param>
        /// <param name="snapTargetElementId">The ID of the element being snapped to.</param>
        /// <param name="snapTargetPosition">The position of the snap target port.</param>
        public ConnectionRouteResult(
            IReadOnlyList<Point> routePoints,
            PortPosition snapTargetPort,
            string snapTargetElementId,
            Point snapTargetPosition)
        {
            RoutePoints = routePoints;
            SnapTargetPort = snapTargetPort;
            SnapTargetElementId = snapTargetElementId;
            SnapTargetPosition = snapTargetPosition;
        }
    }
}
