using System;
using EquipmentDesigner.Utils;
using FluentAssertions;
using Xunit;

namespace EquipmentDesigner.Tests.Utils
{
    public class VersionHelperTests
    {
        #region Version Parsing Tests

        [Fact]
        public void GetMajor_ValidVersionString_ReturnsCorrectInteger()
        {
            // Arrange
            var version = "5.10.15";

            // Act
            var major = VersionHelper.GetMajor(version);

            // Assert
            major.Should().Be(5);
        }

        [Fact]
        public void GetMinor_ValidVersionString_ReturnsCorrectInteger()
        {
            // Arrange
            var version = "5.10.15";

            // Act
            var minor = VersionHelper.GetMinor(version);

            // Assert
            minor.Should().Be(10);
        }

        [Fact]
        public void GetPatch_ValidVersionString_ReturnsCorrectInteger()
        {
            // Arrange
            var version = "5.10.15";

            // Act
            var patch = VersionHelper.GetPatch(version);

            // Assert
            patch.Should().Be(15);
        }

        [Fact]
        public void Parse_ZeroValues_HandlesCorrectly()
        {
            // Arrange
            var version = "0.0.0";

            // Act & Assert
            VersionHelper.GetMajor(version).Should().Be(0);
            VersionHelper.GetMinor(version).Should().Be(0);
            VersionHelper.GetPatch(version).Should().Be(0);
        }

        [Fact]
        public void Parse_LargeNumbers_HandlesCorrectly()
        {
            // Arrange
            var version = "999.888.777";

            // Act & Assert
            VersionHelper.GetMajor(version).Should().Be(999);
            VersionHelper.GetMinor(version).Should().Be(888);
            VersionHelper.GetPatch(version).Should().Be(777);
        }

        #endregion

        #region Version Formatting Tests

        [Fact]
        public void ToDisplayFormat_ValidVersion_AddsVPrefix()
        {
            // Arrange
            var version = "1.0.0";

            // Act
            var display = VersionHelper.ToDisplayFormat(version);

            // Assert
            display.Should().Be("v 1.0.0");
        }

        [Fact]
        public void ToDisplayFormat_ZeroVersion_AddsVPrefix()
        {
            // Arrange
            var version = "0.0.0";

            // Act
            var display = VersionHelper.ToDisplayFormat(version);

            // Assert
            display.Should().Be("v 0.0.0");
        }

        [Fact]
        public void ToDisplayFormat_LargeVersion_AddsVPrefix()
        {
            // Arrange
            var version = "100.200.300";

            // Act
            var display = VersionHelper.ToDisplayFormat(version);

            // Assert
            display.Should().Be("v 100.200.300");
        }

        [Fact]
        public void CreateVersionString_ThreeIntegers_ReturnsFormattedString()
        {
            // Act
            var version = VersionHelper.CreateVersionString(4, 7, 122);

            // Assert
            version.Should().Be("4.7.122");
        }

        [Fact]
        public void CreateVersionString_ZeroValues_ReturnsFormattedString()
        {
            // Act
            var version = VersionHelper.CreateVersionString(0, 0, 0);

            // Assert
            version.Should().Be("0.0.0");
        }

        [Fact]
        public void CreateVersionString_LargeValues_ReturnsFormattedString()
        {
            // Act
            var version = VersionHelper.CreateVersionString(999, 888, 777);

            // Assert
            version.Should().Be("999.888.777");
        }

        #endregion

        #region Version Comparison Tests

        [Fact]
        public void Compare_FirstVersionHigherMajor_ReturnsPositive()
        {
            // Act
            var result = VersionHelper.Compare("3.0.0", "2.0.0");

            // Assert
            result.Should().BePositive();
        }

        [Fact]
        public void Compare_SecondVersionHigherMajor_ReturnsNegative()
        {
            // Act
            var result = VersionHelper.Compare("2.0.0", "3.0.0");

            // Assert
            result.Should().BeNegative();
        }

