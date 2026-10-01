using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

using Core.Board;
using Services;
using Signals;

namespace Controllers
{
    public class DifficultySelectController : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private float _fadeDuration = 0.25f;

        [Header("Фон за панелью — клик закрывает")]
        [SerializeField] private Button _backgroundButton;
        [SerializeField] private Image _panelBackground;

        [Header("Индикатор генерации")]
        [SerializeField] private GameObject _loadingSpinner;

        [Header("Base difficulty (классика 9x9)")]
        [SerializeField] private GameObject _baseDifficultyPanel;
        [SerializeField] private Button _easyButton;
        [SerializeField] private Button _middleButton;
        [SerializeField] private Button _hardButton;
        [SerializeField] private Button _masterlyButton;
        [SerializeField] private Button _impossibleButton;

        [Header("Extra mode: выбор размера поля")]
        [SerializeField] private GameObject _sizeSelectionPanel;
        [SerializeField] private Button _size16Button;
        [SerializeField] private Button _size25Button; 

        [Header("Extra mode: сложность (только 3 варианта)")]
        [SerializeField] private GameObject _extraDifficultyPanel;
        [SerializeField] private Button _extraEasyButton;
        [SerializeField] private Button _extraMiddleButton;
        [SerializeField] private Button _extraHardButton;
        [SerializeField] private Button _backToSizeButton;

        private enum Step { Base, Size, ExtraDifficulty }
        private Step _currentStep;
        private BoardSize _pendingExtraSize;
        private bool _isExtraFlow;

        private GameSessionController _session;
        private GameController _gameController;
        private SettingsController _settingsController;
        private SignalBus _signalBus;

        [Inject] private void Init(
            GameSessionController session,
            GameController gameController,
            SettingsController settingsController,
            IThemeService themeService,
            SignalBus signalBus)
        {
            _session = session;
            _gameController = gameController;
            _settingsController = settingsController;
            _signalBus = signalBus;
            _panelBackground.color = themeService.Current.PopupPanel;
            _signalBus.Subscribe<ThemeChangedSignal>(OnThemeChanged);
        }

        private void OnThemeChanged(ThemeChangedSignal signal) => _panelBackground.color = signal.Theme.PopupPanel;

        private void OnDestroy()
        {
            if (_signalBus != null)
                _signalBus.Unsubscribe<ThemeChangedSignal>(OnThemeChanged);
        }

        private void Awake()
        {
            _backgroundButton.onClick.AddListener(Hide);

            _easyButton.onClick.AddListener(() => OnDifficultySelected(DifficultyLevel.Easy));
            _middleButton.onClick.AddListener(() => OnDifficultySelected(DifficultyLevel.Middle));
            _hardButton.onClick.AddListener(() => OnDifficultySelected(DifficultyLevel.Hard));
            _masterlyButton.onClick.AddListener(() => OnDifficultySelected(DifficultyLevel.Masterly));
            _impossibleButton.onClick.AddListener(() => OnDifficultySelected(DifficultyLevel.Impossible));

            _size16Button.onClick.AddListener(() => OnBoardSizeSelected(BoardSize.Sixteen));
            _size25Button.onClick.AddListener(() => OnBoardSizeSelected(BoardSize.TwentyFive));

            _extraEasyButton.onClick.AddListener(() => OnDifficultySelected(DifficultyLevel.Easy));
            _extraMiddleButton.onClick.AddListener(() => OnDifficultySelected(DifficultyLevel.Middle));
            _extraHardButton.onClick.AddListener(() => OnDifficultySelected(DifficultyLevel.Hard));

            _backToSizeButton.onClick.AddListener(ShowExtraSizeStep);

            Hide();
        }

        public void OpenBaseDifficultySelection()
        {
            _isExtraFlow = false;
            ShowBaseDifficultyStep();
        }

        public void OpenExtraModeSelection()
        {
            _isExtraFlow = true;
            ShowExtraSizeStep();
        }

        private void ShowBaseDifficultyStep()
        {
            _currentStep = Step.Base;
            SetPanels(baseActive: true, sizeActive: false, extraDiffActive: false);
            Show();
        }

        private void ShowExtraSizeStep()
        {
            _currentStep = Step.Size;
            SetPanels(baseActive: false, sizeActive: true, extraDiffActive: false);
            Show();
        }

        private void ShowExtraDifficultyStep()
        {
            _currentStep = Step.ExtraDifficulty;
            SetPanels(baseActive: false, sizeActive: false, extraDiffActive: true);
            Show();
        }

        private void SetPanels(bool baseActive, bool sizeActive, bool extraDiffActive)
        {
            _baseDifficultyPanel.SetActive(baseActive);
            _sizeSelectionPanel.SetActive(sizeActive);
            _extraDifficultyPanel.SetActive(extraDiffActive);
            _loadingSpinner.SetActive(false);
        }

        private void OnBoardSizeSelected(BoardSize size)
        {
            _pendingExtraSize = size;
            ShowExtraDifficultyStep();
        }

        private async void OnDifficultySelected(DifficultyLevel difficulty)
        {
            BoardSize size = _isExtraFlow ? _pendingExtraSize : BoardSize.Nine;

            _loadingSpinner.SetActive(true);

            await _session.StartNewGameAsync(
                size, difficulty, isExtraMode: _isExtraFlow, chillMode: _settingsController.ChillModeEnabled);

            _loadingSpinner.SetActive(false);

            _gameController.ActivateGameScreen();
            Hide();
        }

        public void Show()
        {
            gameObject.SetActive(true);
            DOTween.Kill(this);
            _canvasGroup.alpha = 0f;
            _canvasGroup.DOFade(1f, _fadeDuration)
                .SetUpdate(true)
                .SetId(this)
                .OnStart(() => _canvasGroup.blocksRaycasts = true);
        }

        public void Hide()
        {
            DOTween.Kill(this);
            _canvasGroup.DOFade(0f, _fadeDuration)
                .SetUpdate(true)
                .SetId(this)
                .OnComplete(() =>
                {
                    _canvasGroup.blocksRaycasts = false;
                    gameObject.SetActive(false);
                });
        }
    }
}
