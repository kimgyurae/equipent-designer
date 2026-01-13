using System.Windows.Controls;
using EquipmentDesigner.ViewModels;

namespace EquipmentDesigner.Views.Drawboard.Controls
{
    /// <summary>
    /// Popup control displaying rule violations for a selected element.
    /// Shows each violation with its description and expected/actual values.
    /// </summary>
    public partial class ViolationPopup : UserControl
    {
        public ViolationPopup()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the ViewModel for this popup.
        /// </summary>
        public ViolationPopupViewModel ViewModel
        {
            get => DataContext as ViolationPopupViewModel;
            set => DataContext = value;
        }
    }
}