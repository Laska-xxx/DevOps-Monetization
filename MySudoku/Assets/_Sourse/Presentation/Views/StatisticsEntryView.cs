using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Presentation.Views
{
    public struct StatisticsRowData
    {
        public string DifficultyLabel;
        public int GamesStarted;
        public int GamesCompleted;
        public float AverageScore;
        public float AverageTimeSeconds;
        public int BestScore;
    }

    public class StatisticsEntryView : MonoBehaviour
    {
        [SerializeField] private Button _openButton;
        [SerializeField] private GameObject _info;
        [SerializeField] private TextMeshProUGUI _difficultyText;
        [SerializeField] private TextMeshProUGUI _playedText;
        [SerializeField] private TextMeshProUGUI _completedText;
        [SerializeField] private TextMeshProUGUI _avgScoreText;
        [SerializeField] private TextMeshProUGUI _avgTimeText;
        [SerializeField] private TextMeshProUGUI _bestScoreText;

        private bool _isOpen = false;

        private void Awake()
        {
            _openButton.onClick.AddListener(ToggleInfo);
        }

        private void ToggleInfo()
        {
            _isOpen = !_isOpen;
            _info.SetActive(_isOpen);
        }

        public void SetData(StatisticsRowData data)
        {
            _difficultyText.text = data.DifficultyLabel;
            _playedText.text = "Сыграно игр  -  " + data.GamesStarted.ToString();
            _completedText.text = "Завершено игр  -  " + data.GamesCompleted.ToString();
            _avgScoreText.text = "Средний счет  -  " + data.AverageScore.ToString("F0");
            _avgTimeText.text = "Среднее время  -  "+ FormatTime(data.AverageTimeSeconds);
            _bestScoreText.text = "Лучший счет  -  " + data.BestScore.ToString();
        }

        private static string FormatTime(float seconds)
        {
            int total = Mathf.FloorToInt(seconds);
            return $"{total / 60:00}:{total % 60:00}";
        }
    }
}
