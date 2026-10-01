using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Services;
using Signals;

namespace Controllers
{
    public class BackgroundThemeApplier : MonoBehaviour
    {
        [SerializeField] private Image _backgroundImage;

        private SignalBus _signalBus;

        [Inject] private void Init(IThemeService themeService, SignalBus signalBus)
        {
            _signalBus = signalBus;
            Apply(themeService.Current);
            _signalBus.Subscribe<ThemeChangedSignal>(OnThemeChanged);
        }

        private void OnDestroy()
        {
            _signalBus?.Unsubscribe<ThemeChangedSignal>(OnThemeChanged);
        }

        private void OnThemeChanged(ThemeChangedSignal signal) => Apply(signal.Theme);

        private void Apply(Data.ColorThemeSO theme) => _backgroundImage.color = theme.Background;
    }
}
