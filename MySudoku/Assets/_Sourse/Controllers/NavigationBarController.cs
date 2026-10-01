using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Controllers
{
    public class NavigationBarController : MonoBehaviour
    {
        [SerializeField] private Button _mainMenuButton;
        [SerializeField] private Button _statisticsButton;
        [SerializeField] private Button _settingsButton;

        private IScreenService _screenService;

        [Inject] private void Init(IScreenService screenService) => _screenService = screenService;

        private void Awake()
        {
            _mainMenuButton.onClick.AddListener(OnMainMenuClicked);
            _statisticsButton.onClick.AddListener(OnStatisticsClicked);
            _settingsButton.onClick.AddListener(OnSettingsClicked);
        }

        private void OnMainMenuClicked() => _screenService.ShowExclusive<MainMenuController>();

        private void OnStatisticsClicked() => _screenService.ShowExclusive<StatisticsController>();

        private void OnSettingsClicked() => _screenService.ShowExclusive<SettingsController>();
    }
}
