using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

using Data;
using Services;
using Signals;
using Ads;
using Presentation.Views;

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
        private AdsService _ads;
        private HudView _hud;

        public bool IsOpen { get; private set; }

        private GameSessionController _session;
        private SignalBus _signalBus;

        [Inject]
        private void Init(GameSessionController session, Ads.AdsService ads, Presentation.Views.HudView hud, IThemeService themeService, SignalBus signalBus)
        {
            _session = session;
            _ads = ads;
            _hud = hud;
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
                ? "Жизни закончились. Посмотреть рекламу?"
                : "Бесплатной подсказки нет. Посмотреть рекламу?";
            Show();
        }

        private void OnBuyClicked()
        {
            if (!_ads.IsRewardedReady)
            {
                _messageText.text = "Реклама ещё загружается, попробуйте через пару секунд.";
                _ads.LoadRewarded();
                return;
            }

            _session.Pause();
            _ads.ShowRewarded(
                onRewarded: () =>
                {
                    _session.Resume();
                    if (_currentKind == Kind.ExtraLife)
                    {
                        _session.ReviveFromAd();
                        _hud.SetHealth(_session.CurrentHealth);
                    }
                    else
                    {
                        _session.GrantRewardedHint();
                        _session.UseHint();
                    }
                    Hide();
                },
                onFailed: () =>
                {
                    _session.Resume();
                    _messageText.text = "Реклама не досмотрена — награда не начислена.";
                });
        }

        private void OnCloseClicked()
        {
            Hide();

            if (_currentKind == Kind.ExtraLife)
                _session.ConfirmGameOver();
        }

        private void Show()
        {
            IsOpen = true;
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
            IsOpen = false;
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
