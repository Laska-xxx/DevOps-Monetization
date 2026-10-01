using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

using Services;
using Signals;

namespace Controllers
{
    public class MainMenuController : ScreenControllerBase
    {
        [SerializeField] private Button _continueButton;
        [SerializeField] private Button _newGameButton;
        [SerializeField] private Button _extraModeButton;
        [SerializeField] private TextMeshProUGUI _bestScoreText;
        [SerializeField] private TextMeshProUGUI _currencyText;

        private int _displayedCurrency;

        private GameSessionController _session;
        private IScreenService _screenService;
        private ISaveService _saveService;
        private ICurrencyService _currencyService;
        private IStatsRepository _statsRepository;
        private GameController _gameController;
        private DifficultySelectController _difficultySelectController;
        private SignalBus _signalBus;

        [Inject] private void Init(
            GameSessionController session,
            IScreenService screenService,
            ISaveService saveService,
            ICurrencyService currencyService,
            IStatsRepository statsRepository,
            GameController gameController,
            DifficultySelectController difficultySelectController,
            SignalBus signalBus)
        {
            _session = session;
            _screenService = screenService;
            _saveService = saveService;
            _currencyService = currencyService;
            _statsRepository = statsRepository;
            _gameController = gameController;
            _difficultySelectController = difficultySelectController;
            _signalBus = signalBus;

            _signalBus.Subscribe<ReturnedToMainMenuSignal>(OnReturnedToMainMenu);

            RefreshMenuState();
        }

        private void Awake()
        {
            _continueButton.onClick.AddListener(OnContinueClicked);
            _newGameButton.onClick.AddListener(OnNewGameClicked);
            _extraModeButton.onClick.AddListener(OnExtraModeClicked);
        }

        private void OnDestroy()
        {
            if (_signalBus != null)
                _signalBus.Unsubscribe<ReturnedToMainMenuSignal>(OnReturnedToMainMenu);
        }

        public void RefreshMenuState()
        {
            _continueButton.gameObject.SetActive(_saveService.HasSave);
            _bestScoreText.text = _statsRepository.GetOverallBestScore().ToString();

            DOTween.Kill(this);
            DOTween.To(() => _displayedCurrency, x =>
                {
                    _displayedCurrency = x;
                    _currencyText.text = x.ToString();
                }, _currencyService.Balance, 0.4f)
                .SetId(this);
        }

        private void OnContinueClicked()
        {
            if (!_session.TryContinueGame()) return;

            _gameController.ActivateGameScreen();
            Hide();
        }

        private void OnNewGameClicked() => _difficultySelectController.OpenBaseDifficultySelection();

        private void OnExtraModeClicked() => _difficultySelectController.OpenExtraModeSelection();

        private void OnReturnedToMainMenu(ReturnedToMainMenuSignal signal)
        {
            RefreshMenuState();
            _screenService.ShowExclusive<MainMenuController>();
        }
    }
}
