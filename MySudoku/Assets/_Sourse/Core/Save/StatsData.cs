using System;
using System.Collections.Generic;
using Core.Board;

namespace Core.Save
{
    public struct StatsData
    {
        public Dictionary<StatsKey, DifficultyStats> Entries;
    }

    public struct StatsKey : IEquatable<StatsKey>
    {
        public BoardSize Size;
        public DifficultyLevel Difficulty;
        public bool IsExtraMode;

        public bool Equals(StatsKey other) =>
            Size == other.Size && Difficulty == other.Difficulty && IsExtraMode == other.IsExtraMode;

        public override bool Equals(object obj) => obj is StatsKey other && Equals(other);

        public override int GetHashCode() =>
            ((int)Size * 397) ^ ((int)Difficulty * 31) ^ (IsExtraMode ? 1 : 0);
    }

    public struct DifficultyStats
    {
        public int GamesStarted;
        public int GamesCompleted;
        public float AverageScore;
        public float AverageTimeSeconds;
        public float AverageHealthSpent;
        public int BestScore;
    }
}
