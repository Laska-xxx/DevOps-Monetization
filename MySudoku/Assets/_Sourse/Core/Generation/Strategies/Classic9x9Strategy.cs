using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using Core.Board;
using Core.Generation.Jobs;

namespace Core.Generation.Strategies
{
    public class Classic9x9Strategy : IBoardGenerationStrategy
    {
        private const int N = 9;
        private const int BoxSize = 3;

        public BoardSize Size => BoardSize.Nine;
        public int SideLength => N;
        public int BoxWidth => BoxSize;
        public int BoxHeight => BoxSize;

        private readonly IReadOnlyDictionary<DifficultyLevel, DifficultySettings> _settings;
        private readonly Random _rng = new Random();

        public Classic9x9Strategy(IReadOnlyDictionary<DifficultyLevel, DifficultySettings> settings)
        {
            _settings = settings;
        }

        public async UniTask<BoardModel> GenerateAsync(DifficultyLevel difficulty, CancellationToken ct)
        {
            var settings = _settings[difficulty];

            for (int attempt = 0; attempt < settings.MaxGenerationAttempts; attempt++)
            {
                ct.ThrowIfCancellationRequested();

                byte[] solved = await GenerateFullGridAsync(ct);
                byte[] puzzle = (byte[])solved.Clone();

                bool carved = await TryCarveEmptyCellsAsync(puzzle, settings, ct);
                if (carved)
                    return BoardModel.FromSolutionAndPuzzle(BoardSize.Nine, solved, puzzle);
            }

            throw new InvalidOperationException(
                $"Не удалось сгенерировать судоку 9x9 сложности {difficulty} за {settings.MaxGenerationAttempts} попыток");
        }

        private async UniTask<byte[]> GenerateFullGridAsync(CancellationToken ct)
        {
            var cells = new NativeArray<byte>(N * N, Allocator.TempJob);
            try
            {
                var job = new SudokuGenerationJob
                {
                    Cells = cells,
                    RandomSeed = (uint)_rng.Next(1, int.MaxValue)
                };
                var handle = job.Schedule();
                await UniTask.WaitUntil(() => handle.IsCompleted, cancellationToken: ct);
                handle.Complete();

                var result = new byte[N * N];
                cells.CopyTo(result);
                return result;
            }
            finally
            {
                cells.Dispose();
            }
        }

        private async UniTask<bool> TryCarveEmptyCellsAsync(byte[] puzzle, DifficultySettings settings, CancellationToken ct)
        {
            int[] order = CreateShuffledOrder(N * N);
            int emptyCount = 0;

            foreach (int index in order)
            {
                if (emptyCount >= settings.MaxEmptyCells) break;
                ct.ThrowIfCancellationRequested();

                byte backup = puzzle[index];
                if (backup == 0) continue;
                puzzle[index] = 0;

                int solutions = await CountSolutionsAsync(puzzle, ct);

                if (solutions == 1)
                    emptyCount++;
                else
                    puzzle[index] = backup;
            }

            if (emptyCount < settings.MinEmptyCells)
                return false;

            if (settings.RequireAdvancedTechniques && !RequiresAdvancedTechniques(puzzle))
                return false;

            return true;
        }

        private async UniTask<int> CountSolutionsAsync(byte[] puzzle, CancellationToken ct)
        {
            var cells = new NativeArray<byte>(puzzle, Allocator.TempJob);
            var result = new NativeArray<int>(1, Allocator.TempJob);
            try
            {
                var job = new UniquenessCheckJob
                {
                    Puzzle = cells,
                    N = N,
                    BoxSize = BoxSize,
                    SolutionCount = result
                };
                var handle = job.Schedule();
                await UniTask.WaitUntil(() => handle.IsCompleted, cancellationToken: ct);
                handle.Complete();
                return result[0];
            }
            finally
            {
                cells.Dispose();
                result.Dispose();
            }
        }

        private bool RequiresAdvancedTechniques(byte[] puzzle) =>
            !HumanTechniqueSolver.TrySolveWithBasicTechniques(puzzle, N, BoxSize);

        private int[] CreateShuffledOrder(int count)
        {
            var order = new int[count];
            for (int i = 0; i < count; i++) order[i] = i;
            for (int i = count - 1; i > 0; i--)
            {
                int j = _rng.Next(i + 1);
                (order[i], order[j]) = (order[j], order[i]);
            }
            return order;
        }

        public bool ValidatePlacement(BoardModel board, int row, int col, int value) =>
            BoardValidation.IsPlacementValid(board, row, col, value, BoxSize, BoxSize);

        public bool IsBoardComplete(BoardModel board) => BoardValidation.IsComplete(board);
    }
}