        [Fact]
        public void Compare_FirstVersionHigherMinor_ReturnsPositive()
        {
            // Act
            var result = VersionHelper.Compare("2.1.0", "2.0.0");

            // Assert
            result.Should().BePositive();
        }

        [Fact]
        public void Compare_SecondVersionHigherMinor_ReturnsNegative()
        {
            // Act
            var result = VersionHelper.Compare("2.0.0", "2.1.0");

            // Assert
            result.Should().BeNegative();
        }

        [Fact]
        public void Compare_FirstVersionHigherPatch_ReturnsPositive()
        {
            // Act
            var result = VersionHelper.Compare("2.0.12", "2.0.1");

            // Assert
            result.Should().BePositive();
        }

        [Fact]
        public void Compare_SecondVersionHigherPatch_ReturnsNegative()
        {
            // Act
            var result = VersionHelper.Compare("2.0.1", "2.0.12");

            // Assert
            result.Should().BeNegative();
        }

        [Fact]
        public void Compare_EqualVersions_ReturnsZero()
        {
            // Act
            var result = VersionHelper.Compare("1.2.3", "1.2.3");

            // Assert
            result.Should().Be(0);
        }

        [Fact]
        public void Compare_MinorTakesPrecedenceOverPatch_CorrectOrder()
        {
            // 2.1.1 should be higher than 2.0.12
            var result = VersionHelper.Compare("2.1.1", "2.0.12");

            result.Should().BePositive();
        }

        [Fact]
        public void Compare_MajorTakesPrecedenceOverMinor_CorrectOrder()
        {
            // 3.0.0 should be higher than 2.99.99
            var result = VersionHelper.Compare("3.0.0", "2.99.99");

            result.Should().BePositive();
        }

