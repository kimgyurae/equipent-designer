using System;

namespace EquipmentDesigner.Views.ReusableComponents.Spinner
{
    /// <summary>
    /// Singleton service for displaying a global loading spinner throughout the application.
    /// </summary>
    /// <example>
    /// // Show loading spinner
    /// LoadingSpinnerService.Instance.Show();
    ///
    /// // Hide loading spinner
    /// LoadingSpinnerService.Instance.Hide();
    ///
    /// // Usage with async operations
    /// LoadingSpinnerService.Instance.Show();
    /// try {
    ///     await SomeAsyncOperation();
    /// } finally {
    ///     LoadingSpinnerService.Instance.Hide();
    /// }
    /// </example>
    public sealed class LoadingSpinnerService
    {
        private static readonly Lazy<LoadingSpinnerService> _instance =
            new Lazy<LoadingSpinnerService>(() => new LoadingSpinnerService());

        /// <summary>
        /// Gets the singleton instance of the LoadingSpinnerService.
        /// </summary>
        public static LoadingSpinnerService Instance => _instance.Value;

        /// <summary>
        /// Event raised when the spinner visibility changes.
        /// </summary>
        public event EventHandler<bool> VisibilityChanged;

        /// <summary>
        /// Gets whether the spinner is currently visible.
        /// </summary>
        public bool IsVisible { get; private set; }

        private LoadingSpinnerService()
        {
        }

        /// <summary>
        /// Shows the loading spinner overlay.
        /// </summary>
        public void Show()
        {
            if (IsVisible)
                return;

            IsVisible = true;
            VisibilityChanged?.Invoke(this, true);
        }

        /// <summary>
        /// Hides the loading spinner overlay.
        /// </summary>
        public void Hide()
        {
            if (!IsVisible)
                return;

            IsVisible = false;
            VisibilityChanged?.Invoke(this, false);
        }
    }
}
