using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Data;

namespace Presentation.Views
{
    public class CellView : MonoBehaviour
    {
        [SerializeField] private Image _background;
        [SerializeField] private TMP_Text _valueText;
        [SerializeField] private TMP_Text _notesText;
        [SerializeField] private Button _button;

        private ColorThemeSO _theme;

        public int Row { get; private set; }
        public int Col { get; private set; }

        private bool _isPeerHighlighted;
        private bool _isSameValueHighlighted;
        private bool _isError;

        public event Action<CellView> Clicked;

        private void Awake()
        {
            _button.onClick.AddListener(() => Clicked?.Invoke(this));
        }

        public void ApplyTheme(ColorThemeSO theme)
        {
            _theme = theme;
            ApplyBackgroundColor();
        }

        public void Initialize(int row, int col)
        {
            Row = row;
            Col = col;
        }

        public void SetValue(int value, bool isFixed, bool isWrong)
        {
            _valueText.text = value == 0 ? string.Empty : value.ToString();
            _valueText.color = isFixed ? _theme.FixedDigitText : _theme.FilledDigitText;

            if (value != 0)
                _notesText.text = string.Empty;

            _isError = isWrong;
            ApplyBackgroundColor();
        }

        public void SetNotes(IEnumerable<int> notes)
        {
            _notesText.text = string.Join(" ", notes);
        }

        public void SetHighlighted(bool highlighted)
        {
            _isPeerHighlighted = highlighted;
            ApplyBackgroundColor();
        }

        public void SetSameValueHighlight(bool highlighted)
        {
            _isSameValueHighlighted = highlighted;
            ApplyBackgroundColor();
        }

        private void ApplyBackgroundColor()
        {
            if (_theme == null) return;

            Color target = _isError ? _theme.CellError
                : _isSameValueHighlighted ? _theme.CellSameValue
                : _isPeerHighlighted ? _theme.CellHighlight
                : _theme.CellBackground;

            _background.DOColor(target, 0.1f);
        }

        public void PlayCorrectFillAnimation()
        {
            transform.DOKill();
            transform.localScale = Vector3.one;
            transform.DOPunchScale(Vector3.one * 0.15f, 0.2f);
        }

        public void PlayMistakeAnimation()
        {
            transform.DOKill();
            transform.DOShakePosition(0.3f, strength: 5f);

            _background.DOKill();
            _background.DOColor(_theme.CellError, 0.15f)
                .SetLoops(2, LoopType.Yoyo)
                .OnComplete(ApplyBackgroundColor);
        }

        public void ResetVisual()
        {
            _background.DOKill();
            transform.DOKill();
            _isPeerHighlighted = false;
            _isSameValueHighlighted = false;
            _isError = false;
            if (_theme != null) _background.color = _theme.CellBackground;
            transform.localScale = Vector3.one;
        }
    }
}
