using System.Collections.Generic;
using System.Linq;

namespace EquipmentDesigner.Models
{
    /// <summary>
    /// Result of node configuration validation
    /// </summary>
    public class NodeValidationResult
    {
        /// <summary>
        /// Whether the node configuration is valid
        /// </summary>
        public bool IsValid => !Errors.Any();

        /// <summary>
        /// List of validation error messages
        /// </summary>
        public List<string> Errors { get; } = new List<string>();

        /// <summary>
        /// Node ID that was validated
        /// </summary>
        public string NodeId { get; set; }

        /// <summary>
        /// Node type that was validated
        /// </summary>
        public UMLNodeType NodeType { get; set; }

        /// <summary>
        /// Creates a valid result
        /// </summary>
        public static NodeValidationResult Valid(string nodeId, UMLNodeType nodeType)
        {
            return new NodeValidationResult
            {
                NodeId = nodeId,
                NodeType = nodeType
            };
        }

        /// <summary>
        /// Creates an invalid result with error messages
        /// </summary>
        public static NodeValidationResult Invalid(string nodeId, UMLNodeType nodeType, params string[] errors)
        {
            var result = new NodeValidationResult
            {
                NodeId = nodeId,
                NodeType = nodeType
            };
            result.Errors.AddRange(errors);
            return result;
        }

        /// <summary>
        /// Adds an error message
        /// </summary>
        public void AddError(string error)
        {
            Errors.Add(error);
        }
    }
}
