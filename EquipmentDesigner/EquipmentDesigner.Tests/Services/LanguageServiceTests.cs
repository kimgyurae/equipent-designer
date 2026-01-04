using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using EquipmentDesigner.Services;
using FluentAssertions;
using Xunit;

namespace EquipmentDesigner.Tests.Services
{
    public class LanguageServiceTests
    {
        #region Singleton Pattern Tests

        [Fact]
        public void Instance_WhenAccessed_ReturnsNonNullInstance()
        {
            var instance = LanguageService.Instance;
            instance.Should().NotBeNull();
        }

        [Fact]
        public void Instance_WhenAccessedMultipleTimes_ReturnsSameInstance()
        {
            var instance1 = LanguageService.Instance;
            var instance2 = LanguageService.Instance;
            instance1.Should().BeSameAs(instance2);
        }

        #endregion

        #region Available Languages Tests

        [Fact]
        public void AvailableLanguages_WhenAccessed_ReturnsNonEmptyCollection()
        {
            var languages = LanguageService.Instance.AvailableLanguages;
            languages.Should().NotBeEmpty();
        }

        [Fact]
        public void AvailableLanguages_WhenAccessed_ContainsKorean()
        {
            var languages = LanguageService.Instance.AvailableLanguages;
            languages.Should().Contain(lang => lang.Code == "ko");
        }

        [Fact]
        public void AvailableLanguages_WhenAccessed_ContainsEnglish()
        {
            var languages = LanguageService.Instance.AvailableLanguages;
            languages.Should().Contain(lang => lang.Code == "en");
        }

        [Fact]
        public void AvailableLanguages_KoreanOption_HasCorrectDisplayName()
        {
            var korean = LanguageService.Instance.AvailableLanguages
                .FirstOrDefault(lang => lang.Code == "ko");
            korean.Should().NotBeNull();
            korean.DisplayName.Should().Be("한국어");
        }

        [Fact]
        public void AvailableLanguages_EnglishOption_HasCorrectDisplayName()
        {
            var english = LanguageService.Instance.AvailableLanguages
                .FirstOrDefault(lang => lang.Code == "en");
            english.Should().NotBeNull();
            english.DisplayName.Should().Be("English");
        }

        [Fact]
        public void AvailableLanguages_KoreanOption_HasCorrectCulture()
        {
            var korean = LanguageService.Instance.AvailableLanguages
                .FirstOrDefault(lang => lang.Code == "ko");
            korean.Should().NotBeNull();
            korean.Culture.Name.Should().Be("ko-KR");
        }

        [Fact]
        public void AvailableLanguages_EnglishOption_HasCorrectCulture()
        {
            var english = LanguageService.Instance.AvailableLanguages
                .FirstOrDefault(lang => lang.Code == "en");
            english.Should().NotBeNull();
            english.Culture.Name.Should().Be("en-US");
        }

        #endregion

        #region SelectedLanguage Tests

        [Fact]
        public void SelectedLanguage_WhenAccessed_ReturnsNonNullValue()
        {
            var selectedLanguage = LanguageService.Instance.SelectedLanguage;
            selectedLanguage.Should().NotBeNull();
        }

        [Fact]
        public void SelectedLanguage_WhenSet_UpdatesValue()
        {
            var service = LanguageService.Instance;
            var english = service.AvailableLanguages.FirstOrDefault(l => l.Code == "en");

            service.SelectedLanguage = english;

            service.SelectedLanguage.Should().Be(english);
        }

        [Fact]
        public void SelectedLanguage_WhenSetToNull_DoesNotUpdate()
        {
            var service = LanguageService.Instance;
            var originalLanguage = service.SelectedLanguage;

            service.SelectedLanguage = null;

            service.SelectedLanguage.Should().Be(originalLanguage);
        }

        [Fact]
        public void SelectedLanguage_WhenSetToSameValue_DoesNotTriggerChange()
        {
            var service = LanguageService.Instance;
            var currentLanguage = service.SelectedLanguage;
            var propertyChangedRaised = false;

            service.PropertyChanged += (s, e) => propertyChangedRaised = true;
            service.SelectedLanguage = currentLanguage;

            propertyChangedRaised.Should().BeFalse();
        }

        #endregion

        #region ChangeLanguage Tests

        [Fact]
        public void ChangeLanguage_WithKoreanCode_SelectsKoreanLanguage()
        {
            var service = LanguageService.Instance;

            service.ChangeLanguage("ko");

            service.SelectedLanguage.Code.Should().Be("ko");
        }

        [Fact]
        public void ChangeLanguage_WithEnglishCode_SelectsEnglishLanguage()
        {
            var service = LanguageService.Instance;

            service.ChangeLanguage("en");

            service.SelectedLanguage.Code.Should().Be("en");
        }

        [Fact]
        public void ChangeLanguage_WithInvalidCode_DoesNotChangeSelection()
        {
            var service = LanguageService.Instance;
            var originalLanguage = service.SelectedLanguage;

            service.ChangeLanguage("invalid-code");

            service.SelectedLanguage.Should().Be(originalLanguage);
        }

        #endregion

        #region INotifyPropertyChanged Tests

        [Fact]
        public void SelectedLanguage_WhenChanged_RaisesPropertyChangedEvent()
        {
            var service = LanguageService.Instance;
            var korean = service.AvailableLanguages.FirstOrDefault(l => l.Code == "ko");
            var english = service.AvailableLanguages.FirstOrDefault(l => l.Code == "en");

            // Ensure we're starting with a different language
            service.SelectedLanguage = korean;

            string changedPropertyName = null;
            service.PropertyChanged += (s, e) => changedPropertyName = e.PropertyName;

            service.SelectedLanguage = english;

            changedPropertyName.Should().Be(nameof(LanguageService.SelectedLanguage));
        }

        [Fact]
        public void Service_ImplementsINotifyPropertyChanged()
        {
            var service = LanguageService.Instance;
            service.Should().BeAssignableTo<INotifyPropertyChanged>();
        }

        #endregion

        #region LanguageOption Tests

        [Fact]
        public void LanguageOption_ToString_ReturnsDisplayName()
        {
            var korean = LanguageService.Instance.AvailableLanguages
                .FirstOrDefault(lang => lang.Code == "ko");

            korean.ToString().Should().Be("한국어");
        }

        #endregion
    }
}
