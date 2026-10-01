using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

using Data;
using Services;
using Signals;

namespace Presentation.Views
{
    public class ConfirmationDialogView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Image _panelBackground;
        [SerializeField] private TMP_Text _messageText;
        [SerializeField] private Button _confirmButton;
        [SerializeField] private Image _confirmButtonImage;
        [SerializeField] private Button _cancelButton;
        [SerializeField] private Image _cancelButtonImage;

        private Action _onConfirm;
        private Action _onCancel;
        private SignalBus _signalBus;

        [Inject] private void Init(IThemeService themeService, SignalBus signalBus)
        {
            _signalBus = signalBus;
            ApplyTheme(themeService.Current);
            _signalBus.Subscribe<ThemeChangedSignal>(OnThemeChanged);
        }

        private void Awake()
        {
            _confirmButton.onClick.AddListener(HandleConfirm);
            _cancelButton.onClick.AddListener(HandleCancel);

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
            _confirmButtonImage.color = theme.ConfirmButton;
            _cancelButtonImage.color = theme.CancelButton;
        }

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

        public void ShowWithMessage(string message, Action onConfirm, Action onCancel = null)
        {
            _messageText.text = message;
            _onConfirm = onConfirm;
            _onCancel = onCancel;
            Show();
        }

        private void HandleConfirm()
        {
            Hide();
            _onConfirm?.Invoke();
        }

        private void HandleCancel()
        {
            Hide();
            _onCancel?.Invoke();
        }
    }
}
