namespace EquipmentDesigner.Views.Drawboard.UMLEngine.Results
{
    /// <summary>
    /// Result of a move calculation.
    /// </summary>
    public readonly struct MoveResult
    {
        public double NewX { get; }
        public double NewY { get; }

        public MoveResult(double newX, double newY)
        {
            NewX = newX;
            NewY = newY;
        }
    }
}
