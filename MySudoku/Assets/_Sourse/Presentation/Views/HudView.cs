using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Presentation.Views
{
    public class HudView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private TextMeshProUGUI _timerText;
        [SerializeField] private TextMeshProUGUI _difficultyText;
        [SerializeField] private List<Image> _healthIcons; 
        [SerializeField] private Button _exitButton;

        private int _displayedScore;

        public event Action ExitClicked;

        private void Awake()
        {
            _exitButton.onClick.AddListener(() => ExitClicked?.Invoke());
        }

        public void SetScore(int score)
        {
            DOTween.Kill(this);
            DOTween.To(() => _displayedScore, x =>
                {
                    _displayedScore = x;
                    _scoreText.text = x.ToString();
                }, score, 0.4f)
                .SetId(this);
        }

        public void SetHealth(int health)
        {
            for (int i = 0; i < _healthIcons.Count; i++)
            {
                bool filled = i < health;
                _healthIcons[i].DOFade(filled ? 1f : 0.25f, 0.2f);
            }
        }

        public void SetTime(float seconds)
        {
            int total = Mathf.FloorToInt(seconds);
            int minutes = total / 60;
            int secs = total % 60;
            _timerText.text = $"{minutes:00}:{secs:00}";
        }

        public void SetDifficultyLabel(string label) => _difficultyText.text = label;

        public void PlayComboPulse()
        {
            _scoreText.transform.DOKill();
            _scoreText.transform.DOPunchScale(Vector3.one * 0.2f, 0.25f);
        }
    }
}
