using System.Windows;
using System.Windows.Controls;

namespace EquipmentDesigner.Controls
{
    /// <summary>
    /// Container control that hosts multiple toast messages.
    /// Place this at the top of your MainWindow for proper z-ordering.
    /// </summary>
    public partial class ToastContainer : UserControl
    {
        public ToastContainer()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Bind to the ToastService's collection
            ToastsItemsControl.ItemsSource = ToastService.Instance.Toasts;

            // Subscribe to dismiss requests
            ToastService.Instance.ToastDismissing += OnToastDismissing;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            ToastService.Instance.ToastDismissing -= OnToastDismissing;
        }

        private void OnToastDismissing(object sender, ToastMessageViewModel toast)
        {
            // Find the visual element for this toast and play dismiss animation
            var container = ToastsItemsControl.ItemContainerGenerator.ContainerFromItem(toast) as ContentPresenter;
            if (container != null)
            {
                // Find the ToastMessage control within the ContentPresenter
                var toastControl = FindVisualChild<ToastMessage>(container);
                if (toastControl != null)
                {
                    toastControl.DismissAnimationCompleted += (s, e) =>
                    {
                        // Remove from collection after animation completes
                        ToastService.Instance.RemoveToast(toast);
                    };
                    toastControl.PlayDismissAnimation();
                    return;
                }
            }

            // Fallback: remove immediately if visual not found
            ToastService.Instance.RemoveToast(toast);
        }

        private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);

                if (child is T result)
                    return result;

                var childResult = FindVisualChild<T>(child);
                if (childResult != null)
                    return childResult;
            }
            return null;
        }
    }
}
