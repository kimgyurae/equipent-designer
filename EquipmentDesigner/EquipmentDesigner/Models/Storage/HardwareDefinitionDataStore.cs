using System;
using System.Collections.Generic;

namespace EquipmentDesigner.Models
{
    /// <summary>
    /// Data store for hardware definition workflow sessions.
    /// Used for both incomplete and uploaded workflow data.
    /// Persists to JSON files in LocalApplicationData folder.
    /// </summary>
    public class HardwareDefinitionDataStore
    {
        /// <summary>
        /// Data store version for migration support.
        /// </summary>
        public string Version { get; set; } = "1.0";

        /// <summary>
        /// Last saved timestamp.
        /// </summary>
        public DateTime LastSavedAt { get; set; }

        /// <summary>
        /// Workflow sessions list.
        /// </summary>
        public List<WorkflowSessionDto> WorkflowSessions { get; set; } = new List<WorkflowSessionDto>();
    }
}
