using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace EquipmentDesigner.Controls
{
    /// <summary>
    /// A reusable chip control for displaying state/type information with customizable colors.
    /// </summary>
    public partial class StateChip : UserControl
    {
        public StateChip()
        {
            InitializeComponent();
        }

        /// <summary>
        /// The text to display in the chip.
        /// </summary>
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(StateChip),
            new PropertyMetadata(string.Empty));

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        /// <summary>
        /// The background brush for the chip.
        /// Default is null - will use Neutral background from resources if not set.
        /// </summary>
        public static readonly DependencyProperty ChipBackgroundProperty = DependencyProperty.Register(
            nameof(ChipBackground),
            typeof(Brush),
            typeof(StateChip),
            new PropertyMetadata(null, OnChipBackgroundChanged));

        private static void OnChipBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is StateChip chip && e.NewValue == null)
            {
                chip.ChipBackground = (Brush)Application.Current.FindResource("Brush.Status.Neutral.Background");
            }
        }

        public Brush ChipBackground
        {
            get => (Brush)GetValue(ChipBackgroundProperty);
            set => SetValue(ChipBackgroundProperty, value);
        }

        /// <summary>
        /// The foreground brush for the chip text.
        /// Default is null - will use Neutral foreground from resources if not set.
        /// </summary>
        public static readonly DependencyProperty ChipForegroundProperty = DependencyProperty.Register(
            nameof(ChipForeground),
            typeof(Brush),
            typeof(StateChip),
            new PropertyMetadata(null, OnChipForegroundChanged));

        private static void OnChipForegroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is StateChip chip && e.NewValue == null)
            {
                chip.ChipForeground = (Brush)Application.Current.FindResource("Brush.Status.Neutral");
            }
        }

        public Brush ChipForeground
        {
            get => (Brush)GetValue(ChipForegroundProperty);
            set => SetValue(ChipForegroundProperty, value);
        }
    }
}