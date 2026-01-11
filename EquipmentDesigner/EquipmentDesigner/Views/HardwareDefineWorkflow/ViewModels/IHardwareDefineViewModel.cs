using System.ComponentModel;

namespace EquipmentDesigner.ViewModels
{
    /// <summary>
    /// Common interface for hardware definition ViewModels.
    /// Enables polymorphic storage in tree nodes.
    /// </summary>
    public interface IHardwareDefineViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Component name (required field).
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Whether all required fields are filled.
        /// </summary>
        bool CanProceedToNext { get; }

        /// <summary>
        /// Number of fields that have been filled.
        /// </summary>
        int FilledFieldCount { get; }

        /// <summary>
        /// Total number of fields in the form.
        /// </summary>
        int TotalFieldCount { get; }

        /// <summary>
        /// Whether the form is editable.
        /// </summary>
        bool IsEditable { get; set; }

        /// <summary>
        /// Version information (e.g., v1.0.0).
        /// </summary>
        string Version { get; set; }

        /// <summary>
        /// Hardware unique identification key - all versions of the same hardware share this key.
        /// If null, Name is used as default (backward compatibility).
        /// </summary>
        string HardwareKey { get; set; }
    }
}