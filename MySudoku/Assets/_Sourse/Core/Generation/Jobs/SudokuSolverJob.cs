using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Core.Generation.Jobs
{
    [BurstCompile]
    public struct SudokuSolverJob : IJob
    {
        public NativeArray<byte> Cells;
        public int N;
        public int BoxSize;
        public NativeArray<int> Solved; 

        public void Execute()
        {
            var emptyIndices = new NativeList<int>(Allocator.Temp);
            for (int i = 0; i < Cells.Length; i++)
                if (Cells[i] == 0) emptyIndices.Add(i);

            var triedMask = new NativeArray<uint>(emptyIndices.Length, Allocator.Temp);
            uint fullMask = N >= 32 ? 0xFFFFFFFFu : (1u << N) - 1u;

            int depth = 0;
            bool solved = false;

            while (depth >= 0 && depth <= emptyIndices.Length)
            {
                if (depth == emptyIndices.Length)
                {
                    solved = true;
                    break;
                }

                int cellIndex = emptyIndices[depth];
                int row = cellIndex / N;
                int col = cellIndex % N;

                uint candidates = SudokuJobUtils.GetCandidatesMask(Cells, N, BoxSize, row, col, fullMask);
                candidates &= ~triedMask[depth];

                if (candidates == 0)
                {
                    triedMask[depth] = 0;
                    Cells[cellIndex] = 0;
                    depth--;
                    if (depth < 0) break;

                    int prevIdx = emptyIndices[depth];
                    triedMask[depth] |= 1u << (Cells[prevIdx] - 1);
                    Cells[prevIdx] = 0;
                    continue;
                }

                int value = SudokuJobUtils.LowestSetBitValue(candidates);
                Cells[cellIndex] = (byte)value;
                depth++;
            }

            Solved[0] = solved ? 1 : 0;

            triedMask.Dispose();
            emptyIndices.Dispose();
        }
    }
}
