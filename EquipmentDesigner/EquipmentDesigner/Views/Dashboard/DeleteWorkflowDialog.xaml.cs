using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace EquipmentDesigner.Views.Dashboard
{
    /// <summary>
    /// Interaction logic for DeleteWorkflowDialog.xaml
    /// </summary>
    public partial class DeleteWorkflowDialog : Window
    {
        private const string ConfirmationWord = "delete";
        private static readonly Brush DefaultBorderBrush = (Brush)Application.Current.FindResource("Brush.Border.Primary");
        private static readonly Brush ConfirmedBorderBrush = (Brush)Application.Current.FindResource("Brush.Status.Danger");

        /// <summary>
        /// Gets whether the user confirmed the deletion.
        /// </summary>
        public bool IsConfirmed { get; private set; }

        public DeleteWorkflowDialog()
        {
            InitializeComponent();
        }

        private void ConfirmationTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var text = ConfirmationTextBox.Text;

            // Update placeholder visibility
            PlaceholderText.Visibility = string.IsNullOrEmpty(text)
                ? Visibility.Visible
                : Visibility.Collapsed;

            // Check if "delete" is typed correctly
            var isConfirmed = text.Equals(ConfirmationWord, System.StringComparison.OrdinalIgnoreCase);

            // Enable delete button only when "delete" is typed
            DeleteButton.IsEnabled = isConfirmed;

            // Change border color to red when confirmed
            InputBorder.BorderBrush = isConfirmed ? ConfirmedBorderBrush : DefaultBorderBrush;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            IsConfirmed = false;
            DialogResult = false;
            Close();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            IsConfirmed = true;
            DialogResult = true;
            Close();
        }
    }
}