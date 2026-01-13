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
        /// </summary>
        public Point SourcePortPosition { get; }

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
        /// <param name="sourceBounds">The bounding rectangle of the source element.</param>
        public ConnectionContext(
            string sourceElementId,
            PortPosition sourcePort,
            Point sourcePortPosition,
            Rect sourceBounds)
        {
            SourceElementId = sourceElementId;
            SourcePort = sourcePort;
            SourcePortPosition = sourcePortPosition;
            SourceBounds = sourceBounds;
        }
    }
}
