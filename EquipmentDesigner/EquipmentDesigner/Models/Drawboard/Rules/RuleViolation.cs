namespace EquipmentDesigner.Models.Rules
{
    /// <summary>
    /// Represents a rule violation detected during element validation.
    /// Immutable record containing all information needed to display the violation.
    /// </summary>
    public sealed class RuleViolation
    {
        /// <summary>
        /// The rule that was violated.
        /// </summary>
        public string RuleId { get; }

        /// <summary>
        /// Human-readable message describing the violation.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// String representation of the expected value or condition.
        /// Example: ">1", "1", "0-2"
        /// </summary>
        public string ExpectedValue { get; }

        /// <summary>
        /// String representation of the actual value found.
        /// </summary>
        public string ActualValue { get; }

        /// <summary>
        /// Creates a new rule violation.
        /// </summary>
        /// <param name="ruleId">Unique identifier of the violated rule.</param>
        /// <param name="message">Human-readable violation message.</param>
        /// <param name="expectedValue">String representation of expected value.</param>
        /// <param name="actualValue">String representation of actual value.</param>
        public RuleViolation(string ruleId, string message, string expectedValue, string actualValue)
        {
            RuleId = ruleId;
            Message = message;
            ExpectedValue = expectedValue;
            ActualValue = actualValue;
        }
    }
}
