using System.Windows;
using EquipmentDesigner.Models;

namespace EquipmentDesigner.Views.Drawboard.UMLEngine.Contexts
{
    /// <summary>
    /// Immutable context for connection operations.
    /// Contains all necessary state for calculating connection routes.
    /// </summary>
    public readonly struct ConnectionContext
    {
        /// <summary>
        /// The ID of the source element where the connection starts.
        /// </summary>
        public string SourceElementId { get; }

        /// <summary>
        /// The port position on the source element.
        /// </summary>
        public PortPosition SourcePort { get; }

        /// <summary>
        /// The absolute canvas coordinates of the source port center.
        /// Used for snap detection and visual port display.
        /// </summary>
        public Point SourcePortPosition { get; }

        /// <summary>
        /// The absolute canvas coordinates where the arrow attaches to the element edge.
        /// This is the exact center of the specified side with no offset.
        /// </summary>
        public Point SourceEdgePosition { get; }

        /// <summary>
        /// The bounding rectangle of the source element.
        /// </summary>
        public Rect SourceBounds { get; }

        /// <summary>
        /// Creates a new connection context.
        /// </summary>
        /// <param name="sourceElementId">The ID of the source element.</param>
        /// <param name="sourcePort">The port position on the source element.</param>
        /// <param name="sourcePortPosition">The absolute canvas coordinates of the port.</param>
        /// <param name="sourceEdgePosition">The absolute canvas coordinates of the edge attachment point.</param>
        /// <param name="sourceBounds">The bounding rectangle of the source element.</param>
        public ConnectionContext(
            string sourceElementId,
            PortPosition sourcePort,
            Point sourcePortPosition,
            Point sourceEdgePosition,
            Rect sourceBounds)
        {
            SourceElementId = sourceElementId;
            SourcePort = sourcePort;
            SourcePortPosition = sourcePortPosition;
            SourceEdgePosition = sourceEdgePosition;
            SourceBounds = sourceBounds;
        }
    }
}