using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EquipmentDesigner.Controls
{
    /// <summary>
    /// Reusable white card with hover effects.
    /// Features blue border (Brush.Border.Focus) and shadow on hover.
    /// </summary>
    public partial class DefaultWhiteCard : UserControl
    {
        public DefaultWhiteCard()
        {
            InitializeComponent();
        }

        /// <summary>
        /// The content to display inside the card.
        /// </summary>
        public static readonly DependencyProperty CardContentProperty = DependencyProperty.Register(
            nameof(CardContent),
            typeof(object),
            typeof(DefaultWhiteCard),
            new PropertyMetadata(null));

        public object CardContent
        {
            get => GetValue(CardContentProperty);
            set => SetValue(CardContentProperty, value);
        }

        /// <summary>
        /// The corner radius of the card. Default is 10.
        /// </summary>
        public static readonly DependencyProperty CardCornerRadiusProperty = DependencyProperty.Register(
            nameof(CardCornerRadius),
            typeof(CornerRadius),
            typeof(DefaultWhiteCard),
            new PropertyMetadata(new CornerRadius(10)));

        public CornerRadius CardCornerRadius
        {
            get => (CornerRadius)GetValue(CardCornerRadiusProperty);
            set => SetValue(CardCornerRadiusProperty, value);
        }

        /// <summary>
        /// The padding inside the card. Default is 24.
        /// </summary>
        public static readonly DependencyProperty CardPaddingProperty = DependencyProperty.Register(
            nameof(CardPadding),
            typeof(Thickness),
            typeof(DefaultWhiteCard),
            new PropertyMetadata(new Thickness(24)));

        public Thickness CardPadding
        {
            get => (Thickness)GetValue(CardPaddingProperty);
            set => SetValue(CardPaddingProperty, value);
        }

        /// <summary>
        /// The command to execute when the card is clicked.
        /// </summary>
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
            nameof(Command),
            typeof(ICommand),
            typeof(DefaultWhiteCard),
            new PropertyMetadata(null));

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        /// <summary>
        /// The parameter to pass to the command.
        /// </summary>
        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register(
            nameof(CommandParameter),
            typeof(object),
            typeof(DefaultWhiteCard),
            new PropertyMetadata(null));

        public object CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        /// <summary>
        /// Whether the card is clickable and shows hover effects. Default is true.
        /// </summary>
        public static readonly DependencyProperty IsClickableProperty = DependencyProperty.Register(
            nameof(IsClickable),
            typeof(bool),
            typeof(DefaultWhiteCard),
            new PropertyMetadata(true));

        public bool IsClickable
        {
            get => (bool)GetValue(IsClickableProperty);
            set => SetValue(IsClickableProperty, value);
        }
    }
}