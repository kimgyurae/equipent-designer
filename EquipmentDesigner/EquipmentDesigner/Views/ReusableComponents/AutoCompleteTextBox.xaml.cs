using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EquipmentDesigner.Controls
{
    /// <summary>
    /// AutoComplete TextBox control with suggestions dropdown.
    /// Supports custom input beyond suggestions list.
    /// </summary>
    public partial class AutoCompleteTextBox : UserControl
    {
        private bool _isUpdatingText;
        private bool _suppressPopup;

        public AutoCompleteTextBox()
        {
            InitializeComponent();
            FilteredSuggestions = new ObservableCollection<string>();
            
            // Directly set ItemsSource to ensure binding works
            Loaded += (s, e) =>
            {
                SuggestionsList.ItemsSource = FilteredSuggestions;
            };
        }

        #region Dependency Properties

        /// <summary>
        /// The text value of the control.
        /// </summary>
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(AutoCompleteTextBox),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnTextChanged));

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        /// <summary>
        /// The collection of suggestions to show.
        /// Uses IEnumerable to support XAML x:Array binding.
        /// </summary>
        public static readonly DependencyProperty SuggestionsProperty =
            DependencyProperty.Register(nameof(Suggestions), typeof(IEnumerable), typeof(AutoCompleteTextBox),
                new PropertyMetadata(null, OnSuggestionsChanged));

        public IEnumerable Suggestions
        {
            get => (IEnumerable)GetValue(SuggestionsProperty);
            set => SetValue(SuggestionsProperty, value);
        }

        /// <summary>
        /// Height of the input textbox.
        /// </summary>
        public static readonly DependencyProperty InputHeightProperty =
            DependencyProperty.Register(nameof(InputHeight), typeof(double), typeof(AutoCompleteTextBox),
                new PropertyMetadata(32.0));

        public double InputHeight
        {
            get => (double)GetValue(InputHeightProperty);
            set => SetValue(InputHeightProperty, value);
        }

        /// <summary>
        /// Padding of the input textbox.
        /// </summary>
        public static readonly DependencyProperty InputPaddingProperty =
            DependencyProperty.Register(nameof(InputPadding), typeof(Thickness), typeof(AutoCompleteTextBox),
                new PropertyMetadata(new Thickness(8, 4, 8, 4)));

        public Thickness InputPadding
        {
            get => (Thickness)GetValue(InputPaddingProperty);
            set => SetValue(InputPaddingProperty, value);
        }

        /// <summary>
        /// Background of the input textbox.
        /// </summary>
        public static readonly DependencyProperty InputBackgroundProperty =
            DependencyProperty.Register(nameof(InputBackground), typeof(System.Windows.Media.Brush), typeof(AutoCompleteTextBox),
                new PropertyMetadata(System.Windows.Media.Brushes.White));

        public System.Windows.Media.Brush InputBackground
        {
            get => (System.Windows.Media.Brush)GetValue(InputBackgroundProperty);
            set => SetValue(InputBackgroundProperty, value);
        }

        /// <summary>
        /// Corner radius for the input textbox border.
        /// </summary>
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(AutoCompleteTextBox),
                new PropertyMetadata(new CornerRadius(6)));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        #endregion

        /// <summary>
        /// Filtered suggestions based on current text input.
        /// </summary>
        public ObservableCollection<string> FilteredSuggestions { get; }

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AutoCompleteTextBox control && !control._isUpdatingText)
            {
                control.UpdateFilteredSuggestions();
            }
        }

        private static void OnSuggestionsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AutoCompleteTextBox control)
            {
                control.UpdateFilteredSuggestions();
            }
        }

        private void UpdateFilteredSuggestions()
        {
            FilteredSuggestions.Clear();

            if (Suggestions == null)
                return;

            var text = Text ?? string.Empty;
            
            // Convert IEnumerable to string list, handling various source types
            var allSuggestions = new List<string>();
            foreach (var item in Suggestions)
            {
                if (item != null)
                {
                    allSuggestions.Add(item.ToString());
                }
            }

            var filtered = string.IsNullOrEmpty(text)
                ? allSuggestions
                : allSuggestions.Where(s => s.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0).ToList();

            foreach (var suggestion in filtered)
            {
                FilteredSuggestions.Add(suggestion);
            }
        }

        private void InputTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (!_suppressPopup)
            {
                UpdateFilteredSuggestions();
                ShowPopupIfHasSuggestions();
            }
        }

        private void InputTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // Delay closing to allow click on suggestion
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (!SuggestionsList.IsMouseOver)
                {
                    SuggestionsPopup.IsOpen = false;
                }
            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        private void InputTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdatingText)
                return;

            UpdateFilteredSuggestions();
            ShowPopupIfHasSuggestions();
        }

        private void InputTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!SuggestionsPopup.IsOpen)
            {
                if (e.Key == Key.Down && FilteredSuggestions.Any())
                {
                    ShowPopupIfHasSuggestions();
                    SuggestionsList.SelectedIndex = 0;
                    e.Handled = true;
                }
                return;
            }

            switch (e.Key)
            {
                case Key.Down:
                    if (SuggestionsList.SelectedIndex < FilteredSuggestions.Count - 1)
                    {
                        SuggestionsList.SelectedIndex++;
                        SuggestionsList.ScrollIntoView(SuggestionsList.SelectedItem);
                    }
                    e.Handled = true;
                    break;

                case Key.Up:
                    if (SuggestionsList.SelectedIndex > 0)
                    {
                        SuggestionsList.SelectedIndex--;
                        SuggestionsList.ScrollIntoView(SuggestionsList.SelectedItem);
                    }
                    e.Handled = true;
                    break;

                case Key.Enter:
                case Key.Tab:
                    if (SuggestionsList.SelectedItem != null)
                    {
                        SelectSuggestion(SuggestionsList.SelectedItem.ToString());
                        if (e.Key == Key.Enter)
                            e.Handled = true;
                    }
                    SuggestionsPopup.IsOpen = false;
                    break;

                case Key.Escape:
                    SuggestionsPopup.IsOpen = false;
                    e.Handled = true;
                    break;
            }
        }

        private void SuggestionsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Selection change is handled by mouse click or keyboard
        }

        private void SuggestionsList_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (SuggestionsList.SelectedItem != null)
            {
                SelectSuggestion(SuggestionsList.SelectedItem.ToString());
                SuggestionsPopup.IsOpen = false;
                InputTextBox.Focus();
            }
        }

        private void SelectSuggestion(string suggestion)
        {
            _isUpdatingText = true;
            _suppressPopup = true;
            try
            {
                Text = suggestion;
                InputTextBox.CaretIndex = suggestion.Length;
            }
            finally
            {
                _isUpdatingText = false;
                _suppressPopup = false;
            }
        }

        private void ShowPopupIfHasSuggestions()
        {
            if (_suppressPopup)
                return;

            SuggestionsPopup.IsOpen = FilteredSuggestions.Any() && InputTextBox.IsFocused;
        }
    }
}