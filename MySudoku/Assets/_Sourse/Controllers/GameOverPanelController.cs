using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

using Services;
using Signals;

namespace Controllers
{
    public class GameOverPanelController : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private float _fadeDuration = 0.25f;
        [SerializeField] private Image _panelBackground;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _exitButton;

        private GameSessionController _session;
        private GameController _gamePresenter;
        private SignalBus _signalBus;

        [Inject] private void Init(
            GameSessionController session,
            GameController gamePresenter,
            IThemeService themeService,
            SignalBus signalBus)
        {
            _session = session;
            _gamePresenter = gamePresenter;
            _signalBus = signalBus;
            _panelBackground.color = themeService.Current.PopupPanel;
            _signalBus.Subscribe<GameOverSignal>(OnGameOver);
            _signalBus.Subscribe<ThemeChangedSignal>(OnThemeChanged);
        }

        private void OnThemeChanged(ThemeChangedSignal signal) => _panelBackground.color = signal.Theme.PopupPanel;

        private void Awake()
        {
            _restartButton.onClick.AddListener(OnRestartClicked);
            _exitButton.onClick.AddListener(OnExitClicked);
            Hide();
        }

        private void OnDestroy()
        {
            if (_signalBus == null) return;
            _signalBus.Unsubscribe<GameOverSignal>(OnGameOver);
            _signalBus.Unsubscribe<ThemeChangedSignal>(OnThemeChanged);
        }

        private void OnGameOver(GameOverSignal signal) => Show();

        private void OnRestartClicked()
        {
            Hide();
            _session.RestartCurrentGame();
            _gamePresenter.RefreshAfterRestart();
        }

        private void OnExitClicked()
        {
            Hide();
            _gamePresenter.ReturnToMainMenu();
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
