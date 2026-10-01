using System.Collections.Generic;
using Core.Board;
using Core.Generation.PuzzleBank;

namespace Installers
{
    public class EmptyPuzzleBankProvider : IPuzzleBankProvider
    {
        public IReadOnlyList<PuzzleBankEntry> GetEntries(BoardSize size, DifficultyLevel difficulty) =>
            new List<PuzzleBankEntry>();
    }
}
