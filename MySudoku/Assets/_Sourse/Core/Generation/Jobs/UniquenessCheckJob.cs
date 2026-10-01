using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Core.Generation.Jobs
{
    [BurstCompile]
    public struct UniquenessCheckJob : IJob
    {
        [ReadOnly] public NativeArray<byte> Puzzle;
        public int N;
        public int BoxSize;
        public NativeArray<int> SolutionCount; 

        private const int Limit = 2;

        public void Execute()
        {
            var working = new NativeArray<byte>(Puzzle.Length, Allocator.Temp);
            Puzzle.CopyTo(working);

            var emptyIndices = new NativeList<int>(Allocator.Temp);
            for (int i = 0; i < working.Length; i++)
                if (working[i] == 0) emptyIndices.Add(i);

            var triedMask = new NativeArray<uint>(emptyIndices.Length, Allocator.Temp);
            uint fullMask = N >= 32 ? 0xFFFFFFFFu : (1u << N) - 1u;

            int depth = 0;
            int count = 0;

            while (depth >= 0 && depth <= emptyIndices.Length && count < Limit)
            {
                if (depth == emptyIndices.Length)
                {
                    count++;
                    depth--;
                    if (depth < 0) break;

                    int prevIdx0 = emptyIndices[depth];
                    triedMask[depth] |= 1u << (working[prevIdx0] - 1);
                    working[prevIdx0] = 0;
                    continue;
                }

                int cellIndex = emptyIndices[depth];
                int row = cellIndex / N;
                int col = cellIndex % N;

                uint candidates = SudokuJobUtils.GetCandidatesMask(working, N, BoxSize, row, col, fullMask);
                candidates &= ~triedMask[depth];

                if (candidates == 0)
                {
                    triedMask[depth] = 0;
                    working[cellIndex] = 0;
                    depth--;
                    if (depth < 0) break;

                    int prevIdx = emptyIndices[depth];
                    triedMask[depth] |= 1u << (working[prevIdx] - 1);
                    working[prevIdx] = 0;
                    continue;
                }

                int value = SudokuJobUtils.LowestSetBitValue(candidates);
                working[cellIndex] = (byte)value;
                depth++;
            }

            SolutionCount[0] = count;

            triedMask.Dispose();
            emptyIndices.Dispose();
            working.Dispose();
        }
    }
}
