using System;
using System.Text.RegularExpressions;

namespace EquipmentDesigner.Utils
{
    /// <summary>
    /// Provides utility methods for semantic versioning (major.minor.patch) string manipulation.
    /// </summary>
    public static class VersionHelper
    {
        private static readonly Regex VersionPattern = new Regex(@"^\d+\.\d+\.\d+$", RegexOptions.Compiled);

        /// <summary>
        /// Validates if a string is a valid version format.
        /// </summary>
        /// <param name="version">Version string to validate</param>
        /// <returns>True if valid "major.minor.patch" format with non-negative integers</returns>
        public static bool IsValid(string version)
        {
            if (string.IsNullOrEmpty(version))
                return false;

            if (!VersionPattern.IsMatch(version))
                return false;

            var parts = version.Split('.');
            foreach (var part in parts)
            {
                if (!int.TryParse(part, out var value) || value < 0)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Validates version string and throws appropriate exceptions.
        /// </summary>
        private static void ValidateVersion(string version, string paramName)
        {
            if (version == null)
                throw new ArgumentNullException(paramName);

            if (string.IsNullOrEmpty(version))
                throw new ArgumentException("Version string cannot be empty.", paramName);

            if (!IsValid(version))
                throw new FormatException($"Invalid version format: '{version}'. Expected format: 'major.minor.patch' with non-negative integers.");
        }

        /// <summary>
        /// Extracts the major version component from a version string.
        /// </summary>
        /// <param name="version">Version string in format "major.minor.patch"</param>
        /// <returns>The major version as an integer</returns>
        /// <exception cref="ArgumentNullException">Thrown when version is null</exception>
        /// <exception cref="ArgumentException">Thrown when version is empty</exception>
        /// <exception cref="FormatException">Thrown when version format is invalid</exception>
        public static int GetMajor(string version)
        {
            ValidateVersion(version, nameof(version));
            var parts = version.Split('.');
            return int.Parse(parts[0]);
        }

        /// <summary>
        /// Extracts the minor version component from a version string.
        /// </summary>
        /// <param name="version">Version string in format "major.minor.patch"</param>
        /// <returns>The minor version as an integer</returns>
        /// <exception cref="ArgumentNullException">Thrown when version is null</exception>
        /// <exception cref="ArgumentException">Thrown when version is empty</exception>
        /// <exception cref="FormatException">Thrown when version format is invalid</exception>
        public static int GetMinor(string version)
        {
            ValidateVersion(version, nameof(version));
            var parts = version.Split('.');
            return int.Parse(parts[1]);
        }

        /// <summary>
        /// Extracts the patch version component from a version string.
        /// </summary>
        /// <param name="version">Version string in format "major.minor.patch"</param>
        /// <returns>The patch version as an integer</returns>
        /// <exception cref="ArgumentNullException">Thrown when version is null</exception>
        /// <exception cref="ArgumentException">Thrown when version is empty</exception>
        /// <exception cref="FormatException">Thrown when version format is invalid</exception>
        public static int GetPatch(string version)
        {
            ValidateVersion(version, nameof(version));
            var parts = version.Split('.');
            return int.Parse(parts[2]);
        }

        /// <summary>
        /// Converts a version string to display format with "v " prefix.
        /// </summary>
        /// <param name="version">Version string in format "major.minor.patch"</param>
        /// <returns>Display format string "v major.minor.patch"</returns>
        /// <exception cref="ArgumentNullException">Thrown when version is null</exception>
        /// <exception cref="FormatException">Thrown when version format is invalid</exception>
        public static string ToDisplayFormat(string version)
        {
            if (version == null)
                throw new ArgumentNullException(nameof(version));

            if (!IsValid(version))
                throw new FormatException($"Invalid version format: '{version}'. Expected format: 'major.minor.patch' with non-negative integers.");

            return $"v {version}";
        }

        /// <summary>
        /// Creates a version string from major, minor, and patch components.
        /// </summary>
        /// <param name="major">Major version number (must be non-negative)</param>
        /// <param name="minor">Minor version number (must be non-negative)</param>
        /// <param name="patch">Patch version number (must be non-negative)</param>
        /// <returns>Version string in format "major.minor.patch"</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when any parameter is negative</exception>
        public static string CreateVersionString(int major, int minor, int patch)
        {
            if (major < 0)
                throw new ArgumentOutOfRangeException(nameof(major), "Major version cannot be negative.");

            if (minor < 0)
                throw new ArgumentOutOfRangeException(nameof(minor), "Minor version cannot be negative.");

            if (patch < 0)
                throw new ArgumentOutOfRangeException(nameof(patch), "Patch version cannot be negative.");

            return $"{major}.{minor}.{patch}";
        }

        /// <summary>
        /// Compares two version strings.
        /// </summary>
        /// <param name="version1">First version string</param>
        /// <param name="version2">Second version string</param>
        /// <returns>Positive if version1 > version2, negative if version1 &lt; version2, zero if equal</returns>
        /// <exception cref="ArgumentNullException">Thrown when either version is null</exception>
        /// <exception cref="FormatException">Thrown when either version format is invalid</exception>
        public static int Compare(string version1, string version2)
        {
            ValidateVersion(version1, nameof(version1));
            ValidateVersion(version2, nameof(version2));

            var major1 = GetMajor(version1);
            var major2 = GetMajor(version2);
            if (major1 != major2) return major1.CompareTo(major2);

            var minor1 = GetMinor(version1);
            var minor2 = GetMinor(version2);
            if (minor1 != minor2) return minor1.CompareTo(minor2);

            var patch1 = GetPatch(version1);
            var patch2 = GetPatch(version2);
            return patch1.CompareTo(patch2);
        }

        /// <summary>
        /// Determines if version1 is higher than version2.
        /// </summary>
        /// <param name="version1">First version string</param>
        /// <param name="version2">Second version string</param>
        /// <returns>True if version1 > version2</returns>
        public static bool IsHigherThan(string version1, string version2)
        {
            return Compare(version1, version2) > 0;
        }

        /// <summary>
        /// Determines if version1 is lower than version2.
        /// </summary>
        /// <param name="version1">First version string</param>
        /// <param name="version2">Second version string</param>
        /// <returns>True if version1 &lt; version2</returns>
        public static bool IsLowerThan(string version1, string version2)
        {
            return Compare(version1, version2) < 0;
        }

        /// <summary>
        /// Determines if two versions are equal.
        /// </summary>
        /// <param name="version1">First version string</param>
        /// <param name="version2">Second version string</param>
        /// <returns>True if versions are equal</returns>
        public static bool IsEqualTo(string version1, string version2)
        {
            return Compare(version1, version2) == 0;
        }
    }
}
