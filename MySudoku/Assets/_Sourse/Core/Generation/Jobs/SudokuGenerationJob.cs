using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Core.Generation.Jobs
{
    [BurstCompile]
    public struct SudokuGenerationJob : IJob
    {
        public NativeArray<byte> Cells; 
        public uint RandomSeed;

        private const int N = 9;
        private const int BoxSize = 3;
        private const uint FullMask = 0x1FF; 

        public void Execute()
        {
            for (int i = 0; i < Cells.Length; i++) Cells[i] = 0;

            var rng = new Random(RandomSeed == 0 ? 1u : RandomSeed);
            var triedMask = new NativeArray<uint>(N * N, Allocator.Temp);

            int pos = 0;

            while (pos >= 0 && pos < N * N)
            {
                int row = pos / N;
                int col = pos % N;

                uint candidates = SudokuJobUtils.GetCandidatesMask(Cells, N, BoxSize, row, col, FullMask);
                candidates &= ~triedMask[pos];

                if (candidates == 0)
                {
                    triedMask[pos] = 0;
                    Cells[pos] = 0;
                    pos--;
                    if (pos < 0) break;

                    triedMask[pos] |= 1u << (Cells[pos] - 1);
                    Cells[pos] = 0;
                    continue;
                }

                int value = PickRandomCandidate(candidates, ref rng);
                Cells[pos] = (byte)value;
                pos++;
            }

            triedMask.Dispose();
        }

        private static int PickRandomCandidate(uint mask, ref Random rng)
        {
            int count = 0;
            for (int i = 0; i < N; i++)
                if ((mask & (1u << i)) != 0) count++;

            int target = rng.NextInt(0, count);
            int seen = 0;
            for (int i = 0; i < N; i++)
            {
                if ((mask & (1u << i)) != 0)
                {
                    if (seen == target) return i + 1;
                    seen++;
                }
            }
            return 0; 
        }
    }
}
