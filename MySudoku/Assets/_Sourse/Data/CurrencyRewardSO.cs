using UnityEngine;
using Core.Board;

namespace Data
{
    [CreateAssetMenu(fileName = "CurrencyReward", menuName = "Sudoku/CurrencyReward")]
    public class CurrencyRewardSO : ScriptableObject
    {
        [field: SerializeField] public int ClassicReward { get; private set; } = 1;
        [field: SerializeField] public int Grid16Reward { get; private set; } = 3;
        [field: SerializeField] public int Grid25Reward { get; private set; } = 5;

        public int GetReward(BoardSize size) => size switch
        {
            BoardSize.Nine => ClassicReward,
            BoardSize.Sixteen => Grid16Reward,
            BoardSize.TwentyFive => Grid25Reward,
            _ => ClassicReward
        };
    }
}
