namespace EquipmentDesigner.Views.Drawboard.UMLEngine.Results
{
    /// <summary>
    /// Result of a drawing bounds calculation.
    /// </summary>
    public readonly struct DrawingBounds
    {
        public double X { get; }
        public double Y { get; }
        public double Width { get; }
        public double Height { get; }

        public DrawingBounds(double x, double y, double width, double height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
    }
}
