using System;

namespace EquipmentDesigner.Models.Rules
{
    /// <summary>
    /// Specifies which arrow direction this rule applies to.
    /// </summary>
    public enum ArrowDirection
    {
        /// <summary>
        /// Arrows pointing into the element.
        /// </summary>
        Incoming,

        /// <summary>
        /// Arrows pointing out of the element.
        /// </summary>
        Outgoing
    }

    /// <summary>
    /// Specifies the type of comparison for arrow count validation.
    /// </summary>
    public enum ArrowCountComparisonType
    {
        /// <summary>
        /// Arrow count must be exactly the specified value.
        /// </summary>
        Exact,

        /// <summary>
        /// Arrow count must be at least the specified minimum value.
        /// </summary>
        Minimum,

        /// <summary>
        /// Arrow count must be at most the specified maximum value.
        /// </summary>
        Maximum,

        /// <summary>
        /// Arrow count must be within the specified range (inclusive).
        /// </summary>
        Range
    }

    /// <summary>
    /// Rule that validates the number of incoming or outgoing arrows on an element.
    /// Supports exact count, minimum, maximum, and range validations.
    /// Inherits from ElementRuleBase to follow the Expected vs Actual pattern.
    /// </summary>
    public sealed class ArrowCountRule : ElementRuleBase<int>
    {
        private readonly ArrowDirection _direction;
        private readonly ArrowCountComparisonType _comparisonType;
        private readonly int _value;
        private readonly int _maxValue; // Only used for Range comparison

        /// <summary>
        /// Creates an exact count rule.
        /// </summary>
        public static ArrowCountRule Exact(ArrowDirection direction, int count, string description = null)
        {
            return new ArrowCountRule(direction, ArrowCountComparisonType.Exact, count, 0, description);
        }

        /// <summary>
        /// Creates a minimum count rule.
        /// </summary>
        public static ArrowCountRule Minimum(ArrowDirection direction, int minCount, string description = null)
        {
            return new ArrowCountRule(direction, ArrowCountComparisonType.Minimum, minCount, 0, description);
        }

        /// <summary>
        /// Creates a maximum count rule.
        /// </summary>
        public static ArrowCountRule Maximum(ArrowDirection direction, int maxCount, string description = null)
        {
            return new ArrowCountRule(direction, ArrowCountComparisonType.Maximum, maxCount, 0, description);
        }

        /// <summary>
        /// Creates a range count rule (inclusive).
        /// </summary>
        public static ArrowCountRule Range(ArrowDirection direction, int minCount, int maxCount, string description = null)
        {
            return new ArrowCountRule(direction, ArrowCountComparisonType.Range, minCount, maxCount, description);
        }

        private ArrowCountRule(
            ArrowDirection direction,
            ArrowCountComparisonType comparisonType,
            int value,
            int maxValue,
            string description)
            : base($"ArrowCount_{direction}_{comparisonType}", description ?? GenerateDefaultDescription(direction, comparisonType, value, maxValue))
        {
            _direction = direction;
            _comparisonType = comparisonType;
            _value = value;
            _maxValue = maxValue;
        }

        /// <inheritdoc />
        protected override int GetExpectedValue(DrawingElement element)
        {
            // Returns the primary constraint value
            return _value;
        }

        /// <inheritdoc />
        protected override int GetActualValue(DrawingElement element)
        {
            return _direction == ArrowDirection.Incoming
                ? element.CurrentIncomingCount
                : element.CurrentOutgoingCount;
        }

        /// <inheritdoc />
        protected override bool Compare(int expected, int actual)
        {
            return _comparisonType switch
            {
                ArrowCountComparisonType.Exact => actual == expected,
                ArrowCountComparisonType.Minimum => actual >= expected,
                ArrowCountComparisonType.Maximum => actual <= expected,
                ArrowCountComparisonType.Range => actual >= expected && actual <= _maxValue,
                _ => true
            };
        }

        /// <inheritdoc />
        protected override string FormatExpectedValue(int expected)
        {
            return _comparisonType switch
            {
                ArrowCountComparisonType.Exact => expected.ToString(),
                ArrowCountComparisonType.Minimum => $"≥{expected}",
                ArrowCountComparisonType.Maximum => $"≤{expected}",
                ArrowCountComparisonType.Range => $"{expected}~{_maxValue}",
                _ => expected.ToString()
            };
        }

        private static string GenerateDefaultDescription(
            ArrowDirection direction,
            ArrowCountComparisonType comparisonType,
            int value,
            int maxValue)
        {
            string directionText = direction == ArrowDirection.Incoming ? "Incoming" : "Outgoing";

            return comparisonType switch
            {
                ArrowCountComparisonType.Exact when value == 0 =>
                    $"{directionText} Arrow를 가질 수 없습니다",
                ArrowCountComparisonType.Exact when value == 1 =>
                    $"하나의 {directionText} Arrow를 가져야합니다",
                ArrowCountComparisonType.Exact =>
                    $"{value}개의 {directionText} Arrow를 가져야합니다",
                ArrowCountComparisonType.Minimum when value == 1 =>
                    $"최소 하나 이상의 {directionText} Arrow를 가져야합니다",
                ArrowCountComparisonType.Minimum =>
                    $"최소 {value}개 이상의 {directionText} Arrow를 가져야합니다",
                ArrowCountComparisonType.Maximum =>
                    $"최대 {value}개의 {directionText} Arrow만 가질 수 있습니다",
                ArrowCountComparisonType.Range =>
                    $"{value}~{maxValue}개의 {directionText} Arrow를 가져야합니다",
                _ => $"{directionText} Arrow 수 규칙"
            };
        }
    }
}