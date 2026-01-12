using System;
using System.Windows;

namespace EquipmentDesigner.Controls
{
    /// <summary>
    /// Singleton service for displaying a global loading spinner throughout the application.
    /// Uses a Topmost window to ensure the spinner is always visible above all other windows.
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
        /// Kept for backward compatibility with existing subscribers.
        /// </summary>
        public event EventHandler<bool> VisibilityChanged;

        /// <summary>
        /// Gets whether the spinner is currently visible.
        /// </summary>
        public bool IsVisible { get; private set; }

        private LoadingSpinnerWindow _spinnerWindow;
        private readonly object _lock = new object();

        private LoadingSpinnerService()
        {
        }

        /// <summary>
        /// Shows the loading spinner overlay as a topmost window.
        /// </summary>
        public void Show()
        {
            if (IsVisible)
                return;

            IsVisible = true;

            // Ensure we're on the UI thread
            if (Application.Current?.Dispatcher != null && !Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.Invoke(ShowSpinnerWindow);
            }
            else
            {
                ShowSpinnerWindow();
            }

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

            // Ensure we're on the UI thread
            if (Application.Current?.Dispatcher != null && !Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.Invoke(HideSpinnerWindow);
            }
            else
            {
                HideSpinnerWindow();
            }

            VisibilityChanged?.Invoke(this, false);
        }

        private void ShowSpinnerWindow()
        {
            lock (_lock)
            {
                if (_spinnerWindow == null)
                {
                    _spinnerWindow = new LoadingSpinnerWindow();
                }

                _spinnerWindow.ShowSpinner();
            }
        }

        private void HideSpinnerWindow()
        {
            lock (_lock)
            {
                _spinnerWindow?.HideSpinner();
            }
        }
    }
}