using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

using Core.Board;
using Presentation.Views;
using Services;

namespace Controllers
{
    public class StatisticsController : ScreenControllerBase
    {
        private static readonly DifficultyLevel[] ClassicDifficulties =
        {
            DifficultyLevel.Easy, DifficultyLevel.Middle, DifficultyLevel.Hard,
            DifficultyLevel.Masterly, DifficultyLevel.Impossible
        };

        private static readonly DifficultyLevel[] ExtraDifficulties =
        {
            DifficultyLevel.Easy, DifficultyLevel.Middle, DifficultyLevel.Hard
        };

        [SerializeField] private Button _size9Tab;
        [SerializeField] private Button _size16Tab;
        [SerializeField] private Button _size25Tab;
        [SerializeField] private Transform _entriesContainer;
        [SerializeField] private StatisticsEntryView _entryPrefab;

        private readonly List<StatisticsEntryView> _pool = new List<StatisticsEntryView>();

        private IScreenService _screenService;
        private IStatsRepository _statsRepository;
        private ILocalizationService _localization;

        [Inject]
        private void Init(
            IScreenService screenService,
            IStatsRepository statsRepository,
            ILocalizationService localization)
        {
            _screenService = screenService;
            _statsRepository = statsRepository;
            _localization = localization;
        }

        private void Awake()
        {
            _size9Tab.onClick.AddListener(() => Render(BoardSize.Nine));
            _size16Tab.onClick.AddListener(() => Render(BoardSize.Sixteen));
            _size25Tab.onClick.AddListener(() => Render(BoardSize.TwentyFive));
        }

        public void Open()
        {
            _screenService.ShowExclusive<StatisticsController>();
            Render(BoardSize.Nine);
        }

        private void Render(BoardSize size)
        {
            bool isExtraMode = size != BoardSize.Nine;
            var difficulties = isExtraMode ? ExtraDifficulties : ClassicDifficulties;

            var rows = new List<StatisticsRowData>();
            foreach (var difficulty in difficulties)
                rows.Add(BuildRow(size, difficulty, isExtraMode));

            EnsurePool(rows.Count);

            for (int i = 0; i < _pool.Count; i++)
            {
                bool active = i < rows.Count;
                _pool[i].gameObject.SetActive(active);
                if (active) _pool[i].SetData(rows[i]);
            }
        }

        private StatisticsRowData BuildRow(BoardSize size, DifficultyLevel difficulty, bool isExtraMode)
        {
            var stats = _statsRepository.GetStats(size, difficulty, isExtraMode);

            return new StatisticsRowData
            {
                DifficultyLabel = _localization.Get(difficulty.ToString()),
                GamesStarted = stats.GamesStarted,
                GamesCompleted = stats.GamesCompleted,
                AverageScore = stats.AverageScore,
                AverageTimeSeconds = stats.AverageTimeSeconds,
                BestScore = stats.BestScore
            };
        }

        private void EnsurePool(int required)
        {
            while (_pool.Count < required)
                _pool.Add(Instantiate(_entryPrefab, _entriesContainer));
        }
    }
}
