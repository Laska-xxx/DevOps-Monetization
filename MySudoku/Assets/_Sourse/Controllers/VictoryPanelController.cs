using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

using Services;
using Signals;

namespace Controllers
{
    public class VictoryPanelController : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private float _fadeDuration = 0.25f;
        [SerializeField] private Image _panelBackground;
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private Button _newGameButton;
        [SerializeField] private Button _exitButton;

        private GameSessionController _session;
        private GameController _gamePresenter;
        private DifficultySelectController _difficultySelectController;
        private SignalBus _signalBus;

        [Inject] private void Init(
            GameSessionController session,
            GameController gamePresenter,
            DifficultySelectController difficultySelectController,
            IThemeService themeService,
            SignalBus signalBus)
        {
            _session = session;
            _gamePresenter = gamePresenter;
            _difficultySelectController = difficultySelectController;
            _signalBus = signalBus;
            _panelBackground.color = themeService.Current.PopupPanel;
            _signalBus.Subscribe<BoardCompletedSignal>(OnBoardCompleted);
            _signalBus.Subscribe<ThemeChangedSignal>(OnThemeChanged);
        }

        private void OnThemeChanged(ThemeChangedSignal signal) => _panelBackground.color = signal.Theme.PopupPanel;

        private void Awake()
        {
            _newGameButton.onClick.AddListener(OnNewGameClicked);
            _exitButton.onClick.AddListener(OnExitClicked);
            Hide();
        }

        private void OnDestroy()
        {
            if (_signalBus == null) return;
            _signalBus.Unsubscribe<BoardCompletedSignal>(OnBoardCompleted);
            _signalBus.Unsubscribe<ThemeChangedSignal>(OnThemeChanged);
        }

        private void OnBoardCompleted(BoardCompletedSignal signal)
        {
            _scoreText.text = signal.FinalScore.ToString();
            Show();
        }

        private void OnNewGameClicked()
        {
            Hide();
            _gamePresenter.ReturnToMainMenu(); 

            if (_session.IsExtraMode) _difficultySelectController.OpenExtraModeSelection();
            else _difficultySelectController.OpenBaseDifficultySelection();
        }

        private void OnExitClicked()
        {
            Hide();
            _gamePresenter.ReturnToMainMenu();
        }

        private static string FormatTime(float seconds)
        {
            int total = Mathf.FloorToInt(seconds);
            return $"{total / 60:00}:{total % 60:00}";
        }

        private void Show()
        {
            gameObject.SetActive(true);
            DOTween.Kill(this);
            _canvasGroup.alpha = 0f;
            _canvasGroup.DOFade(1f, _fadeDuration)
                .SetUpdate(true)
                .SetId(this)
                .OnStart(() => _canvasGroup.blocksRaycasts = true);
        }

        private void Hide()
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
