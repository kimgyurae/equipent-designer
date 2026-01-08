namespace EquipmentDesigner.Models.ProcessEditor
{
    /// <summary>
    /// Interface for objects that can provide hints to the user.
    /// Hints may contain bracketed text like [Space] that should be rendered specially.
    /// </summary>
    public interface IHintable
    {
        /// <summary>
        /// Gets the hint text. Empty string if no hint is available.
        /// Text within square brackets (e.g., [Space], [Alt]) should be rendered
        /// in a rounded rectangle badge.
        /// </summary>
        string Hint { get; }
    }
}
