using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace EquipmentDesigner.Controls
{
    /// <summary>
    /// Individual toast message control with slide-in/out animations.
    /// </summary>
    public partial class ToastMessage : UserControl
    {
        /// <summary>
        /// Event raised when the dismiss animation completes.
        /// </summary>
        public event EventHandler DismissAnimationCompleted;

        public ToastMessage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Plays the dismiss animation and raises DismissAnimationCompleted when done.
        /// </summary>
        public void PlayDismissAnimation()
        {
            var storyboard = new Storyboard();

            // Slide up animation
            var slideAnimation = new DoubleAnimation
            {
                From = 0,
                To = -50,
                Duration = TimeSpan.FromMilliseconds(200),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };
            Storyboard.SetTarget(slideAnimation, SlideTransform);
            Storyboard.SetTargetProperty(slideAnimation, new PropertyPath("Y"));
            storyboard.Children.Add(slideAnimation);

            // Fade out animation
            var fadeAnimation = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(200)
            };
            Storyboard.SetTarget(fadeAnimation, ToastBorder);
            Storyboard.SetTargetProperty(fadeAnimation, new PropertyPath("Opacity"));
            storyboard.Children.Add(fadeAnimation);

            storyboard.Completed += (s, e) =>
            {
                DismissAnimationCompleted?.Invoke(this, EventArgs.Empty);
            };

            storyboard.Begin();
        }
    }
}
