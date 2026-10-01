using System.Collections.Generic;
using Core.Board;

namespace Core.Generation.PuzzleBank
{
    public interface IPuzzleBankProvider
    {
        IReadOnlyList<PuzzleBankEntry> GetEntries(BoardSize size, DifficultyLevel difficulty);
    }
}
