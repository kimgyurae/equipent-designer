namespace EquipmentDesigner.Models.Rules
{
    /// <summary>
    /// Interface for rules that can be applied to DrawingElements.
    /// All rules follow the Expected vs Actual value comparison pattern.
    /// </summary>
    public interface IElementRule
    {
        /// <summary>
        /// Unique identifier for the rule.
        /// </summary>
        string RuleId { get; }

        /// <summary>
        /// Human-readable description of what the rule validates.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Gets the expected value as a formatted string for display.
        /// </summary>
        /// <param name="element">The element being validated.</param>
        /// <returns>String representation of the expected value.</returns>
        string GetExpectedValueString(DrawingElement element);

        /// <summary>
        /// Gets the actual value as a formatted string for display.
        /// </summary>
        /// <param name="element">The element being validated.</param>
        /// <returns>String representation of the actual value.</returns>
        string GetActualValueString(DrawingElement element);

        /// <summary>
        /// Determines if the rule passes for the given element.
        /// </summary>
        /// <param name="element">The element to validate.</param>
        /// <returns>True if the rule passes, false if it fails.</returns>
        bool IsValid(DrawingElement element);

        /// <summary>
        /// Evaluates the rule against the given element.
        /// </summary>
        /// <param name="element">The element to validate.</param>
        /// <returns>A RuleViolation if the rule is violated, null if the rule passes.</returns>
        RuleViolation Evaluate(DrawingElement element);
    }
}