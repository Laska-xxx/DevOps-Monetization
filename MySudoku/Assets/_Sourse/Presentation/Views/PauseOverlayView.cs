using System;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

using Data;
using Services;
using Signals;

namespace Presentation.Views
{
    public class PauseOverlayView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Image _panelBackground;
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _exitToMenuButton;

        public event Action ResumeClicked;
        public event Action ExitToMenuClicked;

        private SignalBus _signalBus;

        [Inject] private void Init(IThemeService themeService, SignalBus signalBus)
        {
            _signalBus = signalBus;
            _panelBackground.color = themeService.Current.PopupPanel;
            _signalBus.Subscribe<ThemeChangedSignal>(OnThemeChanged);
        }

        private void Awake()
        {
            _resumeButton.onClick.AddListener(() => ResumeClicked?.Invoke());
            _exitToMenuButton.onClick.AddListener(() => ExitToMenuClicked?.Invoke());

            Hide();
        }

        private void OnDestroy()
        {
            if (_signalBus != null)
                _signalBus.Unsubscribe<ThemeChangedSignal>(OnThemeChanged);
        }

        private void OnThemeChanged(ThemeChangedSignal signal) => _panelBackground.color = signal.Theme.PopupPanel;

        public void Show()
        {
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
    }
}
