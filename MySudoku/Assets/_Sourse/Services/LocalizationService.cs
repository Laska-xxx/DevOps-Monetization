using System;

namespace Services
{
    public enum GameLanguage
    {
        Russian,
        English
    }

    public interface ILocalizationProvider
    {
        string GetString(string key, GameLanguage language);
    }

    public interface ILocalizationService
    {
        GameLanguage CurrentLanguage { get; }
        event Action LanguageChanged;

        void SetLanguage(GameLanguage language);
        string Get(string key);
    }

    public class LocalizationService : ILocalizationService
    {
        private readonly ILocalizationProvider _provider;

        public GameLanguage CurrentLanguage { get; private set; } = GameLanguage.Russian;
        public event Action LanguageChanged;

        public LocalizationService(ILocalizationProvider provider)
        {
            _provider = provider;
        }

        public void SetLanguage(GameLanguage language)
        {
            if (CurrentLanguage == language) return;
            CurrentLanguage = language;
            LanguageChanged?.Invoke();
        }

        public string Get(string key) => _provider.GetString(key, CurrentLanguage);
    }
}
