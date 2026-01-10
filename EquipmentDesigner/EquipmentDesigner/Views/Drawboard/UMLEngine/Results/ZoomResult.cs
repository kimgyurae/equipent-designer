using System.Windows;

namespace EquipmentDesigner.Views.Drawboard.UMLEngine.Results
{
    /// <summary>
    /// Immutable result from zoom calculation.
    /// Contains new zoom state and scroll offset.
    /// </summary>
    public readonly struct ZoomResult
    {
        /// <summary>New zoom level percentage after zoom operation.</summary>
        public int NewZoomLevel { get; }

        /// <summary>New zoom scale factor (NewZoomLevel / 100.0).</summary>
        public double NewZoomScale { get; }

        /// <summary>New scroll offset to maintain focus point.</summary>
        public Point NewScrollOffset { get; }

        /// <summary>Whether zoom level actually changed.</summary>
        public bool ZoomChanged { get; }

        public ZoomResult(
            int newZoomLevel,
            double newZoomScale,
            Point newScrollOffset,
            bool zoomChanged)
        {
            NewZoomLevel = newZoomLevel;
            NewZoomScale = newZoomScale;
            NewScrollOffset = newScrollOffset;
            ZoomChanged = zoomChanged;
        }
    }
}
