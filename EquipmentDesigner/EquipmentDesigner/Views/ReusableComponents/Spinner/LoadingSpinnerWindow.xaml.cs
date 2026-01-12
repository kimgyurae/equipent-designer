using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace EquipmentDesigner.Controls
{
    /// <summary>
    /// Topmost loading spinner window that displays above all other windows.
    /// Managed by LoadingSpinnerService for global loading indication.
    /// </summary>
    public partial class LoadingSpinnerWindow : Window
    {
        private Storyboard _rotationStoryboard;
        private Storyboard _fadeInStoryboard;
        private Storyboard _fadeOutStoryboard;

        public LoadingSpinnerWindow()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Cache storyboards
            _rotationStoryboard = (Storyboard)Resources["SpinnerRotation"];
            _fadeInStoryboard = (Storyboard)Resources["FadeIn"];
            _fadeOutStoryboard = (Storyboard)Resources["FadeOut"];
        }

        /// <summary>
        /// Shows the spinner with fade-in animation.
        /// </summary>
        public void ShowSpinner()
        {
            Opacity = 0;
            Show();

            // Start fade in animation
            _fadeInStoryboard?.Begin(this);

            // Start rotation animation
            _rotationStoryboard?.Begin(this, true);
        }

        /// <summary>
        /// Hides the spinner with fade-out animation.
        /// </summary>
        public void HideSpinner()
        {
            if (_fadeOutStoryboard != null)
            {
                EventHandler handler = null;
                handler = (s, args) =>
                {
                    _fadeOutStoryboard.Completed -= handler;
                    Hide();
                    _rotationStoryboard?.Stop(this);
                };

                _fadeOutStoryboard.Completed += handler;
                _fadeOutStoryboard.Begin(this);
            }
            else
            {
                Hide();
                _rotationStoryboard?.Stop(this);
            }
        }
    }
}
