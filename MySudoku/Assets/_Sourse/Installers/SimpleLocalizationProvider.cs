using Services;

namespace Installers
{
    public class SimpleLocalizationProvider : ILocalizationProvider
    {
        public string GetString(string key, GameLanguage language) => key;
    }
}
