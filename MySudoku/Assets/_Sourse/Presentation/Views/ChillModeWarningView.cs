using System;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

using Data;
using Services;
using Signals;

namespace Presentation.Views
{
    public class ChillModeWarningView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Image _panelBackground;
        [SerializeField] private Button _acceptButton;
        [SerializeField] private Image _acceptButtonImage;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Image _closeButtonImage;

        private Action _onAccept;
        private SignalBus _signalBus;

        [Inject] private void Init(IThemeService themeService, SignalBus signalBus)
        {
            _signalBus = signalBus;
            ApplyTheme(themeService.Current);
            _signalBus.Subscribe<ThemeChangedSignal>(OnThemeChanged);
        }

        private void Awake()
        {
            _acceptButton.onClick.AddListener(HandleAccept);
            _closeButton.onClick.AddListener(HandleClose);

            Hide();
        }

        private void OnDestroy()
        {
            if (_signalBus != null)
                _signalBus.Unsubscribe<ThemeChangedSignal>(OnThemeChanged);
        }

        private void OnThemeChanged(ThemeChangedSignal signal) => ApplyTheme(signal.Theme);

        private void ApplyTheme(ColorThemeSO theme)
        {
            _panelBackground.color = theme.PopupPanel;
            _acceptButtonImage.color = theme.ConfirmButton;
            _closeButtonImage.color = theme.CancelButton;
        }

        public void Show(Action onAccept)
        {
            _onAccept = onAccept;
            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        }

        public void Hide()
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        private void HandleAccept()
        {
            Hide();
            _onAccept?.Invoke();
        }

        private void HandleClose() => Hide();
    }
}
