using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Controllers
{
    public class TutorialController : ScreenControllerBase
    {
        [SerializeField] private List<GameObject> _pages;
        [SerializeField] private Button _nextButton;
        [SerializeField] private Button _closeButton;

        private int _currentPageIndex;
        private IScreenService _screenService;

        [Inject] private void Init(IScreenService screenService) => _screenService = screenService;

        private void Awake()
        {
            _nextButton.onClick.AddListener(OnNextClicked);
            _closeButton.onClick.AddListener(Close);
        }

        public void Open()
        {
            _currentPageIndex = 0;
            UpdatePagesVisibility();
            _screenService.ShowExclusive<TutorialController>();
        }

        private void OnNextClicked()
        {
            if (_currentPageIndex >= _pages.Count - 1)
            {
                Close();
                return;
            }

            _currentPageIndex++;
            UpdatePagesVisibility();
        }

        private void Close() => _screenService.ShowExclusive<SettingsController>();

        private void UpdatePagesVisibility()
        {
            for (int i = 0; i < _pages.Count; i++)
                _pages[i].SetActive(i == _currentPageIndex);
        }
    }
}
