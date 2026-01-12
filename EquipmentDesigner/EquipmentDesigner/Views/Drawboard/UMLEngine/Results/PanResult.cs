namespace EquipmentDesigner.Views.Drawboard.UMLEngine.Results
{
    /// <summary>
    /// Immutable result of a pan calculation.
    /// Contains new scroll offsets to apply.
    /// </summary>
    public readonly struct PanResult
    {
        /// <summary>
        /// New horizontal scroll offset.
        /// </summary>
        public double NewScrollOffsetX { get; }

        /// <summary>
        /// New vertical scroll offset.
        /// </summary>
        public double NewScrollOffsetY { get; }

        public PanResult(double newScrollOffsetX, double newScrollOffsetY)
        {
            NewScrollOffsetX = newScrollOffsetX;
            NewScrollOffsetY = newScrollOffsetY;
        }
    }
}
