using System.Collections.Generic;
using System.IO;
using Zenject;
using Data;
using Signals;

namespace Services
{
    public interface IThemeService
    {
        ColorThemeSO Current { get; }
        void SetTheme(ColorThemeSO theme);
    }

    public class ThemeService : IThemeService
    {
        private readonly IReadOnlyList<ColorThemeSO> _availableThemes;
        private readonly SignalBus _signalBus;
        private readonly string _themeFilePath;

        public ColorThemeSO Current { get; private set; }

        public ThemeService(IReadOnlyList<ColorThemeSO> availableThemes, SignalBus signalBus, string themeFilePath)
        {
            _availableThemes = availableThemes;
            _signalBus = signalBus;
            _themeFilePath = themeFilePath;

            string savedThemeId = LoadSavedThemeId();
            Current = FindById(savedThemeId) ?? _availableThemes[0];
        }

        public void SetTheme(ColorThemeSO theme)
        {
            if (Current == theme) return;

            Current = theme;
            SaveThemeId(theme.ThemeId);
            _signalBus.Fire(new ThemeChangedSignal { Theme = theme });
        }

        private ColorThemeSO FindById(string themeId)
        {
            if (string.IsNullOrEmpty(themeId)) return null;

            foreach (var theme in _availableThemes)
                if (theme.ThemeId == themeId)
                    return theme;

            return null;
        }

        private string LoadSavedThemeId()
        {
            if (!File.Exists(_themeFilePath)) return null;

            try
            {
                using var stream = new FileStream(_themeFilePath, FileMode.Open);
                using var reader = new BinaryReader(stream);
                return reader.ReadString();
            }
            catch (IOException)
            {
                return null;
            }
        }

        private void SaveThemeId(string themeId)
        {
            using var stream = new FileStream(_themeFilePath, FileMode.Create);
            using var writer = new BinaryWriter(stream);
            writer.Write(themeId);
        }
    }
}
