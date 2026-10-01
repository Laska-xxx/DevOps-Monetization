using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

using Data;
using Services;
using Signals;

namespace Controllers
{
    public class PurchaseOfferController : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private float _fadeDuration = 0.25f;
        [SerializeField] private Image _panelBackground;
        [SerializeField] private TextMeshProUGUI _messageText;
        [SerializeField] private Button _buyButton;
        [SerializeField] private Button _closeButton;

        private enum Kind { ExtraLife, ExtraHint }
        private Kind _currentKind;

        private GameSessionController _session;
        private SignalBus _signalBus;

        [Inject] private void Init(GameSessionController session, IThemeService themeService, SignalBus signalBus)
        {
            _session = session;
            _signalBus = signalBus;
            _panelBackground.color = themeService.Current.PopupPanel;
            _signalBus.Subscribe<HealthDepletedSignal>(OnHealthDepleted);
            _signalBus.Subscribe<ThemeChangedSignal>(OnThemeChanged);
        }

        private void OnThemeChanged(ThemeChangedSignal signal) => _panelBackground.color = signal.Theme.PopupPanel;

        private void Awake()
        {
            _buyButton.onClick.AddListener(OnBuyClicked);
            _closeButton.onClick.AddListener(OnCloseClicked);
            Hide();
        }

        private void OnDestroy()
        {
            if (_signalBus != null)
                _signalBus.Unsubscribe<HealthDepletedSignal>(OnHealthDepleted);
        }

        private void OnHealthDepleted(HealthDepletedSignal signal) => ShowFor(Kind.ExtraLife);

        
        public void RequestHintPurchase() => ShowFor(Kind.ExtraHint);

        private void ShowFor(Kind kind)
        {
            _currentKind = kind;
            _messageText.text = kind == Kind.ExtraLife
                ? "Жизни закончились. Купить ещё одну за спец. валюту?"
                : "Бесплатная подсказка уже использована. Купить ещё одну за спец. валюту?";
            Show();
        }

        private void OnBuyClicked()
        {
            bool success = _currentKind == Kind.ExtraLife
                ? _session.TryReviveWithCurrency()
                : _session.UseHint();

            Hide();

            if (!success && _currentKind == Kind.ExtraLife)
                _session.ConfirmGameOver();
        }

        private void OnCloseClicked()
        {
            Hide();

            if (_currentKind == Kind.ExtraLife)
                _session.ConfirmGameOver();
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
