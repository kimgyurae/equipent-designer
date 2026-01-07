using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace EquipmentDesigner.Views.ReusableComponents
{
    /// <summary>
    /// A reusable confirmation dialog for cascading delete operations.
    /// Requires user to type "delete" to confirm the action.
    /// </summary>
    public partial class ConfirmDeleteDialog : Window
    {
        private const string ConfirmationWord = "delete";
        private static readonly Brush DefaultBorderBrush = (Brush)Application.Current.FindResource("Brush.Border.Primary");
        private static readonly Brush ConfirmedBorderBrush = (Brush)Application.Current.FindResource("Brush.Status.Danger");

        /// <summary>
        /// Gets whether the user confirmed the deletion.
        /// </summary>
        public bool IsConfirmed { get; private set; }

        /// <summary>
        /// Gets or sets the name of the item being deleted.
        /// </summary>
        public string ItemName
        {
            get => ItemNameText.Text;
            set => ItemNameText.Text = value;
        }

        /// <summary>
        /// Gets or sets the list of descendant names that will also be deleted.
        /// </summary>
        public IEnumerable<string> DescendantNames
        {
            get => DescendantsListControl.ItemsSource as IEnumerable<string>;
            set
            {
                DescendantsListControl.ItemsSource = value;
                UpdateDescendantsVisibility(value);
            }
        }

        /// <summary>
        /// Initializes a new instance of the ConfirmDeleteDialog.
        /// </summary>
        public ConfirmDeleteDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the ConfirmDeleteDialog with item name and descendants.
        /// </summary>
        /// <param name="itemName">The name of the item being deleted.</param>
        /// <param name="descendantNames">The list of descendant names that will also be deleted.</param>
        public ConfirmDeleteDialog(string itemName, IEnumerable<string> descendantNames) : this()
        {
            ItemName = itemName;
            DescendantNames = descendantNames;
        }

        private void UpdateDescendantsVisibility(IEnumerable<string> descendants)
        {
            bool hasDescendants = descendants != null && HasAnyItems(descendants);
            DescendantsLabel.Visibility = hasDescendants ? Visibility.Visible : Visibility.Collapsed;
            DescendantsBorder.Visibility = hasDescendants ? Visibility.Visible : Visibility.Collapsed;
        }

        private static bool HasAnyItems(IEnumerable<string> items)
        {
            foreach (var _ in items)
            {
                return true;
            }
            return false;
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
