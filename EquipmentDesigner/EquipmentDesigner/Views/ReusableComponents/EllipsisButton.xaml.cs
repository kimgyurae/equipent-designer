using System.Windows;
using System.Windows.Controls;

namespace EquipmentDesigner.Controls
{
    /// <summary>
    /// Reusable ellipsis (three dots) button for "more options" actions.
    /// </summary>
    public partial class EllipsisButton : UserControl
    {
        /// <summary>
        /// Routed event for button click.
        /// </summary>
        public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent(
            nameof(Click),
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(EllipsisButton));

        /// <summary>
        /// Occurs when the ellipsis button is clicked.
        /// </summary>
        public event RoutedEventHandler Click
        {
            add => AddHandler(ClickEvent, value);
            remove => RemoveHandler(ClickEvent, value);
        }

        public EllipsisButton()
        {
            InitializeComponent();
        }

        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(ClickEvent, this));
        }
    }
}
