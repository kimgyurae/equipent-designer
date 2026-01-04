using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using WPFLocalizeExtension.Engine;

namespace EquipmentDesigner.Services
{
    /// <summary>
    /// Service for managing runtime language switching using WPFLocalizeExtension.
    /// Supports Korean and English with fallback to English.
    /// </summary>
    public sealed class LanguageService : INotifyPropertyChanged
    {
        private static readonly Lazy<LanguageService> _instance = new Lazy<LanguageService>(() => new LanguageService());

        public static LanguageService Instance => _instance.Value;

        public event PropertyChangedEventHandler PropertyChanged;

        private LanguageOption _selectedLanguage;

        private LanguageService()
        {
            AvailableLanguages = new ObservableCollection<LanguageOption>
            {
                new LanguageOption { Code = "ko", DisplayName = "한국어", Culture = new CultureInfo("ko-KR") },
                new LanguageOption { Code = "en", DisplayName = "English", Culture = new CultureInfo("en-US") }
            };

            // Default language is Korean
            _selectedLanguage = AvailableLanguages[0];
        }

        /// <summary>
        /// Available languages for selection.
        /// </summary>
        public ObservableCollection<LanguageOption> AvailableLanguages { get; }

        /// <summary>
        /// Currently selected language.
        /// </summary>
        public LanguageOption SelectedLanguage
        {
            get => _selectedLanguage;
            set
            {
                if (_selectedLanguage != value && value != null)
                {
                    _selectedLanguage = value;
                    ApplyLanguage(value.Culture);
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Initialize the language service with default settings.
        /// Default language is Korean, fallback is English.
        /// </summary>
        public void Initialize()
        {
            // Configure WPFLocalizeExtension
            LocalizeDictionary.Instance.SetCurrentThreadCulture = true;
            LocalizeDictionary.Instance.MissingKeyEvent += OnMissingKey;

            // Set default culture to Korean
            ApplyLanguage(new CultureInfo("ko-KR"));
        }

        /// <summary>
        /// Change the application language at runtime.
        /// </summary>
        /// <param name="languageCode">Language code (ko or en)</param>
        public void ChangeLanguage(string languageCode)
        {
            foreach (var lang in AvailableLanguages)
            {
                if (lang.Code == languageCode)
                {
                    SelectedLanguage = lang;
                    break;
                }
            }
        }

        private void ApplyLanguage(CultureInfo culture)
        {
            LocalizeDictionary.Instance.Culture = culture;
        }

        private void OnMissingKey(object sender, MissingKeyEventArgs e)
        {
            // Fallback to English when key is missing
            // The library handles this automatically with the base .resx file
            System.Diagnostics.Debug.WriteLine($"Missing localization key: {e.Key}");
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// Represents a selectable language option.
    /// </summary>
    public class LanguageOption
    {
        public string Code { get; set; }
        public string DisplayName { get; set; }
        public CultureInfo Culture { get; set; }

        public override string ToString() => DisplayName;
    }
}
