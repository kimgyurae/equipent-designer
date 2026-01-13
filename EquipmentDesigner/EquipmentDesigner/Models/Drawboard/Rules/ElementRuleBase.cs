namespace EquipmentDesigner.Models.Rules
{
    /// <summary>
    /// Abstract base class for element rules that follow the Expected vs Actual value comparison pattern.
    /// Uses Template Method pattern to provide common validation logic.
    /// </summary>
    /// <typeparam name="T">The type of value being compared (e.g., int, string, bool).</typeparam>
    public abstract class ElementRuleBase<T> : IElementRule
    {
        /// <inheritdoc />
        public string RuleId { get; }

        /// <inheritdoc />
        public string Description { get; }

        /// <summary>
        /// Creates a new rule with the specified ID and description.
        /// </summary>
        /// <param name="ruleId">Unique identifier for the rule.</param>
        /// <param name="description">Human-readable description of the rule.</param>
        protected ElementRuleBase(string ruleId, string description)
        {
            RuleId = ruleId;
            Description = description;
        }

        /// <summary>
        /// Gets the expected value for validation.
        /// </summary>
        /// <param name="element">The element being validated.</param>
        /// <returns>The expected value.</returns>
        protected abstract T GetExpectedValue(DrawingElement element);

        /// <summary>
        /// Gets the actual value from the element.
        /// </summary>
        /// <param name="element">The element being validated.</param>
        /// <returns>The actual value.</returns>
        protected abstract T GetActualValue(DrawingElement element);

        /// <summary>
        /// Compares the expected and actual values to determine if the rule passes.
        /// </summary>
        /// <param name="expected">The expected value.</param>
        /// <param name="actual">The actual value.</param>
        /// <returns>True if the rule passes, false if it fails.</returns>
        protected abstract bool Compare(T expected, T actual);

        /// <summary>
        /// Formats the expected value for display.
        /// Override to provide custom formatting.
        /// </summary>
        /// <param name="expected">The expected value.</param>
        /// <returns>Formatted string representation.</returns>
        protected virtual string FormatExpectedValue(T expected)
        {
            return expected?.ToString() ?? string.Empty;
        }

        /// <summary>
        /// Formats the actual value for display.
        /// Override to provide custom formatting.
        /// </summary>
        /// <param name="actual">The actual value.</param>
        /// <returns>Formatted string representation.</returns>
        protected virtual string FormatActualValue(T actual)
        {
            return actual?.ToString() ?? string.Empty;
        }

        /// <inheritdoc />
        public string GetExpectedValueString(DrawingElement element)
        {
            if (element == null) return string.Empty;
            return FormatExpectedValue(GetExpectedValue(element));
        }

        /// <inheritdoc />
        public string GetActualValueString(DrawingElement element)
        {
            if (element == null) return string.Empty;
            return FormatActualValue(GetActualValue(element));
        }

        /// <inheritdoc />
        public bool IsValid(DrawingElement element)
        {
            if (element == null) return true;
            return Compare(GetExpectedValue(element), GetActualValue(element));
        }

        /// <inheritdoc />
        public RuleViolation Evaluate(DrawingElement element)
        {
            if (element == null) return null;
            if (IsValid(element)) return null;

            return new RuleViolation(
                RuleId,
                Description,
                GetExpectedValueString(element),
                GetActualValueString(element)
            );
        }
    }
}
