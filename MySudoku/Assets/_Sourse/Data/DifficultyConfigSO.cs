using System;
using System.Collections.Generic;
using UnityEngine;

using Core.Board;
using Core.Generation;

namespace Data
{
    [CreateAssetMenu(fileName = "DifficultyConfig", menuName = "Sudoku/DifficultyConfig")]
    public class DifficultyConfigSO : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            public DifficultyLevel Level;
            public int MinEmptyCells;
            public int MaxEmptyCells;
            public bool RequireAdvancedTechniques;
            public int MaxGenerationAttempts;
        }

        [SerializeField] private List<Entry> _entries = new List<Entry>
        {
            new Entry { Level = DifficultyLevel.Easy, MinEmptyCells = 30, MaxEmptyCells = 35, RequireAdvancedTechniques = false, MaxGenerationAttempts = 20 },
            new Entry { Level = DifficultyLevel.Middle, MinEmptyCells = 36, MaxEmptyCells = 45, RequireAdvancedTechniques = false, MaxGenerationAttempts = 20 },
            new Entry { Level = DifficultyLevel.Hard, MinEmptyCells = 46, MaxEmptyCells = 52, RequireAdvancedTechniques = false, MaxGenerationAttempts = 20 },
            new Entry { Level = DifficultyLevel.Masterly, MinEmptyCells = 53, MaxEmptyCells = 58, RequireAdvancedTechniques = true, MaxGenerationAttempts = 30 },
            new Entry { Level = DifficultyLevel.Impossible, MinEmptyCells = 59, MaxEmptyCells = 64, RequireAdvancedTechniques = true, MaxGenerationAttempts = 40 },
        };

        public IReadOnlyDictionary<DifficultyLevel, DifficultySettings> BuildSettings()
        {
            var result = new Dictionary<DifficultyLevel, DifficultySettings>();

            foreach (var entry in _entries)
            {
                result[entry.Level] = new DifficultySettings
                {
                    MinEmptyCells = entry.MinEmptyCells,
                    MaxEmptyCells = entry.MaxEmptyCells,
                    RequireAdvancedTechniques = entry.RequireAdvancedTechniques,
                    MaxGenerationAttempts = entry.MaxGenerationAttempts
                };
            }

            return result;
        }
    }
}