        [Fact]
        public void IsHigherThan_VersionIsHigher_ReturnsTrue()
        {
            // Act
            var result = VersionHelper.IsHigherThan("5.1.56", "5.0.122");

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void IsHigherThan_VersionIsLower_ReturnsFalse()
        {
            // Act
            var result = VersionHelper.IsHigherThan("2.0.12", "2.1.1");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void IsLowerThan_VersionIsLower_ReturnsTrue()
        {
            // Act
            var result = VersionHelper.IsLowerThan("2.0.12", "2.1.1");

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void IsLowerThan_VersionIsHigher_ReturnsFalse()
        {
            // Act
            var result = VersionHelper.IsLowerThan("5.1.56", "5.0.122");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void IsEqualTo_SameVersions_ReturnsTrue()
        {
            // Act
            var result = VersionHelper.IsEqualTo("1.0.0", "1.0.0");

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void IsEqualTo_DifferentVersions_ReturnsFalse()
        {
            // Act
            var result = VersionHelper.IsEqualTo("1.0.0", "1.0.1");

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region Version Validation Tests

        [Fact]
        public void IsValid_ValidVersionString_ReturnsTrue()
        {
            VersionHelper.IsValid("1.2.3").Should().BeTrue();
        }

        [Fact]
        public void IsValid_ZeroVersion_ReturnsTrue()
        {
            VersionHelper.IsValid("0.0.0").Should().BeTrue();
        }

        [Fact]
        public void IsValid_EmptyString_ReturnsFalse()
        {
            VersionHelper.IsValid("").Should().BeFalse();
        }

        [Fact]
        public void IsValid_NullString_ReturnsFalse()
        {
            VersionHelper.IsValid(null).Should().BeFalse();
        }

        [Fact]
        public void IsValid_MissingPatch_ReturnsFalse()
        {
            VersionHelper.IsValid("1.2").Should().BeFalse();
        }

        [Fact]
        public void IsValid_ExtraPart_ReturnsFalse()
        {
            VersionHelper.IsValid("1.2.3.4").Should().BeFalse();
        }

        [Fact]
        public void IsValid_NonNumericMajor_ReturnsFalse()
        {
            VersionHelper.IsValid("a.2.3").Should().BeFalse();
        }

        [Fact]
        public void IsValid_NonNumericMinor_ReturnsFalse()
        {
            VersionHelper.IsValid("1.b.3").Should().BeFalse();
        }

        [Fact]
        public void IsValid_NonNumericPatch_ReturnsFalse()
        {
            VersionHelper.IsValid("1.2.c").Should().BeFalse();
        }

        [Fact]
        public void IsValid_NegativeNumber_ReturnsFalse()
        {
            VersionHelper.IsValid("-1.2.3").Should().BeFalse();
        }

        [Fact]
        public void IsValid_WhitespaceOnly_ReturnsFalse()
        {
            VersionHelper.IsValid("   ").Should().BeFalse();
        }

        [Fact]
        public void IsValid_LeadingWhitespace_ReturnsFalse()
        {
            VersionHelper.IsValid(" 1.2.3").Should().BeFalse();
        }

        [Fact]
        public void IsValid_TrailingWhitespace_ReturnsFalse()
        {
            VersionHelper.IsValid("1.2.3 ").Should().BeFalse();
        }

        #endregion

        #region Error Handling Tests

        [Fact]
        public void GetMajor_NullInput_ThrowsArgumentNullException()
        {
            Action act = () => VersionHelper.GetMajor(null);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void GetMajor_EmptyInput_ThrowsArgumentException()
        {
            Action act = () => VersionHelper.GetMajor("");
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void GetMajor_InvalidFormat_ThrowsFormatException()
        {
            Action act = () => VersionHelper.GetMajor("invalid");
            act.Should().Throw<FormatException>();
        }

        [Fact]
        public void GetMinor_NullInput_ThrowsArgumentNullException()
        {
            Action act = () => VersionHelper.GetMinor(null);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void GetMinor_InvalidFormat_ThrowsFormatException()
        {
            Action act = () => VersionHelper.GetMinor("1.x.3");
            act.Should().Throw<FormatException>();
        }

        [Fact]
        public void GetPatch_NullInput_ThrowsArgumentNullException()
        {
            Action act = () => VersionHelper.GetPatch(null);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void GetPatch_InvalidFormat_ThrowsFormatException()
        {
            Action act = () => VersionHelper.GetPatch("1.2.x");
            act.Should().Throw<FormatException>();
        }

        [Fact]
        public void ToDisplayFormat_NullInput_ThrowsArgumentNullException()
        {
            Action act = () => VersionHelper.ToDisplayFormat(null);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void ToDisplayFormat_InvalidFormat_ThrowsFormatException()
        {
            Action act = () => VersionHelper.ToDisplayFormat("invalid");
            act.Should().Throw<FormatException>();
        }

        [Fact]
        public void CreateVersionString_NegativeMajor_ThrowsArgumentOutOfRangeException()
        {
            Action act = () => VersionHelper.CreateVersionString(-1, 0, 0);
            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void CreateVersionString_NegativeMinor_ThrowsArgumentOutOfRangeException()
        {
            Action act = () => VersionHelper.CreateVersionString(1, -1, 0);
            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void CreateVersionString_NegativePatch_ThrowsArgumentOutOfRangeException()
        {
            Action act = () => VersionHelper.CreateVersionString(1, 0, -1);
            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void Compare_FirstVersionNull_ThrowsArgumentNullException()
        {
            Action act = () => VersionHelper.Compare(null, "1.0.0");
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Compare_SecondVersionNull_ThrowsArgumentNullException()
        {
            Action act = () => VersionHelper.Compare("1.0.0", null);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Compare_FirstVersionInvalid_ThrowsFormatException()
        {
            Action act = () => VersionHelper.Compare("invalid", "1.0.0");
            act.Should().Throw<FormatException>();
        }

        [Fact]
        public void Compare_SecondVersionInvalid_ThrowsFormatException()
        {
            Action act = () => VersionHelper.Compare("1.0.0", "invalid");
            act.Should().Throw<FormatException>();
        }

        #endregion
    }
}