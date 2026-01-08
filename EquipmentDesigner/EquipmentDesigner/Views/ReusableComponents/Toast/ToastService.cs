using System;
using System.Collections.ObjectModel;
using System.Windows.Threading;

namespace EquipmentDesigner.Controls
{
    /// <summary>
    /// Singleton service for displaying toast notifications throughout the application.
    /// </summary>
    /// <example>
    /// // Show a simple info toast
    /// ToastService.Instance.ShowInfo("Operation completed");
    ///
    /// // Show a success toast with description
    /// ToastService.Instance.ShowSuccess("File saved", "Your changes have been saved successfully.");
    ///
    /// // Show an error toast
    /// ToastService.Instance.ShowError("Error", "Failed to connect to server.");
    ///
    /// // Show a warning toast
    /// ToastService.Instance.ShowWarning("Warning", "This action cannot be undone.");
    ///
    /// // Show a custom toast
    /// ToastService.Instance.Show("Custom Title", "Description", ToastType.Info, TimeSpan.FromSeconds(5));
    /// </example>
    public sealed class ToastService
    {
        private static readonly Lazy<ToastService> _instance = new Lazy<ToastService>(() => new ToastService());

        /// <summary>
        /// Gets the singleton instance of the ToastService.
        /// </summary>
        public static ToastService Instance => _instance.Value;

        /// <summary>
        /// Default duration for toast messages (3 seconds).
        /// </summary>
        public static readonly TimeSpan DefaultDuration = TimeSpan.FromSeconds(3);

        /// <summary>
        /// Collection of currently visible toasts. Bind this to an ItemsControl.
        /// </summary>
        public ObservableCollection<ToastMessageViewModel> Toasts { get; } = new ObservableCollection<ToastMessageViewModel>();

        /// <summary>
        /// Event raised when a toast is requesting dismissal (before animation).
        /// </summary>
        public event EventHandler<ToastMessageViewModel> ToastDismissing;

        private ToastService()
        {
        }

        /// <summary>
        /// Shows a toast notification with full customization.
        /// </summary>
        /// <param name="title">The toast title (required).</param>
        /// <param name="description">Optional description text.</param>
        /// <param name="type">The type of toast (determines color scheme).</param>
        /// <param name="duration">How long to show the toast. Null for default (3 seconds).</param>
        /// <returns>The created toast view model (can be used to dismiss programmatically).</returns>
        public ToastMessageViewModel Show(string title, string description = null, ToastType type = ToastType.Info, TimeSpan? duration = null)
        {
            var toast = new ToastMessageViewModel(title, description, type);
            return ShowToast(toast, duration ?? DefaultDuration);
        }

        /// <summary>
        /// Shows an info toast.
        /// </summary>
        public ToastMessageViewModel ShowInfo(string title, string description = null, TimeSpan? duration = null)
        {
            return Show(title, description, ToastType.Info, duration);
        }

        /// <summary>
        /// Shows a success toast.
        /// </summary>
        public ToastMessageViewModel ShowSuccess(string title, string description = null, TimeSpan? duration = null)
        {
            return Show(title, description, ToastType.Success, duration);
        }

        /// <summary>
        /// Shows a warning toast.
        /// </summary>
        public ToastMessageViewModel ShowWarning(string title, string description = null, TimeSpan? duration = null)
        {
            return Show(title, description, ToastType.Warning, duration);
        }

        /// <summary>
        /// Shows an error toast.
        /// </summary>
        public ToastMessageViewModel ShowError(string title, string description = null, TimeSpan? duration = null)
        {
            return Show(title, description, ToastType.Error, duration);
        }

        /// <summary>
        /// Dismisses a specific toast.
        /// </summary>
        public void Dismiss(ToastMessageViewModel toast)
        {
            if (toast == null || !Toasts.Contains(toast))
                return;

            ToastDismissing?.Invoke(this, toast);
        }

        /// <summary>
        /// Dismisses all visible toasts.
        /// </summary>
        public void DismissAll()
        {
            // Create a copy to avoid collection modification during iteration
            var toastsCopy = new ToastMessageViewModel[Toasts.Count];
            Toasts.CopyTo(toastsCopy, 0);

            foreach (var toast in toastsCopy)
            {
                Dismiss(toast);
            }
        }

        /// <summary>
        /// Removes a toast from the collection (called after dismiss animation completes).
        /// </summary>
        internal void RemoveToast(ToastMessageViewModel toast)
        {
            if (toast != null && Toasts.Contains(toast))
            {
                Toasts.Remove(toast);
            }
        }

        private ToastMessageViewModel ShowToast(ToastMessageViewModel toast, TimeSpan duration)
        {
            // Subscribe to dismiss request from the toast itself (X button)
            toast.DismissRequested += (s, e) => Dismiss(toast);

            // Add to collection (will appear at top due to ItemsControl ordering)
            // New toasts are inserted at the beginning to appear at the top
            Toasts.Insert(0, toast);

            // Set up auto-dismiss timer
            var timer = new DispatcherTimer
            {
                Interval = duration
            };

            timer.Tick += (s, e) =>
            {
                timer.Stop();
                Dismiss(toast);
            };

            timer.Start();

            return toast;
        }
    }
}
