using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Data;
using Services;
using Signals;

namespace Presentation.Views
{
    public class BoardView : MonoBehaviour
    {
        [SerializeField] private RectTransform _gridContainer;
        [SerializeField] private GridLayoutGroup _gridLayout;
        [SerializeField] private CellView _cellPrefab;

        [Header("Толстые линии между боксами (3x3 / 4x4 / 5x5)")]
        [SerializeField] private RectTransform _dividersContainer;
        [SerializeField] private Image _dividerPrefab;
        [SerializeField] private float _boxDividerThickness = 4f;

        private readonly List<CellView> _pool = new List<CellView>();
        private readonly List<Image> _dividerPool = new List<Image>();
        private int _activeSize;

        private IThemeService _themeService;
        private SignalBus _signalBus;

        public event Action<int, int> CellClicked;

        [Inject] private void Init(IThemeService themeService, SignalBus signalBus)
        {
            _themeService = themeService;
            _signalBus = signalBus;
            _signalBus.Subscribe<ThemeChangedSignal>(OnThemeChanged);
        }

        private void OnDestroy()
        {
            _signalBus?.Unsubscribe<ThemeChangedSignal>(OnThemeChanged);
        }

        private void OnThemeChanged(ThemeChangedSignal signal) => ApplyThemeToAll(signal.Theme);

        private void ApplyThemeToAll(ColorThemeSO theme)
        {
            foreach (var cell in _pool)
                cell.ApplyTheme(theme);

            foreach (var divider in _dividerPool)
                divider.color = theme.GridBorder;
        }

        public void BuildBoard(int sideLength, int boxWidth, int boxHeight)
        {
            _activeSize = sideLength;
            EnsurePoolSize(sideLength * sideLength);

            var padding = _gridLayout.padding;
            var spacing = _gridLayout.spacing;

            float availableWidth = _gridContainer.rect.width
                - padding.left - padding.right
                - spacing.x * (sideLength - 1);

            float availableHeight = _gridContainer.rect.height
                - padding.top - padding.bottom
                - spacing.y * (sideLength - 1);

            float cellSize = Mathf.Min(availableWidth, availableHeight) / sideLength;

            _gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            _gridLayout.constraintCount = sideLength;
            _gridLayout.cellSize = new Vector2(cellSize, cellSize);

            var theme = _themeService.Current;

            for (int i = 0; i < _pool.Count; i++)
            {
                bool active = i < sideLength * sideLength;
                _pool[i].gameObject.SetActive(active);

                if (active)
                {
                    int row = i / sideLength;
                    int col = i % sideLength;
                    _pool[i].Initialize(row, col);
                    _pool[i].ApplyTheme(theme);
                    _pool[i].ResetVisual();
                }
            }

            BuildBoxDividers(sideLength, boxWidth, boxHeight, cellSize, theme);
        }

        private void EnsurePoolSize(int required)
        {
            while (_pool.Count < required)
            {
                var cell = Instantiate(_cellPrefab, _gridContainer);
                cell.Clicked += view => CellClicked?.Invoke(view.Row, view.Col);
                _pool.Add(cell);
            }
        }

        private void BuildBoxDividers(int sideLength, int boxWidth, int boxHeight, float cellSize, ColorThemeSO theme)
        {
            var lines = new List<(bool vertical, int index)>();

            if (boxWidth > 0)
                for (int c = boxWidth; c < sideLength; c += boxWidth)
                    lines.Add((true, c));

            if (boxHeight > 0)
                for (int r = boxHeight; r < sideLength; r += boxHeight)
                    lines.Add((false, r));

            EnsureDividerPool(lines.Count);

            var padding = _gridLayout.padding;
            var spacing = _gridLayout.spacing;

            for (int i = 0; i < _dividerPool.Count; i++)
            {
                bool active = i < lines.Count;
                _dividerPool[i].gameObject.SetActive(active);
                if (!active) continue;

                var (vertical, index) = lines[i];
                var rt = (RectTransform)_dividerPool[i].transform;
                _dividerPool[i].color = theme.GridBorder;

                if (vertical)
                {
                    float x = padding.left
                            + index * cellSize
                            + (index - 0.5f) * spacing.x;

                    rt.anchorMin = new Vector2(0f, 0f);
                    rt.anchorMax = new Vector2(0f, 1f);
                    rt.pivot = new Vector2(0.5f, 0.5f);
                    rt.sizeDelta = new Vector2(_boxDividerThickness, 0f);
                    rt.anchoredPosition = new Vector2(x, 0f);
                }
                else
                {
                    float y = padding.top
                            + index * cellSize
                            + (index - 0.5f) * spacing.y;

                    rt.anchorMin = new Vector2(0f, 1f);
                    rt.anchorMax = new Vector2(1f, 1f);
                    rt.pivot = new Vector2(0.5f, 0.5f);
                    rt.sizeDelta = new Vector2(0f, _boxDividerThickness);
                    rt.anchoredPosition = new Vector2(0f, -y);
                }
            }
        }

        private void EnsureDividerPool(int required)
        {
            while (_dividerPool.Count < required)
            {
                var divider = Instantiate(_dividerPrefab, _dividersContainer);
                _dividerPool.Add(divider);
            }
        }

        public CellView GetCell(int row, int col) => _pool[row * _activeSize + col];

        public void RenderCell(int row, int col, int value, bool isFixed, IEnumerable<int> notes, bool isWrong)
        {
            var cell = GetCell(row, col);
            cell.SetValue(value, isFixed, isWrong);
            cell.SetNotes(notes);
        }

        public void ClearHighlights()
        {
            for (int i = 0; i < _activeSize * _activeSize; i++)
            {
                if (!_pool[i].gameObject.activeSelf) continue;
                _pool[i].SetHighlighted(false);
                _pool[i].SetSameValueHighlight(false);
            }
        }

        public void HighlightPeers(int row, int col, int boxWidth, int boxHeight)
        {
            ClearHighlights();

            for (int i = 0; i < _activeSize; i++)
            {
                GetCell(row, i).SetHighlighted(true);
                GetCell(i, col).SetHighlighted(true);
            }

            if (boxWidth > 0 && boxHeight > 0)
            {
                int boxRow = (row / boxHeight) * boxHeight;
                int boxCol = (col / boxWidth) * boxWidth;

                for (int r = 0; r < boxHeight; r++)
                    for (int c = 0; c < boxWidth; c++)
                        GetCell(boxRow + r, boxCol + c).SetHighlighted(true);
            }
        }

        public void SetSameValueHighlight(int row, int col, bool highlighted) =>
            GetCell(row, col).SetSameValueHighlight(highlighted);

        public void ClearSameValueHighlights()
        {
            for (int i = 0; i < _activeSize * _activeSize; i++)
            {
                if (!_pool[i].gameObject.activeSelf) continue;
                _pool[i].SetSameValueHighlight(false);
            }
        }

        public void PlayWinSequence()
        {
            var sequence = DOTween.Sequence();
            int count = _activeSize * _activeSize;

            for (int i = 0; i < count; i++)
            {
                if (!_pool[i].gameObject.activeSelf) continue;
                var cell = _pool[i];
                sequence.Insert(i * 0.01f, cell.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f));
            }
        }
    }
}
