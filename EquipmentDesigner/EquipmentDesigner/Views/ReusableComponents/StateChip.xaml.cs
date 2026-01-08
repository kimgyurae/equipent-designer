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
        /// </summary>
        public static readonly DependencyProperty ChipBackgroundProperty = DependencyProperty.Register(
            nameof(ChipBackground),
            typeof(Brush),
            typeof(StateChip),
            new PropertyMetadata(Brushes.Gray));

        public Brush ChipBackground
        {
            get => (Brush)GetValue(ChipBackgroundProperty);
            set => SetValue(ChipBackgroundProperty, value);
        }

        /// <summary>
        /// The foreground brush for the chip text.
        /// </summary>
        public static readonly DependencyProperty ChipForegroundProperty = DependencyProperty.Register(
            nameof(ChipForeground),
            typeof(Brush),
            typeof(StateChip),
            new PropertyMetadata(Brushes.White));

        public Brush ChipForeground
        {
            get => (Brush)GetValue(ChipForegroundProperty);
            set => SetValue(ChipForegroundProperty, value);
        }
    }
}
