using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace EquipmentDesigner.Views
{
    /// <summary>
    /// Edit mode selection options.
    /// </summary>
    public enum EditModeSelection
    {
        /// <summary>
        /// Edit the original data directly.
        /// </summary>
        DirectEdit,

        /// <summary>
        /// Create a copy and edit the copy.
        /// </summary>
        CreateCopy
    }

    /// <summary>
    /// Dialog for selecting the edit mode when entering edit mode from read-only state.
    /// </summary>
    public partial class EditModeSelectionDialog : Window
    {
        private static readonly Brush DefaultBorderBrush;
        private static readonly Brush SelectedBorderBrush;
        private static readonly Brush SelectedBackgroundBrush;
        private static readonly Brush TransparentBrush;

        static EditModeSelectionDialog()
        {
            DefaultBorderBrush = (Brush)Application.Current.FindResource("Brush.Border.Primary");
            SelectedBorderBrush = new SolidColorBrush(Color.FromRgb(0x2B, 0x7F, 0xFF)); // #2B7FFF
            SelectedBackgroundBrush = new SolidColorBrush(Color.FromRgb(0xEF, 0xF6, 0xFF)); // #EFF6FF
            TransparentBrush = Brushes.Transparent;
        }

        /// <summary>
        /// Gets the selected edit mode.
        /// </summary>
        public EditModeSelection SelectedMode { get; private set; } = EditModeSelection.CreateCopy;

        /// <summary>
        /// Gets whether the user confirmed the selection.
        /// </summary>
        public bool IsConfirmed { get; private set; }

        public EditModeSelectionDialog()
        {
            InitializeComponent();

            // Default selection: CreateCopy (recommended)
            UpdateOptionVisuals();
        }

        private void DirectEditOption_Click(object sender, MouseButtonEventArgs e)
        {
            SelectedMode = EditModeSelection.DirectEdit;
            UpdateOptionVisuals();
        }

        private void CreateCopyOption_Click(object sender, MouseButtonEventArgs e)
        {
            SelectedMode = EditModeSelection.CreateCopy;
            UpdateOptionVisuals();
        }

        private void UpdateOptionVisuals()
        {
            // Direct Edit option
            if (SelectedMode == EditModeSelection.DirectEdit)
            {
                DirectEditOption.BorderBrush = SelectedBorderBrush;
                DirectEditOption.Background = SelectedBackgroundBrush;
                CreateCopyOption.BorderBrush = DefaultBorderBrush;
                CreateCopyOption.Background = TransparentBrush;
            }
            else
            {
                DirectEditOption.BorderBrush = DefaultBorderBrush;
                DirectEditOption.Background = TransparentBrush;
                CreateCopyOption.BorderBrush = SelectedBorderBrush;
                CreateCopyOption.Background = SelectedBackgroundBrush;
            }
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            IsConfirmed = true;
            DialogResult = true;
            Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            IsConfirmed = false;
            DialogResult = false;
            Close();
        }
    }
}
