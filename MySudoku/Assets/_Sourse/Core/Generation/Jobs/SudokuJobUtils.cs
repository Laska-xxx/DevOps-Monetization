using Unity.Collections;

namespace Core.Generation.Jobs
{
    internal static class SudokuJobUtils
    {
        public static uint GetCandidatesMask(NativeArray<byte> cells, int n, int boxSize, int row, int col, uint fullMask)
        {
            uint used = 0;

            for (int i = 0; i < n; i++)
            {
                byte rowVal = cells[row * n + i];
                if (rowVal != 0) used |= 1u << (rowVal - 1);

                byte colVal = cells[i * n + col];
                if (colVal != 0) used |= 1u << (colVal - 1);
            }

            if (boxSize > 0)
            {
                int boxRow = (row / boxSize) * boxSize;
                int boxCol = (col / boxSize) * boxSize;

                for (int r = 0; r < boxSize; r++)
                {
                    for (int c = 0; c < boxSize; c++)
                    {
                        byte v = cells[(boxRow + r) * n + (boxCol + c)];
                        if (v != 0) used |= 1u << (v - 1);
                    }
                }
            }

            return (~used) & fullMask;
        }

        public static int LowestSetBitValue(uint mask)
        {
            for (int i = 0; i < 32; i++)
                if ((mask & (1u << i)) != 0) return i + 1;
            return 0;
        }
    }
}
