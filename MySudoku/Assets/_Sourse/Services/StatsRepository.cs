using System;
using Core.Board;
using Core.Save;

namespace Services
{
    public interface IStatsRepository
    {
        DifficultyStats GetStats(BoardSize size, DifficultyLevel difficulty, bool isExtraMode);
        void RegisterGameStarted(BoardSize size, DifficultyLevel difficulty, bool isExtraMode);

        void RegisterGameCompleted(
            BoardSize size,
            DifficultyLevel difficulty,
            bool isExtraMode,
            int score,
            float timeSeconds,
            int healthSpent);

       
        int GetOverallBestScore();
    }

    public class StatsRepository : IStatsRepository
    {
        private readonly BinarySerializer _serializer = new BinarySerializer();
        private readonly string _statsPath;
        private StatsData _data;

        public StatsRepository(string statsFilePath)
        {
            _statsPath = statsFilePath;
            _data = _serializer.LoadStats(_statsPath);
        }

        public DifficultyStats GetStats(BoardSize size, DifficultyLevel difficulty, bool isExtraMode)
        {
            var key = new StatsKey { Size = size, Difficulty = difficulty, IsExtraMode = isExtraMode };
            return _data.Entries.TryGetValue(key, out var stats) ? stats : default;
        }

        public void RegisterGameStarted(BoardSize size, DifficultyLevel difficulty, bool isExtraMode)
        {
            var key = new StatsKey { Size = size, Difficulty = difficulty, IsExtraMode = isExtraMode };
            var stats = _data.Entries.TryGetValue(key, out var existing) ? existing : default;

            stats.GamesStarted++;
            _data.Entries[key] = stats;
            Persist();
        }

        public void RegisterGameCompleted(
            BoardSize size,
            DifficultyLevel difficulty,
            bool isExtraMode,
            int score,
            float timeSeconds,
            int healthSpent)
        {
            var key = new StatsKey { Size = size, Difficulty = difficulty, IsExtraMode = isExtraMode };
            var stats = _data.Entries.TryGetValue(key, out var existing) ? existing : default;

            int completedBefore = stats.GamesCompleted;
            stats.AverageScore = RunningAverage(stats.AverageScore, completedBefore, score);
            stats.AverageTimeSeconds = RunningAverage(stats.AverageTimeSeconds, completedBefore, timeSeconds);
            stats.AverageHealthSpent = RunningAverage(stats.AverageHealthSpent, completedBefore, healthSpent);
            stats.GamesCompleted++;
            stats.BestScore = Math.Max(stats.BestScore, score);

            _data.Entries[key] = stats;
            Persist();
        }

        private static float RunningAverage(float currentAvg, int countBefore, float newValue) =>
            (currentAvg * countBefore + newValue) / (countBefore + 1);

        public int GetOverallBestScore()
        {
            int best = 0;
            foreach (var stats in _data.Entries.Values)
                if (stats.BestScore > best) best = stats.BestScore;
            return best;
        }

        private void Persist() => _serializer.SaveStats(_data, _statsPath);
    }
}
