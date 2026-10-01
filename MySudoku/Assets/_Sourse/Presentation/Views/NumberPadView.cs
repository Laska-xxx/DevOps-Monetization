using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

using Data;
using Services;
using Signals;

namespace Presentation.Views
{
    public class NumberPadView : MonoBehaviour
    {
        [SerializeField] private RectTransform _container;
        [SerializeField] private Button _buttonPrefab; 
        [SerializeField] private Button _eraseButton;
        [SerializeField] private Button _notesToggleButton;
        [SerializeField] private Image _notesToggleIcon;
        [SerializeField] private Button _hintButton;
        [SerializeField] private Button _undoButton;

        [SerializeField] private Color _notesActiveColor = Color.yellow;
        [SerializeField] private Color _notesInactiveColor = Color.white;

        private readonly List<Button> _numberButtons = new List<Button>();
        private readonly List<Image> _numberButtonImages = new List<Image>();
        private readonly List<bool> _numberButtonAvailable = new List<bool>();

        private IThemeService _themeService;
        private SignalBus _signalBus;

        public event Action<int> NumberClicked;
        public event Action EraseClicked;
        public event Action NotesToggleClicked;
        public event Action HintClicked;
        public event Action UndoClicked;

        [Inject] private void Init(IThemeService themeService, SignalBus signalBus)
        {
            _themeService = themeService;
            _signalBus = signalBus;
            _signalBus.Subscribe<ThemeChangedSignal>(OnThemeChanged);
        }

        private void Awake()
        {
            _eraseButton.onClick.AddListener(() => EraseClicked?.Invoke());
            _notesToggleButton.onClick.AddListener(() => NotesToggleClicked?.Invoke());
            _hintButton.onClick.AddListener(() => HintClicked?.Invoke());
            _undoButton.onClick.AddListener(() => UndoClicked?.Invoke());
        }

        private void OnDestroy()
        {
            if (_signalBus != null)
                _signalBus.Unsubscribe<ThemeChangedSignal>(OnThemeChanged);
        }

        private void OnThemeChanged(ThemeChangedSignal signal) => RepaintAllButtons(signal.Theme);

        public void BuildForSize(int maxValue)
        {
            EnsureButtonCount(maxValue);

            var theme = _themeService.Current;

            for (int i = 0; i < _numberButtons.Count; i++)
            {
                _numberButtons[i].gameObject.SetActive(i < maxValue);
                _numberButtonAvailable[i] = true; 
                _numberButtons[i].interactable = true;
                _numberButtonImages[i].color = theme.NumpadButton;
            }
        }

        private void EnsureButtonCount(int required)
        {
            while (_numberButtons.Count < required)
            {
                int value = _numberButtons.Count + 1;
                var button = Instantiate(_buttonPrefab, _container);
                var label = button.GetComponentInChildren<TMP_Text>();
                label.text = value.ToString();
                button.onClick.AddListener(() => NumberClicked?.Invoke(value));

                _numberButtons.Add(button);
                _numberButtonImages.Add(button.GetComponent<Image>());
                _numberButtonAvailable.Add(true);
            }
        }

        public void SetNotesModeVisual(bool active) =>
            _notesToggleIcon.DOColor(active ? _notesActiveColor : _notesInactiveColor, 0.15f);

        public void SetUndoAvailable(bool available) => _undoButton.interactable = available;

        public void SetHintAvailable(bool available) => _hintButton.interactable = available;

        public void SetNumberAvailable(int value, bool available)
        {
            if (value < 1 || value > _numberButtons.Count) return;

            _numberButtonAvailable[value - 1] = available;
            _numberButtons[value - 1].interactable = available;

            var theme = _themeService.Current;
            _numberButtonImages[value - 1].DOColor(available ? theme.NumpadButton : theme.NumpadButtonLocked, 0.15f);
        }

        private void RepaintAllButtons(ColorThemeSO theme)
        {
            for (int i = 0; i < _numberButtons.Count; i++)
                _numberButtonImages[i].color = _numberButtonAvailable[i] ? theme.NumpadButton : theme.NumpadButtonLocked;
        }
    }
}
