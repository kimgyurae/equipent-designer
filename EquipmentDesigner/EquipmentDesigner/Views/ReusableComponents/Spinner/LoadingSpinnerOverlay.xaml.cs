using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace EquipmentDesigner.Controls
{
    /// <summary>
    /// Interaction logic for LoadingSpinnerOverlay.xaml
    /// </summary>
    public partial class LoadingSpinnerOverlay : UserControl
    {
        private Storyboard _rotationStoryboard;
        private Storyboard _fadeInStoryboard;
        private Storyboard _fadeOutStoryboard;

        public LoadingSpinnerOverlay()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Cache storyboards
            _rotationStoryboard = (Storyboard)Resources["SpinnerRotation"];
            _fadeInStoryboard = (Storyboard)Resources["FadeIn"];
            _fadeOutStoryboard = (Storyboard)Resources["FadeOut"];

            // Subscribe to service events
            LoadingSpinnerService.Instance.VisibilityChanged += OnVisibilityChanged;

            // Initialize visibility based on current state
            if (LoadingSpinnerService.Instance.IsVisible)
            {
                ShowSpinner();
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            // Unsubscribe from service events
            LoadingSpinnerService.Instance.VisibilityChanged -= OnVisibilityChanged;

            // Stop animations
            _rotationStoryboard?.Stop(this);
        }

        private void OnVisibilityChanged(object sender, bool isVisible)
        {
            // Ensure we're on the UI thread
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => OnVisibilityChanged(sender, isVisible));
                return;
            }

            if (isVisible)
            {
                ShowSpinner();
            }
            else
            {
                HideSpinner();
            }
        }

        private void ShowSpinner()
        {
            Visibility = Visibility.Visible;
            Opacity = 0;

            // Start fade in animation
            _fadeInStoryboard?.Begin(this);

            // Start rotation animation
            _rotationStoryboard?.Begin(this, true);
        }

        private void HideSpinner()
        {
            // Setup fade out completion handler
            if (_fadeOutStoryboard != null)
            {
                EventHandler handler = null;
                handler = (s, e) =>
                {
                    _fadeOutStoryboard.Completed -= handler;
                    Visibility = Visibility.Collapsed;
                    _rotationStoryboard?.Stop(this);
                };

                _fadeOutStoryboard.Completed += handler;
                _fadeOutStoryboard.Begin(this);
            }
            else
            {
                Visibility = Visibility.Collapsed;
                _rotationStoryboard?.Stop(this);
            }
        }
    }
}
