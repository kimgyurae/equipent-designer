namespace EquipmentDesigner.Models
{
    /// <summary>
    /// Supported text color options for drawing elements.
    /// Values map to Brushes.xaml design tokens.
    /// </summary>
    public enum SupportedTextColor
    {
        /// <summary>
        /// Black text - Brush.DrawingElement.Text.Black (Color.Black)
        /// </summary>
        Black,

        /// <summary>
        /// Green text - Brush.DrawingElement.Text.Green (Color.Success.600)
        /// </summary>
        Green,

        /// <summary>
        /// Orange text - Brush.DrawingElement.Text.Orange (Color.Warning.600)
        /// </summary>
        Orange,

        /// <summary>
        /// Red text - Brush.DrawingElement.Text.Red (Color.Danger.600)
        /// </summary>
        Red,

        /// <summary>
        /// Purple text - Brush.DrawingElement.Text.Purple (Color.Accent.600)
        /// </summary>
        Purple,

        /// <summary>
        /// Blue text - Brush.DrawingElement.Text.Blue (Color.Primary.600)
        /// </summary>
        Blue
    }
}
