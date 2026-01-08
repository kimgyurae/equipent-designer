using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using EquipmentDesigner.Resources;

namespace EquipmentDesigner.Controls
{
    /// <summary>
    /// A reusable confirmation dialog for various confirmation operations.
    /// Requires user to type a confirmation keyword to confirm the action.
    /// </summary>
    public partial class ConfirmDialog : Window
    {
        private string _confirmationKeyword = "delete";
        private static readonly Brush DefaultBorderBrush = (Brush)Application.Current.FindResource("Brush.Border.Primary");
        private static readonly Brush ConfirmedBorderBrush = (Brush)Application.Current.FindResource("Brush.Status.Danger");

        /// <summary>
        /// Gets whether the user confirmed the action.
        /// </summary>
        public bool IsConfirmed { get; private set; }

        /// <summary>
        /// Gets or sets the dialog title.
        /// </summary>
        public string DialogTitle
        {
            get => TitleText.Text;
            set
            {
                TitleText.Text = value;
                Title = value;
            }
        }

        /// <summary>
        /// Gets or sets the subtitle (optional). Set to null or empty to hide.
        /// </summary>
        public string Subtitle
        {
            get => SubtitleText.Text;
            set
            {
                SubtitleText.Text = value;
                SubtitleText.Visibility = string.IsNullOrEmpty(value)
                    ? Visibility.Collapsed
                    : Visibility.Visible;
            }
        }

        /// <summary>
        /// Gets or sets the description text (supports multiline).
        /// </summary>
        public string Description
        {
            get => DescriptionText.Text;
            set => DescriptionText.Text = value;
        }

        /// <summary>
        /// Gets or sets the keyword that must be typed to confirm.
        /// Default is "delete".
        /// </summary>
        public string ConfirmationKeyword
        {
            get => _confirmationKeyword;
            set
            {
                _confirmationKeyword = value ?? "delete";
                KeywordText.Text = _confirmationKeyword;
                UpdatePlaceholder();
            }
        }

        /// <summary>
        /// Gets or sets the confirm button text.
        /// Default is "Delete".
        /// </summary>
        public string ConfirmText
        {
            get => ConfirmButtonText.Text;
            set => ConfirmButtonText.Text = value;
        }

        /// <summary>
        /// Gets or sets the cancel button text.
        /// Default is "Cancel".
        /// </summary>
        public string CancelText
        {
            get => CancelButtonText.Text;
            set => CancelButtonText.Text = value;
        }

        /// <summary>
        /// Initializes a new instance of the ConfirmDialog with default values.
        /// </summary>
        public ConfirmDialog()
        {
            InitializeComponent();
            SetDefaults();
        }

        /// <summary>
        /// Initializes a new instance of the ConfirmDialog with specified parameters.
        /// </summary>
        /// <param name="title">The dialog title.</param>
        /// <param name="description">The description text (supports multiline).</param>
        /// <param name="confirmationKeyword">The keyword that must be typed to confirm. Default is "delete".</param>
        /// <param name="subtitle">Optional subtitle text.</param>
        public ConfirmDialog(
            string title,
            string description,
            string confirmationKeyword = "delete",
            string subtitle = null) : this()
        {
            DialogTitle = title;
            Description = description;
            ConfirmationKeyword = confirmationKeyword;
            Subtitle = subtitle;
        }

        private void SetDefaults()
        {
            // Set default texts
            DialogTitle = Strings.ConfirmDialog_DefaultTitle;
            Description = Strings.ConfirmDialog_DefaultDescription;
            ConfirmText = Strings.ConfirmDialog_DefaultConfirmButton;
            CancelText = Strings.Common_Cancel;
            ConfirmationKeyword = "delete";
        }

        private void UpdatePlaceholder()
        {
            PlaceholderText.Text = string.Format(
                Strings.ConfirmDialog_PlaceholderFormat,
                _confirmationKeyword);
        }

        private void ConfirmationTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var text = ConfirmationTextBox.Text;

            // Update placeholder visibility
            PlaceholderText.Visibility = string.IsNullOrEmpty(text)
                ? Visibility.Visible
                : Visibility.Collapsed;

            // Check if confirmation keyword is typed correctly
            var isConfirmed = text.Equals(_confirmationKeyword, System.StringComparison.OrdinalIgnoreCase);

            // Enable confirm button only when keyword is typed
            ConfirmButton.IsEnabled = isConfirmed;

            // Change border color when confirmed
            InputBorder.BorderBrush = isConfirmed ? ConfirmedBorderBrush : DefaultBorderBrush;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            IsConfirmed = false;
            DialogResult = false;
            Close();
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            IsConfirmed = true;
            DialogResult = true;
            Close();
        }
    }
}