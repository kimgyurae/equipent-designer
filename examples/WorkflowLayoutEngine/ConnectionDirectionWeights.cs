namespace LaserBurrMachine.Presentation.Components.Flowchart
{
    /// <summary>
    /// Configuration for weighted direction selection in connection point calculation.
    /// Higher weights expand the angular range for that direction, making it more likely
    /// to be selected when the angle is near a boundary.
    /// </summary>
    /// <remarks>
    /// The weights are relative - only the ratios between weights matter.
    /// A weight of 1.5 vs 1.0 means that direction gets 50% more angular range.
    ///
    /// Default behavior (PreferVerticalFlow) expands Bottom-to-Top direction to favor:
    /// - Arrows exiting from the BOTTOM of source nodes
    /// - Arrows entering from the TOP of target nodes
    /// </remarks>
    public class ConnectionDirectionWeights
    {
        #region Weight Constants - SINGLE LOCATION FOR ADJUSTMENT

        /// <summary>
        /// Weight for horizontal rightward flow (Right exit, Left entry).
        /// Applies when target is to the right of source.
        /// </summary>
        /// <remarks>Default: 0.75 (reduced to favor vertical flow)</remarks>
        public const double DefaultRightToLeft = 0.75;

        /// <summary>
        /// Weight for vertical downward flow (Bottom exit, Top entry).
        /// Applies when target is below source.
        /// This is the PREFERRED direction for vertical workflow layouts.
        /// </summary>
        /// <remarks>Default: 1.5 (50% more angular range than base)</remarks>
        public const double DefaultBottomToTop = 30.0;

        /// <summary>
        /// Weight for horizontal leftward flow (Left exit, Right entry).
        /// Applies when target is to the left of source.
        /// </summary>
        /// <remarks>Default: 0.75 (reduced to favor vertical flow)</remarks>
        public const double DefaultLeftToRight = 0.75;

        /// <summary>
        /// Weight for vertical upward flow (Top exit, Bottom entry).
        /// Applies when target is above source (back-references, loops).
        /// </summary>
        /// <remarks>Default: 1.0 (neutral weight for back-references)</remarks>
        public const double DefaultTopToBottom = 0.75;

        #endregion

        /// <summary>
        /// Weight for Right exit / Left entry direction (horizontal rightward flow).
        /// Applies when target is to the right of source.
        /// </summary>
        public double RightToLeft { get; set; } = 1.0;

        /// <summary>
        /// Weight for Bottom exit / Top entry direction (vertical downward flow).
        /// Applies when target is below source. This is typically the PREFERRED direction
        /// for vertical workflow layouts.
        /// </summary>
        public double BottomToTop { get; set; } = 1.0;

        /// <summary>
        /// Weight for Left exit / Right entry direction (horizontal leftward flow).
        /// Applies when target is to the left of source.
        /// </summary>
        public double LeftToRight { get; set; } = 1.0;

        /// <summary>
        /// Weight for Top exit / Bottom entry direction (vertical upward flow).
        /// Applies when target is above source (back-references, loops).
        /// </summary>
        public double TopToBottom { get; set; } = 1.0;

        /// <summary>
        /// Default weights with equal distribution (90 degrees per direction).
        /// Use this for backward-compatible behavior matching the original algorithm.
        /// </summary>
        public static ConnectionDirectionWeights Default => new ConnectionDirectionWeights();

        /// <summary>
        /// Preset that favors vertical downward flow (Bottom exit -> Top entry).
        /// Recommended for vertical workflow layouts where nodes flow top-to-bottom.
        ///
        /// Angular ranges with these weights:
        /// - Right-to-Left: 67.5 degrees (reduced from 90)
        /// - Bottom-to-Top: 135 degrees (expanded from 90)
        /// - Left-to-Right: 67.5 degrees (reduced from 90)
        /// - Top-to-Bottom: 90 degrees (unchanged)
        /// </summary>
        public static ConnectionDirectionWeights PreferVerticalFlow => new ConnectionDirectionWeights
        {
            RightToLeft = DefaultRightToLeft,
            BottomToTop = DefaultBottomToTop,
            LeftToRight = DefaultLeftToRight,
            TopToBottom = DefaultTopToBottom
        };
    }
}
