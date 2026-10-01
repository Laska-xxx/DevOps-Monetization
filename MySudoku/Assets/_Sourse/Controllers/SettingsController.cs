using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

using Presentation.Views;
using Services;

namespace Controllers
{
    public class SettingsController : ScreenControllerBase
    {
        [SerializeField] private Button _tutorialButton;
        [SerializeField] private Button _chillModeButton;
        [SerializeField] private TMP_Text _chillModeButtonText;
        [SerializeField] private Button _languageRussianButton;
        [SerializeField] private Button _languageEnglishButton;

        public bool ChillModeEnabled { get; private set; }

        private ILocalizationService _localization;
        private TutorialController _tutorialController;
        private ChillModeWarningView _chillModeWarning;

        [Inject]
        private void Init(
            ILocalizationService localization,
            TutorialController tutorialController,
            ChillModeWarningView chillModeWarning)
        {
            _localization = localization;
            _tutorialController = tutorialController;
            _chillModeWarning = chillModeWarning;

            UpdateChillModeButtonText();
        }

        private void Awake()
        {
            _tutorialButton.onClick.AddListener(OnTutorialClicked);
            _chillModeButton.onClick.AddListener(OnChillModeClicked);
            _languageRussianButton.onClick.AddListener(() => OnLanguageSelected(GameLanguage.Russian));
            _languageEnglishButton.onClick.AddListener(() => OnLanguageSelected(GameLanguage.English));
        }

        private void OnTutorialClicked() => _tutorialController.Open();

        private void OnChillModeClicked()
        {
            if (ChillModeEnabled)
            {
                ChillModeEnabled = false;
                UpdateChillModeButtonText();
                return;
            }

            _chillModeWarning.Show(onAccept: () =>
            {
                ChillModeEnabled = true;
                UpdateChillModeButtonText();
            });
        }

        private void UpdateChillModeButtonText() =>
            _chillModeButtonText.text = ChillModeEnabled ? "Выключить чилл режим" : "Включить чилл режим";

        private void OnLanguageSelected(GameLanguage language) => _localization.SetLanguage(language);
    }
}
