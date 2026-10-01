using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "ScoreSettings", menuName = "Sudoku/ScoreSettings")]
    public class ScoreSettingsSO : ScriptableObject
    {
        [field: SerializeField] public int BaseScorePerCell { get; private set; } = 10;
        [field: SerializeField] public int ComboBonusPerStep { get; private set; } = 5;
        [field: SerializeField] public float FastFillThresholdSeconds { get; private set; } = 3f;
        [field: SerializeField] public int FastFillBonus { get; private set; } = 15;
    }
}
