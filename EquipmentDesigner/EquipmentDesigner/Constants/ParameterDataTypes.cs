using System.Collections.Generic;

namespace EquipmentDesigner.Constants
{
    /// <summary>
    /// Provides a collection of common parameter data types for autocomplete suggestions.
    /// Includes C# primitive types and common industrial IO communication data types.
    /// </summary>
    public static class ParameterDataTypes
    {
        /// <summary>
        /// Gets all available parameter data type suggestions.
        /// </summary>
        public static IReadOnlyList<string> All { get; } = new List<string>
        {
            // C# Primitive Types
            "bool",
            "byte",
            "sbyte",
            "char",
            "short",
            "ushort",
            "int",
            "uint",
            "long",
            "ulong",
            "float",
            "double",
            "decimal",
            "string",

            // Bit-level Types (Digital IO)
            "bit",
            "BOOL",        // PLC standard
            "BYTE",        // 8-bit unsigned
            "WORD",        // 16-bit unsigned (PLC)
            "DWORD",       // 32-bit unsigned (PLC)
            "LWORD",       // 64-bit unsigned (PLC)

            // Array Types
            "byte[]",
            "int[]",
            "float[]",
            "double[]",
            "bool[]",
            "string[]",

        }.AsReadOnly();
    }
}
