using System.ComponentModel;

namespace EquipmentDesigner.Views.ReusableComponents
{
    /// <summary>
    /// Interface for ViewModels that can show/hide a backdrop overlay.
    /// Inherits from INotifyPropertyChanged to support XAML data binding.
    /// </summary>
    public interface IBackdropHost : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets or sets whether the backdrop should be visible.
        /// </summary>
        bool IsBackdropVisible { get; set; }

        /// <summary>
        /// Shows the backdrop (sets IsBackdropVisible = true).
        /// </summary>
        void ShowBackdrop();

        /// <summary>
        /// Hides the backdrop (sets IsBackdropVisible = false).
        /// </summary>
        void HideBackdrop();
    }
}
