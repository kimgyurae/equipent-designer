using System;

namespace LaserBurrMachine.Presentation.Components.Flowchart
{
    /// <summary>
    /// Stores information about the optimal connection points between two nodes.
    /// Contains the source node's exit direction and target node's entry direction.
    /// </summary>
    public class ConnectionPointInfo
    {
        /// <summary>
        /// The direction from which the connection exits the source node.
        /// </summary>
        public ConnectionDirection SourceDirection { get; }

        /// <summary>
        /// The direction from which the connection enters the target node.
        /// </summary>
        public ConnectionDirection TargetDirection { get; }

        /// <summary>
        /// Creates a new ConnectionPointInfo with the specified source and target directions.
        /// </summary>
        /// <param name="sourceDirection">The exit direction from the source node.</param>
        /// <param name="targetDirection">The entry direction into the target node.</param>
        public ConnectionPointInfo(ConnectionDirection sourceDirection, ConnectionDirection targetDirection)
        {
            SourceDirection = sourceDirection;
            TargetDirection = targetDirection;
        }

        /// <summary>
        /// Gets the opposite direction for a given connection direction.
        /// Top ↔ Bottom, Left ↔ Right
        /// </summary>
        /// <param name="direction">The direction to get the opposite of.</param>
        /// <returns>The opposite direction.</returns>
        public static ConnectionDirection GetOppositeDirection(ConnectionDirection direction)
        {
            return direction switch
            {
                ConnectionDirection.Top => ConnectionDirection.Bottom,
                ConnectionDirection.Right => ConnectionDirection.Left,
                ConnectionDirection.Bottom => ConnectionDirection.Top,
                ConnectionDirection.Left => ConnectionDirection.Right,
                _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, "Invalid connection direction")
            };
        }
    }
}
