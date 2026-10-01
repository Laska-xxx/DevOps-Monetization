using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Core.Board;
using Core.Generation.PuzzleBank;

namespace Core.Generation.Strategies
{
    public class Grid25x25Strategy : IBoardGenerationStrategy
    {
        private const int N = 25;
        private const int BoxSize = 5;

        public BoardSize Size => BoardSize.TwentyFive;
        public int SideLength => N;
        public int BoxWidth => BoxSize;
        public int BoxHeight => BoxSize;

        private readonly IPuzzleBankProvider _bank;
        private readonly Random _rng = new Random();

        public Grid25x25Strategy(IPuzzleBankProvider bank)
        {
            _bank = bank;
        }

        public UniTask<BoardModel> GenerateAsync(DifficultyLevel difficulty, CancellationToken ct)
        {
            var entries = _bank.GetEntries(BoardSize.TwentyFive, difficulty);
            if (entries.Count == 0)
                throw new InvalidOperationException($"Банк пазлов 25x25 пуст для сложности {difficulty}");

            var source = entries[_rng.Next(entries.Count)];
            var transformed = PuzzleTransformer.Randomize(source, BoxSize, BoxSize, _rng);

            var board = BoardModel.FromSolutionAndPuzzle(
                BoardSize.TwentyFive,
                transformed.FullSolution,
                PuzzleBankEntry.ApplyMask(transformed.FullSolution, transformed.PuzzleMask));

            return UniTask.FromResult(board);
        }

        public bool ValidatePlacement(BoardModel board, int row, int col, int value) =>
            BoardValidation.IsPlacementValid(board, row, col, value, BoxSize, BoxSize);

        public bool IsBoardComplete(BoardModel board) => BoardValidation.IsComplete(board);
    }
}
